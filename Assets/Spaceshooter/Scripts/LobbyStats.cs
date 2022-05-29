using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;

using TMPro;
using PlayFab;
using PlayFab.ClientModels;
using System;

public class LobbyStats : MonoBehaviour
{
    [SerializeField]
    TMP_Text tx_coinsDisplay;

    [SerializeField]
    TMP_Text tx_levelDisplay;

    [SerializeField]
    GameObject playFabUtils;

    //Always be updated to latest playfab
    private int playFabCoins;
    private int playFabXP;
    private int playFabLvl;
    private string playFabEquippedSkin;

    //Cached Attributes
    private int coins;
    private int playerXP;
    private int playerLvl;

    string playerEquippedSkin;

    bool completedFirstRefresh = false;
    bool enableLocalCache = false;

    int apiCallbacks = -1;
    int maxApiCallbacks = 2;

    bool activeInHierachyLastFrame = false;

    private void OnEnable()
    {
        refreshStatistics();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    //Predefined Updates
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        refreshStatistics();
    }

    public void refreshStatistics()
    {
        if (!PlayFabClientAPI.IsClientLoggedIn()) return;
        if(isPreviousRefreshCompleted() || apiCallbacks < 0)
        {
            if (tx_coinsDisplay != null && tx_coinsDisplay.text.Length == 0) tx_coinsDisplay.text = "Fetching statistic...";
            if (tx_levelDisplay != null && tx_levelDisplay.text.Length == 0) tx_levelDisplay.text = "Fetching statistic...";

            apiCallbacks = 0;
            int xp = 0;
            int lvl = 1;
            string equippedSkin = null;
            PlayFabClientAPI.GetUserData(new GetUserDataRequest(),
                r =>
                {
                    apiCallbacks += 1;
                    if (apiCallbacks == maxApiCallbacks)
                    {
                        completedFirstRefresh = true;
                    }


                    if (r.Data != null)
                    {
                        if (r.Data.ContainsKey("XP")) xp = Convert.ToInt32(r.Data["XP"].Value);
                        if (r.Data.ContainsKey("Lvl")) lvl = Convert.ToInt32(r.Data["Lvl"].Value);
                        if (r.Data.ContainsKey("SkinEquipped")) equippedSkin = r.Data["SkinEquipped"].Value;
                    }

                    if(!enableLocalCache)
                    {
                        playerXP = xp;
                        playerLvl = lvl;
                        playerEquippedSkin = equippedSkin;
                        
                    }
                    //These vars are always updated to the last fetch.
                    playFabXP = xp;
                    playFabLvl = lvl;
                    playFabEquippedSkin = equippedSkin;

                    if(tx_levelDisplay != null)
                        tx_levelDisplay.text = "Level " + lvl + ", " + xp + "/" + Leveling.getXPRequiredForLevel(lvl + 1) + " XP";
                },
                playFabUtils.GetComponent<PlayFabUserUtils>().OnError);

            PlayFabClientAPI.GetUserInventory(
                new GetUserInventoryRequest(),
                r =>
                {
                    apiCallbacks += 1;
                    if(apiCallbacks == maxApiCallbacks)
                    {
                        completedFirstRefresh = true;
                    }

                    if(!enableLocalCache)
                    {
                        coins = r.VirtualCurrency["GD"];
                    }
                    playFabCoins = coins;

                    if(tx_coinsDisplay != null)
                        tx_coinsDisplay.text = coins + " Coins";

                }, playFabUtils.GetComponent<PlayFabUserUtils>().OnError);
        }
    }

    public bool isPreviousRefreshCompleted()
    {
        return apiCallbacks == maxApiCallbacks;
    }

    public int getXP()
    {
        return playerXP;
    }

    public int getLevel()
    {
        return playerLvl;
    }

    public int getCoins()
    {
        return coins;
    }

    public string getSkinEquipped()
    {
        return playerEquippedSkin;
    }

    public void setEquippedSkin(string skin)
    {
        playerEquippedSkin = skin;
        enableLocalCache = true;
    }

    public void setCoins(int newCoins)
    {
        coins = newCoins;
        if (coins < 0) coins = 0;
        enableLocalCache = true;
        tx_coinsDisplay.text = coins + " Coins";
    }
    
    //If Set functions of cached values are used, local cache will be enabled
    public bool hasLocalCachedValues()
    {
        return enableLocalCache;
    }

    public void sendLocalCacheToPlayfab()
    {
        if(enableLocalCache)
        {
            //Set Coins
            int coinsDifference = coins - playFabCoins;
            print("Caching reflecting coin diff of  " + coinsDifference);
            playFabCoins = coins;
            if(coinsDifference > 0)
            {
                PlayFabClientAPI.AddUserVirtualCurrency(new AddUserVirtualCurrencyRequest()
                {
                    VirtualCurrency = "GD",
                    Amount = coinsDifference
                },
                r => {
                    enableLocalCache = false;
                    //Local cache flushed
                },
                playFabUtils.GetComponent<PlayFabUserUtils>().OnError);
            }else
            {
                int toSubtract = coinsDifference * -1;
                PlayFabClientAPI.SubtractUserVirtualCurrency(new SubtractUserVirtualCurrencyRequest()
                {
                    VirtualCurrency = "GD",
                    Amount = toSubtract

                },
                r => {
                    enableLocalCache = false;
                                    //Local cache flushed
                                },
                playFabUtils.GetComponent<PlayFabUserUtils>().OnError);
            }

            //Set Skin Data
            Dictionary<string, string> newData = new Dictionary<string, string>();
            newData.Add("SkinEquipped", playerEquippedSkin);
            PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest()
            {
                Data = newData
            }, r => { },
            playFabUtils.GetComponent<PlayFabUserUtils>().OnError);

        }
    }



    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
