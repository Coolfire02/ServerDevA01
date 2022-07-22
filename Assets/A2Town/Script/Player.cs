using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;


using PlayFab;
using PlayFab.ClientModels;
using PlayFab.Json;
using System;

public class Player : MonoBehaviourPunCallbacks
{
    private float moveSpeed = 7.0f;
    private bool canMove = true;

    private PhotonView photonView;

    private Rigidbody2D rigidbody;
    private BoxCollider2D boxCollider;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    private float directionX = 0f;
    private GameObject shopPanel;
    private GameObject guildPanel;
    private GameObject friendsPanel;
    private GameObject leaderPanel;
    private GameObject eButton;

    bool initialLoad = false;
    string PlayfabID;
    string PlayfabDisplayName;

    [SerializeField] TMP_Text tx_username;
    [SerializeField] TMP_Text tx_coinsDisplay;

    enum CONTACT_TYPE
    {
        NIL,
        SHOP,
        GUILD,
        FRIEND,
        LEADERBOARD,
    }

    private CONTACT_TYPE contactType;

    private void OnEnable()
    {
        RaiseEvents.FriendsUpdateEvent += FriendsUpdate;
    }

    private void OnDisable()
    {
        RaiseEvents.FriendsUpdateEvent -= FriendsUpdate;
    }

    public void FriendsUpdate(object[] objs)
    {
        string receiverPFabID = (string)objs[0];
        string senderPFabID = (string)objs[1];
        
        if(PhotonNetwork.LocalPlayer.UserId == receiverPFabID || PhotonNetwork.LocalPlayer.UserId == senderPFabID)
        {
            if(photonView.Owner.UserId != PhotonNetwork.LocalPlayer.UserId)
            {
                bool isFriend = (bool)objs[2];
                if(isFriend)
                {
                    tx_username.color = Color.green;
                }
                else
                {
                    tx_username.color = Color.white;
                }
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        photonView = GetComponent<PhotonView>();

        guildPanel = GameObject.FindGameObjectWithTag("Guild"); 
        shopPanel = GameObject.FindGameObjectWithTag("Shop");
        friendsPanel = GameObject.FindGameObjectWithTag("Friends");
        leaderPanel = GameObject.FindGameObjectWithTag("Leaderboard");

        if (photonView.IsMine)
        {
            eButton = GameObject.Find("EButton");
            eButton.SetActive(false);
        }

        canMove = true;

       
        tx_username.text = photonView.Owner.NickName;

        if(PhotonNetwork.LocalPlayer.UserId != photonView.Owner.UserId)
        {
            foreach(FriendInfo fi in PlayfabCache.Instance.friends)
            {
                if(fi.FriendPlayFabId == photonView.Owner.UserId)
                {
                    tx_username.color = Color.green;
                }
            }
        }

        print("Polling playfab id bal: " + photonView.Owner.UserId);
        PlayFabClientAPI.ExecuteCloudScript(
            new ExecuteCloudScriptRequest
            {

                FunctionName="getGoldCoins",
                FunctionParameter = new
                {
                    id = photonView.Owner.UserId
                }
                
            },
            r => {
                
                foreach(PlayFab.ClientModels.LogStatement ls in r.Logs)
                {
                    Debug.Log(ls.Level + " " + ls.Message);
                }

                Debug.Log(PlayFab.PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer).SerializeObject(r.FunctionResult));
                JsonObject jsonResult = (JsonObject)r.FunctionResult;
                object messageValue;
                jsonResult.TryGetValue("goldCoins", out messageValue); // note how "messageValue" directly corresponds to the JSON values set in CloudScript (Legacy)
                int goldCoins = Convert.ToInt32(messageValue);
                tx_coinsDisplay.text = goldCoins + " Coins";
                Debug.Log(messageValue.GetType().Name + " " + goldCoins);
            },
            e => {
                Debug.Log(e.GenerateErrorReport());
            }
            );
    }

    public bool isPlayerLoaded()
    {
        return initialLoad;
    }

    public void UpdateCoinBalanceDisplay(int newValue)
    {
        if(newValue >= 0)
        {
            tx_coinsDisplay.text = newValue + " Coins";
        }
    }

    public void InitPlayfabDetailsOfController(string playfabid, string playfabname)
    {
        this.PlayfabID = playfabid;
        this.PlayfabDisplayName = playfabname;

        initialLoad = true;

        print("Updating username of just joined player to " + this.PlayfabDisplayName);

        ApplyLoadedNameOntoUI();
    }

    public void ApplyLoadedNameOntoUI()
    {
        if(initialLoad)
            tx_username.text = this.PlayfabDisplayName;
    }

    

    public PhotonView GetPhotonView()
    {
        return photonView;
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine)
        {
            if (canMove)
            {
                directionX = Input.GetAxisRaw("Horizontal");
                rigidbody.velocity = new Vector2(directionX * moveSpeed, rigidbody.velocity.y);

                //If we press E
                if (Input.GetKeyDown(KeyCode.E))
                {
                    switch (contactType)
                    {
                        case CONTACT_TYPE.SHOP:
                            if(!MenuManager.Instance.getMenu("ShopUI").GetComponent<Menu>().isOpened())
                            {
                                MenuManager.Instance.OpenMenu("ShopUI");
                                eButton.SetActive(false);
                            }else
                            {
                                MenuManager.Instance.CloseMenu("ShopUI");
                            }
                            break;

                        case CONTACT_TYPE.FRIEND:
                            if (!MenuManager.Instance.getMenu("FriendsUI").GetComponent<Menu>().isOpened())
                            {
                                MenuManager.Instance.OpenMenu("FriendsUI");
                                eButton.SetActive(false);
                            }
                            else
                            {
                                MenuManager.Instance.CloseMenu("FriendsUI");
                            }
                            break;

                        case CONTACT_TYPE.LEADERBOARD:
                            if (!MenuManager.Instance.getMenu("LeaderboardUI").GetComponent<Menu>().isOpened())
                            {
                                MenuManager.Instance.OpenMenu("LeaderboardUI");
                                eButton.SetActive(false);
                            }
                            else
                            {
                                MenuManager.Instance.CloseMenu("LeaderboardUI");
                            }
                            break;

                        case CONTACT_TYPE.GUILD:
                            if (!MenuManager.Instance.getMenu("GuildUI").GetComponent<Menu>().isOpened())
                            {
                                MenuManager.Instance.OpenMenu("GuildUI");
                                eButton.SetActive(false);
                            }
                            else
                            {
                                MenuManager.Instance.CloseMenu("GuildUI");
                            }
                            break;
                    }
                }
                else if (Input.GetKeyDown(KeyCode.R))
                {
                    animator.SetTrigger("Is_Dancing");
                }

                //Update the animation
                UpdateAnimationUpdate();
            }
        }
    }
    private void UpdateAnimationUpdate()
    {
        if(directionX > 0f)
        {
            animator.SetBool("Is_Running", true);
            spriteRenderer.flipX = false;

        }
        else if(directionX < 0f)
        {
            animator.SetBool("Is_Running", true);
            spriteRenderer.flipX = true;
        }
        else
        {
            animator.SetBool("Is_Running", false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (photonView.IsMine)
        {
            if (collision.gameObject.CompareTag("Guild"))
            {
                contactType = CONTACT_TYPE.GUILD;
                eButton.SetActive(true);
            }
            else if (collision.gameObject.CompareTag("Shop"))
            {           
                contactType = CONTACT_TYPE.SHOP;
                eButton.SetActive(true);
            }
            else if (collision.gameObject.CompareTag("Friends"))
            {
                contactType = CONTACT_TYPE.FRIEND;
                eButton.SetActive(true);
            }
            else if (collision.gameObject.CompareTag("Leaderboard"))
            {
                contactType = CONTACT_TYPE.LEADERBOARD;
                eButton.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (photonView.IsMine && (
            collision.gameObject.CompareTag("Guild") ||
            collision.gameObject.CompareTag("Shop") ||
            collision.gameObject.CompareTag("Friends") ||
            collision.gameObject.CompareTag("Leaderboard") 
            ))
        {
            contactType = CONTACT_TYPE.NIL;
            eButton.SetActive(true);

            MenuManager.Instance.CloseMenu("GuildUI");
            MenuManager.Instance.CloseMenu("ShopUI");
            MenuManager.Instance.CloseMenu("LeaderboardUI");
            MenuManager.Instance.CloseMenu("FriendsUI");

        }
    }

}
