using System.Collections.Generic;
using UnityEngine;
using State;
using System;
using System.Linq;

public class StateManager : MonoBehaviour
{
    BoardManager boardManager;
    Debugger debugger;

    //Revise: Better unique id's?
    //Set up pieces
    Piece A_PIECE_1 = new Piece("uida1", Player.A);
    Piece A_PIECE_2 = new Piece("uida2", Player.A);
    Piece A_PIECE_3 = new Piece("uida3", Player.A);
    Piece A_PIECE_4 = new Piece("uida4", Player.A);

    Piece B_PIECE_1 = new Piece("uidb1", Player.B);
    Piece B_PIECE_2 = new Piece("uidb2", Player.B);
    Piece B_PIECE_3 = new Piece("uidb3", Player.B);
    Piece B_PIECE_4 = new Piece("uidb4", Player.B);


    Board board;

    //Main function
    void Start()
    {
        CreateBoard(6, 6);
    }

    //Initialize Board
    void CreateBoard(int rows, int cols)
    {
        board = new Board(rows, cols);

        boardManager = new BoardManager();
        debugger = new Debugger(boardManager);

        //Change starting positions of pieces here
        board.PlacePiece(A_PIECE_1, 3, 2);
        board.PlacePiece(A_PIECE_2, 3, 3);
        board.PlacePiece(A_PIECE_3, 1, 0);
        board.PlacePiece(A_PIECE_4, 0, 1);

        board.PlacePiece(B_PIECE_1, 1, 1);
        board.PlacePiece(B_PIECE_2, 0, 0);
        board.PlacePiece(B_PIECE_3, 3, 4);
        board.PlacePiece(B_PIECE_4, 2, 5);

        StepState();
    }

    /* FOR TESTING:
     * -------------------------------
     * | B2 | A4 |    |    |    |    | 0
     * | A3 | B1 |    |    |    |    | 1
     * |    |    |    |    |    | B4 | 2
     * |    |    | A1 | A2 | B3 |    | 3
     * |    |    |    |    |    |    | 4
     * |    |    |    |    |    |    | 5
     * -------------------------------
     *   0    1    2    3    4    5    
     */

    Move[] PlayMoves()
    {
        //Change next moves here
        Move[] moves =
        {
            A_PIECE_1.MoveRight(),
            A_PIECE_2.MoveRight(),
            A_PIECE_3.MoveUp(),
            A_PIECE_4.MoveDown(),

            B_PIECE_1.MoveLeft(),
            B_PIECE_2.MoveRight(),
            B_PIECE_3.MoveUp(),
            B_PIECE_4.MoveUp(),
        };

        return moves;
    }

    //Runs move phase once
    void StepState()
    {

        Move[] moves = PlayMoves();

        boardManager.Update(board, moves);

        debugger.MoveList();

        debugger.AttackingPieces();
        debugger.CrowdedPieces();
    }

    //Encapsulates player moves with more information
    class MoveData
    {
        public Piece piece;

        public Move.Direction direction;

        public Square currentSquare; // piece's current square
        public Square nextSquare; // cached next square

        public bool bounce; // whether piece will bounce back from next square

        public MoveData(
                        Move move,
                        Square currentSquare,
                        Square nextSquare,
                        bool bounce
                       )
        {
            this.piece = move.piece;

            this.direction = move.direction;

            this.currentSquare = currentSquare;
            this.nextSquare = nextSquare;

            this.bounce = bounce;
        }

        public override string ToString()
        {
            return String.Format(
                "Move: [Piece: {0}, currentSquare: {1}, nextSquare: {2}, direction: {3}, willBounce: {4}]",
                piece.id,
                currentSquare,
                nextSquare,
                Move.DirectionToString(direction),
                bounce
            );
        }

        public bool WillMove()
        {
            return currentSquare != nextSquare;
        }
    }

    class BoardManager
    {
        //Hashtables keep track of piece and square information
        public Dictionary<Piece, Square> currentSquares;
        public Dictionary<Square, Piece> currentSquaresReversed;

        public Dictionary<Piece, Square> nextSquares;

        //Collects potential squares that will be crowded
        public Dictionary<Square, HashSet<Piece>> crowdedPieces;

        //Collects potential squares that will be attacked
        public Dictionary<Piece, Square> attackingPieces;

        //Hashtable keeps track of squares that pieces cannot enter
        public HashSet<Square> lockedSquares;

        //Array containing adavanced move informations
        public MoveData[] moveList;

        public BoardManager()
        {
            ResetTables();
        }

        //Update every phase
        public void Update(Board board, Move[] moves)
        {
            //Clear all tables
            ResetTables();

            UpdateTables(board, moves);

            ResolveMoves(moves);
        }

        //Fill in tables with current board info
        private void UpdateTables(Board board, Move[] moves)
        {
            //Update Tables
            currentSquares = board.currentPositions;

            currentSquaresReversed =
                currentSquares.ToDictionary(
                                            pair => pair.Value,
                                            pair => pair.Key
                                           );

            nextSquares = board.GetNextPositions(moves);

            //Used to iterate through every piece on the board
            //If a piece will be locked in its square,
            //    piece is removed to prevent re-iteration
            HashSet<Piece> pieces = new HashSet<Piece>(board.GetPieces());

            Square currentSquare, nextSquare;

            //Setup hashtables without considering collisions
            foreach (Piece piece in pieces)
            {
                currentSquare = CurrentSquare(piece);
                nextSquare = NextSquare(piece);

                //Consider pieces meeting at the same square
                Crowd(nextSquare, piece);

                //Consider pieces "supposed" to attack
                Attack(piece, nextSquare);
            }

            //Clean up crowdedPieces by removing all squares which will contain
            //    only one piece
            Dictionary<Square, HashSet<Piece>> newCrowdedPieces =
                new Dictionary<Square, HashSet<Piece>>();

            foreach (KeyValuePair<Square, HashSet<Piece>> pair in crowdedPieces)
            {
                if (pair.Value.Count() > 1)
                    newCrowdedPieces.Add(pair.Key, pair.Value);
            }

            crowdedPieces = newCrowdedPieces;

            //Clean up hashtables considering collisions
            foreach (Piece piece in pieces.ToList())
            {
                //Debug.Log(piece);

                currentSquare = CurrentSquare(piece);
                nextSquare = NextSquare(piece);

                //Consider stationary pieces
                //Consider pieces entering a crowded square
                if (IsStationary(piece) || IsCrowded(nextSquare))
                {
                    //Lock current square of piece if stationary
                    if (!IsLocked(currentSquare))
                        Lock(currentSquare);

                    //Remove piece from consideration
                    pieces.Remove(piece);

                    //Lock next square of piece if it will be a crowded square
                    //Only locked once: ignored by the other crowding pieces
                    if (!IsLocked(nextSquare))
                        Lock(nextSquare);
                }
                //Consider possible attack between pieces
                else if (IsAttacking(piece))
                {
                    //Find out other piece being attacked
                    Piece other = GetAttacking(piece);

                    //Check if other piece is attacking back
                    if (IsAttacking(other, currentSquare))
                    {
                        //Lock current square of piece if attacking
                        if (!IsLocked(currentSquare))
                            Lock(currentSquare);

                        //Remove piece from consideration
                        pieces.Remove(piece);
                    }
                }
                //All other moves likely to be valid
            }

            /* Using flood fill
             *
             * Find and remove any piece moving into a locked square
             *
             * Pieces can't move into a locked square, so will also be locked
             *
             * Populate locked squares with findings,
             *     and scan each of them recursively
             */
            foreach (Square square in lockedSquares.ToList())
            {
                LockSurrounders(square, pieces);
            }
        }

        //Flood fill algorithm to search for potential locked squares
        private void LockSurrounders(Square square, HashSet<Piece> pieces)
        {
            int row = square.row;
            int col = square.col;

            Square up = new Square(row - 1, col),
                   down = new Square(row + 1, col),
                   right = new Square(row, col + 1),
                   left = new Square(row, col - 1);

            Piece other;

            //Check Above
            if (!IsLocked(up) && ContainsPiece(up))
            {
                //Debug.Log("UP " + square);
                other = CurrentPiece(up);

                if (NextSquare(other) == square)
                {
                    Lock(up);
                    pieces.Remove(other);

                    LockSurrounders(up, pieces);
                }
            }

            //Check Below
            if (!IsLocked(down) && ContainsPiece(down))
            {
                //Debug.Log("DOWN " + square);
                other = CurrentPiece(down);

                if (NextSquare(other) == square)
                {
                    Lock(down);
                    pieces.Remove(other);

                    LockSurrounders(down, pieces);
                }
            }

            //Check Right
            if (!IsLocked(right) && ContainsPiece(right))
            {
                //Debug.Log("RIGHT " + square);
                other = CurrentPiece(right);

                if (NextSquare(other) == square)
                {
                    Lock(right);
                    pieces.Remove(other);

                    LockSurrounders(right, pieces);
                }
            }

            //Check Left
            if (!IsLocked(left) && ContainsPiece(left))
            {
                //Debug.Log("LEFT " + square);
                other = CurrentPiece(left);

                if (NextSquare(other) == square)
                {
                    Lock(left);
                    pieces.Remove(other);

                    LockSurrounders(left, pieces);
                }
            }
        }

        //Clean up mistakes in tables
        //Update moves that will actually occur this turn
        public void ResolveMoves(Move[] moves)
        {
            bool bounce;

            Piece piece;

            //New list of moves
            moveList = new MoveData[moves.Length];

            for (int i = 0; i < moves.Length; i++)
            {
                piece = moves[i].piece;

                bounce = false;

                Square currentSquare = CurrentSquare(piece);
                Square nextSquare = NextSquare(piece);

                //Check if current piece not stationary this turn
                if (!IsStationary(piece))
                {
                    //Check if current piece cannot move into next square
                    if (IsLocked(nextSquare))
                    {
                        bounce = true; //Therefore bounce!!
                        nextSquare = currentSquare; //Also fix next square
                    }
                    //Check if there is supposed to be an attack,
                    //    but the opponent moved away
                    else if (IsAttacking(piece))
                    {
                        CancelAttack(piece); //Don't attack and move in direction
                    }
                }

                //Create final move information
                MoveData moveData = new MoveData(
                                                    moves[i],
                                                    currentSquare,
                                                    nextSquare,
                                                    bounce
                                                );

                //Populate move list
                moveList[i] = moveData;
            }
        }

        //Update board with new positions
        //To be used at end of each turn
        public void ApplyMoves(Board board)
        {
            Dictionary<Piece, Square> newPositions
                = new Dictionary<Piece, Square>();

            foreach (MoveData moveData in moveList)
            {
                newPositions.Add(moveData.piece, moveData.nextSquare);
            }

            board.SetPositions(newPositions);
        }

        //Refresh all tracked information
        public void ResetTables()
        {
            ResetCurrentSquares();
            ResetNextSquares();

            ResetCrowdedPieces();
            ResetAttackingPieces();

            ResetLockedSquares();

            ResetMoveList();
        }

        private void ResetCurrentSquares()
        {
            currentSquares = new Dictionary<Piece, Square>();
            currentSquaresReversed = new Dictionary<Square, Piece>();
        }

        private void ResetNextSquares()
        {
            nextSquares = new Dictionary<Piece, Square>();
        }

        private void ResetCrowdedPieces()
        {
            crowdedPieces = new Dictionary<Square, HashSet<Piece>>();
        }

        private void ResetAttackingPieces()
        {
            attackingPieces = new Dictionary<Piece, Square>();
        }

        private void ResetLockedSquares()
        {
            lockedSquares = new HashSet<Square>();
        }

        private void ResetMoveList()
        {
            moveList = null;
        }

        //Get current square of piece this turn
        public Square CurrentSquare(Piece piece)
        {
            return currentSquares[piece];
        }

        //Get next square of piece this turn
        public Square NextSquare(Piece piece)
        {
            return nextSquares[piece];
        }

        //Get current piece in square
        public Piece CurrentPiece(Square square)
        {
            return currentSquaresReversed[square];
        }

        //Check if square has a piece
        public bool ContainsPiece(Square square)
        {
            return currentSquaresReversed.ContainsKey(square);
        }

        //Add a crowd
        public void Crowd(Square square, Piece piece)
        {
            if (IsCrowded(square))
            {
                crowdedPieces[square].Add(piece);
            }
            else
            {
                HashSet<Piece> piecesSharingSquare = new HashSet<Piece>();

                piecesSharingSquare.Add(piece);

                crowdedPieces.Add(square, piecesSharingSquare);
            }
        }

        //Check if square will be crowded
        public bool IsCrowded(Square square)
        {
            return crowdedPieces.ContainsKey(square);
        }

        //Get pieces that will crowd a square
        public HashSet<Piece> GetCrowd(Square square)
        {
            return crowdedPieces[square];
        }

        //Add an attack
        public void Attack(Piece attacker, Square square)
        {
            //Check if next square contains a piece
            if (ContainsPiece(square))
            {
                //Get the piece in the attacked square
                Piece defender = CurrentPiece(square);

                //Create attack only if not attacking own pieces
                if (defender.owner != attacker.owner)
                {
                    //Debug.Log("Attack [Piece: " + attacker.id + ", attacking: " + square + " ]\n");
                    attackingPieces.Add(attacker, square);
                }
            }
        }

        //Remove an attack
        public void CancelAttack(Piece attacker)
        {
            if (attackingPieces.ContainsKey(attacker))
                attackingPieces.Remove(attacker);
        }

        //Get the piece being attacked by the attacker
        public Piece GetAttacking(Piece attacker)
        {
            return CurrentPiece(attackingPieces[attacker]);
        }

        //Check if a piece is planning to attack
        public bool IsAttacking(Piece piece)
        {
            return attackingPieces.ContainsKey(piece);
        }

        //Check if a piece is planning to attack a given square
        public bool IsAttacking(Piece piece, Square square)
        {
            bool attack = false;

            if (attackingPieces.ContainsKey(piece))
            {
                Square attackedSquare = attackingPieces[piece];

                if (square == attackedSquare)
                    attack = true;
            }

            return attack;
        }

        //Check if piece will not move this turn
        public bool IsStationary(Piece piece)
        {
            return CurrentSquare(piece) == NextSquare(piece);
        }

        //Lock a given list of squares
        public void Lock(IEnumerable<Square> squares)
        {
            foreach (Square square in squares)
                Lock(square);
        }

        //Lock a square to prevent other pieces entering it
        public void Lock(Square square)
        {
            Debug.Log("Locked " + square + "\n");
            lockedSquares.Add(square);
        }

        //Check if a square has been locked
        public bool IsLocked(Square square)
        {
            return lockedSquares.Contains(square);
        }
    }

    class Debugger
    {
        BoardManager boardManager;

        public Debugger(BoardManager boardManager)
        {
            this.boardManager = boardManager;
        }

        public void MoveList()
        {
            //Remove comment to split debugging info for current move list
            //foreach (MoveData moveData in boardManager.moveList)
            //{
            //    Debug.Log(moveData);
            //}

            Debug.Log("Move List:\n" + string.Join("\n", boardManager.moveList.ToList()) + "\n");
        }

        public void CurrentSquares()
        {
            string message = "Current Squares:\n";

            foreach (KeyValuePair<Piece, Square> pair in boardManager.currentSquares)
            {
                message += String.Format(
                                            "[Piece: {0}, currentSquare: {1}]\n",
                                            pair.Key,
                                            pair.Value);
            }

            Debug.Log(message);
        }

        //public void CurrentSquaresReversed()
        //{
        //    PrintDictionary(boardManager.currentSquaresReversed);
        //}

        public void NextSquares()
        {
            string message = "Next Squares:\n";

            foreach (KeyValuePair<Piece, Square> pair in boardManager.nextSquares)
            {
                message += String.Format(
                                            "[Piece: {0}, nextSquare: {1}]\n",
                                            pair.Key,
                                            pair.Value);
            }

            Debug.Log(message);
        }

        public void CrowdedPieces()
        {
            string message = "Crowds:\n";

            foreach (KeyValuePair<Square, HashSet<Piece>> pair in boardManager.crowdedPieces)
            {
                message += String.Format(
                                            "Crowd: [ {0}: {1} ]\n",
                                            pair.Key,
                                            string.Join(" , ", pair.Value)
                                        );
            }

            Debug.Log(message);
        }

        public void AttackingPieces()
        {
            string message = "Attacks:\n";

            foreach (KeyValuePair<Piece, Square> pair in boardManager.attackingPieces)
            {
                message += String.Format(
                                            "Attack: [Piece: {0}, attacking: {1}]\n",
                                            pair.Key,
                                            pair.Value);
            }

            Debug.Log(message);
        }

        public void LockedSquares()
        {
            Debug.Log("Locked Squares:\n" + string.Join("\n", boardManager.lockedSquares) + "\n");
        }
    }
}