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

    Piece A_PIECE_1 = new Piece("uida1", Player.A);
    Piece A_PIECE_2 = new Piece("uida2", Player.A);
    Piece A_PIECE_3 = new Piece("uida3", Player.A);

    Piece B_PIECE_1 = new Piece("uidb1", Player.B);
    Piece B_PIECE_2 = new Piece("uidb2", Player.B);
    Piece B_PIECE_3 = new Piece("uidb3", Player.B);

    // Start is called before the first frame update
    void Start()
    {
        stateManager = GetComponent<StateManager>();
        networkManager = GetComponent<NetworkEventManager>();
        renderer = GetComponent<Renderer>();
        if (networkManager.IsMasterClient())
        {
            SetUpDummyBoard();
        }
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

    void SetUpDummyBoard()
    {
        stateManager.board = new Board(6, 6);                             //  | 0  | 1  | 2  | 3  | 4  | 5  |
        stateManager.board.SetPieceAtSquare(A_PIECE_1, new Square(0, 0)); //0 | A1 |    |    |    |    |    |
        stateManager.board.SetPieceAtSquare(A_PIECE_2, new Square(2, 2)); //1 |    |    |    | A3 |    |    |
        stateManager.board.SetPieceAtSquare(A_PIECE_3, new Square(1, 3)); //2 |    |    | A2 | B2 |    |    |
        stateManager.board.SetPieceAtSquare(B_PIECE_1, new Square(3, 2)); //3 |    |    | B1 |    | B3 |    |
        stateManager.board.SetPieceAtSquare(B_PIECE_2, new Square(2, 3)); //4 |    |    |    |    |    |    |
        stateManager.board.SetPieceAtSquare(B_PIECE_3, new Square(3, 4)); //5 |    |    |    |    |    |    |
    }                                                                     //---------------------------------

    void OnGameStart()
    {
        if (networkManager.IsMasterClient())
        {
            renderer.material.color = new Color(1.0f, 0, 0); // for debug
            Move[] movesToSend = {
                        new Move(A_PIECE_1, Move.Direction.UP),
                        new Move(A_PIECE_2, Move.Direction.DOWN),
                    };
            networkManager.SendTurn(new List<Move[]> { movesToSend, movesToSend, movesToSend });
        }
        else
        {
            renderer.material.color = new Color(0, 0, 1.0f); // for debug
            Move[] movesToSend = {
                        new Move(B_PIECE_1, Move.Direction.UP),
                        new Move(B_PIECE_2, Move.Direction.DOWN),
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
