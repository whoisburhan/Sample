using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Firebase.Crashlytics;
using Lobby;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.SharedModels;
using TAAASH_KIT;
using UnityEngine;
using UnityEngine.UI;
using PlayFabKit;

public class PlayFabScript : MonoBehaviour
{
    #region Manual Set Up
    [Header("LogIN Buttons")]
    [SerializeField] private Button m_fbLogInButton;
    [SerializeField] private Button m_googleLogInButton;

    [Header("LogIN Buttons From AnotherPanel")]
    [SerializeField] private Button fbLogInButtonInAnotherPanel;
    [SerializeField] private Button fbLoginButtonInFrinendsChallangePanel;
    [SerializeField] private Button globalLeaderboardRetryButton;
    [SerializeField] private Button friendsLeaderboardRetryButton;

    [Header("Leaderboard Loading Animation GameObjects")]
    [SerializeField] private GameObject globalLeaderboardRetryAnim;
    [SerializeField] private GameObject friendLeaderboardRetryAnim;

    [Header("Other Buttons")]
    [SerializeField] private Button m_freeCoinButton;
    [SerializeField] private Button m_settingsButton;

    [Header("Player Profile Buttons")]
    [SerializeField] private Button playerProfileUpdateDoneButton;

    [Header("LeaderBoard Buttons")]
    [SerializeField] private Button leaderBoardButton;
    [SerializeField] private Button leaderBoardFacebookLogInButton;
    [SerializeField] private Button friendsLeaderboardButton;
    [SerializeField] private Button globalLeaderboardButton;
    //[SerializeField] private Button facebookInviteButton;

    [Header("Leaderboard Button Sprite")]
    [SerializeField] private Sprite selectButtonSprite;
    [SerializeField] private Sprite deSelectButtonSprite;


    [Header("Leaderboard")]
    [SerializeField] private int noOfPlayer = 25;


    [Header("Server Data Checking time interval")]
    [SerializeField] private float serverTimeInterverl = 120f;

    //  [Header("Player Profile")]

    [Header("Local Avatar")]
    [SerializeField] private Sprite[] localAvatars;

    [Header("Player Short Profile")]
    [SerializeField] private Image smallPlayerProfilePic;
    [SerializeField] private Text smallPlayerName;
    [SerializeField] private Text smallPlayerCoinAmount;
    [SerializeField] private Image levelUpPlayerImage;

    [Header("LeaderBoard Player Info")]
    [SerializeField] private Image leaderboardPlayerProfilePic;
    [SerializeField] private Text leaderboardPlayerName;
    [SerializeField] private Text leaderboardPlayerCointAmount;
    [SerializeField] private Text leaderboardPlayerRank;

    [Header("Edit Profile Section")]
    [SerializeField] private Image editPlayerPicImage;
    [SerializeField] private InputField editPlayerNameText;
    [SerializeField] private Text editPlayerNameTextHolder;

    [Header("Prefabs")]
    [SerializeField] private GameObject playerRankProfile;

    [Header("UI Reference")]
    [SerializeField] private RectTransform leaderBoardPanel;
    [SerializeField] private RectTransform rankParentPanel;
    [SerializeField] private RectTransform friendRankParentPanel;


    [Header("UI ELEMENTS Top 3 Rank Player")]
    [SerializeField] private Image[] top3RankPlayerAvatar;
    [SerializeField] private Text[] top3RankPlayerMoneyAmount;

    [Header("Friends")]
    [SerializeField] private GameObject friendPrefab;
    [SerializeField] private RectTransform friendlistPanel;
    [SerializeField] private Button friendsButton;
    [SerializeField] private GameObject friendsPanelLoadignAnim;

    [Header("Country Flag")]
    [SerializeField] private Sprite[] countryFlags;

    #endregion

    #region Event and Action
    public static event Action OnLeaderboardReload;

    #endregion

    private int leaderboardCurrentIndex = 0;
    private List<GameObject> playerRankProfileList = new List<GameObject>();
    private List<GameObject> playerRankFriendsProfileList = new List<GameObject>();
  //  private List<GameObject> friendList = new List<GameObject>();

    private int playerRankProfileListCounter = 0;
    private int playerRankFriendsProfileListCounter = 0;

    private bool offlineCoinDataSendToTheServer = true;
    private bool refreshLeaderboardAllowed = true;

    private float refreshLeaderboardButtonEnableTimeInterval = 5f;
    private float refreshLeaderboardTimer;

    private bool showGlobalLeaderboardRetryAnim = false;
    private bool showFriendsLeaderboardRetryAnim = false;
    private float leaderboardRetryButtonShowIntervalTime = 10f;
    bool leaderRetryButtonLoadedOnlyFirstTime = false;

    float leadarboardRetryTimer = 0f;

    float playerFriendListUpdateTimer = 0f;
    float playerFriendListUpdateTimeInterval = 20f;

    [Header("LeaderBoard Players")]
    [SerializeField] private List<GameObject> globalPlayerList;
    [SerializeField] private List<GameObject> friendsPlayerList;

    public static event Action OnFriendReload;

    float timer = 0;

    public Image FB_Avatar;

    [Header("Fb Login Panel")] 
    [SerializeField]
    private GameObject fbLoginPanel;

    private bool checkForGroupData = true;
    
    private void OnEnable()
    {
        // PlayfabController.UpdateUI += GetPlayerProfileInfo;
        // PlayfabController.OnFaceBookLogIn += OnFaceBookLogInUpdate;

        FacebookLogInByButton.OnLogInWithFaceBookEvent += GetPlayerProfileInfo;
        FacebookLogInByButton.OnLogInWithFaceBookEvent += OnFaceBookLogInUpdate;
        FacebookLogInByButton.OnLogInWithFaceBookEvent += VirtualCurrancyUpdate;

        HazariPlayfabAuthentication.OnLogInWithCustomIdEvent += GetPlayerProfileInfo;
        HazariPlayfabAuthentication.OnLogInWithCustomIdEvent += VirtualCurrancyUpdate;
        HazariPlayfabAuthentication.OnLogInFailedEvent += PlayerProfileUpdate;

        PlayfabPlayerProfile.OnSetPlayerName += UpdatePlayerNameInUI;
        PlayfabPlayerProfile.OnSetAvatarURL += UpdatePlayerAvatarInUI;
    }

    private void OnDisable()
    {
        //  PlayfabController.UpdateUI -= GetPlayerProfileInfo;
        //  PlayfabController.OnFaceBookLogIn -= OnFaceBookLogInUpdate;

        FacebookLogInByButton.OnLogInWithFaceBookEvent -= GetPlayerProfileInfo;
        FacebookLogInByButton.OnLogInWithFaceBookEvent -= OnFaceBookLogInUpdate;
        FacebookLogInByButton.OnLogInWithFaceBookEvent -= VirtualCurrancyUpdate;

        HazariPlayfabAuthentication.OnLogInWithCustomIdEvent -= GetPlayerProfileInfo;
        HazariPlayfabAuthentication.OnLogInWithCustomIdEvent -= VirtualCurrancyUpdate;
        HazariPlayfabAuthentication.OnLogInFailedEvent -= PlayerProfileUpdate;

        PlayfabPlayerProfile.OnSetPlayerName -= UpdatePlayerNameInUI;
        PlayfabPlayerProfile.OnSetAvatarURL -= UpdatePlayerAvatarInUI;

    }


    // Start is called before the first frame update
    private void Start()
    {
        PlayfabConstants.Instance.PlayerCurrentActivityState = 0;
        PlayfabConstants.Instance.MyGroupMemberList = new List<string>();
        PlayfabConstants.Instance.FourDigitRandomNumber = null;

        PlayfabConstants.Instance.IsRandomMatchMaking = true;
        PlayfabConstants.Instance.IsPrivateTable = false;
        GameInvitationReceiver.Instance.CheckForInvitation = true;
        
        VirtualCurrancyUpdate();

        timer = 0;

        leadarboardRetryTimer = leaderboardRetryButtonShowIntervalTime;

        PlayerProfileUpdate();
      // Invoke(nameof(PlayerProfileUpdate), 1f);

        leaderboardPlayerRank.text = PlayfabConstants.Instance.PlayerGlobalRank.ToString();
        refreshLeaderboardTimer = refreshLeaderboardButtonEnableTimeInterval;

        if (PlayerPrefs.HasKey(PlayfabConstants.FacebookTokenKey))
        {
            m_fbLogInButton.gameObject.SetActive(false);
            leaderBoardFacebookLogInButton.transform.parent.gameObject.SetActive(false);
            //facebookInviteButton.gameObject.SetActive(true);
            //leaderBoardPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 430);
            //leaderBoardPanel.anchoredPosition = (new Vector2(0,-7f));
            leaderBoardPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 490);
            leaderBoardPanel.anchoredPosition = (new Vector2(0, -35f));
        }
        else
        {
            m_fbLogInButton.gameObject.SetActive(true);
            leaderBoardFacebookLogInButton.transform.parent.gameObject.SetActive(true);
            //facebookInviteButton.gameObject.SetActive(false);
            leaderBoardPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 490);
            leaderBoardPanel.anchoredPosition = (new Vector2(0, -35f));
        }

        AllButtonCofig();
    }

    /// <summary>
    /// 
    /// </summary>
    private void AllButtonCofig()
    {
        fbLogInButtonInAnotherPanel.onClick.AddListener((() =>
        {
            
            
            if (CustomEventSys.instance != null)
            {
                CustomEventSys.instance.FacebookLoginButtonEvent("facebook_login_button_click");
            }

            if (FacebookLogInByButton.Instance != null)
            {
                FacebookLogInByButton.Instance.FaceBookLogInByButtonFunc();
            }
                
        }));
        
        fbLoginButtonInFrinendsChallangePanel.onClick.AddListener((() =>
        {
            
            
            if (CustomEventSys.instance != null)
            {
                CustomEventSys.instance.FacebookLoginButtonEvent("facebook_login_button_click");
            }

            if (FacebookLogInByButton.Instance != null)
            {
                FacebookLogInByButton.Instance.FaceBookLogInByButtonFunc();
                LobbyUIManager.instance.showLoginPanel.SetActive(false);
            }
            
            
                
        }));

        leaderBoardFacebookLogInButton.onClick.AddListener(() =>
        {
            if (FacebookLogInByButton.Instance != null)
                FacebookLogInByButton.Instance.FaceBookLogInByButtonFunc();
        });

        globalLeaderboardButton.onClick.AddListener(() =>
        {
            globalLeaderboardButton.GetComponent<Image>().sprite = selectButtonSprite;
            friendsLeaderboardButton.GetComponent<Image>().sprite = deSelectButtonSprite;

            LobbyUIManager.instance.GlobalOrFriendsButtonClick(0);
            
            rankParentPanel.parent.gameObject.SetActive(true);
            friendRankParentPanel.parent.gameObject.SetActive(false);
        });

        friendsLeaderboardButton.onClick.AddListener(() =>
        {
            globalLeaderboardButton.GetComponent<Image>().sprite = deSelectButtonSprite;
            friendsLeaderboardButton.GetComponent<Image>().sprite = selectButtonSprite;
            
            LobbyUIManager.instance.GlobalOrFriendsButtonClick(1);

            rankParentPanel.parent.gameObject.SetActive(false);
            friendRankParentPanel.parent.gameObject.SetActive(true);
            
        });

        playerProfileUpdateDoneButton.onClick.AddListener(() =>
        {
            Debug.Log("TEXT TEXT");
            editPlayerNameText.DeactivateInputField();

            if (PlayfabConstants.Instance != null)
                editPlayerNameTextHolder.text = PlayfabConstants.Instance.PlayerName;
            Debug.Log("NOT ENTER TEXT");

            if (editPlayerNameText.text != "")
            {
                PlayfabPlayerProfile.SetPlayerName(editPlayerNameText.text);
            }
            //}
            //editPlayerNameText.ActivateInputField();
        });

        leaderBoardButton.onClick.AddListener(() =>
        {
            if (refreshLeaderboardAllowed)
            {
                Debug.Log("UPDATE LEADERBOARD...");
                //LeaderBoardUpdate();
                NewLeaderboardSystem();
                refreshLeaderboardAllowed = false;
            }
        });

        /*
        globalLeaderboardRetryButton.onClick.AddListener(() =>
        {
            leaderRetryButtonLoadedOnlyFirstTime = false;
            globalLeaderboardRetryButton.transform.parent.gameObject.SetActive(false);
            friendsLeaderboardRetryButton.transform.parent.gameObject.SetActive(false);
            showGlobalLeaderboardRetryAnim = true;
            showFriendsLeaderboardRetryAnim = true;
            globalLeaderboardRetryAnim.SetActive(true);
            friendLeaderboardRetryAnim.SetActive(true);
            //LeaderBoardUpdate();
            NewLeaderboardSystem();
        });

        friendsLeaderboardRetryButton.onClick.AddListener(() =>
        {
            leaderRetryButtonLoadedOnlyFirstTime = false;
            globalLeaderboardRetryButton.transform.parent.gameObject.SetActive(false);
            friendsLeaderboardRetryButton.transform.parent.gameObject.SetActive(false);
            showGlobalLeaderboardRetryAnim = true;
            showFriendsLeaderboardRetryAnim = true;
            globalLeaderboardRetryAnim.SetActive(true);
            friendLeaderboardRetryAnim.SetActive(true);
            //LeaderBoardUpdate();
            NewLeaderboardSystem();
        });
        */

      /*  friendsButton.onClick.AddListener(() =>
        {
            GetFriend();
        });*/

    }

    #region VERSION 0.1

    private void HideGarbadgeValueInPanel(Transform _panel, Button _retryButton)
    {
        leaderRetryButtonLoadedOnlyFirstTime = true;
        if (_panel.childCount > 0)
        {
            // bool _isReload = false;
            for (int i = 0; i < _panel.childCount; i++)
            {
                if (_panel.GetChild(i).GetComponent<LeaderboardRankPlayerScript>().playerDisplayName.text == "---")
                {
                    // _isReload = true;
                    _panel.GetChild(i).gameObject.SetActive(false);
                    _retryButton.transform.parent.gameObject.SetActive(true);

                }
            }
        }
    }

    private void CreatePlayerRankPanel(int _total, int leaderBoardType)
    {
        
        if (rankParentPanel == null || friendlistPanel == null)
            return;
        
        //Delete previously loaded data
        if (leaderBoardType == 1)
        {
            if (rankParentPanel.childCount > 0)
            {
                List<GameObject> _tempGOList = new List<GameObject>();
                for (int i = 0; i < rankParentPanel.childCount; i++)
                {
                    _tempGOList.Add(rankParentPanel.GetChild(i).gameObject);
                }

                foreach (var go in _tempGOList)
                {
                    Destroy(go);
                }
            }
        }
        else if (leaderBoardType == 2)
        {
            if (friendRankParentPanel.childCount > 0)
            {
                List<GameObject> _tempGOList = new List<GameObject>();
                for (int i = 0; i < friendRankParentPanel.childCount; i++)
                {
                    _tempGOList.Add(friendRankParentPanel.GetChild(i).gameObject);
                }

                foreach (var go in _tempGOList)
                {
                    Destroy(go);
                }
            }
        }

        int panelHeight = (_total * 60) - 8 + 10;

        if (panelHeight < 490)
            panelHeight = 490;
        

        if (leaderBoardType == 1)
        {
            rankParentPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, panelHeight);
            rankParentPanel.parent.GetComponent<ScrollRect>().normalizedPosition = new Vector2(0, 1);
        }
        else
        {
            friendRankParentPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, panelHeight);
            friendRankParentPanel.parent.GetComponent<ScrollRect>().normalizedPosition = new Vector2(0, 1);
        }

        for (int i = 0; i < _total; i++)
        {
            GameObject go = Instantiate(playerRankProfile, transform.position, Quaternion.identity);

            if (leaderBoardType == 1)
            {
                go.transform.SetParent(rankParentPanel);
                go.transform.localScale = new Vector3(1f, 1f, 1f);
                playerRankProfileList.Add(go);
            }

            else
            {
                go.transform.SetParent(friendRankParentPanel);
                go.transform.localScale = new Vector3(1f, 1f, 1f);
                playerRankFriendsProfileList.Add(go);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        //  Debug.Log(DateTime.UtcNow.ToString("ddMMyyyyhhmm"));

        if (showGlobalLeaderboardRetryAnim && !leaderRetryButtonLoadedOnlyFirstTime)
        {
            leadarboardRetryTimer -= Time.deltaTime;
            if (leadarboardRetryTimer <= 0)
            {
                showGlobalLeaderboardRetryAnim = false;
                leadarboardRetryTimer = leaderboardRetryButtonShowIntervalTime;
                globalLeaderboardRetryAnim.SetActive(false);
                friendLeaderboardRetryAnim.SetActive(false);
                globalLeaderboardRetryButton.transform.parent.gameObject.SetActive(true);
                friendsLeaderboardRetryButton.transform.parent.gameObject.SetActive(true);
            }
        }
        else
        {
            leadarboardRetryTimer = leaderboardRetryButtonShowIntervalTime;
        }

        if (!refreshLeaderboardAllowed)
        {
            refreshLeaderboardTimer -= Time.deltaTime;
            if (refreshLeaderboardTimer <= 0)
            {
                refreshLeaderboardTimer = refreshLeaderboardButtonEnableTimeInterval;
                refreshLeaderboardAllowed = true;
            }
        }

        if (PlayFabClientAPI.IsClientLoggedIn())
        {
            if (Application.internetReachability != NetworkReachability.NotReachable)
            {
                #region Comment
                /*
                if (checkForGroupData && PlayfabConstants.Instance.MyGroupID != null)
                {
                    if(PlayfabConstants.Instance.MyGroupID == PlayfabConstants.Instance.MyPlayfabID)
                    {
                        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
                        {
                            FunctionName = "DeleteOnlineGroup",
                            FunctionParameter = new { GroupID = PlayfabConstants.Instance.MyGroupID },
                            GeneratePlayStreamEvent = true
                        }, OnSuccess =>
                        {

                        }, OnFailed => { checkForGroupData = true; });
                    }

                    else
                    {
                        PlayFabClientAPI.RemoveSharedGroupMembers(new RemoveSharedGroupMembersRequest
                        {
                            SharedGroupId = PlayfabConstants.Instance.MyGroupID,
                            PlayFabIds = new List<string> { PlayfabConstants.Instance.MyPlayfabID }
                        },
                        OnSuccess =>
                        {
                        }, OnFailed => { }
                        );
                    }

                    checkForGroupData = false;
                }
                */
                #endregion
                if (CoinSystemUtilities.Instance != null && offlineCoinDataSendToTheServer)
                {
                    CoinSystemUtilities.Instance.CheckForSavedCoinData();
                    offlineCoinDataSendToTheServer = false;
                }

                if (XP_System.Instance != null && XP_System.Instance.offlineXPDataSendToTheServer)
                {
                    XP_System.Instance.CheckOfflineXP();
                    XP_System.Instance.offlineXPDataSendToTheServer = false;
                }
            }

            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                offlineCoinDataSendToTheServer = true;
                if (XP_System.Instance != null)
                    XP_System.Instance.offlineXPDataSendToTheServer = true;
            }
            //  Debug.Log("LoggedIN");
            if (timer <= 0 && Application.internetReachability != NetworkReachability.NotReachable)
            {
                //  LeaderBoardUpdate();
                NewLeaderboardSystem();
                timer = serverTimeInterverl;
            }

            timer -= Time.deltaTime;
            

            /* if(playerFriendListUpdateTimer<=0 && Application.internetReachability != NetworkReachability.NotReachable)
             {
                 GetFriend();
                 playerFriendListUpdateTimer = playerFriendListUpdateTimeInterval;
             }

             playerFriendListUpdateTimer -= Time.deltaTime;
             */
        }

    }


    /// <summary>
    /// 
    /// </summary>
    public void OnFaceBookLogInUpdate()
    {
        m_fbLogInButton.gameObject.SetActive(false);
        leaderBoardFacebookLogInButton.transform.parent.gameObject.SetActive(false);
        //facebookInviteButton.gameObject.SetActive(true);
        //leaderBoardPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 430);
        //leaderBoardPanel.anchoredPosition = (new Vector2(0, -7f));
        leaderBoardPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 490);
        leaderBoardPanel.anchoredPosition = (new Vector2(0, -35f));
        // LeaderBoardUpdate();
        NewLeaderboardSystem();
    }

    private void LeaderBoardUpdate()
    {
        if (Application.internetReachability != NetworkReachability.NotReachable)
        {
            playerRankProfileList = new List<GameObject>();
            playerRankFriendsProfileList = new List<GameObject>();

            OnLeaderboardReload?.Invoke();

            if (!(globalLeaderboardRetryButton is null) && globalLeaderboardRetryButton.transform != null && globalLeaderboardRetryButton.transform.parent != null)
                globalLeaderboardRetryButton.transform.parent.gameObject.SetActive(false);
            if(globalLeaderboardRetryAnim != null)
                globalLeaderboardRetryAnim.SetActive(true);
            if (friendsLeaderboardRetryButton != null && friendsLeaderboardRetryButton.transform != null
                                                      && friendsLeaderboardRetryButton.transform.parent != null
                                                      && friendsLeaderboardRetryButton.transform.parent.gameObject != null)
            {
                friendsLeaderboardRetryButton.transform.parent.gameObject.SetActive(false);
            }
                
            if(friendLeaderboardRetryAnim != null)
                friendLeaderboardRetryAnim.SetActive(true);

            if (PlayfabController.Instance != null)
            {
                PlayfabController.Instance.GetPlayerRank(PlayerRankInLeaderBoard,
                    () => { Debug.Log("Error On Getting Player Rank info from server..."); });


                PlayfabController.Instance.GetLeaderboard(noOfPlayer, GlobalLeaderboardResult, () =>
                {
                    playerRankProfileList = new List<GameObject>();
                    if(globalLeaderboardRetryAnim != null)
                        globalLeaderboardRetryAnim.SetActive(false);
                    if (globalLeaderboardRetryButton != null && globalLeaderboardButton.transform.parent != null)
                    {
                        globalLeaderboardRetryButton.transform.parent.gameObject.SetActive(true);
                    }
                        
                });
                
                PlayfabController.Instance.GetFriendsLeaderboard(noOfPlayer, FriendsLeaderboardResult, () =>
                {
                    playerRankFriendsProfileList = new List<GameObject>();
                    if(friendLeaderboardRetryAnim != null)
                        friendLeaderboardRetryAnim.SetActive(false);
                    if (friendsLeaderboardRetryButton != null 
                        && friendsLeaderboardRetryButton.transform != null
                        && friendsLeaderboardRetryButton.transform.parent != null
                        && friendsLeaderboardRetryButton.transform.parent.gameObject != null)
                    {
                        friendsLeaderboardRetryButton.transform.parent.gameObject.SetActive(true);
                    }
                });

            }else
            {
                StartCoroutine(nameof(RetryLeaderboard));
            }
            timer = serverTimeInterverl;
        }

        else
        {
            if(leaderboardPlayerRank != null)
                leaderboardPlayerRank.text = PlayerPrefs.GetInt("PLAYER_GLOBAL_RANK", 0).ToString();
            

            if(leaderboardPlayerCointAmount != null && CoinSystem.instance != null)
                leaderboardPlayerCointAmount.text = CoinSystem.instance.GetBalance().ToString();
            
        }
    }

    private IEnumerator RetryLeaderboard()
    {
        yield return new WaitForSeconds(2);

        PlayfabLeaderboard.GetLeaderboardAroundPlayer(1, PlayerRankInLeaderBoard, () => { Debug.Log("Error On Getting Player Rank info from server..."); });

        PlayfabLeaderboard.GetGlobalLeaderboard(noOfPlayer, GlobalLeaderboardResult, () => 
        {
            playerRankProfileList = new List<GameObject>();
            globalLeaderboardRetryAnim.SetActive(false);
            globalLeaderboardRetryButton.transform.parent.gameObject.SetActive(true);
        });

        PlayfabLeaderboard.GetFriendsLeaderboard(noOfPlayer, FriendsLeaderboardResult, () =>
        {
            playerRankFriendsProfileList = new List<GameObject>();
            friendLeaderboardRetryAnim.SetActive(false);
            friendsLeaderboardRetryButton.transform.parent.gameObject.SetActive(true);
        });
    }

    #region Player-Profile
    #endregion

    #region Player Leaderboard Info
    private void PlayerRankInLeaderBoard(GetLeaderboardAroundPlayerResult _result)
    {
        
        if (_result == null || _result.Leaderboard == null)
            return;
        
        var result = _result.Leaderboard;

        if (leaderboardPlayerRank != null)
        {
            leaderboardPlayerRank.text = (result[0].Position + 1).ToString();
            PlayfabConstants.Instance.PlayerGlobalRank = result[0].Position + 1;
        }


        if (leaderboardPlayerCointAmount != null)
        {
            leaderboardPlayerCointAmount.text = result[0].StatValue.ToString();   
        }

        PlayerPrefs.SetInt("PLAYER_GLOBAL_RANK", result[0].Position + 1);
        
    }

    #endregion

    #region global Leaderboard
    private void GlobalLeaderboardResult(GetLeaderboardResult result)
    {
        if (result?.Leaderboard == null)
            return;
        
        var _result = result.Leaderboard;
        //Debug.Log("Global Leaderboard:" + result.Count);

        CreatePlayerRankPanel(_result.Count, 1);

        showGlobalLeaderboardRetryAnim = false;
        if (globalLeaderboardRetryButton != null && globalLeaderboardRetryButton.transform != null 
                                                 && globalLeaderboardRetryButton.transform.parent.gameObject != null)
        {
            globalLeaderboardRetryButton.transform.parent.gameObject.SetActive(false);
            globalLeaderboardRetryAnim.SetActive(false);
        }
        
        for (int i = 0; i < _result.Count; i++)
        {
            Text playerRankText = playerRankProfileList[i].GetComponent<LeaderboardRankPlayerScript>().RankNo;
            playerRankText.text = (_result[i].Position + 1).ToString();

            if(LanguageSelectionScript.instance != null)
                LanguageSelectionScript.instance.ChangeNumberLanguage(playerRankText);

            playerRankProfileList[i].GetComponent<LeaderboardRankPlayerScript>().playerDisplayName.text = _result[i].Profile.DisplayName;

            Text playerCoinAmountText = playerRankProfileList[i].GetComponent<LeaderboardRankPlayerScript>().playerCoinAmount;
            playerCoinAmountText.text = _result[i].StatValue.ToString();
            
            if(LanguageSelectionScript.instance != null)
                LanguageSelectionScript.instance.ChangeNumberLanguage(playerCoinAmountText);
            
            playerRankProfileList[i].GetComponent<LeaderboardRankPlayerScript>().PlayFabID = _result[i].PlayFabId;


            if (i < 3)
            {
                if (_result[i].StatValue > 1000000)
                {
                    float shownAmount = _result[i].StatValue / 1000000f;
                    top3RankPlayerMoneyAmount[i].text = shownAmount.ToString("0.00") + "M";
                }
                else
                {
                    top3RankPlayerMoneyAmount[i].text = _result[i].StatValue.ToString();
                }
                
            }

            if (_result[i].Profile.ContactEmailAddresses.Count > 0)
            {
                if (!string.IsNullOrEmpty(_result[i].Profile.ContactEmailAddresses[0].EmailAddress))
                {
                    var _str = StringSplitter(_result[i].Profile.ContactEmailAddresses[0].EmailAddress);
                    int _index = 0;
                    if (_str != null)
                    {
                        _index = int.Parse(_str[0]);
                    }
                    if (_index == 0)
                    {
                        var _tempAvatarUrl = _result[i].Profile.AvatarUrl;
                        var _avatarHolder = playerRankProfileList[i].GetComponent<LeaderboardRankPlayerScript>().playerProfilePic;

                        if (!string.IsNullOrEmpty(_tempAvatarUrl) && _avatarHolder != null)
                        {
                            UpdateImgInUI(_tempAvatarUrl, _avatarHolder);
                            if (i < 3)
                            {
                                UpdateImgInUI(_tempAvatarUrl, top3RankPlayerAvatar[i]);
                            }
                        }
                    }
                    else
                    {
                        // Extra checking
                        playerRankProfileList[i].GetComponent<LeaderboardRankPlayerScript>().playerProfilePic.sprite = localAvatars[_index - 1];
                        if (i < 3)
                        {
                            top3RankPlayerAvatar[i].sprite = localAvatars[_index - 1];
                        }
                    }
                }
            }
            else
            {
                var _tempAvatarUrl = _result[i].Profile.AvatarUrl;
                var _avatarHolder = playerRankProfileList[i].GetComponent<LeaderboardRankPlayerScript>().playerProfilePic;
                if (!String.IsNullOrEmpty(_tempAvatarUrl) && _avatarHolder != null )
                {
                    Davinci.get().load((string)_result[i].Profile.AvatarUrl).setCached(false).setFadeTime(0f).into(playerRankProfileList[i].GetComponent<LeaderboardRankPlayerScript>().playerProfilePic).start();
                    if (i < 3)
                    {
                        Davinci.get().load((string)_result[i].Profile.AvatarUrl).setFadeTime(0f).into(top3RankPlayerAvatar[i]).start();
                    }
                }
            }


            int ooo = (int)_result[i].Profile.Locations[0].CountryCode;

            //  Debug.Log("|||CC " + ooo);

            playerRankProfileList[i].GetComponent<LeaderboardRankPlayerScript>().CountryFlag.sprite = countryFlags[SelectCountryFlag(ooo)];

            //  Debug.Log("??? "+result[i].Profile.Statistics[0].Name + " " + result[i].Profile.Statistics[0].Value);

        }

        //GlobalLeaderboardProfileAvatarSelector();

        HideGarbadgeValueInPanel(rankParentPanel, globalLeaderboardRetryButton);
    }

    #endregion

    #region friends Leaderboard
    private void FriendsLeaderboardResult(GetLeaderboardResult _result)
    {
        if (_result == null || _result.Leaderboard == null)
            return;
        
        var result = _result.Leaderboard;
        //Debug.Log("Friends Leaderboard:");

        showGlobalLeaderboardRetryAnim = false;

        if (friendsLeaderboardRetryButton != null 
            && friendsLeaderboardRetryButton.transform != null
            && friendsLeaderboardRetryButton.transform.parent.gameObject != null)
        {
            friendsLeaderboardRetryButton.transform.parent.gameObject.SetActive(false);
            friendLeaderboardRetryAnim.SetActive(false);
        }
        
        try
        {
            //Debug.Log("TOP FRIEND RESULT COUNT :" + result.Count);
            if (result.Count > 1)
            {

                if (PlayfabConstants.Instance != null && result[0].PlayFabId == PlayfabConstants.Instance.MyPlayfabID)
                {
                    string[] _temp = result[1].Profile.DisplayName.Split(' ');
                    NotificationManager.Instance.TopRankedFriendName = _temp[0].ToUpper();
                    PlayerPrefs.SetString(NotificationManager.TOP_RANKED_FRIEND_NAME, NotificationManager.Instance.TopRankedFriendName);
                    //  NotificationManager.Instance.SetNotifications(result[1].DisplayName);
                }
                else
                {
                    string[] _temp = result[0].Profile.DisplayName.Split(' ');
                    NotificationManager.Instance.TopRankedFriendName = _temp[0].ToUpper();
                    PlayerPrefs.SetString(NotificationManager.TOP_RANKED_FRIEND_NAME, NotificationManager.Instance.TopRankedFriendName);
                    //  NotificationManager.Instance.SetNotifications(result[1].DisplayName);
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log("TOP FRIEND : OOPS EXCEPTION ");
        }

        CreatePlayerRankPanel(result.Count, 2);

        if (friendsLeaderboardRetryButton != null &&friendsLeaderboardRetryButton.transform != null)
        {
            friendsLeaderboardRetryButton.transform.parent.gameObject.SetActive(false);
            friendLeaderboardRetryAnim.SetActive(false);
        }

        for (int i = 0; i < result.Count; i++)
        {

            if (playerRankProfileList[i] == null) return;

            Text fbPlayerRanktextLanguage = playerRankFriendsProfileList[i].GetComponent<LeaderboardRankPlayerScript>().RankNo;
            
            if (fbPlayerRanktextLanguage == null) return;
            
            fbPlayerRanktextLanguage.text = (result[i].Position + 1).ToString();
            
            if(LanguageSelectionScript.instance != null)
                LanguageSelectionScript.instance.ChangeNumberLanguage(fbPlayerRanktextLanguage);

            var playerDisplayName = playerRankFriendsProfileList[i].GetComponent<LeaderboardRankPlayerScript>().playerDisplayName;
            if (playerDisplayName != null)
                playerDisplayName.text =
                    result[i].Profile.DisplayName;

            Text fbPlayerCoinAmountTextLanguage = playerRankFriendsProfileList[i].GetComponent<LeaderboardRankPlayerScript>().playerCoinAmount;
            
            if (fbPlayerCoinAmountTextLanguage == null) return;
            
            fbPlayerCoinAmountTextLanguage.text = result[i].StatValue.ToString();
            
            if(LanguageSelectionScript.instance != null)
                LanguageSelectionScript.instance.ChangeNumberLanguage(fbPlayerCoinAmountTextLanguage);

            playerRankFriendsProfileList[i].GetComponent<LeaderboardRankPlayerScript>().PlayFabID = result[i].PlayFabId;
            
            
            
            if (result[i].Profile.ContactEmailAddresses.Count > 0)
            {
                //Debug.Log("MMM : " + result[i].Profile.ContactEmailAddresses.Count);
                if (!String.IsNullOrEmpty(result[i].Profile.ContactEmailAddresses[0].EmailAddress))
                {
                    var _str = StringSplitter(result[i].Profile.ContactEmailAddresses[0].EmailAddress);
                    int _index = 0;
                    if (_str != null)
                    {
                        _index = Int32.Parse(_str[0]);
                        _index = Mathf.Abs(_index);
                    }

                    if (_index == 0)
                    {
                        if (!string.IsNullOrEmpty(result[i].Profile.AvatarUrl))
                        {
                            Davinci.get().load(result[i].Profile.AvatarUrl).setFadeTime(0f).into(playerRankFriendsProfileList[i].GetComponent<LeaderboardRankPlayerScript>().playerProfilePic).start();
                        }
                    }
                    else
                    {
                        // Extra checking
                        playerRankFriendsProfileList[i].GetComponent<LeaderboardRankPlayerScript>().playerProfilePic.sprite = localAvatars[_index - 1];
                    }
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(result[i].Profile.AvatarUrl))
                {
                    Davinci.get().load(result[i].Profile.AvatarUrl).setFadeTime(0f).into(playerRankFriendsProfileList[i].GetComponent<LeaderboardRankPlayerScript>().playerProfilePic).start();
                }
            }

            var countryCode = result[i].Profile.Locations[0].CountryCode;
            if (countryCode != null)
            {
                int ooo = (int)countryCode;

                //Debug.Log("|||CC " + ooo);

                playerRankFriendsProfileList[i].GetComponent<LeaderboardRankPlayerScript>().CountryFlag.sprite = countryFlags[SelectCountryFlag(ooo)];
            }
        }

        HideGarbadgeValueInPanel(friendRankParentPanel, friendsLeaderboardRetryButton);

    }

    /// <summary>
    /// Select Country flag index
    /// </summary>
    /// <param name="_code">Country Code</param>
    /// <returns>index</returns>
    private int SelectCountryFlag(int _code)
    {
        //Debug.Log("CC ||" + _code);
        switch (_code)
        {
            case 18: //BD
                return 1;
            case 102: //IN
                return 2;
            case 134: //MY
                return 3;
            case 155: //NP
                return 4;
            case 235: //US
                return 5;
            case 234: //GB
                return 6;
            case 198: //SL
                return 7;
            case 135: //MV
                return 8;
            case 167: //PK
                return 9;
          //  case 0: //AF
          //      return 10;
            case 103: //ID
                return 11;
            case 199: //SG
                return 12;
            case 85: //GR
                return 13;
            case 109: //IT
                return 14;
            case 238: //UZ
                return 15;
            case 65: //EG
                return 16;
            case 17: //BH
                return 17;
            case 119: //KW
                return 18;
            case 104: //IR
                return 19;
            case 194: //SA
                return 20;
            case 152: //MM
                return 21;
            case 141: //MU
                return 22;
            case 227: //TR
                return 23;
            case 233: //AE
                return 24;
            default: //International
                return 0;
        }
    }

    #endregion

    #region Player Profile

    /// <summary>
    /// 
    /// </summary>
    private void PlayerProfileUpdate()
    {
        UpdatePlayerNameInUI();

        if (FB_Avatar != null && !string.IsNullOrEmpty(PlayfabConstants.Instance.PlayerAvatarURL))
            UpdateImgInUI(PlayfabConstants.Instance.PlayerAvatarURL, FB_Avatar, true);



        UpdatePlayerAvatarInUI();
    }

    /// <summary>
    /// 
    /// </summary>
    private void GetPlayerProfileInfo()
    {
        PlayfabPlayerProfile.GetPlayerProfile(PlayerProfileSetInUi, PlayerProfileUpdate);
    }

    IEnumerator WaitGetPlayerProfile(float _waitTime = .8f)
    {
        yield return new WaitForSeconds(_waitTime);
    }

    private void PlayerProfileSetInUi(GetPlayerProfileResult result)
    {
        Debug.Log("123 Get Player Profile Success");

        string _name = result.PlayerProfile.DisplayName;
        Debug.Log("123 Name:" + _name);

        if (!string.IsNullOrEmpty(_name) && PlayfabConstants.Instance != null)
            PlayfabConstants.Instance.PlayerName = _name;

        UpdatePlayerNameInUI();

        if (result.PlayerProfile.ContactEmailAddresses.Count > 0
            && !string.IsNullOrEmpty(result.PlayerProfile.ContactEmailAddresses[0].EmailAddress))
        {
            Debug.Log("123 ENTER");
            string[]  _str = StringSplitter(result.PlayerProfile.ContactEmailAddresses[0].EmailAddress);

            int _index = 0;

            if (_str != null)
            {
                _index = Int32.Parse(_str[0]);
            }

            Debug.Log("123 _INDEX : " + _index);

            if (PlayfabConstants.Instance != null)
            {
                PlayfabConstants.Instance.PlayerAvatarIndex = _index;
                PlayfabConstants.Instance.PlayerCountryCode = (int)result.PlayerProfile.Locations[0].CountryCode;
            }
        }
        else
        {
            PlayfabPlayerProfile.SetPlayerProfilePicIndexAndLogInActivity("0");
            PlayfabConstants.Instance.PlayerAvatarIndex = 0;
        }

        string _avatarUrl = result.PlayerProfile.AvatarUrl;
        if (PlayfabConstants.Instance != null && !string.IsNullOrEmpty(_avatarUrl))
        {
            PlayfabConstants.Instance.PlayerAvatarURL = _avatarUrl;

            UpdateImgInUI(_avatarUrl, FB_Avatar, true);
        }

        UpdatePlayerAvatarInUI();

    }

    #endregion

    #region Player's Friend List
    /*
    public void GetFriend()
    {
        friendsPanelLoadignAnim.SetActive(true);
        PlayfabController.Instance.GetFriend(UpdateFriendList, () => { Debug.Log("[FRIEND] Unable to get friend list....."); });
    }

    private void UpdateFriendList(GetFriendsListResult _result)
    {
        //Debug.Log("||FRIEND|| : " + _result.Friends.Count);
        if (_result.Friends.Count > 0)
        {
            if(friendsPanelLoadignAnim != null)
                friendsPanelLoadignAnim.SetActive(false);
            else
            {
                return;
            }
            
            OnFriendReload?.Invoke();

            int panelHeight = (_result.Friends.Count * (80 + 10)) - 10;

            if (panelHeight < 352)
                panelHeight = 352;

            friendlistPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, panelHeight);
            friendlistPanel.parent.GetComponent<ScrollRect>().normalizedPosition = new Vector2(0, 1);


            friendList = new List<GameObject>();

            for (int i = 0; i < _result.Friends.Count; i++)
            {

                GameObject go = Instantiate(friendPrefab, transform.position, Quaternion.identity);
                go.transform.parent = friendlistPanel;
                go.transform.localScale = Vector3.one;
                OnlineFriendGameObject fs = go.GetComponent<OnlineFriendGameObject>();
                if (fs != null)
                {
                    fs.FriendNo.text = (i + 1).ToString();
                    fs.FriendName.text = _result.Friends[i].Profile.DisplayName;
                    try
                    {
                        if (_result.Friends[i].Profile.ContactEmailAddresses[0].EmailAddress != "")
                        {
                            var _str = StringSplitter(_result.Friends[i].Profile.ContactEmailAddresses[0].EmailAddress);
                            int _index = 0;
                            if (_str != null)
                            {
                                _index = Int32.Parse(_str[0]);
                            }
                            var _avatarIndex = _index;
                            if (_avatarIndex == 0)
                            {
                                Davinci.get().load(_result.Friends[i].Profile.AvatarUrl).setFadeTime(0f).into(fs.FriendProfilePic).start();
                            }
                            else
                            {
                                if (_avatarIndex < localAvatars.Length)
                                    fs.FriendProfilePic.sprite = localAvatars[_avatarIndex - 1];
                            }

                            FriendActivityUpdate(_str,fs);
                        }
                    }
                    catch (Exception e)
                    {
                        fs.FriendProfilePic.sprite = localAvatars[1];
                    }

                    fs.FriendsPlayfabID = _result.Friends[i].FriendPlayFabId;

                    fs.ChallengeButton.onClick.AddListener(()=> 
                    {
                        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
                        {
                            FunctionName = "InvitationRequest",
                            FunctionParameter = new { FriendsPlayfabID = fs.FriendsPlayfabID, RequestTime = DateTime.UtcNow.ToString() },
                            GeneratePlayStreamEvent = true
                        }, OnSuccess => { }, OnFailed => { });
                    });
                }
                friendList.Add(go);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="_lastLogin"></param>
    /// <param name="fs"></param>
    private void FriendActivityUpdate(string[] _lastLogin, OnlineFriendGameObject fs)
    {
        if(_lastLogin.Length >= 6)
        {
            var _currentTime = (DateTime.UtcNow.ToString("yyyy|MM|dd|hh|mm")).Split('|');

            if (Int32.Parse(_lastLogin[1]) != Int32.Parse(_currentTime[0]))
            {
                fs.LogInStatus.text = "LAST SEEN " + (Int32.Parse(_currentTime[0]) - Int32.Parse(_lastLogin[1])) + " YEARS AGO";
                fs.ActivityIcon.color = fs.OfflineColor;
            }
            else if (Int32.Parse(_lastLogin[2]) != Int32.Parse(_currentTime[1]))
            {
                fs.LogInStatus.text = "LAST SEEN " + (Int32.Parse(_currentTime[1]) - Int32.Parse(_lastLogin[2])) + " MONTHS AGO";
                fs.ActivityIcon.color = fs.OfflineColor;
            }
            else if (Int32.Parse(_lastLogin[3]) != Int32.Parse(_currentTime[2]))
            {
                fs.LogInStatus.text = "LAST SEEN " + (Int32.Parse(_currentTime[2]) - Int32.Parse(_lastLogin[3])) + " DAYS AGO";
                fs.ActivityIcon.color = fs.OfflineColor;
            }
            else if (Int32.Parse(_lastLogin[4]) != Int32.Parse(_currentTime[3]))
            {
                fs.LogInStatus.text = "LAST SEEN " + (Int32.Parse(_currentTime[3]) - Int32.Parse(_lastLogin[4])) + " HOURS AGO";
                fs.ActivityIcon.color = fs.OfflineColor;
            }
            else if (Int32.Parse(_lastLogin[5]) != Int32.Parse(_currentTime[4]))
            {
                fs.LogInStatus.text = "LAST SEEN " + (Int32.Parse(_currentTime[4]) - Int32.Parse(_lastLogin[5])) + " MINS AGO";
                fs.ActivityIcon.color = fs.OfflineColor;
            }
            else
            {
                fs.LogInStatus.text = "ONLINE";
                fs.ActivityIcon.color = fs.OnlineColor;
            }
        }

        else
        {
            fs.LogInStatus.text = "NOT AVAILABLE";
            fs.ActivityIcon.color = fs.OfflineColor;
        }
    }

    */
    #endregion

    #region UI - Button

    public void SelectAvatarButton(int _index)
    {
        PlayfabConstants.Instance.PlayerAvatarIndex = _index;

        PlayerProfileScript.instance.SelectedBorderColor(_index);

        UpdatePlayerAvatarInUI();

        PlayfabPlayerProfile.SetPlayerProfilePicIndexAndLogInActivity(_index.ToString());

       // PlayfabController.Instance.UpdatePlayerProfilePicIndex(PlayfabController.Instance.profilePicIndex.ToString());

       // PlayfabController.Instance.SavePlayerInfo();
    }

    #endregion

    private string[] StringSplitter(string _input)
    {
        if (!string.IsNullOrEmpty(_input))
        {
            var _tempString = _input.Split('@');
            var _output = _tempString[0].Split('|');
            return _output;
        }
        else
            return null;
    }

    public void ShowFbLoginPanel()
    {
        fbLoginPanel.SetActive(true);
    }

    #endregion

    #region VERSION 0.2

    /// <summary>
    /// 
    /// </summary>
    public void UpdatePlayerNameInUI(string playerName = null)
    {
        if (playerName == null && PlayfabConstants.Instance != null) playerName = PlayfabConstants.Instance.PlayerName;

        if(!string.IsNullOrEmpty(playerName) && PlayfabConstants.Instance != null)
        {
            if(smallPlayerName != null)   smallPlayerName.text = playerName.Length > 12 ? playerName.Substring(0, 12) : playerName;
            if(leaderboardPlayerName != null)   leaderboardPlayerName.text = playerName;
            if(editPlayerNameText != null)  editPlayerNameText.text = playerName;

            if (PlayfabConstants.Instance != null) PlayfabConstants.Instance.PlayerName = playerName;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="url"></param>
    /// <param name="img"></param>
    /// <param name="isChached"></param>
    public void UpdateImgInUI(string url, Image img, bool isChached = false)
    {
        if (!string.IsNullOrEmpty(url) && img != null)
        {
            Debug.Log("|||" + url);
          //  Davinci.get().load(url).into(img).setCached(isChached).setFadeTime(0).withErrorAction(error => Debug.Log("123 Img "+ img.name+ " " +error)).start();
            Davinci.get().load(url).into(img).setCached(isChached).setFadeTime(0).start();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void UpdatePlayerAvatarInUI()
    {
        if (PlayfabConstants.Instance != null)
        {
            int _index = PlayfabConstants.Instance.PlayerAvatarIndex;
            string _avatarURL = PlayfabConstants.Instance.PlayerAvatarURL;

            if (_index == 0 && _avatarURL != null)
            {
                UpdateImgInUI(_avatarURL, smallPlayerProfilePic, true);
                UpdateImgInUI(_avatarURL, levelUpPlayerImage, true);
                UpdateImgInUI(_avatarURL, leaderboardPlayerProfilePic, true);
                UpdateImgInUI(_avatarURL, editPlayerPicImage, true);
            }

            else
            {
                smallPlayerProfilePic.sprite = _index != 0 ? localAvatars[_index - 1] : FB_Avatar.sprite;
                levelUpPlayerImage.sprite = _index != 0 ? localAvatars[_index - 1] : FB_Avatar.sprite;
                leaderboardPlayerProfilePic.sprite = _index != 0 ? localAvatars[_index - 1] : FB_Avatar.sprite;
                editPlayerPicImage.sprite = _index != 0 ? localAvatars[_index - 1] : FB_Avatar.sprite;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void VirtualCurrancyUpdate()
    {
        if (PlayFabClientAPI.IsClientLoggedIn() && Application.internetReachability != NetworkReachability.NotReachable)
        {
            PlayfabVirtualCurrency.GetAllVirtualCurrency(null, VirtualCurrancyUpdate);
        }
        else
        {
            Invoke(nameof(VirtualCurrancyUpdate), 3f);
        }
            
    }
    

    private void NewLeaderboardSystem()
    {
        if (PlayFabClientAPI.IsClientLoggedIn())
        {
            PlayfabLeaderboard.GetLeaderboardAroundPlayer(1, PlayerRankInLeaderBoard, () => { });
            PlayfabLeaderboard.GetGlobalLeaderboard(50, OnGlobalLeaderBoardResult, () => { });
            PlayfabLeaderboard.GetFriendsLeaderboard(50, OnFriendsLeaderBoardResult, () => { });
        }
    }

    private void OnGlobalLeaderBoardResult(GetLeaderboardResult obj)
    {
        if (obj == null) return;

        var _result = obj.Leaderboard;

        if(_result != null && _result.Count > 1)
        {
            DeactivatePlayerList(globalPlayerList);

            var _countNumber = globalPlayerList.Count < _result.Count ? globalPlayerList.Count : _result.Count;


            int panelHeight = (_countNumber * 60) - 8 + 10;

            if (panelHeight < 490)
                panelHeight = 490;

            rankParentPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, panelHeight);
            rankParentPanel.parent.GetComponent<ScrollRect>().normalizedPosition = new Vector2(0, 1);


            for (int i = 0; i< _countNumber; i++)
            {
                globalPlayerList[i].SetActive(true);
                LeaderboardRankPlayerScript leaderboardRankPlayerScript = globalPlayerList[i].GetComponent<LeaderboardRankPlayerScript>();

                if(leaderboardRankPlayerScript != null)
                {
                    leaderboardRankPlayerScript.SetPlayerInfo(_result[i].Profile);
                    leaderboardRankPlayerScript.playerDisplayName.text = _result[i].DisplayName;
                    leaderboardRankPlayerScript.RankNo.text = (i + 1).ToString();
                    leaderboardRankPlayerScript.playerCoinAmount.text = _result[i].StatValue.ToString();
                }

                if (i < 3)
                {
                    if (_result[i].StatValue > 1000000)
                    {
                        float shownAmount = _result[i].StatValue / 1000000f;
                        top3RankPlayerMoneyAmount[i].text = shownAmount.ToString("0.00") + "M";
                    }
                    else
                    {
                        top3RankPlayerMoneyAmount[i].text = _result[i].StatValue.ToString();
                    }

                  //  Davinci.get().load((string)_result[i].Profile.AvatarUrl).setFadeTime(0f).into(top3RankPlayerAvatar[i]).start();

                    if (_result[i].Profile.ContactEmailAddresses.Count > 0 && !string.IsNullOrEmpty(_result[i].Profile.ContactEmailAddresses[0].EmailAddress))
                    {
                        var _str = PlayfabConstants.Instance.StringSplitter(_result[i].Profile.ContactEmailAddresses[0].EmailAddress);

                        int _avatarIndex = Int32.Parse(_str[0]);

                        if (_avatarIndex != 0) top3RankPlayerAvatar[i].sprite = HazariPlayersCountryFlag.Instance.LocalAvatars[_avatarIndex - 1];
                        else
                        {
                            if (String.IsNullOrEmpty(_result[i].Profile.AvatarUrl)) top3RankPlayerAvatar[i].sprite = HazariPlayersCountryFlag.Instance.LocalAvatars[0];
                            else Davinci.get().load(_result[i].Profile.AvatarUrl).setFadeTime(0f).into(top3RankPlayerAvatar[i]).start();
                        }

                    }
                }

            }
        }
    }

    private void OnFriendsLeaderBoardResult(GetLeaderboardResult obj)
    {
        if (obj == null) return;

        var _result = obj.Leaderboard;

        if (_result != null && _result.Count > 1)
        {
            DeactivatePlayerList(friendsPlayerList);

            var _countNumber = friendsPlayerList.Count < _result.Count ? friendsPlayerList.Count : _result.Count;

            int panelHeight = (_countNumber * 60) - 8 + 10;

            if (panelHeight < 490)
                panelHeight = 490;

            friendRankParentPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, panelHeight);
            friendRankParentPanel.parent.GetComponent<ScrollRect>().normalizedPosition = new Vector2(0, 1);

            for (int i = 0; i < _countNumber; i++)
            {
                friendsPlayerList[i].SetActive(true);
                LeaderboardRankPlayerScript leaderboardRankPlayerScript = friendsPlayerList[i].GetComponent<LeaderboardRankPlayerScript>();

                if (leaderboardRankPlayerScript != null)
                {
                    leaderboardRankPlayerScript.SetPlayerInfo(_result[i].Profile);
                    leaderboardRankPlayerScript.playerDisplayName.text = _result[i].DisplayName;
                    leaderboardRankPlayerScript.RankNo.text = (i + 1).ToString();
                    leaderboardRankPlayerScript.playerCoinAmount.text = _result[i].StatValue.ToString();
                }
            }
        }
    }

    private void DeactivatePlayerList(List<GameObject> list)
    {
        if (list.Count > 0)
        {
            for(int i = 0; i< list.Count; i++)
            {
                if (list[i] != null)
                {
                    list[i].SetActive(false);
                }
            }
        }
    }

    #endregion

}
