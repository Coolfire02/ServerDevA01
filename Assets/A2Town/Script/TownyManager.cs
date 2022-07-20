using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class TownyManager : MonoBehaviour
{
    public static TownyManager Instance;

    [SerializeField] GameObject coinPrefab;

    void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }


    float stateElapsed;
    float sceneElapsed;

    float nextCoinDrop;
    float coinDropCooldown = 5.0f;

    public void Start()
    {
        stateElapsed = 0.0f;
        sceneElapsed = 0.0f;
        nextCoinDrop = coinDropCooldown;
    }

    //public void OnEnable()
    //{
    //    RaiseEvents.SpawnCoinEvent += CoinSpawn;
    //}

    //public void OnDisable()
    //{
    //    RaiseEvents.SpawnCoinEvent -= CoinSpawn;
    //}

    private void OnEnable()
    {
        RaiseEvents.LoadPlayerEvent += onPlayerLoad;
    }

    private void OnDisable()
    {
        RaiseEvents.LoadPlayerEvent -= onPlayerLoad;
    }

    public void onPlayerLoad(object[] objs)
    {
        int pviewID = (int)objs[0];
        string playfabID = (string)objs[1];
        string playerNick = (string)objs[2];

        print("Event reached?");


        Player player = PhotonNetwork.GetPhotonView(pviewID).gameObject.GetComponent<Player>();
        player.InitPlayfabDetailsOfController(playfabID, playerNick);
    }

    private void Update()
    {
        sceneElapsed += Time.deltaTime;
        stateElapsed += Time.deltaTime;

        if(sceneElapsed > nextCoinDrop)
        {
            nextCoinDrop = sceneElapsed + coinDropCooldown;

            //if (PhotonNetwork.IsMasterClient)
            //{
            //    object[] objs = new object[1];
            //    objs[0] = "" + new Vector3(Random.Range(-15.0f, 15.0f), 0, 0);
            //    RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            //    PhotonNetwork.RaiseEvent(RaiseEvents.SPAWNCOIN, objs, raiseEventOptions, SendOptions.SendReliable);
            //}

            if(PhotonNetwork.IsMasterClient)
            {
                Vector3 spawnPos = new Vector3(Random.Range(-15.0f, 15.0f), 8, 0);
                PhotonNetwork.Instantiate(coinPrefab.name, spawnPos, Quaternion.identity);
            }

        }
    }

}
