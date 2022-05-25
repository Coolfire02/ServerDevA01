using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using PlayFab;
using PlayFab.ClientModels;

using TMPro;

public class Leaderboards : MonoBehaviour
{
    [SerializeField]
    GameObject leaderboardItem;

    bool loadingLeaderboard = false;
    string currentLBName = "";
    bool lookingAtLocal = false;

    string cachedLoggedInID;

    private void Start()
    {
        destroyExistingLeaderboard();
    }

    private void OnEnable()
    {
        PlayFabClientAPI.GetPlayerProfile(new GetPlayerProfileRequest(),
            r =>
            {

                if (r.PlayerProfile != null)
                {
                    cachedLoggedInID = r.PlayerProfile.PlayerId;

                }
            },
            OnError);
    }

    public void LookAtLocal(bool local)
    {
        lookingAtLocal = local;
    }

    public void ReOpenCurrentScoreboard()
    {
        openLeaderboard(currentLBName, lookingAtLocal);
    }

    public void ButtonOpenScoreLeaderboard()
    {
        openLeaderboard("Highscore", lookingAtLocal);
    }

    public void ButtonOpenLevelLeaderboard()
    {
        openLeaderboard("Playerlevel", lookingAtLocal);
    }

    public void openLeaderboard(string leaderboardName, bool local)
    {
        destroyExistingLeaderboard();
        loadingLeaderboard = true;
        currentLBName = leaderboardName;
        if (!local)
        {
            var lb_req = new GetLeaderboardRequest
            {
                StatisticName = leaderboardName,
                StartPosition = 0,
                MaxResultsCount = 7,

            };
            PlayFabClientAPI.GetLeaderboard(lb_req, OnLeaderboardGet, OnError);
        }
        else
        {
            loadingLeaderboard = true;
            PlayFabClientAPI.GetPlayerProfile(new GetPlayerProfileRequest(),
            r =>
            {
                
                if (r.PlayerProfile != null)
                {
                    var lb_req = new GetLeaderboardAroundPlayerRequest
                    {
                        StatisticName = leaderboardName,
                        MaxResultsCount = 7,
                        PlayFabId = r.PlayerProfile.PlayerId
                    };
                    PlayFabClientAPI.GetLeaderboardAroundPlayer(lb_req, OnSurroundingLeaderboardGet, OnError);
                    
                }
            },
            OnError);
        }
    }

    void OnSurroundingLeaderboardGet(GetLeaderboardAroundPlayerResult r)
    {
        destroyExistingLeaderboard();
        loadLeaderboard(r.Leaderboard, currentLBName);
        loadingLeaderboard = false;
    }

    void OnLeaderboardGet(GetLeaderboardResult r)
    {
        destroyExistingLeaderboard();
        loadLeaderboard(r.Leaderboard, currentLBName);
        loadingLeaderboard = false;
    }

    void loadLeaderboard(List<PlayerLeaderboardEntry> list, string lbName)
    {
        foreach(var item in list)
        {
            GameObject go = Instantiate(leaderboardItem, transform);

            string displayName = item.DisplayName;
            if(item.PlayFabId == cachedLoggedInID)
            {
                go.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
            }
           
            if (displayName == null || displayName.Length <= 0) displayName = "Guest" + item.PlayFabId;
            go.transform.Find("tx_posName").GetComponent<TMP_Text>().text = (item.Position+1) + ". " + displayName;

            string lbValue = "";
            if (lbName.Equals("Highscore")) lbValue = item.StatValue.ToString() + " Score";
            else if (lbName.Equals("Playerlevel")) lbValue = "Level " + item.StatValue.ToString();
            go.transform.Find("tx_lbValue").GetComponent<TMP_Text>().text = lbValue;
        }
    }

    private void Update()
    {
        foreach(Transform child in transform)
        {
            GameObject bgImg = child.Find("bg_img").gameObject;
            float fill = bgImg.GetComponent<Image>().fillAmount;
            if(fill < 1)
            {
                fill += Time.deltaTime * 7;
                bgImg.GetComponent<Image>().fillAmount = fill;
                if(fill >= 0.6)
                {
                    foreach(Transform grandChild in child)
                    {
                        if (!grandChild.gameObject.activeSelf) grandChild.gameObject.SetActive(true);
                    }
                }
                break;
            }
        }
    }

    private void destroyExistingLeaderboard()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    void OnError(PlayFabError e)
    {
        Debug.Log(e.GenerateErrorReport().Split(": ")[1]);
        loadingLeaderboard = false;
    }
}
