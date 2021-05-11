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
    public class OnlineFriendMock : MonoBehaviour
    {
        public Text No;
        public Text PlayerName;
        public Image PlayerAvatar;
        [HideInInspector] public string PlayfabID;
        public Image ActivityIcon;
        public Image CoinImg;
        public Text CoinAmountText;
        public Image PlayerCountryFlag;
        public Button ChallengeButton;
        public Text ChallengeButtonText;
        public Text PlayerActivityStatus;
        [HideInInspector] public DateTime ServerTime;
        [HideInInspector] public bool IsOnline;
        private int totalCoinAmount;



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

            ChallengeButtonText.text = IsOnline ? "Challenge" : "Invite";

            if (PlayfabConstants.Instance.MyPlayfabID != null && PlayfabID == PlayfabConstants.Instance.MyPlayfabID) Destroy(gameObject);


            if (profile.ContactEmailAddresses.Count <= 0)  Destroy(gameObject);

            else
            {
                if (string.IsNullOrEmpty(profile.ContactEmailAddresses[0].EmailAddress)) Destroy(gameObject);

                else
                {
                    var _str = PlayfabConstants.Instance.StringSplitter(profile.ContactEmailAddresses[0].EmailAddress);

                  //  if (!(_str[6] == "0" || _str[6] == "4" || _str[6] == "5")) { }// Destroy(gameObject);
                  //else
                    if(true)
                    {
                        //if (IsOnline && !(_str[6] == "0" || _str[6] == "4" || _str[6] == "5")) { ChallengeButton.gameObject.SetActive(false); }
                        PlayerName.text = profile.DisplayName;

                        int _avatarIndex = Int32.Parse(_str[0]);

                        if (_avatarIndex != 0) PlayerAvatar.sprite = HazariPlayersCountryFlag.Instance.LocalAvatars[_avatarIndex - 1];
                        else
                        {
                            if (String.IsNullOrEmpty(profile.AvatarUrl)) PlayerAvatar.sprite = HazariPlayersCountryFlag.Instance.LocalAvatars[0];
                            else Davinci.get().load(profile.AvatarUrl).setFadeTime(0f).into(PlayerAvatar).start();
                        }

                        PlayerCountryFlag.sprite = HazariPlayersCountryFlag.Instance.GetCountryFlag((int)profile.Locations[0].CountryCode);
                        No.text = PlayfabConstants.Instance.OnlineFriendCounter.ToString();
                        PlayfabConstants.Instance.OnlineFriendCounter++;

                        if (IsOnline && _str.Length>=7)
                        {
                            SetCurrentOnlineState(_str[6]);
                        }
                        else
                        {
                            string _lastLogin = _str[2] + "/" + _str[3] + "/" + _str[1] + " " + _str[4] + ":" + _str[5];
                            DateTime _lastLoginDateTime = DateTime.ParseExact(_lastLogin, "MM/dd/yyyy HH:mm", null);

                            if ((ServerTime - _lastLoginDateTime).TotalMinutes < 60)
                            {
                                PlayerActivityStatus.text = Convert.ToInt32((ServerTime - _lastLoginDateTime).TotalMinutes) + " Minutes Ago";
                                ChallengeButton.gameObject.SetActive(true);
                            }
                            else if ((ServerTime - _lastLoginDateTime).TotalHours < 24)
                            {
                                PlayerActivityStatus.text = Convert.ToInt32((ServerTime - _lastLoginDateTime).TotalHours) + " Hours Ago";
                                ChallengeButton.gameObject.SetActive(true);
                            }
                            else
                            {
                                PlayerActivityStatus.text = "A While Ago";
                                ChallengeButton.gameObject.SetActive(true);
                            }
                        }
                    }

                }
            }

            
        }


        private void SetCurrentOnlineState(string index)
        {
            switch (index)
            {
                case "0":
                    PlayerActivityStatus.text = "In Lobby";
                    ChallengeButton.gameObject.SetActive(true);
                    break;
                case "1":
                    PlayerActivityStatus.text = "In Matchmaking";
                    ChallengeButton.gameObject.SetActive(false);
                    break;
                case "2":
                    PlayerActivityStatus.text = "Playing Hazari";
                    ChallengeButton.gameObject.SetActive(false);
                    break;
                case "3":
                    PlayerActivityStatus.text = "Playing NineCard";
                    ChallengeButton.gameObject.SetActive(false);
                    break;
                case "4":
                    PlayerActivityStatus.text = "Practicing Hazari";
                    ChallengeButton.gameObject.SetActive(true);
                    break;
                case "5":
                    PlayerActivityStatus.text = "Practicing NineCard";
                    ChallengeButton.gameObject.SetActive(true);
                    break;
                default:
                    PlayerActivityStatus.text = "Unavailable";
                    ChallengeButton.gameObject.SetActive(true);
                    break;
            }

        }

        public void CoinTextHandler(int totalCoin)
        {
            totalCoinAmount = totalCoin;
            string _coinString = totalCoin.ToString();

            if(_coinString.Length >= 7)
            {
                CoinAmountText.text = _coinString.Substring(0, _coinString.Length - 6) + "." + _coinString[_coinString.Length - 6] + "M";
            }
            else if(_coinString.Length >= 4)
            {
                CoinAmountText.text = _coinString.Substring(0, _coinString.Length - 3) + "." + _coinString[_coinString.Length - 3] + "K";
            }
            else
            {
                CoinAmountText.text = _coinString;
            }
        }

        public void AutoDestroy()
        {
            Destroy(this.gameObject);
        }


    }
}