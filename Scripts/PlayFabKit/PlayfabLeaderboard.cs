#region Library
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System;
#endregion

namespace PlayFabKit
{
    public static class PlayfabLeaderboard
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="count"></param>
        /// <param name="leaderboardResult"></param>
        /// <param name="OnFailedToLoad"></param>
        /// <param name="leaderboardName"></param>
        public static void GetGlobalLeaderboard(int count, Action<GetLeaderboardResult> leaderboardResult, Action OnFailedToLoad = null, string leaderboardName = "TotalCurrencyBased_Global")
        {
            if (PlayFabClientAPI.IsClientLoggedIn())
            {
                var request = new GetLeaderboardRequest
                {
                    StatisticName = leaderboardName,
                    StartPosition = 0,
                    MaxResultsCount = count
                };

                request.ProfileConstraints = new PlayerProfileViewConstraints();
                request.ProfileConstraints.ShowAvatarUrl = true;
                request.ProfileConstraints.ShowDisplayName = true;
                request.ProfileConstraints.ShowLocations = true;
                request.ProfileConstraints.ShowContactEmailAddresses = true;

                PlayFabClientAPI.GetLeaderboard(request, leaderboardResult, _OnError =>
                {
                    if (OnFailedToLoad != null)
                        OnFailedToLoad();
                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="count"></param>
        /// <param name="leaderboardResult"></param>
        /// <param name="OnFailedToLoad"></param>
        /// <param name="leaderboardName"></param>
        public static void GetFriendsLeaderboard(int count, Action<GetLeaderboardResult> leaderboardResult, Action OnFailedToLoad = null, string leaderboardName = "TotalCurrencyBased_Global")
        {
            if (PlayFabClientAPI.IsClientLoggedIn())
            {
                var request = new GetFriendLeaderboardRequest
                {
                    StatisticName = leaderboardName,
                    StartPosition = 0,
                    MaxResultsCount = count,
                    IncludeFacebookFriends = true
                };

                request.ProfileConstraints = new PlayerProfileViewConstraints();
                request.ProfileConstraints.ShowAvatarUrl = true;
                request.ProfileConstraints.ShowDisplayName = true;
                request.ProfileConstraints.ShowLocations = true;
                request.ProfileConstraints.ShowContactEmailAddresses = true;

                PlayFabClientAPI.GetFriendLeaderboard(request, leaderboardResult, _OnError =>
                {
                    Debug.Log("567 " + _OnError.ErrorMessage);
                    if (OnFailedToLoad != null)
                        OnFailedToLoad();
                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="count"></param>
        /// <param name="leaderboardResult"></param>
        /// <param name="OnFailedToLoad"></param>
        /// <param name="leaderboardName"></param>
        public static void GetLeaderboardAroundPlayer(int count, Action<GetLeaderboardAroundPlayerResult> leaderboardResult, Action OnFailedToLoad = null, string leaderboardName = "TotalCurrencyBased_Global")
        {
            if (PlayFabClientAPI.IsClientLoggedIn())
            {
                var request = new GetLeaderboardAroundPlayerRequest
                {
                    StatisticName = "TotalCurrencyBased_Global",
                    MaxResultsCount = count
                };

                request.ProfileConstraints = new PlayerProfileViewConstraints();
                request.ProfileConstraints.ShowAvatarUrl = true;
                request.ProfileConstraints.ShowDisplayName = true;
                request.ProfileConstraints.ShowLocations = true;
                request.ProfileConstraints.ShowContactEmailAddresses = true;

                PlayFabClientAPI.GetLeaderboardAroundPlayer(request, leaderboardResult, op =>
                {
                    if (OnFailedToLoad != null)
                        OnFailedToLoad();
                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public static void SendLeaderboard(int value, Action onSucess = null, Action onFailed = null)
        {
            if (PlayFabClientAPI.IsClientLoggedIn())
            {
                PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
                {
                    FunctionName = "SendLeaderboard",
                    FunctionParameter = new { value = value },
                    GeneratePlayStreamEvent = true
                },
               cloudResult =>
               {
                   if (onSucess != null) onSucess();
               },
               failedToSend =>
               {
                   if (onFailed != null) onFailed();
               });
            }
        }
    }
}