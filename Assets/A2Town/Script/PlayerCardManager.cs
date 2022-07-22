using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Photon.Realtime;

using PlayFab;
using PlayFab.ClientModels;
using PlayFab.Json;
using FriendInfo = PlayFab.ClientModels.FriendInfo;

public class PlayerCardManager : MonoBehaviour
{
    [SerializeField] GameObject playCardPrefab;
    [SerializeField] GameObject gridLayout;

    float lastRefresh = 0.0f;
    float refreshTime = 6.5f;

    private void OnEnable()
    {
        RaiseEvents.FriendsUpdateEvent += FriendsUpdate;
        if(gridLayout.transform.childCount > 0)
        {
            foreach(Transform trans in gridLayout.transform)
            {
                GameObject.Destroy(trans.gameObject);
            }
        }
        PollInfoThenRefreshPlayerCards();
    }

    private void OnDisable()
    {
        RaiseEvents.FriendsUpdateEvent -= FriendsUpdate;
    }

    public void FriendsUpdate(object[] objs)
    {
        string receiverPFabID = (string)objs[0];
        string senderPFabID = (string)objs[1];
        if(receiverPFabID == PhotonNetwork.LocalPlayer.UserId || senderPFabID == PhotonNetwork.LocalPlayer.UserId)
        {
            lastRefresh = Time.realtimeSinceStartup - refreshTime + 0.3f;
            
        }
    }

    public void Update()
    {
        if(Time.realtimeSinceStartup - refreshTime > lastRefresh)
        {
            lastRefresh = Time.realtimeSinceStartup;
            PollInfoThenRefreshPlayerCards();
        }
    }

    public void PollInfoThenRefreshPlayerCards()
    {
        print("Repoll cards");
        lastRefresh = Time.realtimeSinceStartup;
        PlayFabClientAPI.GetFriendsList(new GetFriendsListRequest
        {
            IncludeFacebookFriends = false,
            IncludeSteamFriends = false,
            XboxToken = null
        }, frReqResult => {

            List<FriendInfo> friends = frReqResult.Friends;
            JsonObject wantToBeFriendsList = null;

            PlayFabClientAPI.GetUserData(new GetUserDataRequest(),
            r =>
            {
                if (r.Data != null)
                {
                    if(r.Data.ContainsKey("FriendRequests"))
                    {
                        wantToBeFriendsList = (JsonObject)PlayFabSimpleJson.DeserializeObject(r.Data["FriendRequests"].Value);
                    }
                }

                RefreshPlayerCards(friends, wantToBeFriendsList);

            },e => { });



    },
        e => { Debug.Log(e.GenerateErrorReport()); });
    }

    public void RefreshPlayerCards(List<FriendInfo> playfabFriends /*Local Player Friends*/, JsonObject wantsToBeFriendsList /*With Local Player*/ )
    {
        Dictionary<int, Photon.Realtime.Player> photonPlayers = PhotonNetwork.CurrentRoom.Players;
        foreach (Photon.Realtime.Player photonP in photonPlayers.Values)
        {
            bool cardExist = false;
            foreach (Transform trans in gridLayout.transform)
            {
                if (trans.gameObject.GetComponent<PlayerCard>().GetPlayFabID() == photonP.UserId)
                {
                    PlayerCard card = trans.gameObject.GetComponent<PlayerCard>();

                    bool isFriend = false;
                    if (playfabFriends != null)
                    {
                        foreach (FriendInfo fi in playfabFriends)
                        {
                            if (fi.FriendPlayFabId == photonP.UserId)
                            {
                                isFriend = true;
                                card.SetFriendState(PlayerCard.FRIEND_STATE.FRIEND);
                                break;
                            }
                        }
                    }

                    bool wantsToBeFriend = false;
                    if (wantsToBeFriendsList != null)
                    {
                        object value;
                        wantsToBeFriendsList.TryGetValue(photonP.UserId, out value);
                        if (value != null && !isFriend)
                        {
                            wantsToBeFriend = true;
                            card.SetFriendState(PlayerCard.FRIEND_STATE.WANTS_TO_BE_FRIENDS);
                        }
                    }

                    if (!isFriend && !wantsToBeFriend)
                    {
                        if (photonP.UserId == PhotonNetwork.LocalPlayer.UserId)
                        {
                            card.SetFriendState(PlayerCard.FRIEND_STATE.ISSELF);
                        }
                        else
                            card.SetFriendState(PlayerCard.FRIEND_STATE.NOT_FRIEND);
                    }

                    cardExist = true;
                    break;
                }
            }
            if (!cardExist)
            {
                PlayerCard newCard = GameObject.Instantiate(playCardPrefab, gridLayout.transform).GetComponent<PlayerCard>();
                newCard.SetCardDetails(photonP.NickName, photonP.UserId);
                if (photonP.UserId == PhotonNetwork.LocalPlayer.UserId)
                {
                    newCard.SetFriendState(PlayerCard.FRIEND_STATE.ISSELF);
                }else
                {
                    bool isFriend = false;
                    if(playfabFriends != null)
                    {
                        foreach (FriendInfo fi in playfabFriends)
                        {
                            if (fi.FriendPlayFabId == photonP.UserId)
                            {
                                isFriend = true;
                                newCard.SetFriendState(PlayerCard.FRIEND_STATE.FRIEND);
                                break;
                            }
                        }
                    }

                    bool wantsToBeFriend = false;
                    if (wantsToBeFriendsList != null)
                    {
                        object value;
                        wantsToBeFriendsList.TryGetValue(photonP.UserId, out value);
                        if (value != null && !isFriend)
                        {
                            wantsToBeFriend = true;
                            newCard.SetFriendState(PlayerCard.FRIEND_STATE.WANTS_TO_BE_FRIENDS);
                        }
                    }

                    if(!isFriend && !wantsToBeFriend)
                    {
                        newCard.SetFriendState(PlayerCard.FRIEND_STATE.NOT_FRIEND);
                    }
                }
            }
        }

        foreach (Transform trans in gridLayout.transform)
        {
            bool found = false;
            foreach (Photon.Realtime.Player photonP in photonPlayers.Values)
            {
                if (trans.gameObject.GetComponent<PlayerCard>().GetPlayFabID() == photonP.UserId)
                {
                    found = true;
                    break;
                }
            }
            if(!found)
            {
                //Player left
                GameObject.Destroy(trans.gameObject);
            }
                
        }
    }


}
