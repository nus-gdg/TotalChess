using UnityEditor;
using UnityEngine;
using static State;

[CustomEditor(typeof(StateManager))]
public class BoardEditor : Editor
{
    class Window : EditorWindow
    {
        GameObject stateManagerObject;
        StateManager stateManager;

        Editor editor;

        [MenuItem("Window/Board")]
        static void ShowWindow()
        {
            GetWindow<Window>(false, "Board");
        }

        void OnGUI()
        {
            stateManagerObject = GameObject.Find("StateManager");

            if (stateManagerObject != null)
            {
                stateManager = stateManagerObject.GetComponent<StateManager>();
            }

            if (stateManager != null)
            {
                if (editor == null)
                    editor = CreateEditor(stateManager);

                editor.OnInspectorGUI();
            }
        }
    }

    StateManager stateManager;

    GUISkin skin;

    int rows = 6,
        cols = 6;

    int boardRows = 0,
        boardCols = 0;

    bool createdBoard = false,
         createdPieces = false;

    void OnEnable()
    {
        stateManager = (StateManager)target;

        skin = Resources.Load<GUISkin>("StateManager");
    }

    public override void OnInspectorGUI()
    {
        DrawCreateBoard();

        DisplayBoard();

        if (createdBoard && !createdPieces)
        {
            stateManager.CreatePieces(3);
            createdPieces = true;
        }
    }

    void DrawCreateBoard()
    {
        GUILayout.Label("Board Editor", new GUIStyle(skin.GetStyle("Editor")));

        EditorGUIUtility.labelWidth = 24;

        EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical();

                    EditorGUILayout.BeginHorizontal();

                        GUILayout.Label("Rows:", GUILayout.Width(36));
                        rows = EditorGUILayout.IntField(boardRows + "    ", rows);

                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();

                        GUILayout.Label("Cols:", GUILayout.Width(36));
                        cols = EditorGUILayout.IntField(boardCols + "    ", cols);

                    EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();

                if (GUILayout.Button("Create", GUILayout.Height(32)))
                {
                    stateManager.CreateBoard(rows, cols);
                    createdBoard = true;
                    createdPieces = false;
                }

            EditorGUILayout.EndVertical();

            GUILayout.FlexibleSpace();

        EditorGUILayout.EndHorizontal();
    }

    public void DisplayBoard()
    {
        Board board = stateManager.board;

        GUIStyle style;

        if (board != null)
        {
            Square square;
            string squareContents;

            //Display board editor
            EditorGUILayout.BeginHorizontal();

                for (int col = 0; col < board.cols; col++)
                {
                    EditorGUILayout.BeginVertical();

                        for (int row = 0; row < board.rows; row++)
                        {
                            square = new Square(row, col);
                            
                            squareContents = board.ContainsPiece(square)
                                                ? board.GetCurrentPiece(square).uid
                                                : " ";

                            switch (squareContents[0])
                            {
                                case 'A':
                                    style = skin.GetStyle("Player A");
                                    break;
                                case 'B':
                                    style = skin.GetStyle("Player B");
                                    break;
                                default:
                                    style = skin.GetStyle("Empty Square");
                                    break;
                            }
                            
                            GUILayout.Box(
                                            new GUIContent(squareContents, row + ", " + col),
                                            style,
                                            GUILayout.Width(48), GUILayout.Height(48)
                                         );
                        }

                    EditorGUILayout.EndVertical();
                }

                GUILayout.FlexibleSpace();

            EditorGUILayout.EndHorizontal();

            boardRows = board.rows;
            boardCols = board.cols;
        }
    }
}
