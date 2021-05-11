using PlayFab;
using PlayFab.ClientModels;
using PlayFabKit;
using System;
using System.Collections;
using System.Collections.Generic;
using Lobby;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class MyPlayfabPlayerInfo
{
    public string PlayfabId;
    public GameObject MyPlayfabPlayerGameObject;
}

public class PlayfabFriendsInfo : MonoBehaviour
{
    public static event Action OnOnlineFriendReload;

    public GameObject friendsPanelLoadignAnim;
    [SerializeField] private GameObject friendPrefab;
    [SerializeField] private GameObject allFriendPrefabMock;
    [SerializeField] private RectTransform friendlistPanel;
    [SerializeField] private RectTransform rankListPanel;
    [SerializeField] private GameObject SetTableCoinPanel;
    [SerializeField] private GameObject _setTableContentHolder;

    [Header("Friends Button Related Stuff")]
    [SerializeField] private Sprite activeButtonSprite;
    [SerializeField] private Sprite inactiveButtonSprite;
    [SerializeField] private Button onlineFriendsButton;
    [SerializeField] private Button allFriendsButton;

    private List<GameObject> onlineFriendsList = new List<GameObject>();
    private List<GameObject> offlineFriendsList = new List<GameObject>();
    private List<GameObject> allFriendList = new List<GameObject>();

   // private List<GameObject> playerHolder = new List<GameObject>();    
    private float onlineFriendsReloadTimer = 0;
    private float onlineFriendsReloadTimeInterval = 8f;

    private bool isFirstTimeLoad;
   // private List<MyPlayfabPlayerInfo> myAllfriendList;
    private Dictionary<string, MyPlayfabPlayerInfo> myAllfriendList;

    private void Start()
    {
        onlineFriendsReloadTimer = 5f;
        isFirstTimeLoad = true;

        onlineFriendsButton.onClick.AddListener(()=> 
        {
            onlineFriendsButton.GetComponent<Image>().sprite = activeButtonSprite;
            allFriendsButton.GetComponent<Image>().sprite = inactiveButtonSprite;
        });

        allFriendsButton.onClick.AddListener(() =>
        {
            onlineFriendsButton.GetComponent<Image>().sprite = inactiveButtonSprite;
            allFriendsButton.GetComponent<Image>().sprite = activeButtonSprite;
        });
    }

    private void FixedUpdate()
    {
        if (PlayFabClientAPI.IsClientLoggedIn() && onlineFriendsReloadTimer <= 0)
        {
            PlayfabLeaderboard.GetFriendsLeaderboard(100, UpdateFriendList);
            onlineFriendsReloadTimer = onlineFriendsReloadTimeInterval;
        }

        onlineFriendsReloadTimer -= Time.deltaTime;
    }

    private void UpdateFriendList(GetLeaderboardResult result)
    {
        if (result == null || result.Leaderboard == null)
            return;

        if (PlayfabConstants.Instance == null) return;

        var _result = result.Leaderboard;

        Debug.Log("190 FRIENDS COUNT  Before getting ServerTime: " + _result.Count);

        if (_result.Count > 0)
        {
            #region new Working Code
            PlayFabServerAPI.GetTime(new PlayFab.ServerModels.GetTimeRequest { }, OnGetTimeSuccess =>
            {
              //  OnOnlineFriendReload?.Invoke();

                if (isFirstTimeLoad)
                {
                    myAllfriendList = new Dictionary<string, MyPlayfabPlayerInfo>();

                    // Create new Player Prefabs
                    for (int i = 0; i < _result.Count; i++)
                    {
                        if (_result[i].PlayFabId != PlayfabConstants.Instance.MyPlayfabID)
                        {
                            GameObject _go = Instantiate(friendPrefab, transform.position, Quaternion.identity);
                            Debug.Log("555 GameObject Created");

                            OnlineFriendMock _friend = _go.GetComponent<OnlineFriendMock>();
                            if (_friend != null)
                            {                                
                                _friend.PlayfabID = _result[i].PlayFabId;
                                _friend.CoinTextHandler(_result[i].StatValue);
                                _friend.ServerTime = OnGetTimeSuccess.Time;

                                _friend.ChallengeButton.onClick.AddListener(() =>
                                {

                                    Debug.Log("555 GameObject challengeButton");
                                    SetTableCoinPanel.SetActive(true);
                                    _setTableContentHolder.SetActive(true);
                                    var _CoinSetObject = SetTableCoinPanel.GetComponent<SetTableCoinScript>(); //.friendInfo = _friend;

                                    if (_CoinSetObject != null)
                                    {
                                        _CoinSetObject.friendInfo = _friend;
                                    }

                                    if (LobbyUIManager.instance != null) LobbyUIManager.instance.HidenlineFrinedsPanel();
                                    //Deactivate onlineplayerPanel
                                    this.gameObject.SetActive(false);

                                });


                                if (IsOnline(_result[i].Profile, OnGetTimeSuccess.Time))
                                {
                                    Debug.Log("555 GameObject Online");
                                    _friend.ActivityIcon.color = Color.green;
                                    _friend.IsOnline = true;
                                    onlineFriendsList.Add(_go);
                                }
                                else
                                {
                                    Debug.Log("555 GameObject offline");
                                    _friend.ActivityIcon.color = Color.red;
                                    _friend.IsOnline = false;
                                    offlineFriendsList.Add(_go);
                                }


                                _friend.SetPlayerInfo(_result[i].Profile);  // [Warning : never add this line before other line.] 

                            }

                            MyPlayfabPlayerInfo _myPlayfabPlayerInfo = new MyPlayfabPlayerInfo();
                            _myPlayfabPlayerInfo.PlayfabId = _result[i].PlayFabId;
                            _myPlayfabPlayerInfo.MyPlayfabPlayerGameObject = _go;
                            myAllfriendList.Add(_result[i].PlayFabId, _myPlayfabPlayerInfo);
                        }
                    }

                    int _friendHolderPanelHeight = ((_result.Count - 1) * (80 + 10));

                    if (_friendHolderPanelHeight < 530)
                        _friendHolderPanelHeight = 530;

                    if (friendlistPanel == null && rankListPanel == null) return;
                    friendlistPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _friendHolderPanelHeight);
                    friendlistPanel.parent.GetComponent<ScrollRect>().normalizedPosition = new Vector2(0, 1);

                    isFirstTimeLoad = false;
                }

                // Deactivate the players list in the beginning and remove parent transform
                if(myAllfriendList.Count > 0)
                {
                    foreach(var i in myAllfriendList)
                    {
                        var _myfriend = i.Value;

                        if (_myfriend.MyPlayfabPlayerGameObject != null)
                        {
                            _myfriend.MyPlayfabPlayerGameObject.transform.parent = null;
                            _myfriend.MyPlayfabPlayerGameObject.SetActive(false);
                        }
                    }

                    PlayfabConstants.Instance.OnlineFriendCounter = 1;

                    // Update Online Friend
                    for (int i = 0; i < _result.Count; i++)
                    {
                        if (_result[i].PlayFabId != PlayfabConstants.Instance.MyPlayfabID)
                        {
                            var _checkForOnline = IsOnline(_result[i].Profile, OnGetTimeSuccess.Time);

                            if (_checkForOnline && myAllfriendList.ContainsKey(_result[i].PlayFabId))
                            {
                                var _myfriend = myAllfriendList[_result[i].PlayFabId];

                                if (_myfriend.MyPlayfabPlayerGameObject == null) return;

                                _myfriend.MyPlayfabPlayerGameObject.SetActive(true);
                                _myfriend.MyPlayfabPlayerGameObject.transform.SetParent(friendlistPanel);
                                _myfriend.MyPlayfabPlayerGameObject.transform.localScale = Vector3.one;

                                OnlineFriendMock _friend = _myfriend.MyPlayfabPlayerGameObject.GetComponent<OnlineFriendMock>();
                                if (_friend != null)
                                {
                                    _friend.PlayfabID = _result[i].PlayFabId;
                                    _friend.CoinTextHandler(_result[i].StatValue);
                                    _friend.ServerTime = OnGetTimeSuccess.Time;

                                    _friend.ChallengeButton.onClick.AddListener(() =>
                                    {

                                        Debug.Log("555 GameObject challengeButton");
                                        SetTableCoinPanel.SetActive(true);
                                        _setTableContentHolder.SetActive(true);
                                        var _CoinSetObject = SetTableCoinPanel.GetComponent<SetTableCoinScript>(); //.friendInfo = _friend;

                                        if (_CoinSetObject != null)
                                        {
                                            _CoinSetObject.friendInfo = _friend;
                                        }

                                        if (LobbyUIManager.instance != null) LobbyUIManager.instance.HidenlineFrinedsPanel();
                                        //Deactivate onlineplayerPanel
                                        this.gameObject.SetActive(false);

                                    });


                                    if (IsOnline(_result[i].Profile, OnGetTimeSuccess.Time))
                                    {
                                        Debug.Log("555 GameObject Online");
                                        _friend.ActivityIcon.color = Color.green;
                                        _friend.IsOnline = true;
                                    }
                                    else
                                    {
                                        Debug.Log("555 GameObject offline");
                                        _friend.ActivityIcon.color = Color.red;
                                        _friend.IsOnline = false;
                                    }


                                    _friend.SetPlayerInfo(_result[i].Profile);  // [Warning : never add this line before other line.] 

                                }

                            }
                        }
                    }

                    // Update Offline Friend
                    for (int i = 0; i < _result.Count; i++)
                    {
                        if (_result[i].PlayFabId != PlayfabConstants.Instance.MyPlayfabID)
                        {
                            var _checkForOnline = IsOnline(_result[i].Profile, OnGetTimeSuccess.Time);

                            if (!_checkForOnline && myAllfriendList.ContainsKey(_result[i].PlayFabId))
                            {
                                var _myfriend = myAllfriendList[_result[i].PlayFabId];

                                _myfriend.MyPlayfabPlayerGameObject.SetActive(true);
                                _myfriend.MyPlayfabPlayerGameObject.transform.SetParent(friendlistPanel);
                                _myfriend.MyPlayfabPlayerGameObject.transform.localScale = Vector3.one;

                                OnlineFriendMock _friend = _myfriend.MyPlayfabPlayerGameObject.GetComponent<OnlineFriendMock>();
                                if (_friend != null)
                                {
                                    _friend.PlayfabID = _result[i].PlayFabId;
                                    _friend.CoinTextHandler(_result[i].StatValue);
                                    _friend.ServerTime = OnGetTimeSuccess.Time;

                                    _friend.ChallengeButton.onClick.AddListener(() =>
                                    {

                                        Debug.Log("555 GameObject challengeButton");
                                        SetTableCoinPanel.SetActive(true);
                                        _setTableContentHolder.SetActive(true);
                                        var _CoinSetObject = SetTableCoinPanel.GetComponent<SetTableCoinScript>(); //.friendInfo = _friend;

                                        if (_CoinSetObject != null)
                                        {
                                            _CoinSetObject.friendInfo = _friend;
                                        }

                                        if (LobbyUIManager.instance != null) LobbyUIManager.instance.HidenlineFrinedsPanel();
                                        //Deactivate onlineplayerPanel
                                        this.gameObject.SetActive(false);

                                    });


                                    if (IsOnline(_result[i].Profile, OnGetTimeSuccess.Time))
                                    {
                                        Debug.Log("555 GameObject Online");
                                        _friend.ActivityIcon.color = Color.green;
                                        _friend.IsOnline = true;
                                    }
                                    else
                                    {
                                        Debug.Log("555 GameObject offline");
                                        _friend.ActivityIcon.color = Color.red;
                                        _friend.IsOnline = false;
                                    }


                                    _friend.SetPlayerInfo(_result[i].Profile);  // [Warning : never add this line before other line.] 

                                }

                            }
                        }
                    }
                }

                

            }, LogFailure => { });
            #endregion 



            #region Old working Code
            /*
            allFriendList = new List<GameObject>();
            onlineFriendsList = new List<GameObject>();
            offlineFriendsList = new List<GameObject>();
            //List<GameObject> playerHolder = new List<GameObject>();
            //OnOnlineFriendReload?.Invoke();

            if (friendsPanelLoadignAnim != null)
                friendsPanelLoadignAnim.SetActive(false);
            else
            {
                return;
            }

            if(PlayfabConstants.Instance==null) return;
            PlayfabConstants.Instance.OnlineFriendCounter = 1;
            

            int _panelHeight = ((_result.Count-1) * (80 + 10)) ;

            if (_panelHeight < 530)
                _panelHeight = 530;

            if(friendlistPanel==null && rankListPanel==null) return;
            friendlistPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _panelHeight);
            friendlistPanel.parent.GetComponent<ScrollRect>().normalizedPosition = new Vector2(0, 1);

            rankListPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _panelHeight);
            rankListPanel.parent.GetComponent<ScrollRect>().normalizedPosition = new Vector2(0, 1);


            //friendList = new List<GameObject>();
            PlayFabServerAPI.GetTime(new PlayFab.ServerModels.GetTimeRequest { }, OnGetTimeSuccess =>
            {
                OnOnlineFriendReload?.Invoke();

                Debug.Log("190 FRIENDS COUNT : " + _result.Count);

                for (int i = 0; i < _result.Count; i++)
                {
                    // For All Friends
                    if (LobbyUIManager.instance == null)
                        return;

                    // Only for Online Playable Friends
                    if (_result[i].Profile.PlayerId != PlayfabConstants.Instance.MyPlayfabID)
                    {
                        if (transform.position == null) return;

                        GameObject _go = Instantiate(friendPrefab, transform.position, Quaternion.identity);
                        Debug.Log("555 GameObject Created");
                      //  playerHolder.Add(_go);
                     //   _go.transform.parent = friendlistPanel;
                     //   _go.transform.localScale = Vector3.one;
                        OnlineFriendMock _friend = _go.GetComponent<OnlineFriendMock>();
                        if (_friend != null)
                        {
                            // _friend.FriendNo.text = (i + 1).ToString();
                            _friend.PlayfabID = _result[i].PlayFabId;
                          //  _friend.CoinAmountText.text = _result[i].StatValue.ToString();
                            _friend.CoinTextHandler(_result[i].StatValue);
                            _friend.ServerTime = OnGetTimeSuccess.Time;

                            _friend.ChallengeButton.onClick.AddListener(() =>
                            {

                                Debug.Log("555 GameObject challengeButton");
                                SetTableCoinPanel.SetActive(true);
                                _setTableContentHolder.SetActive(true);
                                var _CoinSetObject = SetTableCoinPanel.GetComponent<SetTableCoinScript>(); //.friendInfo = _friend;

                                if (_CoinSetObject != null)
                                {
                                    _CoinSetObject.friendInfo = _friend;
                                }

                                if (LobbyUIManager.instance != null) LobbyUIManager.instance.HidenlineFrinedsPanel();
                                //Deactivate onlineplayerPanel
                                this.gameObject.SetActive(false);

                            });


                            if (IsOnline(_result[i].Profile, OnGetTimeSuccess.Time))
                            {
                                Debug.Log("555 GameObject Online");
                                _friend.ActivityIcon.color = Color.green;
                                _friend.IsOnline = true;
                                onlineFriendsList.Add(_go);
                            }
                            else
                            {
                                Debug.Log("555 GameObject offline");
                                _friend.ActivityIcon.color = Color.red;
                                _friend.IsOnline = false;
                                offlineFriendsList.Add(_go);
                            }


                            _friend.SetPlayerInfo(_result[i].Profile);  // [Warning : never add this line before other line.] 

                        }
                        //  friendList.Add(_go);
                    }

                }

                allFriendList = new List<GameObject>(onlineFriendsList.Count + offlineFriendsList.Count);
                allFriendList.AddRange(onlineFriendsList);
                allFriendList.AddRange(offlineFriendsList);

                Debug.Log("555 Count: " + allFriendList.Count);
                Debug.Log("555 OnlineCount: " + onlineFriendsList.Count);
                Debug.Log("555 OfflineCount: " + offlineFriendsList.Count);

                for (int _g = 0; _g<allFriendList.Count; _g++)
                {
                    allFriendList[_g].transform.parent = friendlistPanel;
                    allFriendList[_g].transform.localScale = Vector3.one;
                    allFriendList[_g].GetComponent<OnlineFriendMock>().No.text = (_g+1).ToString();
                    // PlayfabConstants.Instance.OnlineFriendCounter++;
                }

            }, LogFailure => { });
            */
            #endregion
        }
    }

    private bool IsOnline(PlayerProfileModel playerProfile, DateTime serverTime)
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
                Debug.Log("456 Servers Time: " + serverTime);
                Debug.Log("456 Server Time Difference" + (serverTime - _lastLoginDateTime).TotalMinutes);


                if ((serverTime - _lastLoginDateTime).TotalSeconds <= 90) return true;

                else return false;

            }
        }
    }
}
   
