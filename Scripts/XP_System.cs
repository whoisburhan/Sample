using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using System;
using PlayFab.ClientModels;
using Lobby;
using TAAASH_KIT;
//using Firebase.Crashlytics;

public class XP_System : MonoBehaviour
{
    public static event Action OnXPUpdateAction;

    public static XP_System Instance { get; private set; }
    private int xpPoint = 0;

    public int XP_Point { get { return xpPoint; } set { xpPoint = value; } }

    // Secret key of encryption.
    private const string XP_ENCRYPTION_KEY = "ulka-z1x2-y78u-i903-xy7z"; // Must be its size 128 or 192 bit, otherwise not worked
    private const string XP_COIN_SAVE = "XP_POINT";
    private const string XP_COIN_SAVE_OFFLINE = "XP_POINT_OFFLINE";

    public bool offlineXPDataSendToTheServer = true;

    private void OnEnable()
    {
        PlayfabController.OnLoggedInEvent += CheckXPUpdate;
        PlayfabController.OnLoggedInFailedEvent += CheckXPUpdate;
        PlayfabController.OnDataResetEvent += Reset_XP;
    }

    private void OnDisable()
    {
        PlayfabController.OnLoggedInEvent -= CheckXPUpdate;
        PlayfabController.OnLoggedInFailedEvent -= CheckXPUpdate;
        PlayfabController.OnDataResetEvent -= Reset_XP;
    }

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        xpPoint = 0;
        string xpStringEncrypted = PlayerPrefs.GetString(XP_COIN_SAVE, "");
        if (xpStringEncrypted != "")
        {
            string  xpStringDecrypted = CryptoEngine.Decrypt(xpStringEncrypted, XP_ENCRYPTION_KEY);
            xpPoint = Int32.Parse(xpStringDecrypted);
        }
        //Debug.Log(xpPoint);
        OnXPUpdateAction?.Invoke();
    }

    public void CheckXPUpdate()
    {
        // string xpStringEncrypted = PlayerPrefs.GetString(XP_COIN_SAVE, "");
        //  string xpStringDecrypted = "";
        //  if (xpStringEncrypted != "")
        //  {
        //      xpStringDecrypted = CryptoEngine.Decrypt(xpStringEncrypted, XP_ENCRYPTION_KEY);
        //  }
        //  xpPoint = xpStringDecrypted != "" ? Int32.Parse(xpStringDecrypted) : xpPoint;
        //xpPoint = xpPoint >= PlayerPrefs.GetInt(XP_COIN_SAVE, 0) ? xpPoint : PlayerPrefs.GetInt(XP_COIN_SAVE, 0);
        OnXPUpdateAction?.Invoke();
        XP_Equalization();
    }

    public void Add_XP(int amount)
    {
        xpPoint += amount;
        Debug.Log(xpPoint);
        string xpStringDecrypted = xpPoint.ToString();
        string xpStringEncrypted = CryptoEngine.Encrypt(xpStringDecrypted, XP_ENCRYPTION_KEY);
        PlayerPrefs.SetString(XP_COIN_SAVE, xpStringEncrypted);

        // PlayerPrefs.SetInt("XP_POINT", xpPoint);
        //OnXPUpdateAction?.Invoke();

        if (PlayFabClientAPI.IsClientLoggedIn())
        {
            if (Application.internetReachability != NetworkReachability.NotReachable)
            {
                
                    PlayfabController.Instance.Add_XP(amount);
                    //XP_Equalization()
            }
            else
            {
                //Crashlytics.Log("XP_System.cs|Add_XP(int amount) : Saving xp offline which are failed to save in server");
                OfflineXPSave(amount);
            }
        }
        else
        {
            //Crashlytics.Log("XP_System.cs|Add_XP(int amount) : Saving xp offline which are failed to save in server");
            OfflineXPSave(amount);
        }
    }

    public void OfflineXPSave(int amount)
    {
        string xpDataOffline0 = PlayerPrefs.GetString(XP_COIN_SAVE_OFFLINE, "");

        int xpDataInt = 0;
        string xpDataOffline = "";
        if (xpDataOffline0 != "")
        {
            xpDataOffline = CryptoEngine.Decrypt(xpDataOffline0, XP_ENCRYPTION_KEY);
            Debug.Log(XP_ENCRYPTION_KEY);
            xpDataInt = Int32.Parse(xpDataOffline);
        }
         
        xpDataInt += amount;
        string xpDataOffline1 = xpDataInt.ToString();

        string xpDataEncrypted = CryptoEngine.Encrypt(xpDataOffline1, XP_ENCRYPTION_KEY);
        PlayerPrefs.SetString(XP_COIN_SAVE_OFFLINE, xpDataEncrypted);

        Debug.Log("XP_POINT_SAVE_OFFLINE: " + PlayerPrefs.GetString(XP_COIN_SAVE_OFFLINE, ""));
    }

    public void CheckOfflineXP()
    {
        string offlineXP0 = PlayerPrefs.GetString(XP_COIN_SAVE_OFFLINE, "");

        string offlineXP = "";
        if (offlineXP0 != "")
        {
            offlineXP = CryptoEngine.Decrypt(offlineXP0, XP_ENCRYPTION_KEY);
        }

        if (offlineXP != "")
        {
            int offlineXPInt = Int32.Parse(offlineXP);
                
            PlayfabController.Instance.Add_XP_Offline(offlineXPInt,
            () =>
             {
                PlayerPrefs.DeleteKey(XP_COIN_SAVE_OFFLINE);
                //Crashlytics.Log("XP_System|CheckOfflineXP()-> PlayfabController.Instance.Add_XP_Offline() :  offline xp data added to the server sucessfully!!!");
             });
        }
    }


    public void XP_Equalization()
    {
        if (PlayFabClientAPI.IsClientLoggedIn())
        {
            try
            {
                Dictionary<string, int> vc = new Dictionary<string, int>();
                PlayFabClientAPI.GetUserInventory(
                    new GetUserInventoryRequest { },
                    GetResult =>
                    {
                        vc = GetResult.VirtualCurrency;
                        //Debug.Log("GOLD :" + vc["GD"] + " | XP:" + vc["XP"]);
                        //Debug.Log("XP Point on Game: " + xpPoint);

                        if(CoinSystem.instance != null)
                            CoinSystem.instance.SetBalance(vc["GD"]);

                        if (vc["XP"] >= xpPoint)
                        {

                            xpPoint = vc["XP"];
                            string xpStringDecrypted = xpPoint.ToString();
                            string xpStringEncrypted = CryptoEngine.Encrypt(xpStringDecrypted, XP_ENCRYPTION_KEY);
                            PlayerPrefs.SetString(XP_COIN_SAVE, xpStringEncrypted);
                           // PlayerPrefs.SetInt("XP_POINT", xpPoint);
                        }
                        else
                        {
                            PlayfabController.Instance.Add_XP(xpPoint - vc["XP"]);
                        }
                        OnXPUpdateAction?.Invoke();
                    },
                    (op) => { Debug.LogError("unable to gets xp data from server !!!" + op.ErrorMessage); }
                    );
               
            }
            catch (Exception e)
            {
//                Crashlytics.Log("XP_System.Add_XP() : " + e);
//                Crashlytics.LogException(e);
                Debug.LogError("unable to gets xp data from server !!!" + e.Message);
            }

        }
        OnXPUpdateAction?.Invoke();
    }

    public void Reset_XP()
    {
        xpPoint = 0;
        xpPoint = xpPoint >= PlayerPrefs.GetInt(XP_COIN_SAVE, 0) ? xpPoint : PlayerPrefs.GetInt(XP_COIN_SAVE, 0);
    }
    
    public bool CheckNetwrokConnectionOfPlay()
    {
        if (Application.internetReachability != NetworkReachability.NotReachable && PlayFabClientAPI.IsClientLoggedIn())
        {
            return true;
        }

        return false;
    }
    

    
}
