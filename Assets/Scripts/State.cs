using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class State {

    public enum Player { A, B }

    public class Piece
    {
        public enum Type { SWORD, SPEAR, HORSE }

        public Player owner;

        //Revise: Change to id? Haha
        public string uid; // unique id for a piece

        public Type type;

        public int maxHealth = 100;
        public int health = 100;
        public int attack = 5;
        public int def = 5;

        public Piece (string uid, Player owner, Type type = Type.SWORD)
        {
            this.uid = uid;
            this.owner = owner;
            this.type = type;
        }

        public Piece(Piece piece)
        {
            uid = piece.uid;
            owner = piece.owner;
            type = piece.type;

            maxHealth = piece.maxHealth;
            health = piece.health;
            attack = piece.attack;
            def = piece.def;
        }

        public override int GetHashCode()
        {
            return uid.GetHashCode();
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
            return a.uid == b.uid;
        }

        public static bool operator !=(Piece a, Piece b)
        {
            return !(a == b);
        }

        public override string ToString()
        {
            return uid;
        }

        public Move MakeMove(Move.Direction direction)
        {
            return new Move(new Piece(this), direction);
        }

        public Move MoveUp()
        {
            return MakeMove(State.Move.Direction.UP);
        }

        public Move MoveDown()
        {
            return MakeMove(State.Move.Direction.DOWN);
        }

        public Move MoveLeft()
        {
            return MakeMove(State.Move.Direction.LEFT);
        }

        public Move MoveRight()
        {
            return MakeMove(State.Move.Direction.RIGHT);
        }

        public Move Move()
        {
            return MakeMove(State.Move.Direction.NONE);
        }
    }

    public class Move
    {
        public enum Direction { UP, DOWN, LEFT, RIGHT, NONE }

        public Piece piece;
        public Direction direction;

        public Move(Piece piece, Direction direction = Move.Direction.NONE)
        {
            this.piece = piece;
            this.direction = direction;
        }

        //Revise: Think can just call enum.ToString()
        public static string DirectionToString(Direction direction)
        {
            switch (direction)
            {
                case Direction.UP:    return "UP";
                case Direction.DOWN:  return "DOWN";
                case Direction.LEFT:  return "LEFT";
                case Direction.RIGHT: return "RIGHT";
                case Direction.NONE:  return "NONE";
                default:              return "NONE";
            }
        }

        public static Direction OppositeDirection(Direction direction)
        {
            switch (direction)
            {
                case Direction.UP:    return Direction.DOWN;
                case Direction.DOWN:  return Direction.UP;
                case Direction.LEFT:  return Direction.RIGHT;
                case Direction.RIGHT: return Direction.LEFT;
                case Direction.NONE:  return Direction.NONE;
                default:              return Direction.NONE;
            }
        }
    }

    public class Square
    {
        public int row;
        public int col;

        public Square(int row, int col)
        {
            this.row = row;
            this.col = col;
        }

        public Square(Square square)
        {
            row = square.row;
            col = square.col;
        }

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

            return this == (Square) obj;
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
            return string.Format("[Square row:{0} col:{1}]", row, col);
        }
    }

    public class Board
    {
        public int rows;
        public int cols;

        Dictionary<Piece, Square> pieceToSquare = new Dictionary<Piece, Square>();
        Dictionary<Square, Piece> squareToPiece = new Dictionary<Square, Piece>();

        public Board(int rows, int cols)
        {
            this.rows = rows;
            this.cols = cols;
        }

        public Board(Board board)
        {
            rows = board.rows;
            cols = board.cols;
            pieceToSquare = new Dictionary<Piece, Square>(board.pieceToSquare);
            squareToPiece = new Dictionary<Square, Piece>(board.squareToPiece);
        }

        public Square NextSquare(Square currentSquare, Move.Direction direction)
        {
            int row, col;

            switch (direction)
            {
                case Move.Direction.UP:
                    row = currentSquare.row > 0 ? currentSquare.row - 1 : 0;
                    return new Square(row, currentSquare.col);
                case Move.Direction.DOWN:
                    row = currentSquare.row < rows -1 ? currentSquare.row + 1 : rows - 1;
                    return new Square(row, currentSquare.col);
                case Move.Direction.LEFT:
                    col = currentSquare.col > 0 ? currentSquare.col - 1 : 0;
                    return new Square(currentSquare.row, col);
                case Move.Direction.RIGHT:
                    col = currentSquare.col < cols - 1 ? currentSquare.col + 1 : cols - 1;
                    return new Square(currentSquare.row, col);
            }
            return currentSquare;
        }

        public void SetPieceAtSquare(Piece piece, Square square)
        {
            pieceToSquare[piece] = square;
            squareToPiece[square] = piece;
        }

        public Square NextSquare(Move move)
        {
            Square currentSquare = GetCurrentSquare(move.piece);
            Move.Direction direction = move.direction;

            return NextSquare(currentSquare, direction);
        }

        public Square GetCurrentSquare(Piece piece)
        {
            return pieceToSquare[piece];
        }

        public Piece GetCurrentPiece(Square square)
        {
            return squareToPiece[square];
        }

        public bool ContainsPiece(Square square)
        {
            return squareToPiece.ContainsKey(square);
        }

        public List<Piece> GetPieces()
        {
            List<Piece> pieces = new List<Piece>();

            foreach (Piece piece in pieceToSquare.Keys.ToList())
            {
                pieces.Add(new Piece(piece));
            }

            return pieces;
        }

        public override string ToString()
        {
            string message = "";

            foreach (Piece piece in GetPieces())
            {
                message += piece.uid + " " + GetCurrentSquare(piece);
            }

            return message;
        }

        public void ResetPositions()
        {
            pieceToSquare = new Dictionary<Piece, Square>();
            squareToPiece = new Dictionary<Square, Piece>();
        }
    }

}
