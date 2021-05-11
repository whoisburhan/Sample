using Firebase.Messaging;
using PlayFab;
using PlayFab.ClientModels;
using PlayFabKit;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameInvitationUI : MonoBehaviour
{
    public Text InvitationSender;
    public Image InvitationSendersAvatar;
    public Text GameType;
    public Text TableAmount;

    public Button IgnoreButton;
    public Button AcceptButton;

    public Button CloseButton;

    private int tableAmountCoin = 0;
    private string fourDigitRandomNumberFromServer = null;

    [SerializeField] private String GrpID;
    private int gameType = 0;

    private void Start()
    {
        AcceptButton.onClick.AddListener(()=>
        {
            GameInvitationReceiver.Instance.CheckForInvitation = false;

            Invoke("WaitForSomeTime",2f);

            if (PlayFabClientAPI.IsClientLoggedIn() && !(Application.internetReachability == NetworkReachability.NotReachable))
            {
                PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
                {
                    FunctionName = "AcceptInvitation",
                    FunctionParameter = new { GroupID = GrpID },
                    GeneratePlayStreamEvent = true
                }, _OnSuccess =>
                {
                    Debug.Log("AC 4");
                    PlayfabConstants.Instance.MyGroupID = GrpID;
                    PlayfabConstants.Instance.IsRandomMatchMaking = false;
                    PlayfabConstants.Instance.TableAmount = tableAmountCoin;
                    PlayfabConstants.Instance.OnlineGameType = gameType;
                    PlayfabConstants.Instance.FourDigitRandomNumber = fourDigitRandomNumberFromServer;
                    PlayfabConstants.Instance.SetProfileGameProfilSystemForFriendlyMatch();
                    SceneLoader.instance.LoadMatchMakingScene("RandomMatchMaking");
                    gameObject.SetActive(false);
                }, OnFailed =>
                {
                    Debug.Log("AC 4 fail but working");
                    gameObject.SetActive(false);
                });
            }
        });

        IgnoreButton.onClick.AddListener(() => 
        {
            GameInvitationReceiver.Instance.CheckForInvitation = true;
            PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
            {
                FunctionName = "ResetInvitation",
                FunctionParameter = new { },
                GeneratePlayStreamEvent = true
            }, OnSuccess => { }, OnFailed => { });
            this.gameObject.SetActive(false);
        });
        
        CloseButton.onClick.AddListener(() => 
        {
            GameInvitationReceiver.Instance.CheckForInvitation = true;
            PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
            {
                FunctionName = "ResetInvitation",
                FunctionParameter = new { },
                GeneratePlayStreamEvent = true
            }, OnSuccess => { }, OnFailed => { });
            this.gameObject.SetActive(false);
        });
        
    }

    private void WaitForSomeTime()
    {
        if(PlayfabConstants.Instance.PlayerCurrentActivityState == 0)
            GameInvitationReceiver.Instance.CheckForInvitation = true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="result"></param>
    public void SetGameInvitationInfo(FirebaseMessage result/*GetUserDataResult result*/)
    {
        if (result == null) return;

        if (result.Data.ContainsKey("FourDigitRandomNumber"))
        {
            if (!string.IsNullOrEmpty(result.Data["FourDigitRandomNumber"]))
            {
                fourDigitRandomNumberFromServer = result.Data["FourDigitRandomNumber"];
                Debug.Log("4444 Four Digit Random No : " + result.Data["FourDigitRandomNumber"]);
            }
        }

        if (result.Data.ContainsKey("GROUP_ID"))
        {
            if (!string.IsNullOrWhiteSpace(result.Data["GROUP_ID"]))
            {
                GrpID = result.Data["GROUP_ID"];
               
            }
        }


        if (result.Data.ContainsKey("GAME REQUEST"))
        {
            if(!string.IsNullOrWhiteSpace(result.Data["GAME REQUEST"]))
            {
                PlayfabPlayerProfile.GetPlayerProfile(SetSenderInfo, null, result.Data["GAME REQUEST"]);
            }
        }

        if (result.Data.ContainsKey("CoinAmount"))
        {
            Debug.Log("789 COIN AMOUNT :" + result.Data["CoinAmount"]);

            if (!string.IsNullOrEmpty(result.Data["CoinAmount"]))
            {
                Debug.Log("789 COIN AMOUNT :" + result.Data["CoinAmount"]);
                
                TableAmount.text = result.Data["CoinAmount"] + " TABLE";
                tableAmountCoin = Int32.Parse(result.Data["CoinAmount"]);
            }
        }

        if (result.Data.ContainsKey("GameType"))
        {
            if (!string.IsNullOrEmpty(result.Data["GameType"]))
            {
                GameType.text = result.Data["GameType"] == "0" ? "Hazari Multiplayer On" : "Nine Card Multiplayer On";
                gameType = Int32.Parse(result.Data["GameType"]);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="obj"></param>
    private void SetSenderInfo(GetPlayerProfileResult obj)
    {
        if (!String.IsNullOrEmpty(obj.PlayerProfile.DisplayName)) InvitationSender.text = obj.PlayerProfile.DisplayName;

        if (obj.PlayerProfile.ContactEmailAddresses.Count > 0) { 

            Debug.Log("000 Contact email > 0");
            var _str = obj.PlayerProfile.ContactEmailAddresses[0].EmailAddress;

            Debug.Log("000 Email Address :" + obj.PlayerProfile.ContactEmailAddresses[0].EmailAddress);

            if (!String.IsNullOrEmpty(_str))
            {
                var _str1 = PlayfabPlayerProfile.StringSplitter(_str);

                Debug.Log("000 not null");

                if (_str1 != null && _str.Length>= 7)
                {
                    int _avatarIndex = Int32.Parse(_str1[0]);
                    Debug.Log("000 Avatar Index" + _avatarIndex);

                    if (_avatarIndex != 0) InvitationSendersAvatar.sprite = HazariPlayersCountryFlag.Instance.GetAvatarSprite(_avatarIndex);
                    else
                    {
                        Debug.Log("000 Facebook Image");
                    if (!String.IsNullOrEmpty(obj.PlayerProfile.AvatarUrl))
                    {
                        Debug.Log("000 Avatar Url :" + obj.PlayerProfile.AvatarUrl);
                        Davinci.get().load(obj.PlayerProfile.AvatarUrl).setFadeTime(0f).into(InvitationSendersAvatar).start();
                    }
                    else InvitationSendersAvatar.sprite = HazariPlayersCountryFlag.Instance.GetAvatarSprite(1);
                    }
                }

                else InvitationSendersAvatar.sprite = HazariPlayersCountryFlag.Instance.GetAvatarSprite(1);

            }

            else InvitationSendersAvatar.sprite = HazariPlayersCountryFlag.Instance.GetAvatarSprite(1);
        }
        else InvitationSendersAvatar.sprite = HazariPlayersCountryFlag.Instance.GetAvatarSprite(1);

    }
}
