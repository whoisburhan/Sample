using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFabKit;
using PlayFab.ClientModels;
using PlayFab;
using System;

public class HazariPlayfabAuthentication : PlayfabAuthentication
{
    public static HazariPlayfabAuthentication Instance { get; private set; }

    public static event Action OnLogInWithCustomIdEvent;

    public static event Action OnLogInFailedEvent;

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
    protected override void LogInToPlayfab()
    {
        //base.LogInToPlayfab();
        if (PlayerPrefs.HasKey(PlayfabConstants.FacebookTokenKey))
        {
            if (FacebookLogInByButton.Instance != null) FacebookLogInByButton.Instance.FaceBookLogInByButtonFunc();
            else StartCoroutine(WaitForFaceBookInstanceLoad());
        }
        else
            LogInWithCustomID();
    }

    private IEnumerator WaitForFaceBookInstanceLoad()
    {
        yield return new WaitForSeconds(1.5f);
        if (FacebookLogInByButton.Instance != null) FacebookLogInByButton.Instance.FaceBookLogInByButtonFunc();
    }

    protected override void OnLogInWithCustomIDSuccess(LoginResult obj)
    {
        base.OnLogInWithCustomIDSuccess(obj);

        Debug.Log("123 CustomID Success");

        PlayfabConstants.Instance.MyPlayfabID = obj.PlayFabId;

        OnLogInWithCustomIdEvent?.Invoke();

        SetUpPushNotification();
    }

    protected override void OnLogInWithCustomIDFailed(PlayFabError obj)
    {
        base.OnLogInWithCustomIDFailed(obj);

        Debug.Log("123 CustomID failed :" + obj.ErrorMessage);
        OnLogInFailedEvent?.Invoke();
    }

    protected override void OnLogInWithFacebookSuccess(LoginResult obj)
    {
        base.OnLogInWithFacebookSuccess(obj);
        Debug.Log("123 FB Success");

        int _currentSelected = PlayerPrefs.GetInt("currentSelectedLanguageNo");
       // PlayerPrefs.DeleteAll();
        PlayerPrefs.SetInt("currentSelectedLanguageNo", _currentSelected);

        PlayfabPlayerProfile.GetPlayerData(new List<string> { "SignUPRewardGiven" }, PlayerSignInInfo);

        PlayfabConstants.Instance.MyPlayfabID = obj.PlayFabId;

        SetUpPushNotification();
    }

    protected override void OnLogInWithFacebookFailed(PlayFabError obj)
    {
        base.OnLogInWithFacebookFailed(obj);
        Debug.Log("123 FB Failed : " + obj.ErrorMessage);
        OnLogInFailedEvent?.Invoke();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="obj"></param>
    private void PlayerSignInInfo(GetUserDataResult obj)
    {
        if (!obj.Data.ContainsKey("SignUPRewardGiven"))
        {
            // First time log in
            Debug.Log("123 First Time Login");
            FacebookLogInByButton.Instance.FirstTimeLogInActivity();
        }
        else
        {
            // Normal Log in
            Debug.Log("123 Normal Time Login");
            FacebookLogInByButton.Instance.NormalLogInActivity();
        }
    }

    private void SetUpPushNotification()
    {
        if (PlayFabClientAPI.IsClientLoggedIn() && !string.IsNullOrEmpty(PlayfabConstants.Instance.MyPlayfabID))
        {
            if (string.IsNullOrEmpty(GameInvitationReceiver.Instance.FirebaseToken) || string.IsNullOrEmpty(PlayfabConstants.Instance.MyPlayfabID))
                return;

#if UNITY_ANDROID
            var request = new AndroidDevicePushNotificationRegistrationRequest
            {
                DeviceToken = GameInvitationReceiver.Instance.FirebaseToken,
                SendPushNotificationConfirmation = true,
                ConfirmationMessage = "Push notifications registered successfully"
            };
            PlayFabClientAPI.AndroidDevicePushNotificationRegistration(request, OnPfAndroidReg => { Debug.Log("PUSH NOTIFICATION SET UP SUCCEDED"); },
                OnPfFail => { Debug.Log("PUSH NOTIFICATION SET UP FAILED :" + OnPfFail.ErrorMessage); });
#endif
        }
    }
}
