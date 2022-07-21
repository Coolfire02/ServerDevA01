using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class RaiseEvents : MonoBehaviour, IOnEventCallback
{
    public const byte SPAWNCOIN = 1;
    public const byte PLAYERBALANCEUPDATE = 2; 
    public const byte LOADPLAYER = 3;

    public delegate void OnSpawnCoin(object[] objs); //SpawnCoinPos(VecToString)
    public static event OnSpawnCoin SpawnCoinEvent;

    public delegate void OnPlayerBalanceUpdate(object[] objs); //PhotonView(int), New Bal(int)
    public static event OnPlayerBalanceUpdate PlayerBalanceUpdateEvent;

    public delegate void OnLoadPlayer(object[] objs); //PViewID(int), PlayerPlayfabID(string), PlayerNick(string)
    public static event OnLoadPlayer LoadPlayerEvent;


    //Raise Event uses Unity OnEnable and OnDisable too to include the event in the system
    private void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    private void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;
        if (eventCode == SPAWNCOIN)
        {

            SpawnCoinEvent?.Invoke((object[])photonEvent.CustomData);

        }
        else if (eventCode == PLAYERBALANCEUPDATE)
        {
            PlayerBalanceUpdateEvent?.Invoke((object[])photonEvent.CustomData);
        }
        else if (eventCode == LOADPLAYER)
        {
            LoadPlayerEvent?.Invoke((object[])photonEvent.CustomData);
        }
    }
}

