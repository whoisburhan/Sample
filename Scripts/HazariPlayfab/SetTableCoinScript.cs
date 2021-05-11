using PlayFab;
using PlayFab.ClientModels;
using PlayFabKit;
using System;
using System.Collections;
using System.Collections.Generic;
using Lobby;
using TAAASH_KIT;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class SetTableCoinScript : MonoBehaviour
{
    public Button SetTableButton;
    public Button CreateTableButton;

    [SerializeField] private GameObject contentHolder;
    
    [SerializeField] private Text coinAmountText;
    [SerializeField] private Button incrementButton;
    [SerializeField] private Button hazariGameButton;
    [SerializeField] private Button ninecardGameButton;
    [SerializeField] private Button decrementButton;
    [SerializeField] private GameObject[] hazariGameSelectionStates; // 0 for non selected, 1 for selected
    [SerializeField] private GameObject[] ninecardGameSelectionStates;

    [HideInInspector] public OnlineFriendMock friendInfo;

    public List<int> HazariAmountSlot = new List<int> { 250, 500, 1000, 2500, 5000, 10000 };
    public List<int> NineCardAmountSlot = new List<int> { 10, 25, 50, 100, 250, 500 };

    public int OnlineGameType = 0; // hazari = 0, nineCard = 1

    public int Index = 0;
    private int n;

    private void Start()
    {
        coinAmountText.text = HazariAmountSlot[Index].ToString();
        n = valueOf_N();

        if (PlayfabConstants.Instance.IsPrivateTable)
        {
            SetTableButton.gameObject.SetActive(false);
            CreateTableButton.gameObject.SetActive(true);
        }
        else
        {
            SetTableButton.gameObject.SetActive(true);
            CreateTableButton.gameObject.SetActive(false);
        }

        OnHazariSelect();
        
        hazariGameButton.onClick.AddListener(() =>
        {
            OnHazariSelect();
        });
        
        ninecardGameButton.onClick.AddListener(() =>
        {
            OnNineCardSelect();
        });

        incrementButton.onClick.AddListener(()=> 
        {
            if(Index < n - 1)
            {
                Index++;
                coinAmountText.text = OnlineGameType == 0 ? HazariAmountSlot[Index].ToString() : NineCardAmountSlot[Index].ToString();
            }
        });

        decrementButton.onClick.AddListener(() =>
        {
            if (Index > 0)
            {
                Index--;
                coinAmountText.text = OnlineGameType == 0 ? HazariAmountSlot[Index].ToString() : NineCardAmountSlot[Index].ToString();
            }
        });

        SetTableButton.onClick.AddListener(() =>
        {
            if (UserHaveEnoughCoinToSendRequest())
            { 
                contentHolder.SetActive(false);
                ToastNotification.instance.Show("Game Request Sent");
                Debug.Log("456 HazariAmountSlot[Index] : " + HazariAmountSlot[Index]);
                PlayfabConstants.Instance.MyGroupID = PlayfabConstants.Instance.MyPlayfabID;

                PlayfabConstants.Instance.FourDigitRandomNumber = UnityEngine.Random.Range(1000, 9999).ToString();

                if (friendInfo != null)
                {
                    PlayFabServerAPI.GetTime(new PlayFab.ServerModels.GetTimeRequest { }, OnGetTimeSuccess =>
                    {
                        #region comment
                        // DateTime.Now.ToString("yyyy-MM-dd-HH:mm:ss");
                         PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
                         {
                             FunctionName = "InvitationRequest",
                             FunctionParameter = new { GroupID = PlayfabConstants.Instance.MyPlayfabID, FriendsPlayfabID = friendInfo.PlayfabID, RequestTime = OnGetTimeSuccess.Time.ToString("MM/dd/yyyy HH:mm:ss"), CoinAmount = GetTableCoinAmount(OnlineGameType, Index), OnlineGameType = OnlineGameType, CreateSharedGroupData = "1" , FourDigitRandomNumber = PlayfabConstants.Instance.FourDigitRandomNumber},
                             GeneratePlayStreamEvent = true
                         }, OnSuccess =>
                         {
                             Debug.Log("135 SUccess");

                             if (OnlineGameType == 0)
                             {
                                 PlayfabConstants.Instance.TableAmount = HazariAmountSlot[Index];
                             }
                             else
                             {
                                 PlayfabConstants.Instance.TableAmount = NineCardAmountSlot[Index];
                             }

                             PlayfabConstants.Instance.IsRandomMatchMaking = false;
                             PlayfabConstants.Instance.OnlineGameType = OnlineGameType;
                             PlayfabConstants.Instance.SetProfileGameProfilSystemForFriendlyMatch();
                             SceneLoader.instance.LoadMatchMakingScene("RandomMatchMaking");
                             // SceneManager.LoadScene(4);

                         }, OnFailed =>
                         {
                             Debug.Log("135 Failed");
                             ToastNotification.instance.Show("Internet Connection Problem !!!");
                         });
                         
                        #endregion
/*
                        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
                        {
                            FunctionName = "ChallengePlayer",
                            FunctionParameter = new { FriendsPlayfabID = friendInfo.PlayfabID},
                            GeneratePlayStreamEvent = true
                        }, a => { Debug.Log("999 Send"); }, b => { Debug.Log("999 Failed to send" + b.ErrorMessage); }); */

                    }, OnError => 
                    {
                        Debug.Log("135 Failed");
                        ToastNotification.instance.Show("Internet Connection Problem !!!");
                    });
                } 
            }
            else
            {
                ToastNotification.instance.Show("Not Enough Balance !!");
            }
            
        });
        
        CreateTableButton.onClick.AddListener(() =>
        {
            if (UserHaveEnoughCoinToSendRequest())
            { 
                contentHolder.SetActive(false);
                
                if (OnlineGameType == 0)
                {
                    PlayfabConstants.Instance.TableAmount = HazariAmountSlot[Index];
                }
                else
                {
                    PlayfabConstants.Instance.TableAmount = NineCardAmountSlot[Index];
                }

                PlayfabConstants.Instance.IsCreateTable = true;
                PlayfabConstants.Instance.IsRandomMatchMaking = true;
                PlayfabConstants.Instance.OnlineGameType = OnlineGameType;
                PlayfabConstants.Instance.SetProfileGameProfilSystemForFriendlyMatch();
                SceneLoader.instance.LoadMatchMakingScene("RandomMatchMaking");
            }
            else
            {
                ToastNotification.instance.Show("Not Enough Balance !!");
            }
            
        });
    }



    private void OnHazariSelect()
    {
        HazariSelectedUi();
        
        OnlineGameType = 0;
        Index = 0;
        n = valueOf_N();
        coinAmountText.text = HazariAmountSlot[Index].ToString();
    }

    private void OnNineCardSelect()
    {
        NinecardSelectedUi();
            
        OnlineGameType = 1;
        Index = 0;
        n = valueOf_N();
        coinAmountText.text = NineCardAmountSlot[Index].ToString();
    }

    private int valueOf_N()
    {
        if (OnlineGameType == 0)
            return HazariAmountSlot.Count;
        else
            return NineCardAmountSlot.Count;
    }

    private int GetTableCoinAmount(int gameType, int coinIndex)
    {
        if(gameType == 0 && coinIndex< HazariAmountSlot.Count)
        {
            return HazariAmountSlot[coinIndex];
        }
        else if(gameType == 1 && coinIndex < NineCardAmountSlot.Count)
        {
            return NineCardAmountSlot[coinIndex];
        }
        
        return HazariAmountSlot[coinIndex];
    }

    bool UserHaveEnoughCoinToSendRequest()
    {
        if (CoinSystem.instance.GetBalance() >= GetTableCoinAmount(OnlineGameType, Index))
            return true;

        return false;
    }

    private void HazariSelectedUi()
    {
        hazariGameSelectionStates[0].SetActive(false);
        hazariGameSelectionStates[1].SetActive(true);
        
        ninecardGameSelectionStates[0].SetActive(true);
        ninecardGameSelectionStates[1].SetActive(false);
    }

    private void NinecardSelectedUi()
    {
        hazariGameSelectionStates[0].SetActive(true);
        hazariGameSelectionStates[1].SetActive(false);
        
        ninecardGameSelectionStates[0].SetActive(false);
        ninecardGameSelectionStates[1].SetActive(true);
    }

    public void OnBackButtonPressed()
    {
        PlayfabConstants.Instance.IsPrivateTable = false;
        this.gameObject.SetActive(false);
    }
}
