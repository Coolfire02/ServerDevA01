using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

using PlayFab;
using PlayFab.ClientModels;
using PlayFab.Json;

using TMPro;

public class PlayerCard : MonoBehaviour
{
    string displayName;
    string playfabID;

    public enum FRIEND_STATE
    {
        FRIEND,
        WANTS_TO_BE_FRIENDS,
        NOT_FRIEND,
        ISSELF
    }

    public FRIEND_STATE frState = FRIEND_STATE.ISSELF;

    [SerializeField] GameObject bn_dropDownMenu;
    [SerializeField] GameObject bn_addFriend;
    [SerializeField] GameObject bn_acceptFriend;
    [SerializeField] GameObject bn_removeFriend;
    [SerializeField] GameObject bn_viewFriendStats;
    [SerializeField] GameObject bg_normalPlayer;
    [SerializeField] GameObject bg_friendPlayer;
    [SerializeField] GameObject bg_wantsToBeFriendsPlayer;
    [SerializeField] TMP_Text tx_Username;

    [SerializeField]
    GameObject go_DropDownMenu;
    
    public void SetCardDetails(string displayName, string playfabID)
    {
        this.displayName = displayName;
        this.playfabID = playfabID;
        this.tx_Username.text = displayName;
    }

    public void ButtonToggleDropDown()
    {
        go_DropDownMenu.SetActive(!go_DropDownMenu.activeSelf);
    }

    public string GetPlayFabID()
    {
        return this.playfabID;
    }

    public void SetFriendState(FRIEND_STATE newState)
    {
        if(frState != newState)
        {
            frState = newState;

            if(frState == FRIEND_STATE.FRIEND)
            {
                bn_dropDownMenu.SetActive(true);

                bg_normalPlayer.SetActive(false);
                bg_friendPlayer.SetActive(true);
                bg_wantsToBeFriendsPlayer.SetActive(false);

                bn_addFriend.SetActive(false);
                bn_removeFriend.SetActive(true);
                bn_viewFriendStats.SetActive(true);
                bn_acceptFriend.SetActive(false);
            }

            else if (frState == FRIEND_STATE.NOT_FRIEND)
            {
                bn_dropDownMenu.SetActive(true);

                bg_normalPlayer.SetActive(true);
                bg_friendPlayer.SetActive(false);
                bg_wantsToBeFriendsPlayer.SetActive(false);

                bn_addFriend.SetActive(true);
                bn_removeFriend.SetActive(false);
                bn_viewFriendStats.SetActive(false);
                bn_acceptFriend.SetActive(false);
            }
            
            else if (frState == FRIEND_STATE.WANTS_TO_BE_FRIENDS)
            {
                bn_dropDownMenu.SetActive(true);

                bg_normalPlayer.SetActive(false);
                bg_friendPlayer.SetActive(false);
                bg_wantsToBeFriendsPlayer.SetActive(true);

                bn_addFriend.SetActive(false);
                bn_removeFriend.SetActive(false);
                bn_viewFriendStats.SetActive(false);
                bn_acceptFriend.SetActive(true);
            }

        }else if (newState == FRIEND_STATE.ISSELF)
        {
            bn_dropDownMenu.SetActive(false);

            bg_normalPlayer.SetActive(true);
            bg_friendPlayer.SetActive(false);
            bg_wantsToBeFriendsPlayer.SetActive(false);
        }
    }

    public void ButtonRemoveFriend()
    {
        PlayFabClientAPI.ExecuteCloudScript(
            new ExecuteCloudScriptRequest
            {

                FunctionName = "removeFriend",
                FunctionParameter = new
                {
                    id = playfabID
                },
                GeneratePlayStreamEvent = true

            },
            r => {



                object[] objs = new object[3];
                objs[0] = playfabID;
                objs[1] = PhotonNetwork.LocalPlayer.UserId;
                objs[2] = false;
                RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
                PhotonNetwork.RaiseEvent(RaiseEvents.FRIENDSUPDATE, objs, raiseEventOptions, SendOptions.SendReliable);
            },
            e => {
                Debug.Log(e.GenerateErrorReport());
            }
            );
    }

    public void ButtonAddFriend()
    {
        PlayFabClientAPI.ExecuteCloudScript(
            new ExecuteCloudScriptRequest
            {

                FunctionName = "addFriend",
                FunctionParameter = new
                {
                    id = playfabID
                },
                GeneratePlayStreamEvent = true

            },
            r => {

                foreach (PlayFab.ClientModels.LogStatement ls in r.Logs)
                {
                    Debug.Log(ls.Level + " " + ls.Message);
                }

                Debug.Log(PlayFab.PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer).SerializeObject(r.FunctionResult));
                JsonObject jsonResult = (JsonObject)r.FunctionResult;
                object friendRequestSuccess;
                jsonResult.TryGetValue("friendRequestSuccess", out friendRequestSuccess); // note how "messageValue" directly corresponds to the JSON values set in CloudScript (Legacy)
                if((bool)friendRequestSuccess)
                {
                    Debug.Log("Sent Fr Req");

                    object bothNowAreFriends;
                    jsonResult.TryGetValue("bothNowAreFriends", out bothNowAreFriends);
                    

                    object[] objs = new object[3];
                    objs[0] = playfabID;
                    objs[1] = PhotonNetwork.LocalPlayer.UserId;
                    objs[2] = (bool)bothNowAreFriends;
                    RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
                    PhotonNetwork.RaiseEvent(RaiseEvents.FRIENDSUPDATE, objs, raiseEventOptions, SendOptions.SendReliable);
                }
                else
                {
                    Debug.Log("Failed to sent Req");
                }
            },
            e => {
                Debug.Log(e.GenerateErrorReport());
            }
            );
    }
}
