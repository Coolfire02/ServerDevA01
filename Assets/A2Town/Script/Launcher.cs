using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class Launcher : MonoBehaviourPunCallbacks
{
    public GameObject PlayerPrefab;

    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected");
        PhotonNetwork.JoinRandomOrCreateRoom();
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined");
        GameObject go = PhotonNetwork.Instantiate(PlayerPrefab.name, new Vector3(0, -2.9f, 0), Quaternion.identity);


        object[] obj = new object[3];
        obj[0] = go.GetComponent<PhotonView>().ViewID;
        obj[1] = PlayfabCache.Instance.PlayfabID;
        obj[2] = PlayfabCache.Instance.DisplayName;

        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(RaiseEvents.LOADPLAYER, obj, raiseEventOptions, SendOptions.SendReliable);
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
