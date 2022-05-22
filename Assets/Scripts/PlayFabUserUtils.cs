using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PlayFab;
using PlayFab.ClientModels;


public class PlayFabUserUtils : MonoBehaviour
{
    public void updateHighscore(int value)
    {
        var req = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate
                {
                    StatisticName="Highscore",
                    Value=value
                }
            }
        };
        PlayFabClientAPI.UpdatePlayerStatistics(req, OnLeaderboardUpdate, OnError);
    }

    void OnLeaderboardUpdate(UpdatePlayerStatisticsResult r)
    {
        UpdateMsg("Successful leaderboard sent: " + r.ToString());
    }
    void OnError(PlayFabError e)
    {
        UpdateMsg(e.GenerateErrorReport().Split(": ")[1]);
    }

    void UpdateMsg(string msg)
    {
        Debug.Log(msg);
    }
}
