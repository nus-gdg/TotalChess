using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace State {

    enum Player { A, B }

    class Piece
    {
        public enum Type { SWORD, SPEAR, HORSE }
        public Player owner;
        public int health = 100;
        public int attack = 5;
        public int def = 5;
        public Type type;

        public string uid; // unique id for a piece

        public Piece(
            string uid,
            Player owner,
            Type type = Type.SWORD
        )
        {
            this.uid = uid;
            this.owner = owner;
            this.type = type;
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

    class Square
    {
        public int row;
        public int col;
        public Square(int row, int col)
        {
            this.row = row;
            this.col = col;
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

    class Board
    {
        int numRows;
        int numCols;
        Dictionary<Piece, Square> pieceToSquare = new Dictionary<Piece, Square>();

        List<Piece> pieces;

        public Board(int rows, int cols)
        {
            numRows = rows;
            numCols = cols;
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
                    row = currentSquare.row < numRows -1 ? currentSquare.row + 1 : numRows - 1;
                    return new Square(row, currentSquare.col);
                case Move.Direction.LEFT:
                    col = currentSquare.col > 0 ? currentSquare.col - 1 : 0;
                    return new Square(currentSquare.row, col);
                case Move.Direction.RIGHT:
                    col = currentSquare.col < numCols - 1 ? currentSquare.col + 1 : numCols - 1;
                    return new Square(currentSquare.row, col);
            }
            return currentSquare;
        }

        public void SetPieceAtSquare(Piece piece, Square square)
        {
            pieceToSquare[piece] = square;
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
    }

}
