using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFabKit;
using Facebook.Unity;
using PlayFab;
using PlayFab.ClientModels;
using LoginResult = PlayFab.ClientModels.LoginResult;
using System;
using TAAASH_KIT;

public class FacebookLogInByButton : FacebookLogIn
{
    public static FacebookLogInByButton Instance { get; private set; }

    public static event Action OnLogInWithFaceBookEvent;

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


    /// <summary>
    /// 
    /// </summary>
    /// <param name="result"></param>
    protected override void FacebookData(IGraphResult result)
    {
        base.FacebookData(result);

        if (!PlayerPrefs.HasKey(PlayfabConstants.FacebookTokenKey))
        {
            // Try to link fb account to PlayfabID
            if (PlayFabClientAPI.IsClientLoggedIn() && Application.internetReachability != NetworkReachability.NotReachable)
            {
                PlayFabClientAPI.LinkFacebookAccount(new LinkFacebookAccountRequest { AccessToken = fbToken },
                    OnLinkFacebookSuccessful,
                    (op) =>         //Already Linked this fb id with another account
                    {
                        PlayFabClientAPI.ForgetAllCredentials();

                        HazariPlayfabAuthentication.Instance.LogInWithFacebookID(fbToken);
                    });
            }
        }
        else
        {
            HazariPlayfabAuthentication.Instance.LogInWithFacebookID(fbToken);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="obj"></param>
    private void OnLinkFacebookSuccessful(LinkFacebookAccountResult obj)
    {
        FirstTimeLogInActivity();
    }

    /// <summary>
    /// 
    /// </summary>
    public void FirstTimeLogInActivity()
    {
            PlayfabConstants.Instance.FacebookToken = fbToken;

            PlayfabPlayerProfile.SetPlayerName(fbName);

            PlayfabPlayerProfile.SetAvatarURL(fbAvatarURL);

            fbDataDic["SignUPRewardGiven"] = "1";

            PlayfabPlayerProfile.SetPlayerData(fbDataDic, () =>
            {
                if (CoinSystem.instance != null) CoinSystem.instance.AddCoins(5000);

            });

        HazariPlayfabAuthentication.Instance.UnlinkCustomID();
        Debug.Log("123 FirstTimeLogIn");
        OnLogInWithFaceBookEvent?.Invoke();
    }

    /// <summary>
    /// 
    /// </summary>
    public void NormalLogInActivity()
    {
        PlayfabPlayerProfile.GetPlayerData(new List<string> { "FB ID" }, NormalLogInActivitySubFunction);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="obj"></param>
    private void NormalLogInActivitySubFunction(GetUserDataResult obj)
    {
        if(obj.Data.ContainsKey("FB ID"))
        {
            Debug.Log("123 FB ID");
            if (obj.Data["FB ID"].Value == fbId)
            {
                PlayfabPlayerProfile.SetAvatarURL(fbAvatarURL);

                PlayfabConstants.Instance.FacebookToken = fbToken;
            }
        }

        OnLogInWithFaceBookEvent?.Invoke();
    }
}