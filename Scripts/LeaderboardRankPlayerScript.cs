using PlayFab.ClientModels;
using PlayFabKit;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardRankPlayerScript : MonoBehaviour
{
    public Text RankNo;
    public Image playerProfilePic;
    public Text playerDisplayName;
    public Text playerCoinAmount;
    public Text playerActivityStatus;
    public Image CountryFlag;
    public string PlayFabID;

    private void OnEnable()
    {
        PlayFabScript.OnLeaderboardReload += AutoDestroy;
    }

    private void OnDisable()
    {
        PlayFabScript.OnLeaderboardReload -= AutoDestroy;
    }

    private void AutoDestroy()
    {
        gameObject.SetActive(false);
    }

    public void SetPlayerInfo(PlayerProfileModel profile)
    {
        if (profile == null) Destroy(gameObject);

        if (profile.ContactEmailAddresses.Count > 0 && !string.IsNullOrEmpty(profile.ContactEmailAddresses[0].EmailAddress))
        {
            var _str = PlayfabConstants.Instance.StringSplitter(profile.ContactEmailAddresses[0].EmailAddress);


          //  if (string.IsNullOrEmpty(profile.DisplayName))
          //      playerDisplayName.text = profile.DisplayName;

            int _avatarIndex = Int32.Parse(_str[0]);

            if (_avatarIndex != 0) playerProfilePic.sprite = HazariPlayersCountryFlag.Instance.LocalAvatars[_avatarIndex - 1];
            else
            {
                if (String.IsNullOrEmpty(profile.AvatarUrl)) playerProfilePic.sprite = HazariPlayersCountryFlag.Instance.LocalAvatars[0];
                else Davinci.get().load(profile.AvatarUrl).setFadeTime(0f).into(playerProfilePic).start();
            }

        }

        CountryFlag.sprite = HazariPlayersCountryFlag.Instance.GetCountryFlag((int)profile.Locations[0].CountryCode);
    }
}
