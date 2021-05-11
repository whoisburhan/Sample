#region Library
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TAAASH_KIT;
using Lobby;
using System;
#endregion
namespace PlayFabKit
{
    public class PlayfabConstants : MonoBehaviour
    {
        public static PlayfabConstants Instance { get; private set; }

        [Tooltip("PlayerCurrentActivityState [0 = Lobby Idle, 1 = In Matchmaking Group, 2 = Playing Hazari MP, 3 = Playing Nine-Card MP")]
        [HideInInspector] public int PlayerCurrentActivityState = 0;
        [HideInInspector] public List<string> MyGroupMemberList = new List<string>();

        public const string FacebookTokenKey = "FB_TOKEN";
        public const string PlayerNameKey = "PLAYER_NAME";
        public const string PlayerAvatarIndexKey = "PLAYER_AVATAR_INDEX";
        public const string PlayerAvatarURLKey = "AVATAR_URL";
        public const string PlayerGlobalRankKey = "PLAYER_GLOBAL_RANK";
        public const string PlayerCountryCodeKey = "PLAYER_COUNTRY_CODE";
        public string RoomId = "";

        public string MyPlayfabID { get; set; }
        public string MyGroupID { get; set; }
        public string FourDigitRandomNumber { get; set; }
        public bool IsRandomMatchMaking { get; set; }
        
        public bool IsPrivateTable { get; set; }
        public bool IsCreateTable { get; set; }
        public bool IsAllowedToSendInvitation { get; set; }
        public int TableAmount { get; set; }
        public int OnlineGameType { get; set; }
        public int OnlineFriendCounter { get; set; }
        

        private string playerName = "";
        private string playerAvatarUrl = "";
        private int playerAvatarIndex = 6;
        private string facebookToken = "";
        private int playerGlobalRank = 0;
        private int playerCountryCode = 18; // Bangladesh

        private float playerActivityStatusUpdateTimer = 0f;
        private float playerActivityStatusSendingTimeInterval = 5f;

        [HideInInspector]
        public string PlayerName
        {
            get
            {
                return playerName;
            }
            set
            {
                this.playerName = value;
                PlayerPrefs.SetString(PlayerNameKey, value);
            }
        }

        
        [HideInInspector]
        public int PlayerAvatarIndex
        {
            get
            {
                return playerAvatarIndex;
            }
            set
            {
                this.playerAvatarIndex = value;
                PlayerPrefs.SetInt(PlayerAvatarIndexKey, value);
            }
        }

        [HideInInspector]
        public string PlayerAvatarURL
        {
            get
            {
                return playerAvatarUrl;
            }
            set
            {
                this.playerAvatarUrl = value;
                PlayerPrefs.SetString(PlayerAvatarURLKey, value);
            }
        }

        [HideInInspector]
        public string FacebookToken
        {
            get
            {
                return facebookToken;
            }
            set
            {
                this.facebookToken = value;
                PlayerPrefs.SetString(FacebookTokenKey, value);
            }
        }

        [HideInInspector]
        public int PlayerGlobalRank
        {
            get
            {
                return playerGlobalRank;
            }
            set
            {
                this.playerGlobalRank = value;
                PlayerPrefs.SetInt(PlayerGlobalRankKey, value);
            }
        }

        [HideInInspector]
        public int PlayerCountryCode
        {
            get
            {
                return playerCountryCode;
            }
            set
            {
                this.playerCountryCode = value;
                PlayerPrefs.SetInt(PlayerCountryCodeKey, value);
            }
        }

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
           
            playerName = PlayerPrefs.GetString(PlayerNameKey, "Guest" + UnityEngine.Random.Range(1000, 9999));
            playerAvatarUrl = PlayerPrefs.GetString(PlayerAvatarURLKey, "");
            playerAvatarIndex = PlayerPrefs.GetInt(PlayerAvatarIndexKey, 1);
            facebookToken = PlayerPrefs.GetString(FacebookTokenKey, "");
            playerGlobalRank = PlayerPrefs.GetInt(PlayerGlobalRankKey, 0);
            playerCountryCode = PlayerPrefs.GetInt(PlayerCountryCodeKey, 18); //18 - Bangladesh
            playerActivityStatusUpdateTimer = playerActivityStatusSendingTimeInterval;
            IsRandomMatchMaking = true;
            OnlineFriendCounter = 1;
        }

        private void Update()
        {
            if(playerActivityStatusUpdateTimer <= 0 && PlayFabClientAPI.IsClientLoggedIn())
            {
                UpdatePlayerActivityState();
                playerActivityStatusUpdateTimer = playerActivityStatusSendingTimeInterval;
            }

            playerActivityStatusUpdateTimer -= Time.deltaTime;
        }

        private void UpdatePlayerActivityState()
        {
            if (PlayFabClientAPI.IsClientLoggedIn())
            {
                PlayfabPlayerProfile.GetPlayerProfile(PlayerProfileResult);
            }
        }

        private void PlayerProfileResult(GetPlayerProfileResult result)
        {
            if(result.PlayerProfile.ContactEmailAddresses.Count > 0)
            {
                if (!String.IsNullOrEmpty(result.PlayerProfile.ContactEmailAddresses[0].EmailAddress))
                {
                    string[] _str = StringSplitter(result.PlayerProfile.ContactEmailAddresses[0].EmailAddress);

                    //int _index = Int32.Parse(_str[0]);

                    PlayfabPlayerProfile.SetPlayerProfilePicIndexAndLogInActivity(_str[0]);
                }
            }
            else
                PlayfabPlayerProfile.SetPlayerProfilePicIndexAndLogInActivity("0");
        }
        
        public void SetProfileGameProfilSystemForFriendlyMatch()
        {
            if (OnlineGameType == 0)
            {
                LobbyUIManager.instance.SetGameProfileSystemForFriendlyMatch(GameType.HAZARI, TableAmount);
            }
            else
            {
                LobbyUIManager.instance.SetGameProfileSystemForFriendlyMatch(GameType.NINE_CARD, TableAmount);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_input"></param>
        /// <returns></returns>
        public string[] StringSplitter(string _input)
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
    }
}