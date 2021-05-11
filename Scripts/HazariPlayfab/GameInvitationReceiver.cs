using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using PlayFabKit;
using System;
using PlayFab.ServerModels;
using System.Globalization;
using Firebase.Messaging;
using PlayFab.Json;

public class GameInvitationReceiver : MonoBehaviour
{
    public static GameInvitationReceiver Instance { get; private set; }

    public bool CheckForInvitation = true;
    [HideInInspector] public string FirebaseToken { get; private set; }

    private float gameInvitationCheckerTimer = 0;
    private float gameInvitationCheckTimeInterval = 5f;

    [SerializeField] private GameObject GameInvitationUI;



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

    private void Start()
    {
        gameInvitationCheckerTimer = gameInvitationCheckTimeInterval;

        Firebase.Messaging.FirebaseMessaging.TokenReceived += OnTokenReceived;
        Firebase.Messaging.FirebaseMessaging.MessageReceived += OnMessageReceived;

    }

    private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
    {
        if (e.Message.Data != null)
        {
            string test1 = PlayFabSimpleJson.SerializeObject(e.Message.Data);

            StartCoroutine(PlayfabLogInDelay(e.Message));

            /*
            string _dataTest = "[";
            foreach (var pair in e.Message.Data)
            {
                _dataTest += "{" + pair.Key + ":" + pair.Value + "}"; 
                Debug.Log("PlayFab data element: " + pair.Key + "," + pair.Value);
            }
            _dataTest += "]";

            ToastNotification.instance.Show(_dataTest);*/
        }

    }

    IEnumerator PlayfabLogInDelay(FirebaseMessage result)
    {
        yield return new WaitUntil(() => PlayFabClientAPI.IsClientLoggedIn());
        GetData(result);
    }

    private void OnTokenReceived(object sender, TokenReceivedEventArgs e)
    {
        FirebaseToken = e.Token;
        //ToastNotification.instance.Show("Token Received");
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
            PlayFabClientAPI.AndroidDevicePushNotificationRegistration(request, OnPfAndroidReg=> {  Debug.Log("PUSH NOTIFICATION SET UP SUCCEDED"); }, 
                OnPfFail => { Debug.Log("PUSH NOTIFICATION SET UP FAILED :" + OnPfFail.ErrorMessage); });
#endif
        }

    }

    private void Update()
    {
       /* if (CheckForInvitation)
        {
            if (gameInvitationCheckerTimer <= 0 && PlayFabClientAPI.IsClientLoggedIn() && (PlayfabConstants.Instance.PlayerCurrentActivityState == 0 || PlayfabConstants.Instance.PlayerCurrentActivityState == 4 || PlayfabConstants.Instance.PlayerCurrentActivityState == 5))
            {
                Debug.Log("789 Checking for Inivitation");

                PlayfabPlayerProfile.GetPlayerReadOnlyData(new List<string> { "GROUP_ID", "GAME REQUEST", "REQUEST TIME:", "CoinAmount", "GameType", "FourDigitRandomNumber" }, GetData1=> { }, () =>
                {
                    Debug.Log("789 Failed to get Data");
                });

                gameInvitationCheckerTimer = gameInvitationCheckTimeInterval;

            }
        }
        gameInvitationCheckerTimer -= Time.deltaTime;
        */
    }

    private void GetData(FirebaseMessage result/*PlayFab.ClientModels.GetUserDataResult result*/)
    {
        if (CheckForInvitation)
        {
            if (result.Data.ContainsKey("REQUEST TIME"))
            {
                Debug.Log("789  get Data Successfully");

                // IFormatProvider.
                CultureInfo provider = CultureInfo.InvariantCulture;

                // DateTime _invitationSendingTime = DateTime.Parse(result.Data["REQUEST TIME:"].Value);
                DateTime _invitationSendingTime = new DateTime();

                if (DateTime.TryParseExact(result.Data["REQUEST TIME"], "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out _invitationSendingTime))
                {
                    Debug.Log("000 Invitation Time:" + _invitationSendingTime);

                    PlayFabServerAPI.GetTime(new PlayFab.ServerModels.GetTimeRequest { }, OnGetTimeSuccess =>
                    {
                        if ((OnGetTimeSuccess.Time - _invitationSendingTime).TotalMinutes <= 5)
                        {
                            Debug.Log("789  less then 5 min");
                            GameInvitationUI.SetActive(true);
                            GameInvitationUI.GetComponent<GameInvitationUI>().SetGameInvitationInfo(result);
                            CheckForInvitation = false;
                        }
                        else
                        {
                            Debug.Log("789  more then 5 min");
                            GameInvitationUI.SetActive(false);
                        }
                    }, LogFailure => { });
                }
                
            }
           
        }
       
    }

    private void OnGetTimeSuccess(PlayFab.ServerModels.GetTimeResult obj)
    {
        throw new NotImplementedException();
    }
}
