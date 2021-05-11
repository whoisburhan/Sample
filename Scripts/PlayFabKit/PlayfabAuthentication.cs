#region Library
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using LoginResult = PlayFab.ClientModels.LoginResult;

using PlayFabKit;
using System;
using Lobby;

#endregion

namespace PlayFabKit
{
    public class PlayfabAuthentication : MonoBehaviour
    {
        //public static PlayfabAuthentication Instance { get; private set; }

        private bool retryToLogIn = false;

        private IEnumerator Start()
        {
            yield return new WaitForSeconds(1f);
            LogInToPlayfab();
        }

        private void Update()
        {
            if(Application.internetReachability != NetworkReachability.NotReachable && !PlayFabClientAPI.IsClientLoggedIn() && retryToLogIn)
            {
                LogInToPlayfab();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void LogInToPlayfab()
        {
            if (PlayerPrefs.HasKey(PlayfabConstants.FacebookTokenKey))
                LogInWithFacebookID();
            else
                LogInWithCustomID();
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void LogInWithCustomID()
        {
            retryToLogIn = false;
            var request = new LoginWithCustomIDRequest
            {
                CustomId = SystemInfo.deviceUniqueIdentifier,
                CreateAccount = true
            };
            PlayFabClientAPI.LoginWithCustomID(request, OnLogInWithCustomIDSuccess, OnLogInWithCustomIDFailed);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        protected virtual void OnLogInWithCustomIDSuccess(LoginResult obj) { retryToLogIn = true; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        protected virtual void OnLogInWithCustomIDFailed(PlayFabError obj) { retryToLogIn = true; }


        /// <summary>
        /// 
        /// </summary>
        public virtual void LogInWithFacebookID(string _token = null)
        {
            retryToLogIn = false;
            if (string.IsNullOrEmpty(_token) && PlayfabConstants.Instance != null)
                _token = PlayfabConstants.Instance.FacebookToken;

            if (!String.IsNullOrEmpty(_token))
            {
                var request = new LoginWithFacebookRequest
                {
                    CreateAccount = true,
                    AccessToken = _token
                };

                PlayFabClientAPI.LoginWithFacebook(request, OnLogInWithFacebookSuccess, OnLogInWithFacebookFailed);
                if (LobbyUIManager.instance != null)
                {
                    Invoke(nameof(ProfileSyncStart), 1f);
                }
                
            }
            else
            {
                LogInWithCustomID();
            }
        }

        void ProfileSyncStart()
        {
            LobbyUIManager.instance.ProfileSyncAnimStart();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        protected virtual void OnLogInWithFacebookSuccess(LoginResult obj)
        {
            LobbyUIManager.instance.ShowRealCoinText();
            retryToLogIn = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        protected virtual void OnLogInWithFacebookFailed(PlayFabError obj) { retryToLogIn = true; }

        /// <summary>
        /// 
        /// </summary>
        public void UnlinkCustomID(Action _onUnlinkSucess = null, Action _onUnlinkFailed = null)
        {
            PlayFabClientAPI.UnlinkCustomID(new UnlinkCustomIDRequest { },
            OnUnlinkSucess =>
            {
                if (_onUnlinkSucess != null) _onUnlinkSucess();
                Debug.Log("Successfully Unlink custom ID");
            },
            OnUnlinkError =>
            {
                if (_onUnlinkFailed != null) _onUnlinkFailed();
                Debug.Log("Failed to Unlink custom ID");
            });
        }
    }
}
