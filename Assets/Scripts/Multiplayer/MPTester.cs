using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using State;
using Log;

[RequireComponent(typeof(StateManager))]
[RequireComponent(typeof(NetworkEventManager))]
[RequireComponent(typeof(Renderer))]
public class MPTester : MonoBehaviour
{
    private StateManager stateManager;
    private NetworkEventManager networkManager;
    private Renderer renderer;
    private UITileManager utm;
    private List<Piece> pieces = new List<Piece>();

    // Start is called before the first frame update
    void Start()
    {
        stateManager = GetComponent<StateManager>();
        networkManager = GetComponent<NetworkEventManager>();
        renderer = GetComponent<Renderer>();
        utm = GameObject.FindGameObjectsWithTag("TileManager")[0].GetComponent<UITileManager>();

        //if (networkManager.IsMasterClient())
        //{
        SetUpDummyBoard();
        //}
    }

    void OnEnable()
    {
        NetworkEventManager.OnGameStart += OnGameStart;
        NetworkEventManager.OnReceiveTurn += OnReceiveTurn;
        NetworkEventManager.OnReceiveTurnLog += OnReceiveTurnLog;
    }

    void OnDisable()
    {
        NetworkEventManager.OnGameStart -= OnGameStart;
        NetworkEventManager.OnReceiveTurn -= OnReceiveTurn;
        NetworkEventManager.OnReceiveTurnLog -= OnReceiveTurnLog;
    }

    void SetPieceAtSquare(Player player, Square square, string puid)
    {
        int piecetype = player == Player.A ? 0 : 1;
        utm.InitializePiece(piecetype, square.col, square.row, puid);
        Piece piece = new Piece(puid, player);
        pieces.Add(piece);
        Debug.Log(square);
        stateManager.board.SetPieceAtSquare(piece, square);
    }

    void SetUpDummyBoard()
    {
        stateManager.board = new Board(6, 6);
        SetPieceAtSquare(Player.A, new Square(0, 0), "A1");
        SetPieceAtSquare(Player.A, new Square(2, 2), "A2");
        SetPieceAtSquare(Player.A, new Square(1, 3), "A3");
        SetPieceAtSquare(Player.B, new Square(3, 2), "B1");
        SetPieceAtSquare(Player.B, new Square(2, 3), "B2");
        SetPieceAtSquare(Player.B, new Square(3, 4), "B3");

        //5 |    |    |    |    |    |    |
        //4 |    |    |    |    |    |    |
        //3 |    |    | B1 |    | B3 |    |
        //2 |    |    | A2 | B2 |    |    |
        //1 |    |    |    | A3 |    |    |
        //0 | A1 |    |    |    |    |    |
        //  | 0  | 1  | 2  | 3  | 4  | 5  |
    }

    void OnGameStart()
    {
        if (networkManager.IsMasterClient())
        {
            renderer.material.color = new Color(1.0f, 0, 0); // for debug
            Move[] movesToSend = {
                        new Move(pieces[0], Move.Direction.UP),
                        new Move(pieces[1], Move.Direction.LEFT),
                        new Move(pieces[2], Move.Direction.NONE),
                    };
            networkManager.SendTurn(new List<Move[]> { movesToSend, movesToSend, movesToSend });
        }
        else
        {
            renderer.material.color = new Color(0, 0, 1.0f); // for debug
            Move[] movesToSend = {
                        new Move(pieces[3], Move.Direction.DOWN),
                        new Move(pieces[4], Move.Direction.DOWN),
                        new Move(pieces[5], Move.Direction.NONE),
                    }; // debug as well
            networkManager.SendTurn(new List<Move[]> { movesToSend, movesToSend, movesToSend });
        }
    }


    void OnReceiveTurn(Move[][] turn)
    {
        foreach (Move[] moves in turn)
        {
            foreach (Move move in moves)
            {
                Debug.LogFormat("MPTester Player {0} OnReceiveTurn UID {1}, DIR {2}", move.piece.owner, move.piece.uid, Move.DirectionToString(move.direction));
            }
        }
    }

    void OnReceiveTurnLog(TurnLog turnLog)
    {
        foreach (PhaseLog[] phaseLogs in turnLog.phases)
        {
            foreach (PhaseLog phaseLog in phaseLogs)
            {
                Debug.LogFormat("MPTester OnReceiveTurnLog UID {0}, CombatSq {1}, FinalSq {2}", phaseLog.piece.uid, phaseLog.moveLog.combatSquare, phaseLog.moveLog.finalSquare);
            }
        }
    }
}
