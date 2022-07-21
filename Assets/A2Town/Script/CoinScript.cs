using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

using PlayFab;
using PlayFab.ClientModels;
using PlayFab.Json;
using System;

public class CoinScript : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<Player>())
        {
            Player p = collision.gameObject.GetComponent<Player>();
            string playfabID = p.GetPhotonView().Owner.UserId;


            PlayFabClientAPI.ExecuteCloudScript(
            new ExecuteCloudScriptRequest
            {

                FunctionName = "addCoins",
                FunctionParameter = new
                {
                    Amount = 1,
                    id = p.GetPhotonView().Owner.UserId
                }

            },
            r => {
                Debug.Log("CoinScript " + PlayFab.PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer).SerializeObject(r.FunctionResult));
                int newBal = -1;
                JsonObject jsonResult = (JsonObject)r.FunctionResult;
                {
                    object messageValue;
                    jsonResult.TryGetValue("newCoins", out messageValue); // note how "messageValue" directly corresponds to the JSON values set in CloudScript (Legacy)
                    int goldCoins = Convert.ToInt32(messageValue);
                    newBal = goldCoins;
                    Debug.Log("new coins " + goldCoins);
                }
                {
                    object messageValue;
                    jsonResult.TryGetValue("messageValue", out messageValue); // note how "messageValue" directly corresponds to the JSON values set in CloudScript (Legacy)

                    Debug.Log((string)messageValue);
                }

                object[] objs = new object[2];
                objs[0] = p.GetPhotonView().ViewID;
                objs[1] = newBal;
                RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
                PhotonNetwork.RaiseEvent(RaiseEvents.PLAYERBALANCEUPDATE, objs, raiseEventOptions, SendOptions.SendReliable);

            },
            e => {
                Debug.Log(e.GenerateErrorReport());
            }
            );

            PhotonNetwork.Destroy(gameObject);
        }
    }

}
