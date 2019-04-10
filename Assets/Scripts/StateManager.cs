using System.Collections.Generic;
using UnityEngine;
using Log;
using static State;
using System;
using System.Linq;

public class StateManager : MonoBehaviour {
    HashSet<Square> lockedSquares = new HashSet<Square>();

    // FOR TESTING:
    public Board board;
    public List<Board> moveHistory;


    /* FOR TESTING:
     *  | 0  | 1  | 2  | 3  | 4  | 5  |
     * --------------------------------
     *0 | B2 | A4 |    |    |    |    |
     *1 | A3 | B1 |    |    |    |    |
     *2 |    |    |    |    |    | B4 |
     *3 |    |    | A1 | A2 | B3 |    |
     *4 |    |    |    |    |    |    |
     *5 |    |    |    |    |    |    |
     * -------------------------------
     */

    public void CreatePieces(int numPieces)//sets up creating numbered pieces for 2 players
    {
        CreatePieces(Player.A, numPieces);//uses the overloaded method 
        CreatePieces(Player.B, numPieces);//uses the overloaded method

        moveHistory.Add(new Board(board));
    }

    public void CreatePieces(Player player, int numPieces)//recursively create for 1 player, overload
    {
        Piece piece;
        Square square;
        //create given number of pieces to the player
        for (int pieceNumber = 1; pieceNumber <= numPieces; pieceNumber++)
        {
            //Create Player A pieces
            piece = ChoosePiece(pieceNumber, player);//create piece
            square = ChooseSquare(piece);//create a square for the piece to place

            board.SetPieceAtSquare(piece, square);//set piece at a destinated square
            //Debug.Log(string.Format("{0} {1}", piece, square));
        } 
    }

    public Piece ChoosePiece(int pieceNumber, Player player)//piecenumber is from the for loop, player is from player(Player.A,Player.B)
    {
        //STUB
        Piece.Type type = ChooseType();

        return new Piece(player.ToString() + pieceNumber, player, type);
    }

    public Piece.Type ChooseType()
    {
        //STUB
        return Piece.Type.SWORD;
    }

    //Actual will not contain @param piece
    //set the pieces to the destinated square
    public Square ChooseSquare(Piece piece)
    {
        //STUB
        switch (piece.uid)
        {
            case "A1": return new Square(0, 0);
            case "A2": return new Square(2, 2);
            case "A3": return new Square(1, 3);

            case "B1": return new Square(3, 2);
            case "B2": return new Square(2, 3);
            case "B3": return new Square(3, 4);

            default: return null;
        }
    }

    public void CreateBoard(int rows = 6, int cols = 6)
    {
        board = new Board(rows, cols);
        moveHistory = new List<Board>();
    }

    //public void Start()
    //{
    //    CreateBoard(6, 6);
    //    CreatePieces(3);

    //    //Run
    //    CalculateNextState();
    //}

    //public Move[] PlayMoves()
    //{
    //    Move[] moves = new Move[board.GetPieces().Count];

    //    int i = 0;

    //    foreach (Piece piece in board.GetPieces())
    //    {
    //        moves[i] = ChooseMove(piece);
    //        i++;
    //    }

    //    return moves;
    //}

    public Move ChooseMove(Piece piece)
    {
        //STUB
        switch (piece.uid)
        {
            case "A1": return piece.MoveRight();
            case "A2": return piece.MoveLeft();
            case "A3": return piece.MoveRight();

            case "B1": return piece.MoveUp();
            case "B2": return piece.MoveUp();
            case "B3": return piece.MoveLeft();

            default: return null;
        }
    }

    public void CalculateNextState(Move[] moves)
    {
        MoveData[] resolvedMoveDatas = ResolveMovement(moves);
        PhaseLog[] phaseLogs = ResolveCombat(resolvedMoveDatas);

        Debug.Log("PL Format: puid, health, combatSq, finalSq");
        Debug.Log("Attack Format: puid, damage, direction");

        foreach (PhaseLog pl in phaseLogs)
        {
            string plDebug = string.Format(
                "{0} {1} {2} {3}",
                pl.piece.uid,
                pl.piece.health,
                pl.moveLog.combatSquare,
                pl.moveLog.finalSquare
            );
            Debug.Log(plDebug);
            foreach (AttackLog al in pl.attackLogs)
            {
                string alDebug = string.Format(
                   "{0} {1} {2}",
                   pl.piece.uid,
                   al.damage,
                   Move.DirectionToString(al.direction)
               );
                Debug.Log(alDebug);
            }
        }

        board.ResetPositions();

        for (int i = 0; i < resolvedMoveDatas.Length; i++)
        {
            Piece piece = resolvedMoveDatas[i].piece;
            Square nextSquare = resolvedMoveDatas[i].currentSquare;

            board.SetPieceAtSquare(new Piece(piece), nextSquare);
        }

        moveHistory.Add(new Board(board));
    }

    class MoveData
    {
        public Piece piece;
        public Move.Direction direction;

        public Square currentSquare; // piece's current square
        public Square nextSquare; // cached next square

        public bool isBounce;

        public MoveData(Move move, Square currentSquare, Square nextSquare)
        {
            piece = move.piece;
            direction = move.direction;

            this.currentSquare = currentSquare;
            this.nextSquare = nextSquare;

            isBounce = false;
        }

        public override string ToString()
        {
            return string.Format(
                "[MoveData Piece {0}, currentSq {1}, direction {2}, isBounce {3}]",
                piece.uid,
                currentSquare,
                direction.ToString(),
                isBounce
            );
        }
    }

    void ResetLockedSquares()
    {
        lockedSquares = new HashSet<Square>();
    }

    void LockSquares(IEnumerable<Square> squares)
    {
        foreach (Square square in squares)
            LockSquare(square);
    }

    void LockSquare(Square square)
    {
        lockedSquares.Add(square);
    }

    bool IsSquareLocked(Square square)
    {
        return lockedSquares.Contains(square);
    }

    PhaseLog[] ResolveCombat(MoveData[] moveDatas)
    {
        PhaseLog[] phaseLogs = new PhaseLog[moveDatas.Length];
        for (int i = 0; i < moveDatas.Length; i++)
        {
            MoveData moveData = moveDatas[i];
            Piece piece = moveData.piece;
            Move.Direction direction = moveData.direction;
            // where the piece is during combat
            Square combatSquare = moveData.isBounce ? moveData.nextSquare : moveData.currentSquare;

            // check for attackers
            List<MoveData> attackedBy = new List<MoveData>();
            foreach (MoveData otherMoveData in moveDatas)
            {
                Piece otherPiece = otherMoveData.piece;
                if (piece == otherPiece) continue;
                if (otherMoveData.direction == Move.Direction.NONE) continue;
                if (piece.owner == otherPiece.owner) continue;
                Square attackingSquare = otherMoveData.nextSquare;
                if (combatSquare == attackingSquare) attackedBy.Add(otherMoveData);
            }

            // Create Phase Log Here
            List<AttackLog> attackLogs = ApplyAndRecordDamage(piece, attackedBy);
            Square finalSquare = moveData.isBounce ? moveData.currentSquare : combatSquare;
            MoveLog moveLog = new MoveLog(combatSquare, finalSquare);
            PhaseLog phaseLog = new PhaseLog(piece, moveLog, attackLogs);
            phaseLogs[i] = phaseLog;
        }

        return phaseLogs;
    }

    // Damage Calculation to be implemented
    // This is the retaliation damage done to other enemies
    List<AttackLog> ApplyAndRecordDamage(Piece piece, ICollection<MoveData> attackerMoveDatas)
    {
        int baseDamage = piece.attack;
        List<AttackLog> attackLogs = new List<AttackLog>();
        foreach (MoveData attackerMoveData in attackerMoveDatas)
        {
            int damage = baseDamage;
            // apply enemy specific multipliers here

            Piece attacker = attackerMoveData.piece;
            attacker.health -= damage;
            Move.Direction directionOfRetaliation = Move.OppositeDirection(attackerMoveData.direction);
            AttackLog attackLog = new AttackLog(damage, directionOfRetaliation);
            attackLogs.Add(attackLog);
        }
        return attackLogs;
    }

    MoveData[] ResolveMovement(Move[] moves)
    {
        ResetLockedSquares();

        MoveData[] moveMetaDatas =
            moves.Select(move => new MoveData(move, board.GetCurrentSquare(move.piece), board.NextSquare(move)))
                 .ToArray();

        List<int> moveIndices = Enumerable.Range(0, moves.Length).ToList();

        // Consider stationary pieces
        moveIndices = moveIndices.Where(mInd =>
        {
            MoveData moveData = moveMetaDatas[mInd];
            bool isStationary =
                moveData.direction == Move.Direction.NONE ||
                moveData.currentSquare == moveData.nextSquare;
            if (isStationary) LockSquare(moveData.currentSquare);
            return !isStationary;
        }).ToList();

        bool hasInvalidMoves = true;

        while (hasInvalidMoves)
        {
            int previousLength = moveIndices.Count;
            moveIndices = moveIndices.Where((mInd, index) =>
            {
                MoveData moveData = moveMetaDatas[mInd];
                bool isValidMove = !moveData.isBounce;
                //Consider entering a locked square
                if (IsSquareLocked(moveData.nextSquare))
                {
                    LockSquare(moveData.currentSquare);
                    isValidMove = false;
                }

                //Consider interactions with other pieces
                for (int i = 0; i < moveIndices.Count; i++) // can optimize
                {
                    if (i == index) continue;
                    int otherIndex = moveIndices[i]; // other move index for consideration
                    MoveData otherMoveData = moveMetaDatas[otherIndex];

                    bool enteringSameSquare = moveData.nextSquare == otherMoveData.nextSquare;
                    bool opposingMovement =
                        moveData.nextSquare == otherMoveData.currentSquare &&
                        moveData.currentSquare == otherMoveData.nextSquare;

                    if (enteringSameSquare || opposingMovement)
                    {
                        LockSquare(moveData.currentSquare);
                        LockSquare(otherMoveData.currentSquare);
                        isValidMove = false;
                    }
                }
                return isValidMove;
            }).ToList();
            hasInvalidMoves = moveIndices.Count != previousLength;
        }

        // apply valid moves
        List<int> validMoveIndices = moveIndices;
        validMoveIndices.ForEach(validMoveIndex =>
        {
            MoveData moveData = moveMetaDatas[validMoveIndex];
            //Debug.Assert(!IsSquareLocked(moveData.nextSquare)); // sanity check
            //Debug.Assert(!moveData.isBounce); // sanity check
            moveData.currentSquare = moveData.nextSquare;
            moveData.direction = Move.Direction.NONE;
        });

        // figure out bounces
        for (int i = 0; i < moveMetaDatas.Length; i++)
        {
            MoveData moveData = moveMetaDatas[i];
            // piece did not move or already moved to a unlocked (empty) square
            if (moveData.direction == Move.Direction.NONE) continue;
            bool nextSquareLocked = IsSquareLocked(moveData.nextSquare);
            moveData.isBounce = !nextSquareLocked;
        }

        // return
        return moveMetaDatas;
    }
}
