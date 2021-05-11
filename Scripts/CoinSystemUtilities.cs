//using Firebase.Crashlytics;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections;
using System.Collections.Generic;
using TAAASH_KIT;
using UnityEngine;

public class CoinSystemUtilities : MonoBehaviour
{
    public static CoinSystemUtilities Instance { get; private set; }

    // Secret key of encryption.
    private const string COIN_ENCRYPTION_KEY = "ulka-z1x2-y78u-i903-xy7z"; // Must be its size 128 or 192 bit, otherwise not worked

    public const string COIN_DATA_ADD_OFFLINE = "COIN_DATA_ADD_OFFLINE";
    public const string COIN_DATA_DEDUCT_OFFLINE = "COIN_DATA_DEDUCT_OFFLINE";

    // public bool offlineCoinDataSendToTheServer = true;

    int[] gameCurrencyIndex_1 = new int[] { 10, 25, 50, 100, 250, 565, 500, 1000, 1125, 2250, 5000, 5625, 10000, 11250, 22500,2500 , 120,150, 110, 200 };

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    private void OnEnable()
    {
        CoinSystem.OnAddCoinEvent += AddCoins;
        CoinSystem.OnDedeuctCoinEvent += DeductCoins;
    }

    private void OnDisable()
    {
        CoinSystem.OnAddCoinEvent -= AddCoins;
        CoinSystem.OnDedeuctCoinEvent -= DeductCoins;
    }

    #region Encryption - Decryption
    public string StringEncryption(string _data)
    {
        return _data;
    }

    public string StringDecryption(string _data)
    {
        return _data;
    }
    #endregion

    #region Game Trade Cost
    // Converting coin to Alphabetic Index value
    public string AmountSlot(int _coin)
    {
        switch (_coin)
        {
            case 10:
                return "A";
            case 25:
                return "B";
            case 50:
                return "C";
            case 100:
                return "D";
            case 250:
                return "E";
            case 565:
                return "F";
            case 500:
                return "G";
            case 1000:
                return "H";
            case 1125:
                return "I";
            case 2250:
                return "J";
            case 5000:
                return "K";
            case 5625:
                return "L";
            case 10000:
                return "M";
            case 11250:
                return "N";
            case 22500:
                return "O";
            case 2500:
                return "P";
            case 120:
                return "Q";
            case 150:
                return "R";
            case 110:
                return "S";
            case 200:
                return "T";

            default:
                return UnExpectedAmountSlot(_coin);
        }
    }
    #endregion

    // If the coin amount does not matched with coin index split the coin in several index
    public string UnExpectedAmountSlot(int _amount)
    {
        string _temp = "";

        while (_amount > 9)
        {
            if(_amount >= 22500)
            {
                _temp += "O";
                _amount -= 22500;
            }
            else if(_amount >= 11250)
            {
                _temp += "N";
                _amount -= 11250;
            }
            else if (_amount >= 10000)
            {
                _temp += "M";
                _amount -= 10000;
            }
            else if (_amount >= 5625)
            {
                _temp += "L";
                _amount -= 5625;
            }
            else if (_amount >= 5000)
            {
                _temp += "K";
                _amount -= 5000;
            }
            else if (_amount >= 2500)
            {
                _temp += "P";
                _amount -= 2500;
            }
            else if (_amount >= 2250)
            {
                _temp += "J";
                _amount -= 2250;
            }
            else if (_amount >= 1125)
            {
                _temp += "I";
                _amount -= 1125;
            }
            else if (_amount >= 1000)
            {
                _temp += "H";
                _amount -= 1000;
            }
            else if (_amount >= 565)
            {
                _temp += "F";
                _amount -= 565;
            }
            else if (_amount >= 500)
            {
                _temp += "G";
                _amount -= 500;
            }
            else if (_amount >= 250)
            {
                _temp += "E";
                _amount -= 250;
            }
            else if (_amount >= 200)
            {
                _temp += "T";
                _amount -= 200;
            }
            else if (_amount >= 150)
            {
                _temp += "R";
                _amount -= 150;
            }
            else if (_amount >= 120)
            {
                _temp += "Q";
                _amount -= 120;
            }
            else if (_amount >= 110)
            {
                _temp += "S";
                _amount -= 110;
            }
            else if (_amount >= 100)
            {
                _temp += "D";
                _amount -= 100;
            }
            else if (_amount >= 50)
            {
                _temp += "C";
                _amount -= 50;
            }
            else if (_amount >= 25)
            {
                _temp += "B";
                _amount -= 25;
            }
            else if (_amount >= 10)
            {
                _temp += "A";
                _amount -= 10;
            }
        }

        if (_temp != "")
            return _temp;
        else
            return "A";
    }

    public void AddCoins(int _coin)
    {
        string coinIndex = AmountSlot(_coin).ToString();
        Debug.Log("ADD COIN INDEX : " + coinIndex);

        if (PlayfabController.Instance != null)
        {
            PlayfabController.Instance.AddCoinInServer(coinIndex);
        }
        else
        {
            Debug.LogError("PlayfabController.Instance == null");
        }
    }

    public void DeductCoins(int _coin)
    {
        string coinIndex = AmountSlot(_coin).ToString();
        //Debug.Log("DEDUCT COIN INDEX : " + coinIndex);

        if (PlayfabController.Instance != null)
            PlayfabController.Instance.DeductCoinInServer(coinIndex);
        else
        {
            //Debug.LogError("PlayfabController.Instance == null");
        }
    }

    /// <summary>
    /// When failed to save coin data in server it will save data in local store
    /// </summary>
    /// <param name="actionType"> 0 for ADDITION, 1 for DEDUCTION</param>
    /// <param name="_gameType"></param>
    /// <param name="coinIndex"></param>
    public void OnAddCoinFailed(string coinIndex)
    {
        //Crashlytics.Log("CoinSystemUtilities.cs|OnAddCoinFailed(string coinIndex) : Saving coin Index offline which are failed to save in server");
        string coinDataOffline0 = PlayerPrefs.GetString(COIN_DATA_ADD_OFFLINE, "");

        string coinDataOffline = "";
        if (coinDataOffline0 != "")
        {
            coinDataOffline = CryptoEngine.Decrypt(coinDataOffline0, COIN_ENCRYPTION_KEY);
        }
        coinDataOffline += coinIndex;

        string coinDataEncrypted = CryptoEngine.Encrypt(coinDataOffline, COIN_ENCRYPTION_KEY);
        PlayerPrefs.SetString(COIN_DATA_ADD_OFFLINE, coinDataEncrypted);

        Debug.Log("COIN_DATA_ADD_OFFLINE: " + PlayerPrefs.GetString(COIN_DATA_ADD_OFFLINE, ""));
    }

    public void OnDeductCoinFailed(string coinIndex)
    {
        //Crashlytics.Log("CoinSystemUtilities.cs|OnDeductCoinFailed(string coinIndex) : Saving coin Index offline which are failed to save in server");

        string coinDataOffline0 = PlayerPrefs.GetString(COIN_DATA_DEDUCT_OFFLINE, "");

        string coinDataOffline = "";
        if (coinDataOffline0 != "")
        {
            coinDataOffline = CryptoEngine.Decrypt(coinDataOffline0, COIN_ENCRYPTION_KEY);
        }
        coinDataOffline += coinIndex;
        Debug.Log("OnDedecutCoinFailed :" + coinDataOffline);

        string coinDataEncrypted = CryptoEngine.Encrypt(coinDataOffline, COIN_ENCRYPTION_KEY);
        PlayerPrefs.SetString(COIN_DATA_DEDUCT_OFFLINE, coinDataEncrypted);

        Debug.Log("COIN_DATA_DEDUCT_OFFLINE: " + PlayerPrefs.GetString(COIN_DATA_DEDUCT_OFFLINE, ""));
    }

    public void CheckForSavedCoinData()
    {
        // PlayfabController.Instance.GetAllVirtualCurrency();
        //Crashlytics.Log("CoinSystemUtilities.cs|CheckForSavedCoinData() : Checking for offline coin data...........");
        string addOfflineCoinDataList0 = PlayerPrefs.GetString(COIN_DATA_ADD_OFFLINE, "");

        string addOfflineCoinDataList = "";
        if(addOfflineCoinDataList0 != "")
        {
            addOfflineCoinDataList = CryptoEngine.Decrypt(addOfflineCoinDataList0, COIN_ENCRYPTION_KEY);
        }

        if (addOfflineCoinDataList != "")
        {
            PlayfabController.Instance.AddOfflineCoinsToServer(addOfflineCoinDataList,
                ()=> 
                {
                    //Crashlytics.Log("CoinSystemUtilities.cs|CheckForSavedCoinData()-> PlayfabController.Instance.AddOfflineCoinsToServer() : winning offline coin data added to the server sucessfully!!!");
                    PlayerPrefs.DeleteKey(COIN_DATA_ADD_OFFLINE);
                });
        }

        string deductOfflineCoinDataList0 = PlayerPrefs.GetString(COIN_DATA_DEDUCT_OFFLINE, "");

        string deductOfflineCoinDataList = "";
        if (deductOfflineCoinDataList0 != "")
        {
            deductOfflineCoinDataList = CryptoEngine.Decrypt(deductOfflineCoinDataList0, COIN_ENCRYPTION_KEY);
            Debug.Log(" deductOfflineCoinDataList : " + deductOfflineCoinDataList);
        }
        if (deductOfflineCoinDataList != "")
        {
            PlayfabController.Instance.DeductOfflineCoinsToServer(deductOfflineCoinDataList,
                ()=>
                {
                    //Crashlytics.Log("CoinSystemUtilities.cs|CheckForSavedCoinData()-> PlayfabController.Instance.DeductOfflineCoinsToServer() : loosing offline coin data deducted from the server sucessfully!!!");
                    PlayerPrefs.DeleteKey(COIN_DATA_DEDUCT_OFFLINE);
                });
        }
    }

}
