using PlayFab.ClientModels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayfabCache : MonoBehaviour
{
    public static PlayfabCache Instance;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public string DisplayName;
    public string PlayfabID;
    public List<FriendInfo> friends;
}
