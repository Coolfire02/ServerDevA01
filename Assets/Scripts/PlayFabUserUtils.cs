using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using PlayFab;
using PlayFab.ClientModels;


public class PlayFabUserUtils : MonoBehaviour
{

    public void updateHighscore(int value)
    {
        updateLeaderboard("Highscore", value);
    }

    public void addCoins(int toAdd)
    {
        PlayFabClientAPI.AddUserVirtualCurrency(new AddUserVirtualCurrencyRequest()
        {
            VirtualCurrency = "GD",
            Amount = toAdd
        },
        r => { print("new amt of GD " + r.BalanceChange); },
        OnError);
    }

    public void addXP(int toAdd)
    {
        int xp = 0;
        int lvl = 1;
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(),
            r =>
            {
                if(r.Data != null)
                {
                    if (r.Data.ContainsKey("XP")) xp = Convert.ToInt32(r.Data["XP"].Value);
                    if (r.Data.ContainsKey("Lvl")) lvl = Convert.ToInt32(r.Data["Lvl"].Value); 
                }

                xp += toAdd;
                while (xp > Leveling.getXPRequiredForLevel(lvl + 1))
                {
                    xp -= Leveling.getXPRequiredForLevel(lvl + 1);
                    lvl += 1;
                    print("Level up");
                }
                Dictionary<string, string> data = new Dictionary<string, string>();
                data.Add("XP", xp.ToString());
                data.Add("Lvl", lvl.ToString());
                print("new lvl " + lvl + " new xp " + xp);
                updateUserData(data);
            },
            OnError);
    }

    public void updateUserData(Dictionary<string, string> newValues)
    {
        PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest()
        {
            Data = newValues
        }, r => { },
        OnError);
    }

    private void updateLeaderboard(string lbName, int value)
    {
        var req = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate
                {
                    StatisticName=lbName,
                    Value=value
                }
            }
        };
        PlayFabClientAPI.UpdatePlayerStatistics(req,
            r => { }, OnError);
    }

    public void OnError(PlayFabError e)
    {
        UpdateMsg(e.GenerateErrorReport().Split(": ")[1]);
    }

    void UpdateMsg(string msg)
    {
        Debug.Log(msg);
    }
}
