using PlayFab.ClientModels;
using PlayFab;
using PlayFabKit;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PlayFabKit
{
    public class FriendsMockScript : MonoBehaviour
    {
        public Text No;
        public Text PlayerName;
        public Image PlayerAvatar;
        public string PlayfabID;
        public Image ActivityIcon;
        public Image CoinImg;
        public Text CoinAmountText;
        public Image PlayerCountryFlag;
        // public Button ChallengeButton;
        public Text ActivityText;

        [HideInInspector] public DateTime ServerTime;

        [Header("Colors")]
        [SerializeField] Color onlineColor;
        [SerializeField] Color offlineColor;

        private void OnEnable()
        {
            PlayfabFriendsInfo.OnOnlineFriendReload += AutoDestroy;
        }

        private void OnDisable()
        {
            PlayfabFriendsInfo.OnOnlineFriendReload -= AutoDestroy;
        }

        public void SetPlayerInfo(PlayerProfileModel profile)
        {
            if (profile == null) Destroy(gameObject);

            if (PlayfabConstants.Instance.MyPlayfabID != null && PlayfabID == PlayfabConstants.Instance.MyPlayfabID) Destroy(gameObject);


            if (profile.ContactEmailAddresses.Count <= 0) Destroy(gameObject);

            else
            {
                if (string.IsNullOrEmpty(profile.ContactEmailAddresses[0].EmailAddress)) Destroy(gameObject);

                else
                {
                    bool _isOnline = IsOnline(profile);
                    ActivityIcon.color = _isOnline ? onlineColor : offlineColor;

                    var _str = PlayfabConstants.Instance.StringSplitter(profile.ContactEmailAddresses[0].EmailAddress);

                    if (_str.Length < 7) Destroy(gameObject);

                    if (_isOnline) SetCurrentOnlineState(_str[6]);

                    PlayerName.text = profile.DisplayName;

                    int _avatarIndex = Int32.Parse(_str[0]);

                    if (_avatarIndex != 0) PlayerAvatar.sprite = HazariPlayersCountryFlag.Instance.LocalAvatars[_avatarIndex - 1];
                    else
                    {
                        if (String.IsNullOrEmpty(profile.AvatarUrl)) PlayerAvatar.sprite = HazariPlayersCountryFlag.Instance.LocalAvatars[0];
                        else Davinci.get().load(profile.AvatarUrl).setFadeTime(0f).into(PlayerAvatar).start();
                    }

                    PlayerCountryFlag.sprite = HazariPlayersCountryFlag.Instance.GetCountryFlag((int)profile.Locations[0].CountryCode);
                    //No.text = PlayfabConstants.Instance.OnlineFriendCounter.ToString();

                }
            }

        }

        private void SetCurrentOnlineState(string index)
        {
            switch (index)
            {
                case "0":
                    ActivityText.text = "In Lobby";
                    break;
                case "1":
                    ActivityText.text = "In Matchmaking";
                    break;
                case "2":
                    ActivityText.text = "Playing Hazari";
                    break;
                case "3":
                    ActivityText.text = "Playing NineCard";
                    break;
                case "4":
                    ActivityText.text = "Practicing Hazari";
                    break;
                case "5":
                    ActivityText.text = "Practicing NineCard";
                    break;
                default:
                    ActivityText.text = "Unavailable";
                    break;
            }

        }

        private bool IsOnline(PlayerProfileModel playerProfile)
        {
            if (string.IsNullOrEmpty(playerProfile.ContactEmailAddresses[0].EmailAddress)) return false;

            else
            {
                var _str = PlayfabConstants.Instance.StringSplitter(playerProfile.ContactEmailAddresses[0].EmailAddress);

                if (_str.Length < 7) return false;
                else
                {

                    string _lastLogin = _str[2] + "/" + _str[3] + "/" + _str[1] + " " + _str[4] + ":" + _str[5];
                    DateTime _lastLoginDateTime = DateTime.ParseExact(_lastLogin, "MM/dd/yyyy HH:mm", null);

                    Debug.Log("456 Last Login Time: " + _lastLoginDateTime);
                    Debug.Log("456 Servers Time: " + ServerTime);
                    Debug.Log("456 Server Time Difference" + (ServerTime - _lastLoginDateTime).TotalMinutes);


                    if ((ServerTime - _lastLoginDateTime).TotalSeconds <= 90) return true;

                    else
                    {
                        if((ServerTime - _lastLoginDateTime).TotalMinutes < 60)
                            ActivityText.text = Convert.ToInt32((ServerTime - _lastLoginDateTime).TotalMinutes) + " Minutes Ago";
                        else if ((ServerTime - _lastLoginDateTime).TotalHours < 24)
                            ActivityText.text = Convert.ToInt32((ServerTime - _lastLoginDateTime).TotalHours) + " Hours Ago";
                        else
                            ActivityText.text = "A While Ago";
                        return false;
                    }

                }
            }
        }


        public void AutoDestroy()
        {
            Destroy(this.gameObject);
        }
    }
}