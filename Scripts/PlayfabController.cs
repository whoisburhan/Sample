using Facebook.Unity;
using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LoginResult = PlayFab.ClientModels.LoginResult;
using Lobby;
using UnityEngine.SceneManagement;
//using Firebase.Crashlytics;


//using GooglePlayGames;
//using GooglePlayGames.BasicApi;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections;
//using System.Data.SqlClient;
using TAAASH_KIT;

public class PlayfabController : MonoBehaviour
{
    #region Attributes

    public static PlayfabController Instance { get; private set; }

    public Sprite MySprite;

    [SerializeField] private Image m_profilePic;
    [SerializeField] private Text m_userName;

    private string fb_token = "";
    private string playerEmail = "";
    private string playerProfilePic = "";
    private string playerID = "";

    public string PlayerName;
    public string PlayFabID;
    public int profilePicIndex = 0;
    [HideInInspector] public string pictureUrl;

    [HideInInspector] public bool isFacebookLogggedIn = false;
    [HideInInspector] public bool isGoogleLoggedIn = false;
    [HideInInspector] public bool isLoggedIn = false;
    [HideInInspector] public bool RetryLogin = false;
    private bool isAuthenticationOperationNotCompletedYet = false;
    bool firstTimeFBLoggedIn = false;
    List<string> permissions = new List<string>() { "public_profile", "email", "user_friends" };

    public static event Action OnLoggedInEvent;
    public static event Action OnLoggedInFailedEvent;
    public static event Action OnDataResetEvent;
    public static event Action UpdateUI;
    public static event Action OnFaceBookLogIn;

    #region All PlayerPref
    public const string PLAYER_NAME = "PlayerName";
    public const string PROFILE_PIC_INDEX = "ProfilePicIndex";
    public const string PROFILE_PIC_URL = "ProfilePicUrl";

    #endregion


    #endregion


    // holds the latest message to be displayed on the screen
    private string _message;

    private bool
        dataResetFlag; // check if the fb id logged in with other players acount | if yes , then make it true and reset all the data and load from fb logged in player acount

    private bool showProfileUpdate = false;
    private bool showFailedToLogin = false;

    private float activityStatusTimer = 0;
    private float activityStatusCheckIntervel = 10f;

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


        #region PlayerInfo

        profilePicIndex = PlayerPrefs.GetInt(PROFILE_PIC_INDEX, 0);
        PlayerName = PlayerPrefs.GetString(PLAYER_NAME, "Guest" + UnityEngine.Random.Range(1000, 9999).ToString());
        pictureUrl = PlayerPrefs.GetString(PROFILE_PIC_URL, "");

        #endregion


    }

    public void Start()
    {
        // OnGoogleInitialize();
        //activityStatusTimer = activityStatusCheckIntervel



    }

    IEnumerator waitForFB()
    {
        yield return new WaitForSeconds(2f);
        FaceBookLogIn();
    }

    #region FaceBook Login

    //Button
    public void FaceBookLogIn()
    {
        //SetMessage("Initializing Facebook..."); // logs the given message and displays it on the screen using OnGUI method
        //        Crashlytics.Log("PlayfabController.cs|FaceBookLogIN() : Start Initializing Facebook");

        // This call is required before any other calls to the Facebook API. We pass in the callback to be invoked once initialization is finished
        
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            ToastNotification.instance.Show("No Internet Connection Available.", "NOINTERNETAVAILABLE");
        }
        else
        {
            if (!FB.IsInitialized)
            {
                Debug.Log("FB NOT Initialized");
                FB.Init(OnFacebookInitialized);
            }
            else
            {
                Debug.Log("FB Is Initialized already");
                OnFacebookInitialized();
            }
        }
        


    }

    private void OnFacebookInitialized()
    {
        Debug.Log("|FB|");
        //        Crashlytics.Log("PlayfabController.cs|OnFacebookInitialized() : Logging into Facebook...");
        // SetMessage("Logging into Facebook...");
        FB.ActivateApp();
        // Once Facebook SDK is initialized, if we are logged in, we log out to demonstrate the entire authentication cycle.
        if (FB.IsLoggedIn)
        {
            //Crashlytics.Log("PlayfabController.cs|OnFacebookInitialized() : ALL READY LOGGED IN");
            Debug.Log("|FB| ALL READY LOGGED IN");
            //FB.LogOut();
            //ReadFBData(FB.IsLoggedIn);
        }


        // We invoke basic login procedure and pass in the callback to process the result
        if (FB.IsLoggedIn && AccessToken.CurrentAccessToken != null)
        {
            //Crashlytics.Log("PlayfabController.cs|OnFacebookInitialized() : Already logged in to facebook!");
            Debug.Log("|FB| Already logged in to facebook!");
        }
        else
        {
            FB.LogInWithReadPermissions(permissions, OnFacebookLoggedIn);
        }
    }


    private void OnFacebookLoggedIn(ILoginResult result)
    {
        // If result has no errors, it means we have authenticated in Facebook successfully
        if (!result.Cancelled && string.IsNullOrEmpty(result.Error))
        {
            //SetMessage("Facebook Auth Complete! Access Token: " + AccessToken.CurrentAccessToken.TokenString + "\nLogging into PlayFab...");
            fb_token = AccessToken.CurrentAccessToken.TokenString;
            
            // Crashlytics.Log("PlayfabController.cs|OnFacebookLoggedIn(ILoginResult result) : Trying to link facebook id with playfabID.......");
            PlayFabClientAPI.LinkFacebookAccount(new LinkFacebookAccountRequest { AccessToken = fb_token },
                OnLinkFacebookSuccessful,
                (op) =>
                {
                    //ToastNotification.instance.Show("Failed To Login", "LOGINFAILED");
                    //Crashlytics.Log("PlayfabController.cs|OnFacebookLoggedIn(ILoginResult result) ->  PlayFabClientAPI.LinkFacebookAccount():Failed to link!!! This FB ID is Already linked with another Playfab ID");

                    Debug.Log(op.HttpCode + " " + op.ErrorMessage + " | " + op.ErrorDetails + " | " + op.ApiEndpoint +
                              "|" + op.Error);
                    Debug.Log("Trying to log in linked Account .........");

                    firstTimeFBLoggedIn = true;

                    //  Crashlytics.Log("PlayfabController.cs|OnFacebookLoggedIn(ILoginResult result) : Trying to logged IN playfabID associated with this fb id");
                    // dataResetFlag = true;
                    //  Crashlytics.Log("PlayfabController.cs|OnFacebookLoggedIn(ILoginResult result) : Logged Out from currently logged In Custom Acount ID..........");
                    PlayFabClientAPI.ForgetAllCredentials();
                    Debug.Log("PlayFabClientAPI.IsClientLoggedIn() :" + PlayFabClientAPI.IsClientLoggedIn());
                    StartCoroutine(TimeGap(1.2f));
                    //LogInWithFacebookID(fb_token);
                });

            /* PlayFabClientAPI.LoginWithFacebook(new LoginWithFacebookRequest { CreateAccount = true, AccessToken = fb_token },
                 OnPlayfabAuthComplete, OnError); */
            Debug.Log("||| IS LOGGED IN : " + PlayFabClientAPI.IsClientLoggedIn());
        }
        else
        {
            // If Facebook authentication failed, we stop the cycle with the message
            //  SetMessage("Facebook Auth Failed: " + result.Error + "\n" + result.RawResult, true);
            Debug.Log("Facebook Auth Failed: " + result.Error + "\n" + result.RawResult);
            ToastNotification.instance.Show("Failed To Login", "LOGINFAILED");
            //Crashlytics.Log("PlayfabController.cs|OnFacebookLoggedIn(ILoginResult result) : " + "Facebook Auth Failed: " + result.Error + "\n" + result.RawResult);
        }
    }

    private void OnLinkFacebookSuccessful(LinkFacebookAccountResult obj)
    {
        if (CoinSystem.instance != null)
        {
            CoinSystem.instance.AddCoins(5000);
        }
        else
        {
            StartCoroutine(nameof(LateAddCoin));
        }
            
        
        SaveFirstTimeFBLogINReward();
        
        if(ToastNotification.instance != null)
            ToastNotification.instance.Show("Login Successful!!", "LOGINSUCCESSFUL");

        PlayerPrefs.SetString("FB_TOKEN", fb_token);

        ReadFBData(FB.IsLoggedIn);

        //Unlink customID

        PlayFabClientAPI.UnlinkCustomID(new UnlinkCustomIDRequest { },
            OnUnlinkSucess =>
            {
                Debug.Log("Successfully Unlink custom ID");
            },
            OnUnlinkError =>
            {
                Debug.Log("Failed to Unlink custom ID");
            });

        GetAllVirtualCurrency(() =>
        {
            if(LobbyUIManager.instance != null)
                LobbyUIManager.instance.CoinTextUpdateFromServer();
            else
            {
                StartCoroutine(nameof(LateUpdateCoinText));
            }
        });
    }

    IEnumerator LateAddCoin()
    {
        yield return new WaitForSeconds(2f);
        if(CoinSystem.instance != null)
            CoinSystem.instance.AddCoins(5000);
    }
    
    IEnumerator LateUpdateCoinText()
    {
        yield return new WaitForSeconds(2.5f);
        if(LobbyUIManager.instance != null)
            LobbyUIManager.instance.CoinTextUpdateFromServer();
    }


    //------------------------------------------------------
    private void ReadFBData(bool _isLoggedIn, bool _isFirstTimeLogIn = false)
    {
        //Debug.Log("||FB|| : " + _isLoggedIn);
        if (_isLoggedIn)
        {
            // FB.API("/me?fields=id,name,email", HttpMethod.GET, DisplayUsername, new Dictionary<string, string>() { });
            // FB.API("/me/picture.?redirect=false", HttpMethod.GET, DisplayProfilePic);

            FB.API("me?fields=id,name,email,picture.width(256).height(256)", HttpMethod.GET, _graphResult =>
            {
                if (_graphResult.Error != null) return;
                if (!PlayerPrefs.HasKey("UpdatedDisplayNameFromFBBefore"))
                {
                    try
                    {
                        PlayerName = _graphResult.ResultDictionary["name"].ToString();
                        if (PlayerName.Length > 24) PlayerName = PlayerName.Substring(0, 24);
                        PlayerPrefs.SetString("PlayerName", PlayerName);
                        UpdateDisplayName(PlayerName);
                        PlayerPrefs.SetInt("UpdatedDisplayNameFromFBBefore", 1);
                    }
                    catch (Exception e)
                    {
                        //Crashlytics.Log("PlayfabController.ReadFBData() :" + e.Message);
                        //Crashlytics.LogException(e);
                        Debug.Log(e.Message);
                    }
                }

                try
                {
                    playerEmail = _graphResult.ResultDictionary["email"].ToString();
                }
                catch (Exception e)
                {
                    //Crashlytics.Log("PlayfabController.ReadFBData() :" + e.Message);
                    //Crashlytics.LogException(e);
                    Debug.Log(e.Message);
                    Debug.Log("Something wrong on getting Email Address from facebook");
                }

                try
                {
                    playerID = _graphResult.ResultDictionary["id"].ToString();
                }
                catch (Exception e)
                {
                    //Crashlytics.Log("PlayfabController.ReadFBData() :" + e.Message);
                    //Crashlytics.LogException(e);
                    Debug.Log(e.Message);
                    //Debug.Log(e.Message);
                }

                try
                {
                    pictureUrl =
                        ((Dictionary<string, object>)((Dictionary<string, object>)_graphResult.ResultDictionary[
                            "picture"])["data"])["url"].ToString();

                    PlayerPrefs.SetString(PROFILE_PIC_URL, pictureUrl);

                    Debug.Log("<><> " + pictureUrl);
                    UpdateProfilePic(pictureUrl,()=> {
                        After();
                    });
                }
                catch (Exception e)
                {
                    //Crashlytics.Log("PlayfabController.ReadFBData() :" + e.Message);
                    //Crashlytics.LogException(e);
                    Debug.Log(e.Message);
                    Debug.Log(e.Message);
                }

                profilePicIndex = 0;
                UpdatePlayerProfilePicIndex(profilePicIndex.ToString());

                SavePlayerInfo();
                OnFaceBookLogIn?.Invoke();
              //  UpdateUI?.Invoke();

                /*
                GetPlayerInfo(op =>
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(op.Data["ProfilePicIndex"].Value))
                        {
                            profilePicIndex = Int32.Parse(op.Data["ProfilePicIndex"].Value);
                            PlayerPrefs.SetInt("ProfilePicIndex", profilePicIndex);
                        }

                        SavePlayerInfo();
                    }
                    catch (Exception e)
                    {
                        //                        Crashlytics.Log("PlayfabController.ReadFBData() :" + e.Message);
                        //                        Crashlytics.LogException(e);
                        Debug.LogError(e.Message);
                        Debug.LogError("GetPlayer Info got some problem on profile pic index may be not found!!!!");
                    }
                }); */
            });
        }
        else
        {
            Debug.Log("FB IS NOT LOGGED IN");
        }
    }

    // Depricated Function (Might be need to use in later)
    private void DisplayUsername(IGraphResult result)
    {
        if (result.Error == null)
        {
            var dic = result.ResultDictionary;
            foreach (var d in dic)
            {
                Debug.Log(d.Key);
            }

            string name = "" + result.ResultDictionary["name"];
            // m_userName.text = name;
            PlayerName = name;
            playerEmail = "" + result.ResultDictionary["email"];
            playerID = "" + result.ResultDictionary["id"];

         //   SavePlayerInfo();
            UpdateDisplayName(PlayerName);
            PlayerPrefs.SetString(PLAYER_NAME, PlayerName);

         /*   GetPlayerInfo(op =>
            {
                if (!string.IsNullOrEmpty(op.Data["ProfilePicIndex"].Value))
                {
                    profilePicIndex = Int32.Parse(op.Data["ProfilePicIndex"].Value);
                    PlayerPrefs.SetInt(PROFILE_PIC_INDEX, profilePicIndex);
                }

                SavePlayerInfo();
            });
            */
        }
        else
        {
            Debug.Log(result.Error);
        }
    }

    // Depricated Function (Might be need later)
    private void DisplayProfilePic(IGraphResult result)
    {
        Debug.Log("||| IS LOGGED IN : " + PlayFabClientAPI.IsClientLoggedIn());
        Debug.Log("NOW DISPLAY ...............");
        if (string.IsNullOrEmpty(result.Error) && !result.Cancelled)
        {
            Debug.Log("NOW DISPLAY ...............");
            IDictionary data = result.ResultDictionary["data"] as IDictionary;
            string photoURL = data["url"] as string;


            UpdateProfilePic(photoURL);
        }
        /*  if (result.Texture != null)
          {
              Debug.Log("Profile Pic Loading.....");
              TextureToString(result.Texture);
             // m_profilePic.sprite = Sprite.Create(result.Texture, new Rect(0, 0, 128, 128), new Vector2());
              SavePlayerInfo();
              UpdateDisplayName(playerName);
              UpdateProfilePicIndex("0");
              
              //mySprite = Texture2DToSprite(StringToTexture(playerProfilePic));
          }*/
        else
        {
            Debug.Log(result.Error);
        }
    }

    //------------------------------------------------------

    #endregion

    private void Update()
    {   /*
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            PlayfabController.Instance.RetryLogin = true;
        }

        if (Application.internetReachability != NetworkReachability.NotReachable &&
            !PlayFabClientAPI.IsClientLoggedIn() && Instance != null && Instance.RetryLogin)
        {
            if (!isAuthenticationOperationNotCompletedYet)
            {
                isAuthenticationOperationNotCompletedYet = true;
                if (!PlayerPrefs.HasKey("FB_TOKEN"))
                {
                    LogInWithCustomID();
                }
                else
                {
                    // Log In With Facebook
                    //Debug.Log("LOGIN WITH FACEBOOK");
                    fb_token = PlayerPrefs.GetString("FB_TOKEN");
                    LogInWithFacebookID(fb_token);
                }
            }
        }

        if(Application.internetReachability != NetworkReachability.NotReachable && PlayFabClientAPI.IsClientLoggedIn())
        {
            if(activityStatusTimer < 0)
            {
                UpdatePlayerProfilePicIndex(profilePicIndex.ToString());
                activityStatusTimer = activityStatusCheckIntervel;
            }
        }

        activityStatusTimer -= Time.deltaTime;
        */
    }

    #region GOOGLE LogIn

    /* private void OnGoogleInitialize()
     {
         // The following grants profile access to the Google Play Games SDK.
         // Note: If you also want to capture the player's Google email, be sure to add
         // .RequestEmail() to the PlayGamesClientConfiguration
         PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder()
         .AddOauthScope("profile")
         .RequestServerAuthCode(false)
         .Build();
         PlayGamesPlatform.InitializeInstance(config);

         // recommended for debugging:
         PlayGamesPlatform.DebugLogEnabled = true;

         // Activate the Google Play Games platform
         PlayGamesPlatform.Activate();
     }

     public void GoogleLogIn()
     {
         Social.localUser.Authenticate((bool success) => {

             if (success)
             {

                 SetMessage("Google Signed In");
                 var serverAuthCode = PlayGamesPlatform.Instance.GetServerAuthCode();
                 Debug.Log("Server Auth Code: " + serverAuthCode);

                 PlayFabClientAPI.LoginWithGoogleAccount(new LoginWithGoogleAccountRequest()
                 {
                     TitleId = PlayFabSettings.TitleId,
                     ServerAuthCode = serverAuthCode,
                     CreateAccount = true
                 }, (result) =>
                 {
                     SetMessage("Signed In as " + result.PlayFabId);

                 }, OnError);
             }
             else
             {
                 SetMessage("Google Failed to Authorize your login");
             }

         });
     }

     */

    #endregion

    #region PlayFeb

    #region Login With CustomID

    private void LogInWithCustomID()
    {
        var request = new LoginWithCustomIDRequest
        {
            CustomId = SystemInfo.deviceUniqueIdentifier,
            CreateAccount = true
        };
        PlayFabClientAPI.LoginWithCustomID(request, OnLogInWithCustomID,
            (op) =>
            {
                isAuthenticationOperationNotCompletedYet = false;
                RetryLogin = true;
                OnLoggedInFailedEvent?.Invoke();
                OnError(op);
            });
    }

    private void OnLogInWithCustomID(LoginResult obj)
    {
        //        Crashlytics.Log("PlayfabController.cs|OnLogInWithCustomID(LoginResult obj) : Logged In with custom ID........");

        isAuthenticationOperationNotCompletedYet = false;
        RetryLogin = false;
        UpdatePlayerProfilePicIndex(profilePicIndex.ToString());

        //        Crashlytics.Log("PlayfabController.cs|OnLogInWithCustomID(LoginResult obj) : Get Player PlayfabID");
        GetPlayFabID();
        GetAllVirtualCurrency(() => { LobbyUIManager.instance.CoinTextUpdateFromServer(); });

       
        OnLoggedInEvent?.Invoke();


        isLoggedIn = true;
    //    SavePlayerInfo();
        Debug.Log("||| YES YES YEs OnLogInWithCustomID()");

        //        Crashlytics.Log("PlayfabController.cs|OnLogInWithCustomID(LoginResult obj) ->  GetPlayerProfile() : Trying to get player Info from Server........");
        GetPlayerProfile(op =>
        {
            //            Crashlytics.Log("PlayfabController.cs|OnLogInWithCustomID(LoginResult obj) ->  GetPlayerProfile() : Successfullly get player data!!");
            if (op.PlayerProfile.DisplayName == null || op.PlayerProfile.DisplayName == "")
            {
                UpdateDisplayName(PlayerName);
            }

            else
            {
                PlayerName = op.PlayerProfile.DisplayName;
                PlayerPrefs.SetString(PLAYER_NAME, PlayerName);
            }

            if(op.PlayerProfile.ContactEmailAddresses[0].EmailAddress == null || op.PlayerProfile.ContactEmailAddresses[0].EmailAddress == "")
            {
                profilePicIndex = 0;
                UpdatePlayerProfilePicIndex(profilePicIndex.ToString());
            }
            else
            {
                var _str = op.PlayerProfile.ContactEmailAddresses[0].EmailAddress.Split('|');
                profilePicIndex = Int32.Parse(_str[0]);
                PlayerPrefs.SetInt("ProfilePicIndex", profilePicIndex);
            }
        });

        UpdateUI?.Invoke();
        //        Crashlytics.Log("PlayfabController.cs|OnLogInWithCustomID(LoginResult obj) ->  GetPlayerInfo() : Trying to get player database Info from Server like profile pic index........");
        /*   GetPlayerInfo(op =>
           {

               try
               {
                   if (!string.IsNullOrEmpty(op.Data["ProfilePicIndex"].Value))
                   {
                       //Crashlytics.Log("PlayfabController.cs|OnLogInWithCustomID(LoginResult obj) ->  GetPlayerInfo() : profile pic index get successfully!!!");
                       profilePicIndex = Int32.Parse(op.Data["ProfilePicIndex"].Value);
                       PlayerPrefs.SetInt("ProfilePicIndex", profilePicIndex);
                   }
               }
               catch (Exception e)
               {
                   //                Crashlytics.Log("PlayfabController.ReadFBData() :" + e.Message);
                   //                Crashlytics.LogException(e);
                   //Debug.LogError(e.Message);
                   //Debug.Log("op.Data[\"ProfilePicIndex\"] Is Not found");
               }

               try
               {
                   if (!string.IsNullOrEmpty(op.Data["FB_ID"].Value))
                   {
                       playerID = op.Data["FB_ID"].Value;
                   }
               }
               catch (Exception e)
               {
                   //                Crashlytics.Log("PlayfabController.ReadFBData() :" + e.Message);
                   //                Crashlytics.LogException(e);
                   //Debug.LogError(e.Message);
                   //Debug.Log("(op.Data[\"FB_ID\"] Is Not Found");
               }

               try
               {
                   if (!string.IsNullOrEmpty(op.Data["Name"].Value))
                   {
                       PlayerName = op.Data["Name"].Value;
                   }
               }
               catch (Exception e)
               {
                   //                Crashlytics.Log("PlayfabController.ReadFBData() :" + e.Message);
                   //                Crashlytics.LogException(e);
                   //Debug.LogError(e.Message);
                   //Debug.Log("op.Data[\"Name\"] Is Not Found");
               }

               try
               {
                   if (!string.IsNullOrEmpty(op.Data["Email"].Value))
                   {
                       playerEmail = op.Data["Email"].Value;
                   }
               }
               catch (Exception e)
               {
                   //                Crashlytics.Log("PlayfabController.ReadFBData() :" + e.Message);
                   //                Crashlytics.LogException(e);
                   //Debug.LogError(e.Message);
                   //Debug.Log("op.Data[\"Email\"] Is Not Found");
               }

               SavePlayerInfo();
           });  */
    }

    #endregion

    #region Login With Facebook ID

    private void LogInWithFacebookID(string _token, bool _logInFromStartFunc = false)
    {
        //Crashlytics.Log("PlayfabController.cs|LogInWithFacebookID(string _token) : Trying to logged IN with facebook");
        PlayFabClientAPI.LoginWithFacebook(new LoginWithFacebookRequest { CreateAccount = false, AccessToken = _token },
            onSuccess => {
                if (_logInFromStartFunc)
                {
                    StartCoroutine(waitForFB());
                }
                else
                {
                    OnPlayfabAuthComplete(onSuccess);
                }
                    PlayerPrefs.SetString("FB_TOKEN", _token);
            }, (op) =>
            {
                firstTimeFBLoggedIn = false;
                isAuthenticationOperationNotCompletedYet = false;
                RetryLogin = true;
                OnLoggedInFailedEvent?.Invoke();
                if (ToastNotification.instance != null && !showFailedToLogin)
                {
                    ToastNotification.instance.Show("Failed To Login", "LOGINFAILED");
                    showFailedToLogin = true;
                }
                    
                OnError(op);
            });
    }


    private void OnPlayfabAuthComplete(LoginResult result)
    {
        if (firstTimeFBLoggedIn)
        {
            int _currentSelected = PlayerPrefs.GetInt("currentSelectedLanguageNo");
            PlayerPrefs.DeleteAll();
            PlayerPrefs.SetInt("currentSelectedLanguageNo", _currentSelected);
            firstTimeFBLoggedIn = false;
            CheckTittlePlayer();

        }
        //        Crashlytics.Log("PlayfabController.cs|OnPlayfabAuthComplete(LoginResult result) : Playfab Logged in with facebook Successfully!!");
        //        Crashlytics.Log("PlayfabController.cs|OnPlayfabAuthComplete(LoginResult result) : Get Player PlayfabID");
        else
        {
            After();
        }

       // StartCoroutine(After());
    }

    private void After()
    {
        isAuthenticationOperationNotCompletedYet = false;
        GetPlayFabID();

        if (LobbyUIManager.instance != null)
            GetAllVirtualCurrency(() => { LobbyUIManager.instance.CoinTextUpdateFromServer(); });

        PlayerPrefs.SetString("FB_TOKEN", fb_token);
        if (ToastNotification.instance != null && !showProfileUpdate)
        {
            ToastNotification.instance.Show("Profile Updated", "PROFILEUPDATE");
            showProfileUpdate = true;
        }

        ReadFBData(FB.IsLoggedIn, true);
      //  OnFaceBookLogIn?.Invoke();
      //  UpdateUI?.Invoke();


        OnLoggedInEvent?.Invoke();
        RetryLogin = false;
        isLoggedIn = true;
        // ReadFBData(FB.IsLoggedIn);
        //Debug.Log("||| IS LOGGED IN : " + PlayFabClientAPI.IsClientLoggedIn());

        //Crashlytics.Log("PlayfabController.cs|OnPlayfabAuthComplete(LoginResult result) : Trying to unlink custom Game ID.........");

        PlayFabClientAPI.UnlinkCustomID(new UnlinkCustomIDRequest { },
            OnUnlinkSucess =>
            {
                //Crashlytics.Log("PlayfabController.cs|OnPlayfabAuthComplete(LoginResult result) : Successfully unlinked custom Game ID after linked it with facebook");
            },
            OnUnlinkError =>
            {
                //Crashlytics.Log("PlayfabController.cs|OnPlayfabAuthComplete(LoginResult result) : " + OnUnlinkError.ErrorMessage);
            });
    }

    #endregion

    #region Player Details [Name and profilePic index]

    public void GetPlayFabID()
    {
        PlayFabClientAPI.GetAccountInfo(new GetAccountInfoRequest { }, OnSucess =>
        {
            PlayFabID = OnSucess.AccountInfo.PlayFabId;
        }, OnFailed =>
        {
            Debug.LogError("Failed to get PlayfabID");
        });
    }

    public void UpdateDisplayName(string _name)
    {
        PlayFabClientAPI.UpdateUserTitleDisplayName(new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = _name
        },
            OnSucess => {
                Debug.Log("Suceessfully player display name is updated");
                //Crashlytics.Log("PlayfabController.cs|UpdateDisplayName(string _name) : Suceessfully player display name is updated");
                PlayerPrefs.SetString(PLAYER_NAME, _name);
            }
            , OnError);
    }


    public void UpdateProfilePic(string _index, Action _action = null)
    {
        PlayFabClientAPI.UpdateAvatarUrl(new UpdateAvatarUrlRequest
        {
            ImageUrl = _index
        },
            OnSucess => {
                Debug.Log("Suceessfully player profile Pic Url is updated");
                if(_action != null)
                {
                    _action();
                }
                //Crashlytics.Log("PlayfabController.cs|UpdateProfilePic(url) : Suceessfully player profile Pic Index is updated");
            }
            , OnError);
    }

    public void UpdatePlayerProfilePicIndex(string _index)
    {
        string _temp = DateTime.UtcNow.ToString("|yyyy|MM|dd|hh|mm");
        string _activeScene = SceneManager.GetActiveScene().buildIndex.ToString();
        string _data = _index + _temp + "|" + _activeScene + "|" +"@ulka.com";
        //Debug.Log("+++ " + _data);
        PlayFabClientAPI.AddOrUpdateContactEmail(new AddOrUpdateContactEmailRequest
        {
            EmailAddress = _data
        }, OnSuccess =>
        {
            //Debug.Log("||_ Successfully updated Player profile pic index.....");
        }, op =>
        {
            //Debug.Log("||_  Failed to update player profile Index..............");
            OnError(op);
        });

    }


    public void GetPlayerProfile(Action<GetPlayerProfileResult> _action)
    {
        var request = new GetPlayerProfileRequest
        {
            ProfileConstraints = new PlayerProfileViewConstraints
            {
                ShowAvatarUrl = true,
                ShowDisplayName = true,
                ShowLocations = true,
                ShowLastLogin = true,
                ShowContactEmailAddresses = true
                
            }
        };

        PlayFabClientAPI.GetPlayerProfile(request, _action, OnError);
    }

    
    private void CheckTittlePlayer()
    {
        Debug.Log("<><> CHECK TITTLE PLAYER....");
        PlayFabClientAPI.GetAccountInfo(new GetAccountInfoRequest { }, 
            OP => 
            {
                if(OP.AccountInfo.Created != OP.AccountInfo.TitleInfo.Created)
                {
                    Debug.Log("This FB id previously connected with master.......");
                     // Save player info from facebook

                    Debug.Log("TIME SPAN : " + (OP.AccountInfo.Created - OP.AccountInfo.TitleInfo.Created));
                    GetFirstTimeFBLogInReward(
                        op=> {
                            if (!op.Data.ContainsKey("SignUPRewardGiven"))
                            {
                                if (CoinSystem.instance != null)
                                    CoinSystem.instance.AddCoins(5000);
                                SaveFirstTimeFBLogINReward();
                                ReadFBData(FB.IsLoggedIn, true);
                            }
                            else
                            {
                                After();
                            }
                        });
                    
                }

                else
                {
                    Debug.Log("<><>This FB id previously not connected with master.......");
                    After();
                }
            },OnError);
    }

    private void OnPlayerProfile(GetPlayerProfileResult result)
    {
        
    }

    #endregion

    #region Save and Get Player Personal Data

    public void SaveFirstTimeFBLogINReward()
    {
        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>
            {
                {"SignUPRewardGiven", "1"}
            },
            Permission = UserDataPermission.Public
        };

        PlayFabClientAPI.UpdateUserData(request, (obj) =>
        {
            Debug.Log("Successfully save player Info to playfeb............");
            //Crashlytics.Log("PlayfabController.cs|SavePlayerInfo() : Successfully save player Info to playfeb............");
            //UpdateUI?.Invoke();
        },
            OnError);
        Debug.Log(playerProfilePic);
    }

    public void GetFirstTimeFBLogInReward(Action<GetUserDataResult> _action)
    {
        var request = new GetUserDataRequest
        {
            Keys = new List<string>() { "SignUPRewardGiven" }
        };
        PlayFabClientAPI.GetUserData(request, _action, OnError);
    }


    /// <summary>
    /// Save Player Info Name, ID, Email and profile Pic in string form
    /// </summary>
    public void SavePlayerInfo()
    {
        StartCoroutine(UpdateUserDataWait());
    }

    IEnumerator UpdateUserDataWait()
    {
        var _dataDic = new Dictionary<string, string>();

        if (playerID != null)
            _dataDic["FB_ID"] = playerID;
        if (PlayerName != null)
            _dataDic["Name"] = PlayerName;
        if (playerEmail != null)
            _dataDic["Email"] = playerEmail;
        _dataDic["ProfilePicIndex"] = profilePicIndex.ToString();

        yield return new WaitForSeconds(0.5f);
        var request = new UpdateUserDataRequest
        {
            Data = _dataDic,
            Permission = UserDataPermission.Public
        };

        PlayFabClientAPI.UpdateUserData(request, (obj) =>
        {
            //Debug.Log("Successfully save player Info to playfeb............");
            //Crashlytics.Log("PlayfabController.cs|SavePlayerInfo() : Successfully save player Info to playfeb............");
            UpdateUI?.Invoke();
        },
            OnError);
        //Debug.Log(playerProfilePic);
    }

    private void OnDataSend(UpdateUserDataResult obj)
    {
        //Debug.Log("Successfully save player Info to playfeb............");
    }


    public void GetPlayerInfo(Action<GetUserDataResult> _action, string _playfabID = null)
    {
        if (_playfabID != null)
        {
            var request = new GetUserDataRequest
            {
                PlayFabId = _playfabID,
                Keys = new List<string>() { "Name", "Email", "ID", "ProfilePicIndex" }
            };
            PlayFabClientAPI.GetUserData(request, _action, OnError);
        }
        else
        {
            PlayFabClientAPI.GetUserData(new GetUserDataRequest(), _action, OnError);
        }
    }

    #endregion

    #region On Error

    // Common Error Function for all the playfeb activity indication
    private void OnError(PlayFabError obj)
    {
         //Debug.Log("Playfab Controller: OnError 869: "+obj.HttpCode + " " + obj.ErrorMessage + " | " + obj.ErrorDetails + " | " + obj.ApiEndpoint + "|" +obj.Error);
        //Crashlytics.Log("PlayfabController.cs|OnError() : " + obj.ErrorMessage);
    }

    #endregion

    #region Virtual Currency

    #region Not Used In this Game
    
    public void Add_XP(int amount, Action _action = null)
    {
        string _temp = amount.ToString();
        Debug.Log(_temp);
        Debug.Log("XP" + " | " + amount);
        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
        {
            FunctionName = "AddCurrencyAmount",
            FunctionParameter = new { VirtualCurrency = "XP", Amount = amount.ToString() },
            GeneratePlayStreamEvent = true
        },
            cloudResult =>
            {
                if (_action != null)
                {
                    _action();
                }

                Debug.Log("Virtual Currancy (" + "XP" + ") Added " + amount.ToString() + "Successfully");
                //Crashlytics.Log("PlayfabContrpller.cs|Add_XP(int amount) :" + "Virtual Currancy (" + "XP" + ") Added " + amount.ToString() + "Successfully");
            },
            cloudResultError =>
            {
                Debug.Log("ERORR to save XP in server");
                //Crashlytics.Log("PlayfabContrpller.cs|Add_XP(int amount) : " + cloudResultError.ErrorMessage);
                if(XP_System.Instance != null)
                    XP_System.Instance.OfflineXPSave(amount);
            }
        );
    }

    public void Add_XP_Offline(int amount, Action _action)
    {
        string _temp = amount.ToString();
        Debug.Log(_temp);
        Debug.Log("XP" + " | " + amount);
        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
        {
            FunctionName = "AddCurrencyAmount",
            FunctionParameter = new { VirtualCurrency = "XP", Amount = amount.ToString() },
            GeneratePlayStreamEvent = true
        },
            cloudResult =>
            {
                _action();
                Debug.Log("Offline Virtual Currancy (" + "XP" + ") Added " + amount.ToString() + " Successfully");
                //Crashlytics.Log("PlayfabContrpller.cs|Add_XP_Offline(int amount, Action _action) :" + "Virtual Currancy (" + "XP" + ") Added " + amount.ToString() + "Successfully");
            },
            cloudResultError =>
            {
                if(XP_System.Instance != null)
                    XP_System.Instance.offlineXPDataSendToTheServer = true;
                Debug.Log("ERORR to save Offline XP in server");
                //Crashlytics.Log("PlayfabContrpller.cs|Add_XP_Offline(int amount, Action _action) : " + cloudResultError.ErrorMessage);
            }
        );
    }

    // Not used in this game
    public void SubstractCurrency(string _virtualCurrancy, int amount)
    {
        string _temp = amount.ToString();
        Debug.Log(_temp);
        Debug.Log(_virtualCurrancy + " | " + amount);
        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
        {
            FunctionName = "SubstractCurrencyAmount",
            FunctionParameter = new { VirtualCurrency = _virtualCurrancy, Amount = amount.ToString() },
            GeneratePlayStreamEvent = true
        },
            cloudResult => { },
            OnError
        );
    }

    #endregion


    public void AddCoinInServer(string amountIndex)
    {
        if (Application.internetReachability == NetworkReachability.NotReachable ||
            !PlayFabClientAPI.IsClientLoggedIn())
        {
            //Crashlytics.Log("PlayfabController.cs|AddCoinInServer(string amountIndex) : " + "No Internet !! failed to save coin in server...");
            if(CoinSystemUtilities.Instance != null)
                CoinSystemUtilities.Instance.OnAddCoinFailed(amountIndex);
            // Toast notification : Internet connection need to sync coin with server 
            //ToastNotification.instance.Show("No Internet Connection. Coin Can Not Save Into Cloud.", "NOINTERNET");
            return;
        }

        else
        {
            PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
            {
                /*  FunctionName = "AddCoinInServer",
                  FunctionParameter = new { AmountIndex = amountIndex },
                  GeneratePlayStreamEvent = true*/

                FunctionName = "AddOfflineCoinInServer",
                FunctionParameter = new { CoinAddList = amountIndex },
                GeneratePlayStreamEvent = true
            },
                cloudResult =>
                {
                    if (ToastNotification.instance != null && !PlayerPrefs.HasKey("FB_TOKEN"))
                    {
                        ToastNotification.instance.Show("Login To Facebook For Saving Coin Amount", "LOGINFACEBOOK");
                    }

                    Debug.Log("Added Amount of index" + amountIndex + "Successfully");
                    //Crashlytics.Log("PlayfabController.cs|AddCoinInServer(string amountIndex) : " + "Added Amount of index" + amountIndex + "Successfully");
                    GetAllVirtualCurrency();
                },
                cloudResultError =>
                {
                    Debug.Log("ERORR to save coin in server");
                    //Crashlytics.Log("PlayfabController.cs|AddCoinInServer(string amountIndex) : " + cloudResultError.ErrorMessage);
                    // Toast notification : Internet connection need to sync coin with server 
                    
                    if(ToastNotification.instance != null)
                        ToastNotification.instance.Show("No Internet Connection. Coin Can Not Save Into Cloud.", "NOINTERNET");
                    
                    if(CoinSystemUtilities.Instance != null)
                        CoinSystemUtilities.Instance.OnAddCoinFailed(amountIndex);
                }
            );
        }
    }

    public void DeductCoinInServer(string amountIndex)
    {
        if (Application.internetReachability == NetworkReachability.NotReachable ||
            !PlayFabClientAPI.IsClientLoggedIn())
        {
            //Crashlytics.Log("PlayfabController.cs|AddCoinInServer(string amountIndex) : " + "No Internet !! failed to save coin in server...");
            if(CoinSystemUtilities.Instance != null)
                CoinSystemUtilities.Instance.OnDeductCoinFailed(amountIndex);
            //ToastNotification.instance.Show("No Internet Connection. Coin Can Not Save Into Cloud.", "NOINTERNET");
            return;
        }

        else
        {
            PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
            {
                FunctionName = "DeductCoinInServer",
                FunctionParameter = new { AmountIndex = amountIndex },
                GeneratePlayStreamEvent = true
            },
                cloudResult =>
                {
                    //Debug.Log("Deducted Amount Index " + amountIndex + "Successfully");
                    //Crashlytics.Log("PlayfabController.cs|AddCoinInServer(string amountIndex) : " + "Deducted Amount Index " + amountIndex + "Successfully");
                    GetAllVirtualCurrency();
                },
                cloudResultError =>
                {
                    if(CoinSystemUtilities.Instance != null)
                        CoinSystemUtilities.Instance.OnDeductCoinFailed(amountIndex);
                    
                    if(ToastNotification.instance != null)
                        ToastNotification.instance.Show("No Internet Connection. Coin Can Not Save Into Cloud.", "NOINTERNET");
                    
                    //Crashlytics.Log("PlayfabController.cs|AddCoinInServer(string amountIndex) : " + cloudResultError.ErrorMessage);
                }
            );
        }
    }


    private void GetAllVirtualCurrency(Action onSuccessAction = null)
    {
        Dictionary<string, int> vc = new Dictionary<string, int>();
        PlayFabClientAPI.GetUserInventory(
            new GetUserInventoryRequest { },
            GetResult =>
            {
                //Crashlytics.Log("PlayfabController.cs|GetAllVirtualCurrency() : Get Coin amount from server");
                vc = GetResult.VirtualCurrency;
                //Debug.Log("GOLD :" + vc["GD"] + " | XP:" + vc["XP"]);

                //Crashlytics.Log("PlayfabController.cs|GetAllVirtualCurrency() : Set Game Coin amount = server coin amount");
                if (CoinSystem.instance != null)
                {
                    CoinSystem.instance.SetBalance(vc["GD"]);
                }

                if (LobbyUIManager.instance != null)
                {
                    LobbyUIManager.instance.CoinTextUpdateFromServer();
                    //Crashlytics.Log("PlayfabController.cs|GetAllVirtualCurrency() : Set coin amount in the leaderboard as leaderboard data");
                }

                SendLeaderboard(vc["GD"]);
                onSuccessAction?.Invoke();
            },
            OnError
        );
        // return vc;
    }

    #region Offline Coin Data Sending to Server 

    public void AddOfflineCoinsToServer(string _addCoinList, Action _action)
    {
        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
        {
            FunctionName = "AddOfflineCoinInServer",
            FunctionParameter = new { CoinAddList = _addCoinList }
        },
            cloudResult =>
            {
                Debug.Log("Offline coin data sent to AddOfflineCoinInServer() function in server successfully");
                _action();
                GetAllVirtualCurrency(() => { LobbyUIManager.instance.CoinTextUpdateFromServer(); });
            },
            cloudResultError =>
            {
                Debug.Log("Offline coin data failed to sent  AddOfflineCoinInServer() function in server !!!");
            });
    }

    public void DeductOfflineCoinsToServer(string _deductCoinList, Action _action)
    {
        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
        {
            FunctionName = "DeductOfflineCoinInServer",
            FunctionParameter = new { CoinDeductList = _deductCoinList }
        },
            cloudResult =>
            {
                Debug.Log("Offline coin data sent to AddOfflineCoinInServer() function in server successfully");

                _action();
                GetAllVirtualCurrency(() => { LobbyUIManager.instance.CoinTextUpdateFromServer(); });
            },
            cloudResultError =>
            {
                Debug.Log("Offline coin data failed to sent  AddOfflineCoinInServer() function in server !!!");
            });
    }

    public void OfflineCoinDataSend(string _HazariAdd, string _HazariSub, string _NineCardAdd, string _NineCardSub,
        Action _action)
    {
        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
        {
            FunctionName = "AddCoinInServer",
            FunctionParameter = new
            {
                HazariAdd = _HazariAdd,
                HazariSub = _HazariSub,
                NineCardAdd = _NineCardAdd,
                NineCardSub = _NineCardSub
            },
            GeneratePlayStreamEvent = true
        },
            cloudResult =>
            {
                Debug.Log("Offline coin data sent to server successfully");
                _action();
                GetAllVirtualCurrency();
            },
            cloudResultError => { Debug.Log("ERORR to save coin in server"); }
        );
    }

    #endregion

    #endregion

    #region Friends

    public void GetFriend(Action<GetFriendsListResult> _action, Action _errorAction = null)
    {
        var request = new GetFriendsListRequest
        {
            IncludeFacebookFriends = true,
            ProfileConstraints = new PlayerProfileViewConstraints
            {
                ShowAvatarUrl = true,
                ShowDisplayName = true,
                ShowLastLogin = true,
                ShowContactEmailAddresses = true
            }
        };
        PlayFabClientAPI.GetFriendsList(request, _action, op => {
            if (_errorAction != null)
                _errorAction();
            OnError(op);
        });
    }

    void DisplayFriends(GetFriendsListResult result)
    {
        var friendsCache = result.Friends;
        // friendsCache[0].Profile.AvatarUrl

        Debug.Log("YOUR FRIEND LIST :");
        friendsCache.ForEach(f => Debug.Log(f.FriendPlayFabId));
        Debug.Log("YOUR FRIEND LIST END");
    }

    #endregion

    #region LeaderBoard

    // Task: Two LeaderBoard 1-Global 2-Friends | Based On: Total Currency(Gold Amount) | Data Reset : NO

    public void SendLeaderboard(int _value)
    {
        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
        {
            FunctionName = "SendLeaderboard",
            FunctionParameter = new { value = _value },
            GeneratePlayStreamEvent = true
        },
            cloudResult => {
                //Debug.Log("Leaderboard sync successful");
                //Crashlytics.Log("PlayfabController.cs|SendLeaderboard(int _value) : Leaderboard updated successfully!!");
            },
            OnError
        );
    }

    private void OnLeaderboardUpdate(UpdatePlayerStatisticsResult obj)
    {
        Debug.Log("Leaderboard Successfully Updated!!");
    }

    public void GetLeaderboard(int _count, Action<GetLeaderboardResult> _action, Action _OnFailedToLoad = null)
    {
        var request = new GetLeaderboardRequest
        {
            StatisticName = "TotalCurrencyBased_Global",
            StartPosition = 0,
            MaxResultsCount = _count
        };

        request.ProfileConstraints = new PlayerProfileViewConstraints();
        request.ProfileConstraints.ShowAvatarUrl = true;
        request.ProfileConstraints.ShowDisplayName = true;
        request.ProfileConstraints.ShowLocations = true;
        request.ProfileConstraints.ShowContactEmailAddresses = true;

        PlayFabClientAPI.GetLeaderboard(request, _action, _OnError =>
        {
            if (_OnFailedToLoad != null)
                _OnFailedToLoad();
            OnError(_OnError);
        });
    }

    public void GetPlayerRank(Action<GetLeaderboardAroundPlayerResult> _action, Action _errorAction = null)
    {
        var request = new GetLeaderboardAroundPlayerRequest
        {
            StatisticName = "TotalCurrencyBased_Global",
            MaxResultsCount = 1
        };

        request.ProfileConstraints = new PlayerProfileViewConstraints();
        request.ProfileConstraints.ShowAvatarUrl = true;
        request.ProfileConstraints.ShowDisplayName = true;
        request.ProfileConstraints.ShowLocations = true;
        request.ProfileConstraints.ShowContactEmailAddresses = true;

        if (PlayFabClientAPI.IsClientLoggedIn())
        {
            PlayFabClientAPI.GetLeaderboardAroundPlayer(request, _action,op => {
                if (_errorAction != null)
                    _errorAction();
                OnError(op);
            });
        }else Debug.Log("GetPlayerRank() -> PlayFabClient not logged in ");
        
    }


    public void GetFriendsLeaderboard(int _count, Action<GetLeaderboardResult> _action, Action _OnFailedToLoad = null)
    {
        var request = new GetFriendLeaderboardRequest
        {
            StatisticName = "TotalCurrencyBased_Global",
            StartPosition = 0,
            MaxResultsCount = _count,
            IncludeFacebookFriends = true
        };

        request.ProfileConstraints = new PlayerProfileViewConstraints();
        request.ProfileConstraints.ShowAvatarUrl = true;
        request.ProfileConstraints.ShowDisplayName = true;
        request.ProfileConstraints.ShowLocations = true;
        request.ProfileConstraints.ShowContactEmailAddresses = true;

        PlayFabClientAPI.GetFriendLeaderboard(request, _action, _OnError =>
        {
            if (_OnFailedToLoad != null)
                _OnFailedToLoad();
            OnError(_OnError);
        });
    }

    private void OnLeaderboardGet(GetLeaderboardResult result)
    {
        foreach (var item in result.Leaderboard)
        {
            Debug.Log(item.Position + " " + item.PlayFabId + " " + item.StatValue);
        }
    }

    #endregion

    #endregion

    #region Others

    public void TextureToString(Texture2D _texture)
    {
        byte[] bytes;

        bytes = _texture.EncodeToPNG();
        playerProfilePic = Convert.ToBase64String(bytes);
    }

    public Texture2D StringToTexture(string _str)
    {
        byte[] imageBytes = Convert.FromBase64String(_str);
        Texture2D _texture = new Texture2D(128, 128);
        _texture.LoadImage(imageBytes);
        return _texture;
    }

    public Sprite Texture2DToSprite(Texture2D _texture)
    {
        return Sprite.Create(_texture, new Rect(0, 0, 128, 128), new Vector2());
    }


    public void SetMessage(string message, bool error = false)
    {
        _message = message;
        if (error)
            Debug.LogError(_message);
        else
            Debug.Log(_message);
    }

    public void OnGUI()
    {
        var style = new GUIStyle
        {
            fontSize = 40,
            normal = new GUIStyleState { textColor = Color.white },
            alignment = TextAnchor.MiddleCenter,
            wordWrap = true
        };
        var area = new Rect(0, 0, Screen.width, Screen.height);
        GUI.Label(area, _message, style);
    }

    public IEnumerator TimeGap(float waitTime = 1f)
    {
        yield return new WaitForSeconds(waitTime);
        LogInWithFacebookID(fb_token);
    }

    #endregion
}