using System.Collections.Generic;
using System.Linq;

namespace State
{

    enum Player { A, B }

    class Piece
    {
        public enum Type { SWORD, SPEAR, HORSE }

        //Revise: uid --> id?
        public string id; // unique id for a piece

        public Player owner;
        public Type type;

        public int health = 100;
        public int attack = 5;
        public int def = 5;

        public Piece(string id,
                     Player owner,
                     Type type = Type.SWORD)
        {
            this.id = id;
            this.owner = owner;
            this.type = type;
        }

        public override int GetHashCode()
        {
            return id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !GetType().Equals(obj.GetType()))
            {
                return false;
            }

            return this == (Piece)obj;
        }

        public static bool operator ==(Piece a, Piece b)
        {
            return a.id == b.id;
        }

        public static bool operator !=(Piece a, Piece b)
        {
            return !(a == b);
        }

        //Revise: Piece creates moves
        public Move Move(string direction)
        {
            return new Move(this, State.Move.StringToDirection(direction));
        }

        public Move Move(Move.Direction direction = State.Move.Direction.NONE)
        {
            return new Move(this, direction);
        }

        public Move MoveUp()
        {
            return Move(State.Move.Direction.UP);
        }

        public Move MoveDown()
        {
            return Move(State.Move.Direction.DOWN);
        }

        public Move MoveLeft()
        {
            return Move(State.Move.Direction.LEFT);
        }

        public Move MoveRight()
        {
            return Move(State.Move.Direction.RIGHT);
        }

        public override string ToString()
        {
            return id;
        }
    }

    class Move
    {
        public enum Direction { UP, DOWN, LEFT, RIGHT, NONE }

        public Piece piece;
        public Direction direction;

        public Move(Piece piece, Direction direction = Move.Direction.NONE)
        {
            this.piece = piece;
            this.direction = direction;
        }

        public string DirectionName()
        {
            return DirectionToString(direction);
        }

        public static string DirectionToString(Direction direction)
        {
            switch (direction)
            {
                case Direction.UP: return "UP";

                case Direction.DOWN: return "DOWN";

                case Direction.LEFT: return "LEFT";

                case Direction.RIGHT: return "RIGHT";

                case Direction.NONE: default: return "NONE";
            }
        }

        public static Direction StringToDirection(string direction = "")
        {
            switch (direction.ToUpper())
            {
                case "UP": case "U": return Direction.UP;

                case "DOWN": case "D": return Direction.DOWN;

                case "LEFT": case "L": return Direction.LEFT;

                case "RIGHT": case "R": return Direction.RIGHT;

                default: return Direction.NONE;
            }
        }
    }

    class Square
    {
        public int row;
        public int col;
        public Square(int row, int col)
        {
            this.row = row;
            this.col = col;
        }

        //Revise: * 32?
        public override int GetHashCode()
        {
            return row * 31 + col;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !GetType().Equals(obj.GetType()))
            {
                return false;
            }

            return this == (Square)obj;
        }

        public static bool operator ==(Square a, Square b)
        {
            return a.row == b.row && a.col == b.col;
        }

        public static bool operator !=(Square a, Square b)
        {
            return !(a == b);
        }

        public override string ToString()
        {
            return string.Format("[Square - row: {0} col: {1}]", row, col);
        }
    }

    class Board
    {
        public int numRows, numCols;

        public Dictionary<Piece, Square> currentPositions =
            new Dictionary<Piece, Square>();

        public Board(int rows, int cols)
        {
            numRows = rows;
            numCols = cols;
        }

        // Revise: SetPieceAtSquare --> PlacePiece?
        public void PlacePiece(Piece piece, int row, int col)
        {
            Square square = new Square(row, col);

            currentPositions[piece] = square;
        }

        public void PlacePiece(Piece piece, Square square)
        {
            currentPositions[piece] = square;
        }

        public void SetPositions(Dictionary<Piece, Square> newPositions)
        {
            currentPositions = newPositions;
        }

        public Square CurrentSquare(Move move)
        {
            return currentPositions[move.piece];
        }

        public Square CurrentSquare(Piece piece)
        {
            return currentPositions[piece];
        }

        public Square NextSquare(Move move)
        {
            int row, col;

            Square currentSquare = CurrentSquare(move.piece);

            row = currentSquare.row;
            col = currentSquare.col;

            Move.Direction direction = move.direction;

            switch (direction)
            {
                case Move.Direction.UP:
                    if (row > 0)
                        row--;
                    return new Square(row, col);

                case Move.Direction.DOWN:
                    if (row < numRows - 1)
                        row++;
                    return new Square(row, col);

                case Move.Direction.LEFT:
                    if (col > 0)
                        col--;
                    return new Square(row, col);

                case Move.Direction.RIGHT:
                    if (col < numCols - 1)
                        col++;
                    return new Square(row, col);

                case Move.Direction.NONE:
                default:
                    return currentSquare;
            }
        }

        public Dictionary<Piece, Square> GetNextPositions(Move[] moves)
        {

            Dictionary<Piece, Square> nextSquares =
                new Dictionary<Piece, Square>();

            foreach (Move move in moves)
            {
                nextSquares.Add(move.piece, NextSquare(move));
            }

            return nextSquares;
        }

        public Piece[] GetPieces()
        {
            return currentPositions.Keys.ToArray();
        }
    }

}