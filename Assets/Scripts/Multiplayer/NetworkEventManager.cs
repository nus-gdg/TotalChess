using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

using UnityEngine;
using UnityEngine.SceneManagement;

using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

using State;
using Log;

public class NetworkEventManager : MonoBehaviourPunCallbacks, IOnEventCallback
{

    public bool IsMasterClient()
    {
        return PhotonNetwork.IsMasterClient;
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(0);
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player other)
    {
        Debug.LogFormat("OnPlayerEnteredRoom() {0}", other.NickName); // not seen if you're the player connecting

        if (PhotonNetwork.IsMasterClient)
        {
            Debug.LogFormat("OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom
            RaiseStartEvent();
        }
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player other)
    {
        Debug.LogFormat("OnPlayerLeftRoom() {0}", other.NickName); // seen when other disconnects

        if (PhotonNetwork.IsMasterClient)
        {
            Debug.LogFormat("OnPlayerLeftRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom
        }
    }

    private byte[] serializeObject(object obj)
    {
        BinaryFormatter bf = new BinaryFormatter();
        MemoryStream ms = new MemoryStream();
        bf.Serialize(ms, obj);
        byte[] data = ms.ToArray();
        ms.Close();
        return data;
    }

    private object deserializeData(byte[] data)
    {
        MemoryStream memStream = new MemoryStream();
        BinaryFormatter binForm = new BinaryFormatter();
        memStream.Write(data, 0, data.Length);
        memStream.Seek(0, SeekOrigin.Begin);
        object obj = binForm.Deserialize(memStream);
        memStream.Close();
        return obj;
    }

    // FE can use this to send their move information
    // note: Currently set as List<Move[]>
    public void SendTurn(List<Move[]> turnList)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // cache it here or send to game manager
            OnReceiveTurn?.Invoke(turnList.ToArray());
            return;
        }
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
        SendOptions sendOptions = new SendOptions { Reliability = true };
        byte[] data = serializeObject(turnList.ToArray());
        PhotonNetwork.RaiseEvent(SEND_TURN_NEVENT, data, raiseEventOptions, sendOptions);
    }

    public void SendTurnLog(TurnLog turnLog)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return; // only master can send turn logs
        }
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        SendOptions sendOptions = new SendOptions { Reliability = true };
        byte[] data = serializeObject(turnLog);
        PhotonNetwork.RaiseEvent(SEND_TURN_LOG_NEVENT, data, raiseEventOptions, sendOptions);
    }

    public void RaiseStartEvent()
    {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        SendOptions sendOptions = new SendOptions { Reliability = true };
        PhotonNetwork.RaiseEvent(GAME_START_NEVENT, null, raiseEventOptions, sendOptions);
    }

    // NEVENT (FE should not have to worry about this)
    // note: NEVENT = NETWORK EVENT, need to separate from Events used by client
    public const byte GAME_START_NEVENT = 0;
    public const byte SEND_TURN_NEVENT = 1;
    public const byte SEND_TURN_LOG_NEVENT = 2;

    //EVENTS (FE can use these)
    public delegate void GameStartEvent();
    public static event GameStartEvent OnGameStart;

    public delegate void ReceiveTurnEvent(Move[][] turn);
    public static event ReceiveTurnEvent OnReceiveTurn;

    // FE can hook onto this
    public delegate void ReceiveTurnLogEvent(TurnLog turn);
    public static event ReceiveTurnLogEvent OnReceiveTurnLog;

    public void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;
        switch (eventCode)
        {
            case GAME_START_NEVENT:
                OnGameStart?.Invoke();
                break;
            case SEND_TURN_NEVENT:
                byte[] data = (byte[])photonEvent.CustomData;
                Move[][] turn = (Move[][])deserializeData(data);
                OnReceiveTurn?.Invoke(turn);
                break;
            case SEND_TURN_LOG_NEVENT:
                byte[] turnData = (byte[])photonEvent.CustomData;
                TurnLog turnLog = (TurnLog)deserializeData(turnData);
                OnReceiveTurnLog?.Invoke(turnLog);
                break;
        }
    }

    public override void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    public override void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }
}
