using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PlayFab;
using PlayFab.ClientModels;

using UnityEngine.SceneManagement;

public class MenuSceneMangement : MonoBehaviour
{
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if(PlayFabClientAPI.IsClientLoggedIn()) //If logged in, dont send to login screen which is turned on by default
        {
            GameObject.Find("Menus").GetComponent<MenuManager>().OpenMenu("StartScreen");
            GameObject.Find("Menus").GetComponent<MenuManager>().getMenu("StartScreen").GetComponentInChildren<LobbyStats>().refreshStatistics();
        }
    }
}
