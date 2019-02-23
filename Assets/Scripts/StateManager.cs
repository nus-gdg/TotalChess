using System.Collections.Generic;
using UnityEngine;
using State;
using System;
using System.Linq;

public class StateManager : MonoBehaviour {
    HashSet<Square> lockedSquares = new HashSet<Square>();
    // FOR TESTING:
    Board board = new Board(6, 6);                   //-------------------------------
    Piece A_PIECE_1 = new Piece("uida1", Player.A);  //| A1 |    |    |    |    |    |
    Piece A_PIECE_2 = new Piece("uida2", Player.A);  //|    |    |    | A3 |    |    |
    Piece A_PIECE_3 = new Piece("uida3", Player.A);  //|    |    | A2 |    |    |    |
                                                     //|    |    | B1 | B2 | B3 |    |
    Piece B_PIECE_1 = new Piece("uidb1", Player.B);  //|    |    |    |    |    |    |
    Piece B_PIECE_2 = new Piece("uidb2", Player.B);  //|    |    |    |    |    |    |
    Piece B_PIECE_3 = new Piece("uidb3", Player.B);  //-------------------------------
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
        MoveMetaData[] resolvedMoveDatas = ResolveMovement(moves);
        foreach (MoveMetaData resolveData in resolvedMoveDatas)
        {
            Debug.Log(resolveData);
        }
        ResolveCombat(resolvedMoveDatas);
    }

    class MoveMetaData
    {
        public Move.Direction direction;
        public Piece piece;
        public Square currentSquare; // piece's current square
        public Square nextSquare; // cached next square
        public bool isBounce = false;

        public MoveMetaData(Move move, Square currentSquare, Square nextSquare)
        {
            this.direction = move.direction;
            this.piece = move.piece;
            this.currentSquare = currentSquare;
            this.nextSquare = nextSquare;
        }

        public override string ToString()
        {
            return String.Format(
                "[Move Piece {0}, currentSq {1}, direction {2}, isBounce {3}]",
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

    void ResolveCombat(MoveMetaData[] moveDatas)
    {
        foreach (MoveMetaData moveData in moveDatas)
        {
            Piece piece = moveData.piece;
            Move.Direction direction = moveData.direction;
            // where the piece is during combat
            Square combatSquare = moveData.isBounce ? moveData.nextSquare : moveData.currentSquare;

            // check for attackers
            List<MoveMetaData> attackedBy = new List<MoveMetaData>();
            foreach (MoveMetaData otherMoveData in moveDatas)
            {
                Piece otherPiece = otherMoveData.piece;
                if (piece == otherPiece) continue;
                if (otherMoveData.direction == Move.Direction.NONE) continue;
                if (piece.owner == otherPiece.owner) continue;
                Square attackingSquare = otherMoveData.nextSquare;
                if (combatSquare == attackingSquare) attackedBy.Add(otherMoveData);
            }
            ApplyRetaliationDamage(piece, attackedBy);
        }
    }

    // Damage Calculation to be implemented
    void ApplyRetaliationDamage(Piece piece, ICollection<MoveMetaData> attackerMoveDatas)
    {
        int damageToEachAttacker = piece.attack;
        foreach (MoveMetaData attackerMoveData in attackerMoveDatas)
        {
            Piece attacker = attackerMoveData.piece;
            attacker.health -= damageToEachAttacker;
            Debug.Log(attacker.uid);
        }
    }

    MoveMetaData[] ResolveMovement(Move[] moves)
    {
        ResetLockedSquares();

        MoveMetaData[] moveMetaDatas =
            moves.Select(move => new MoveMetaData(move, board.GetCurrentSquare(move.piece), board.NextSquare(move)))
                 .ToArray();

        List<int> moveIndices = Enumerable.Range(0, moves.Length).ToList();

        // Consider stationary pieces
        moveIndices = moveIndices.Where(mInd =>
        {
            MoveMetaData moveData = moveMetaDatas[mInd];
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
                MoveMetaData moveData = moveMetaDatas[mInd];
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
                    MoveMetaData otherMoveData = moveMetaDatas[otherIndex];

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
            MoveMetaData moveData = moveMetaDatas[validMoveIndex];
            Debug.Assert(!IsSquareLocked(moveData.nextSquare)); // sanity check
            Debug.Assert(!moveData.isBounce); // sanity check
            moveData.currentSquare = moveData.nextSquare;
            moveData.direction = Move.Direction.NONE;
        });

        // figure out bounces
        for (int i = 0; i < moveMetaDatas.Length; i++)
        {
            MoveMetaData moveData = moveMetaDatas[i];
            // piece did not move or already moved to a unlocked (empty) square
            if (moveData.direction == Move.Direction.NONE) continue;
            bool nextSquareLocked = IsSquareLocked(moveData.nextSquare);
            moveData.isBounce = !nextSquareLocked;
        }

        // return
        return moveMetaDatas;
    }
}
