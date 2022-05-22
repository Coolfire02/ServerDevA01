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

    //Cached Attributes
    private int coins;
    private int playerXP;
    private int playerLvl;

    int apiCallbacks = -1;
    int maxApiCallbacks = 2;

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
            if (tx_coinsDisplay != null) tx_coinsDisplay.text = "Fetching statistic...";
            if (tx_levelDisplay != null) tx_levelDisplay.text = "Fetching statistic...";

            apiCallbacks = 0;
            int xp = 0;
            int lvl = 1;
            PlayFabClientAPI.GetUserData(new GetUserDataRequest(),
                r =>
                {
                    apiCallbacks += 1;
                    if (r.Data != null)
                    {
                        if (r.Data.ContainsKey("XP")) xp = Convert.ToInt32(r.Data["XP"].Value);
                        if (r.Data.ContainsKey("Lvl")) lvl = Convert.ToInt32(r.Data["Lvl"].Value);
                    }

                    playerXP = xp;
                    playerLvl = lvl;

                    if(tx_levelDisplay != null)
                        tx_levelDisplay.text = "Level " + lvl + ", " + xp + "/" + Leveling.getXPRequiredForLevel(lvl + 1) + " XP";
                },
                playFabUtils.GetComponent<PlayFabUserUtils>().OnError);

            PlayFabClientAPI.GetUserInventory(
                new GetUserInventoryRequest(),
                r =>
                {
                    apiCallbacks += 1;
                    coins = r.VirtualCurrency["GD"];

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
        if(isPreviousRefreshCompleted())
        {
            return playerXP;
        }
        return int.MinValue;
    }

    public int getLevel()
    {
        if(isPreviousRefreshCompleted())
        {
            return playerLvl;
        }
        return int.MinValue;
    }

    public int getCoins()
    {
        if(isPreviousRefreshCompleted())
        {
            return coins;
        }
        return int.MinValue;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
