#region Library
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System;
using PlayFab.ServerModels;
#endregion

namespace PlayFabKit
{
    public static class PlayfabPlayerProfile //: MonoBehaviour
    {

        public static event Action<string> OnSetPlayerName;     // PlayfabScript
        public static event Action OnSetAvatarURL;      // PlayfabScript
        public static String Index;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        public static void SetPlayerName(string name)
        {
            if (name.Length > 24) name = name.Substring(0, 24);

            if (PlayFabClientAPI.IsClientLoggedIn() && Application.internetReachability != NetworkReachability.NotReachable)
            {
                PlayFabClientAPI.UpdateUserTitleDisplayName(new UpdateUserTitleDisplayNameRequest
                {
                    DisplayName = name
                },
                onSuccess =>
                {
                    PlayfabConstants.Instance.PlayerName = name;
                    OnSetPlayerName?.Invoke(name);
                },
                OnUnsuccess =>
                {
                // can be used toast notification to notify player Name failed to change
                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        public static void SetAvatarURL(string url)
        {
            if (PlayFabClientAPI.IsClientLoggedIn() && Application.internetReachability != NetworkReachability.NotReachable)
            {
                PlayFabClientAPI.UpdateAvatarUrl(new PlayFab.ClientModels.UpdateAvatarUrlRequest
                {
                    ImageUrl = url
                },
                onSuccess =>
                {
                    PlayfabConstants.Instance.PlayerAvatarURL = url;
                    OnSetAvatarURL?.Invoke();
                },
                OnUnsuccess =>
                {
                    // can be used toast notification to notify avatar url failed to change
                });
            }
        }

        public static void SetPlayerProfilePicIndexAndLogInActivity(string index)
        {
            Index = index;
            PlayFabServerAPI.GetTime(new PlayFab.ServerModels.GetTimeRequest { }, OnGetTimeSuccess, LogFailure => { });

        }

        private static void OnGetTimeSuccess(PlayFab.ServerModels.GetTimeResult obj)
        {
            string _temp = obj.Time.ToString("|yyyy|MM|dd|HH|mm");

          //  string _temp = "|" + obj.Time.ToString() + ("|MM|dd|hh|mm");

            string _activeScene = "";
            if (PlayfabConstants.Instance != null)
            {
                _activeScene = PlayfabConstants.Instance.PlayerCurrentActivityState.ToString();
            }
            string _data = Index + _temp + "|" + _activeScene + "|" + "@ulka.com";

            if ( PlayFabClientAPI.IsClientLoggedIn())
            {
                PlayFabClientAPI.AddOrUpdateContactEmail(new AddOrUpdateContactEmailRequest
                    {
                        EmailAddress = _data
                    },
                    onSuccess =>
                    {
                        Debug.Log("777 Success");
                    },
                    onFailed =>
                    {

                    });
            }
            else Debug.Log("OnGetTimeSuccess() -> PlayFabClient not logged in ");
            
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_dataKeys"></param>
        /// <param name="_playerDataResult"></param>
        /// <param name="_playerDataResultError"></param>
        public static void GetPlayerData(List<string> _dataKeys, Action<PlayFab.ClientModels.GetUserDataResult> _playerDataResult, Action _playerDataResultError = null)
        {
            var request = new PlayFab.ClientModels.GetUserDataRequest
            {
                Keys = _dataKeys
            };
            PlayFabClientAPI.GetUserData(request, _playerDataResult,
                _OnError => 
                {
                    if (_playerDataResultError != null)
                        _playerDataResultError();
                });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_dataKeys"></param>
        /// <param name="_playerDataResult"></param>
        /// <param name="_playerDataResultError"></param>
        public static void GetPlayerReadOnlyData(List<string> _dataKeys, Action<PlayFab.ClientModels.GetUserDataResult> _playerDataResult, Action _playerDataResultError = null)
        {
            var request = new PlayFab.ClientModels.GetUserDataRequest
            {
                Keys = _dataKeys
            };
            PlayFabClientAPI.GetUserReadOnlyData(request, _playerDataResult,
                _OnError =>
                {
                    if (_playerDataResultError != null)
                        _playerDataResultError();
                });
        }

        /// <summary>
        /// 
        /// </summary>
        public static void SetPlayerData(Dictionary<string,string> _dataDic, Action _playerDataSetSuccess = null, Action _playerDataSetError = null)
        {
            var request = new PlayFab.ClientModels.UpdateUserDataRequest
            {
                Data = _dataDic,
                Permission = PlayFab.ClientModels.UserDataPermission.Public
            };

            PlayFabClientAPI.UpdateUserData(request, _OnSucess => 
            {
                if (_playerDataSetSuccess != null) _playerDataSetSuccess();
            },
            _OnError => 
            {
                if (_playerDataSetError != null) _playerDataSetError();
                Debug.Log("Failed To Update Player Data");
            }
            );
        }

        /// <summary>
        /// 
        /// </summary>
        public static void GetPlayerProfile(Action<PlayFab.ClientModels.GetPlayerProfileResult> _playerProfileData, Action _playerProfileResultFailed = null, string playfabId = null)
        {
            var request = new PlayFab.ClientModels.GetPlayerProfileRequest
            {
                ProfileConstraints = new PlayFab.ClientModels.PlayerProfileViewConstraints
                {
                    ShowAvatarUrl = true,
                    ShowDisplayName = true,
                    ShowLocations = true,
                    ShowLastLogin = true,
                    ShowContactEmailAddresses = true

                },
                PlayFabId = playfabId
            };

            PlayFabClientAPI.GetPlayerProfile(request, _playerProfileData, _OnError => 
            {
                if (_playerProfileResultFailed != null) _playerProfileResultFailed();
            });
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="_input"></param>
        /// <returns></returns>
        public static string[] StringSplitter(string _input)
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