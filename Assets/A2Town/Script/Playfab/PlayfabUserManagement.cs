using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using PlayFab;
using PlayFab.ClientModels;

using System;


using TMPro;

public class PlayfabUserManagement : MonoBehaviour
{
    [SerializeField] TMP_InputField userEmail, userPassword, userName, currentScore, displayName, emailCfm, passCfm;
    [SerializeField] TMP_Text Msg;

    public void OnButtonRegUser()
    {
        if (emailCfm.text != userEmail.text)
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
        if (userName.text.Contains("@"))
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
        }
        else
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
        //print(Guid.NewGuid().ToString("N").Substring(0,12));

    }

    public void SetPlayfabCache()
    {
        PlayFabClientAPI.GetPlayerProfile(new GetPlayerProfileRequest()
        {
            
        }, r => {
            if(r.PlayerProfile != null && r.PlayerProfile.DisplayName != null && r.PlayerProfile.DisplayName.Length > 0)
            {
                PlayfabCache.Instance.DisplayName = r.PlayerProfile.DisplayName;
                PlayfabCache.Instance.PlayfabID = r.PlayerProfile.PlayerId;
            }
            else
            {
                PlayfabCache.Instance.DisplayName = "Guest " + SystemInfo.deviceUniqueIdentifier;
                PlayfabCache.Instance.PlayfabID = SystemInfo.deviceUniqueIdentifier;
            }
            SceneManager.LoadScene("Game");
            MenuManager.Instance.GetBlackScreenObject().GetComponent<AlphaFading>().StopAllCoroutines();
            MenuManager.Instance.GetBlackScreenObject().GetComponent<AlphaFading>().FadeOut(0.7f);
        },
OnError);
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
        UpdateMsg("Registration Successful");

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
        UpdateMsg("Successful Login!");
        SetPlayfabCache(); //Change to lobby UI is set after cache is updated
        MenuManager.Instance.GetBlackScreenObject().GetComponent<AlphaFading>().FadeIn(0.4f);
        MenuManager.Instance.CloseMenu("LoginUI");

        //Menu[] parentMenu = MenuManager.Instance.GetMenuParents("PlayfabLoginScreenUI");

        //for (int i = 0; i < parentMenu.Length; ++i)
        //{
        //    MenuManager.Instance.CloseMenu(parentMenu[i]);
        //}

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
            MaxResultsCount = 10,

        };
        PlayFabClientAPI.GetLeaderboard(lb_req, OnLeaderboardGet, OnError);
    }

    void OnLeaderboardGet(GetLeaderboardResult r)
    {
        string LeaderboardStr = "Leaderboard\n";
        foreach (var item in r.Leaderboard)
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
    public void OnError(PlayFabError e)
    {
        UpdateMsg(e.GenerateErrorReport().Split(": ")[1]);
    }

    void UpdateMsg(string msg)
    {
        Debug.Log(msg);
        Msg.text = msg;
    }

    //Utils
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
                
                if (r.Data != null)
                {
                    if (r.Data.ContainsKey("XP")) xp = Convert.ToInt32(r.Data["XP"].Value);
                    if (r.Data.ContainsKey("Lvl")) lvl = Convert.ToInt32(r.Data["Lvl"].Value);
                }

                xp += toAdd;
                while (xp > PlayfabLeveling.getXPRequiredForLevel(lvl + 1))
                {
                    xp -= PlayfabLeveling.getXPRequiredForLevel(lvl + 1);
                    lvl += 1;
                    print("Level up");
                }
                Dictionary<string, string> data = new Dictionary<string, string>();
                data.Add("XP", xp.ToString());
                data.Add("Lvl", lvl.ToString());
                print("new lvl " + lvl + " new xp " + xp);
                updateUserData(data);
                updateLeaderboard("Playerlevel", lvl);
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

    public void Logout()
    {
        PlayFabClientAPI.ForgetAllCredentials();
        //LobbyManager.Instance.switchState(LobbyManager.LOBBYSTATE.LOGIN);
    }

}
