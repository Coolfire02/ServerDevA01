using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using PlayFab;
using PlayFab.ClientModels;


using TMPro;

public class PlayFabUserMgt : MonoBehaviour
{
    [SerializeField] TMP_InputField userEmail, userPassword, userName, currentScore, displayName, emailCfm, passCfm;
    [SerializeField] TMP_Text Msg;

    public void OnButtonRegUser()
    {
        if(emailCfm.text != userEmail.text)
        {
            Msg.text = "Email does not match";
            return;
        }
        else if (passCfm.text != userPassword.text)
        {
            Msg.text = "Password does not match";
            return;
        }
        var registerRequest = new RegisterPlayFabUserRequest
        {
            Email = userEmail.text,
            Password = userPassword.text,
            Username = userName.text
        };
        PlayFabClientAPI.RegisterPlayFabUser(registerRequest, OnRegSuccess, OnError);
    }
    
    public void OnButtonLoginGeneric()
    {
        if(userName.text.Contains("@"))
        {
            var loginRequest = new LoginWithEmailAddressRequest
            {
                Email = userName.text,
                Password = userPassword.text,

                InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
                {
                    GetPlayerProfile = true
                }
            };
            PlayFabClientAPI.LoginWithEmailAddress(loginRequest, OnLoginSuccess, OnError);
        }else
        {
            OnButtonLoginUserName();
        }
    }

    public void onButtonLoginGuest()
    {

        var guestLoginRequest = new LoginWithCustomIDRequest
        {
            TitleId = PlayFabSettings.TitleId,
            CreateAccount = true,
            CustomId = SystemInfo.deviceUniqueIdentifier,
        };
        PlayFabClientAPI.LoginWithCustomID(guestLoginRequest, OnLoginSuccess, OnError);
    }

    public void OnButtonLoginEmail()
    {
        var loginRequest = new LoginWithEmailAddressRequest
        {
            Email = userEmail.text,
            Password = userPassword.text,

            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
            {
                GetPlayerProfile = true
            }
        };
        PlayFabClientAPI.LoginWithEmailAddress(loginRequest, OnLoginSuccess, OnError);
    }

    public void OnButtonLoginUserName()
    {
        var loginRequest = new LoginWithPlayFabRequest
        {
            Username = userName.text,
            Password = userPassword.text,
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
            {
                GetPlayerProfile = true
            }
        };
        PlayFabClientAPI.LoginWithPlayFab(loginRequest, OnLoginSuccess, OnError);
    }

    void OnRegSuccess(RegisterPlayFabUserResult r)
    {
        UpdateMsg("Register Success!");

        string name = displayName.text;
        if (name.Length < 1) name = userName.text;
        var req = new UpdateUserTitleDisplayNameRequest
        {
            
            DisplayName = name
                

        };
        PlayFabClientAPI.UpdateUserTitleDisplayName(req, OnDisplayNameUpdate, OnError);
    }

    void OnLoginSuccess(LoginResult r)
    {
        UpdateMsg("Login Success");
        MenuManager.Instance.OpenMenu("StartScreen");
        //SceneManager.LoadScene("InventoryScene");
    }

    void OnDisplayNameUpdate(UpdateUserTitleDisplayNameResult r)
    {
        UpdateMsg("display name updated!" + r.DisplayName);
    }    




    //Leaderboard
    public void OnButtonGetLeaderboard()
    {
        var lb_req = new GetLeaderboardRequest
        {
            StatisticName = "Highscore",
            StartPosition = 0,
            MaxResultsCount = 10
        };
        PlayFabClientAPI.GetLeaderboard(lb_req, OnLeaderboardGet, OnError);
    }

    void OnLeaderboardGet(GetLeaderboardResult r)
    {
        string LeaderboardStr = "Leaderboard\n";
        foreach(var item in r.Leaderboard)
        {
            string row = item.Position + "/" + item.DisplayName + "/" + item.StatValue + "\n";
            LeaderboardStr += row;
        }
        UpdateMsg(LeaderboardStr);
    }

    public void OnButtonSendLeaderboard()
    {
        var req = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate
                {
                    StatisticName="Highscore",
                    Value=int.Parse(currentScore.text)
                }
            }
        };
        UpdateMsg("Submitting Score: " + currentScore.text);
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
        Msg.text = msg;
    }

}
