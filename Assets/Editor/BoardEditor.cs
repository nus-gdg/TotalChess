using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using static State;

using static UnityEditor.EditorGUILayout;


[CustomEditor(typeof(StateManager))]
public class BoardEditor : Editor
{
    private class Window : EditorWindow
    {
        static Editor editor;

        [MenuItem("Window/Board")]
        static void ShowWindow()
        {
            GetWindow<Window>(false, "Board");
        }

        void OnFocus()
        {
            if (editor == null)
            {
                GameObject gameObject = GameObject.Find("StateManager");

                if (gameObject != null)
                {
                    StateManager stateManager = gameObject.GetComponent<StateManager>();

                    if (stateManager != null && editor == null)
                        editor = CreateEditor(stateManager);
                }
            }
        }

        void OnGUI()
        {
            if (editor != null)
            {
                editor.OnInspectorGUI();
            }
        }
    }

    #region Settings

    #region Settings: Options

    static GUIStyle buttonStyle,
                    labelStyle,
                    inputStyle,
                    layoutStyle,
                    titleStyle,
                    popupStyle;

    static GUISkin customSkin;

    static GUIStyle boxStyle,
                    emptySquare,
                    playerStyleA,
                    playerStyleB;

    static GUIStyle healthStyle,
                    healthGreen,
                    healthRed;

    #endregion

    private void Settings()
    {
        EditorGUIUtility.labelWidth = 4;

        buttonStyle = new GUIStyle("button")
        {
            stretchWidth = false,
            stretchHeight = false
        };

        labelStyle = new GUIStyle("label")
        {
            stretchWidth = false,
            stretchHeight = false,
        };

        layoutStyle = new GUIStyle()
        {
            stretchWidth = false,
            stretchHeight = false
        };

        inputStyle = new GUIStyle("textField")
        {
            stretchWidth = false,
            stretchHeight = false
        };

        titleStyle = new GUIStyle("label")
        {
            fontSize = 14,
            fontStyle = FontStyle.Bold,
            fixedHeight = 20
        };

        popupStyle = new GUIStyle("popup")
        {
            stretchWidth = false,
            stretchHeight = false,
            fixedWidth = 64,
            fixedHeight = 20
        };

        boxStyle = customSkin.GetStyle("Square Settings");

        emptySquare = new GUIStyle(boxStyle)
        {
            normal = customSkin.GetStyle("Empty Square").normal
        };

        playerStyleA = new GUIStyle(boxStyle)
        {
            normal = customSkin.GetStyle("Player A").normal
        };

        playerStyleB = new GUIStyle(boxStyle)
        {
            normal = customSkin.GetStyle("Player B").normal
        };

        healthStyle = customSkin.GetStyle("Health Bar");

        healthGreen = new GUIStyle(healthStyle)
        {
            normal = customSkin.GetStyle("Health Green").normal,
        };

        healthRed = new GUIStyle(healthStyle)
        {
            normal = customSkin.GetStyle("Health Red").normal,
        };
    }
    #endregion

    static StateManager stateManager;
    static Board board;

    void OnEnable()
    {
        stateManager = (StateManager)target;
        board = stateManager.board;

        customSkin = Resources.Load<GUISkin>("StateManager");
    }

    public override void OnInspectorGUI()
    {
        Settings();

        ToolBoard();

        DisplayBoard();

        ToolPieces();

    }

    #region Tools: Pieces Options

    static List<Piece> pieces;
    static List<Move> moves;
    static int[] prow = new int[] { 0, 0, 0, 0, 0, 0 };//input fields  //make array here and set to null(empty list)
    static int[] pcol = new int[] { 0, 0, 0, 0, 0, 0 };//input fields
    #endregion

    private void ToolPieces()
    {
        if (board != null)
        {
            CheckPiecesCreated();

            if (board.rows > 0 && board.cols > 0)
            {
                TitlePieceInfo();

                Space();

                BeginVertical(layoutStyle);
                //put stuff here

                foreach (Move move in moves)
                {
                    DisplayPieceInfo(move);

                    Space();
                }
                DrawPiece();
                EndVertical();
            }
        }
        else
        {
            createdPieces = false;
        }
    }

    private void CheckPiecesCreated()
    {
        if (!createdPieces)
        {
            //For you PX!!
            stateManager.CreatePieces(3);
            createdPieces = true;

            pieces = board.GetPieces();
            moves = new List<Move>();

            foreach (Piece piece in pieces)
            {
                moves.Add(piece.MoveDown());
            }
        }
    }

    private void TitlePieceInfo()
    {
        BeginHorizontal(layoutStyle, GUILayout.Width(196));

        {
            LabelField("Piece", titleStyle, GUILayout.Width(48));

            Space();

            LabelField("Type", titleStyle, GUILayout.Width(64));

            Space();

            LabelField("Move", titleStyle, GUILayout.Width(64));

            LabelField("Health", titleStyle, GUILayout.Width(64));
        }

        EndHorizontal();
    }

    private void DisplayPieceInfo(Move move)
    {
        BeginHorizontal(layoutStyle, GUILayout.Width(312));

        {
            //=== Show Piece Id
            labelStyle.alignment = TextAnchor.UpperCenter;

            LabelField(move.piece.uid, labelStyle, GUILayout.Width(48));

            labelStyle.alignment = TextAnchor.UpperLeft;

            Space();

            ChooseType(move.piece);

            Space();

            ChooseMove(move);

            Space();

            DisplayHealth(move.piece);
        }

        EndHorizontal();
    }

    private void DisplayHealth(Piece piece)
    {
        if (piece.health > 0)
            GUILayout.Box("", healthGreen, GUILayout.Width(piece.health));

        if (piece.maxHealth > piece.health)
        {
            if (piece.health > 0)
                GUILayout.Box("", healthRed, GUILayout.Width(piece.maxHealth - piece.health));
            else
                GUILayout.Box("", healthRed, GUILayout.Width(piece.maxHealth));

        }
    }

    private void ChooseType(Piece piece)
    {
        piece.type = (Piece.Type)EnumPopup(piece.type, popupStyle);
    }

    private void ChooseMove(Move move)
    {
        move.direction = (Move.Direction)EnumPopup(move.direction, popupStyle);
    }

    private void DisplayBoard()
    {
        if (board != null)
        {
            GUIStyle boxImage;

            Square square;

            Piece piece;
            string piece_id;

            //Display board editor
            BeginHorizontal(layoutStyle);

            for (int col = 0; col < board.cols; col++)
            {
                BeginVertical(layoutStyle);

                for (int row = board.rows; row >= 0; row--)
                {
                    piece_id = "";
                    boxImage = emptySquare;

                    square = new Square(row, col);

                    if (board.ContainsPiece(square))
                    {
                        piece = board.GetCurrentPiece(square);
                        piece_id = piece.uid;

                        switch (piece.owner)
                        {
                            case Player.A:
                                boxImage = playerStyleA;
                                break;
                            case Player.B:
                                boxImage = playerStyleB;
                                break;
                        }
                    }

                    GUILayout.Box(new GUIContent(piece_id, row + ", " + col),
                                  boxImage);
                }
                EndVertical();
            }
            EndHorizontal();
        }
    }

    #region Tools: Board

    #region Tools: Board Options

    static int rows = 6,
               cols = 6;

    static bool createdPieces = false;

    #endregion

    private void ToolBoard()
    {
        Title("Board Editor");

        Space();

        BeginHorizontal(layoutStyle, GUILayout.Width(128));

        {
            //=== Create Rows and Cols:
            BeginVertical(layoutStyle);

            {
                InputBoardRows();

                InputBoardColumns();
            }

            EndVertical();

            //=== Create Buttons

            //Button: Create
            ButtonCreate();

            Space();

            //Button: Previous
            ButtonPrev();

            //Button: Next
            ButtonNext();
        }

        EndHorizontal();

        Space();
    }

    private void InputBoardRows()
    {
        BeginHorizontal(layoutStyle);

        LabelField("Rows:", labelStyle, GUILayout.Width(36));

        if (board != null)
            LabelField(board.rows.ToString(), labelStyle, GUILayout.Width(28));
        else
            LabelField("--", labelStyle, GUILayout.Width(28));

        rows = IntField(rows, inputStyle, GUILayout.Width(64));

        EndHorizontal();
    }

    private void InputBoardColumns()
    {
        BeginHorizontal(layoutStyle);

        LabelField("Cols:", labelStyle, GUILayout.Width(36));

        if (board != null)
            LabelField(board.cols.ToString(), labelStyle, GUILayout.Width(28));
        else
            LabelField("--", labelStyle, GUILayout.Width(28));

        cols = IntField(cols, inputStyle, GUILayout.Width(64));

        EndHorizontal();
    }

    private void ButtonCreate()
    {
        if (GUILayout.Button("Create",
                     buttonStyle,
                     GUILayout.Width(48),
                     GUILayout.Height(32)))
        {
            stateManager.CreateBoard(rows, cols);
            board = stateManager.board;
            createdPieces = false;
        }
    }

    private void ButtonPrev()
    {
        //Button: Previous
        if (GUILayout.Button("Prev",
                             buttonStyle,
                             GUILayout.Width(40),
                             GUILayout.Height(32)))
        {
            int numMovePhases = 1;

            int currentBoard = stateManager.moveHistory.Count - 1;

            if (currentBoard >= numMovePhases)
            {
                for (int i = 0; i < numMovePhases; i++)
                {
                    stateManager.moveHistory.RemoveAt(currentBoard);
                    currentBoard -= 1;
                }
            }
            else
            {
                currentBoard = 0;
            }

            stateManager.board = new Board(stateManager.moveHistory[currentBoard]);
            board = stateManager.board;

            foreach (Piece piece in board.GetPieces())
                Debug.Log(string.Format("{0} >>> {1}", piece, piece.health));

            pieces = board.GetPieces();
        }
    }

    private void ButtonNext()
    {
        //Button: Next
        if (GUILayout.Button("Next",
                             buttonStyle,
                             GUILayout.Width(40),
                             GUILayout.Height(32)))
        {
            int numMovePhases = 1;

            for (int i = 0; i < numMovePhases; i++)
                stateManager.CalculateNextPhase(moves.ToArray());

            board = stateManager.board;
            pieces = board.GetPieces();
        }
    }

    #endregion

    void DrawPiece()
    {
        //pieces is a List contain all pieces
        for (int i = 0; i < pieces.Count; i++)
        {


        }
        for (int i = 0; i < pieces.Count; i++)// go thru all the pieces 1 by 1
        {
            BeginHorizontal();
            LabelField("Piece " + (pieces[i].uid) + ":", GUILayout.Width(60));
            prow[i] = board.GetCurrentSquare(pieces[i]).row;
            pcol[i] = board.GetCurrentSquare(pieces[i]).col;
            prow[i] = IntField(board.GetCurrentSquare(pieces[i]).row + "   ", prow[i]);//set piece's row
            pcol[i] = IntField(board.GetCurrentSquare(pieces[i]).col + "   ", pcol[i]);//set piece's col
            Square temp = board.GetCurrentSquare(pieces[i]);
            temp.row = prow[i];
            temp.col = pcol[i];
            board.SetPieceAtSquare(pieces[i], temp);
            EndHorizontal();
        }

        //Console.WriteLine("name {0} and place{1}");
    }

    private void Title(string title)
    {
        LabelField(title, titleStyle);
    }
}