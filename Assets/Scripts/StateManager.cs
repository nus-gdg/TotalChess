using System.Collections.Generic;
using UnityEngine;
using Log;
using State;
using System;
using System.Linq;

public class StateManager : MonoBehaviour {
    HashSet<Square> lockedSquares = new HashSet<Square>();
    // FOR TESTING:
    Board board = new Board(6, 6);                   //  | 0  | 1  | 2  | 3  | 4  | 5  |
                                                     //---------------------------------
    Piece A_PIECE_1 = new Piece("uida1", Player.A);  //0 | A1 |    |    |    |    |    |
    Piece A_PIECE_2 = new Piece("uida2", Player.A);  //1 |    |    |    | A3 |    |    |
    Piece A_PIECE_3 = new Piece("uida3", Player.A);  //2 |    |    | A2 | B2 |    |    |
                                                     //3 |    |    | B1 |    | B3 |    |
    Piece B_PIECE_1 = new Piece("uidb1", Player.B);  //4 |    |    |    |    |    |    |
    Piece B_PIECE_2 = new Piece("uidb2", Player.B);  //5 |    |    |    |    |    |    |
    Piece B_PIECE_3 = new Piece("uidb3", Player.B);  //---------------------------------

    void Start()
    {
        board.SetPieceAtSquare(A_PIECE_1, new Square(0, 0));
        board.SetPieceAtSquare(A_PIECE_2, new Square(2, 2));
        board.SetPieceAtSquare(A_PIECE_3, new Square(1, 3));
        board.SetPieceAtSquare(B_PIECE_1, new Square(3, 2));
        board.SetPieceAtSquare(B_PIECE_2, new Square(2, 3));
        board.SetPieceAtSquare(B_PIECE_3, new Square(3, 4));
        CalculateNextState();
    }

    void CalculateNextState()
    {
        Move[] moves = new Move[]
        {
            new Move(A_PIECE_1),
            new Move(A_PIECE_2, Move.Direction.RIGHT),
            new Move(A_PIECE_3, Move.Direction.DOWN),
            new Move(B_PIECE_1, Move.Direction.UP),
            new Move(B_PIECE_2, Move.Direction.UP),
            new Move(B_PIECE_3, Move.Direction.LEFT),
        };
        MoveData[] resolvedMoveDatas = ResolveMovement(moves);
        PhaseLog[] phaseLogs = ResolveCombat(resolvedMoveDatas);

        Debug.Log("PL Format: puid, health, combatSq, finalSq");
        Debug.Log("Attack Format: puid, damage, direction");
        foreach (PhaseLog pl in phaseLogs)
        {
            String plDebug = string.Format(
                "{0} {1} {2} {3}",
                pl.piece.uid,
                pl.piece.health,
                pl.moveLog.combatSquare,
                pl.moveLog.finalSquare
            );
            Debug.Log(plDebug);
            foreach (AttackLog al in pl.attackLogs)
            {
                String alDebug = string.Format(
                   "{0} {1} {2}",
                   pl.piece.uid,
                   al.damage,
                   Move.DirectionToString(al.direction)
               );
                Debug.Log(alDebug);
            }
        }
    }

    class MoveData
    {
        public Move.Direction direction;
        public Piece piece;
        public Square currentSquare; // piece's current square
        public Square nextSquare; // cached next square
        public bool isBounce = false;

        public MoveData(Move move, Square currentSquare, Square nextSquare)
        {
            this.direction = move.direction;
            this.piece = move.piece;
            this.currentSquare = currentSquare;
            this.nextSquare = nextSquare;
        }

        public override string ToString()
        {
            return String.Format(
                "[MoveData Piece {0}, currentSq {1}, direction {2}, isBounce {3}]",
                piece.uid,
                currentSquare,
                Move.DirectionToString(direction),
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
            double damage = baseDamage;
            // apply enemy specific multipliers here
            // damage dampening if outnumbered
            switch (attackerMoveDatas.Count)
            {
                case 2:   // 40% on 2 enemies
                    damage *= 0.4;  break;
                case 3:   // 20% on 3 enemies
                    damage *= 0.2;  break;
                case 4:   // 5% on 4 enemies
                    damage *= 0.05; break;
            }
            if (piece.IsCounteredBy(attackerMoveData.piece)) damage *= 1.2;
            damage *= piece.health / (double)piece.maxHealth;

            Piece attacker = attackerMoveData.piece;
            attacker.health -= (damage < 1) ? 1 : (int)damage;   // at least deal 1 dmg
            Move.Direction directionOfRetaliation = Move.OppositeDirection(attackerMoveData.direction);
            AttackLog attackLog = new AttackLog((int)damage, directionOfRetaliation);
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
            Debug.Assert(!IsSquareLocked(moveData.nextSquare)); // sanity check
            Debug.Assert(!moveData.isBounce); // sanity check
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
