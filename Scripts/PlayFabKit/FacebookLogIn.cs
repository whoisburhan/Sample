using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Facebook.Unity;
using PlayFabKit;
using System;
using Lobby;

namespace PlayFabKit
{
    public class FacebookLogIn : MonoBehaviour
    {
        protected string fbToken = "";

        [Tooltip("Permission list asking to Facebook")]
        private List<string> permissions = new List<string>() { "public_profile", "email", "user_friends" };

        [Header("Facebook Data")]
        protected string fbId = "";
        protected string fbName = "";
        protected string fbEmail = "";
        protected string fbAvatarURL = "";

        protected Dictionary<string, string> fbDataDic = new Dictionary<string, string>();

        private void Start()
        {
            if(!FB.IsInitialized)   FB.Init();
        }

        /// <summary>
        /// 
        /// </summary>
        public void FaceBookLogInByButtonFunc()
        {
            if (!FB.IsLoggedIn)
            {
                if (FB.IsInitialized) OnFacebookInitialized();
                else FB.Init(OnFacebookInitialized);
            }
            else
            {
                OnFacebookLoggedIn();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnFacebookInitialized()
        {
            if (LobbyUIManager.instance != null)
            {
                Invoke(nameof(ProfileSyncStart), 1f);
            }
            FB.ActivateApp();
            FB.LogInWithReadPermissions(permissions, OnFacebookLoggedIn);
        }

        void ProfileSyncStart()
        {
            LobbyUIManager.instance.ProfileSyncAnimStart();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="result"></param>
        protected virtual void OnFacebookLoggedIn(ILoginResult result = null)
        {
            if (AccessToken.CurrentAccessToken != null &&
                !String.IsNullOrEmpty(AccessToken.CurrentAccessToken.TokenString))
            {
                fbToken = AccessToken.CurrentAccessToken.TokenString;
                ReadFBData();
            }
        }

        private void ReadFBData()
        {
            FB.API("me?fields=id,name,email,picture.width(256).height(256)", HttpMethod.GET, FacebookData);
        }

        protected virtual void FacebookData(IGraphResult result)
        {
            if (result.Error != null) return;

            if (result.ResultDictionary.ContainsKey("name"))
            {
                fbName = result.ResultDictionary["name"].ToString();
                fbDataDic["FB Name"] = fbName;
            }

            if (result.ResultDictionary.ContainsKey("email"))
            {
                fbEmail = result.ResultDictionary["email"].ToString();
                fbDataDic["Email"] = fbEmail;
            }

            if (result.ResultDictionary.ContainsKey("id"))
            {
                fbId = result.ResultDictionary["id"].ToString();
                fbDataDic["FB ID"] = fbId;
            }

            if (result.ResultDictionary.ContainsKey("picture"))
                fbAvatarURL = ((Dictionary<string, object>)((Dictionary<string, object>)result.ResultDictionary[
                            "picture"])["data"])["url"].ToString();
        }
    }
}
