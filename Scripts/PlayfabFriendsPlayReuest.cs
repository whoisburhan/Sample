using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;

public class PlayfabFriendsPlayReuest : MonoBehaviour
{
    public static PlayfabFriendsPlayReuest Instance { get; private set; }

    private float gameRequestCheckTimeInterval = 20f;
    private float gameRequestCheckTimer = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    private void Update()
    {
        gameRequestCheckTimer -= Time.deltaTime;

        if (PlayFabClientAPI.IsClientLoggedIn() && gameRequestCheckTimer<=0)
        {
            // Do stuff

            gameRequestCheckTimer = gameRequestCheckTimeInterval;
        }
    }


}
