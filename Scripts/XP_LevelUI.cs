using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Lobby;
using PlayFab;
using UnityEngine.UI;
using Random = UnityEngine.Random;


public class XP_LevelUI : MonoBehaviour
{
    public static XP_LevelUI instance;
    [SerializeField] private Image progressBar;
    
    [Header("Xp Animation")] 
    [SerializeField] private RectTransform xpTargetPosition = null;
    [SerializeField] private GameObject xpPrefeb;
    [SerializeField] Transform xpHolder = null;

    [SerializeField] private Text fillAmountPercentagetext;
    
    private int xpReachCount = 0;
    Sequence[] xpSeq = new Sequence[0];

    public string key = "ShowXpAnim";
    private int numberOfXpElements = 10;
    private int previousXpPoint;

    [Header("ProgressBar Fill With Time")]
    private float lastSaveFillAmount = 0;
    private String lastSaveFillAmountKey = "lastSaveFillAmount";
    [SerializeField] private float XpAmountShownWaitTime;  // This value not use

    private int previousMyPoint;
    private string previousMyPointKey = "previousMyPoint";

    private string XpSavedKey = "XpSavedKey";
    
    private void Awake()
    {
        instance = this;
    }

    private void OnEnable()
    {
        XP_System.OnXPUpdateAction += UI_Update;
    }

    private void OnDisable()
    {
        XP_System.OnXPUpdateAction -= UI_Update;
    }

    private void Start()
    {
        
        if(XP_System.Instance != null)
            previousXpPoint = XP_System.Instance.XP_Point;
        
        if (PlayerPrefs.HasKey(key))
        {
            int showXpAnim = PlayerPrefs.GetInt(key);
            if (showXpAnim == 1)
            { 
                SetValuesInXpBeforePlaying();
                PlayerWinningCoinAndXpAnimation();
                PlayerPrefs.SetInt(key, 0);
                
            }
            else
            {
                UI_Update();
            }
            
        }
        else
        {
            UI_Update();
        }

        XpSavedForFirstTime();


    }

    public void ShowXpAnim()
    {
        PlayerPrefs.SetInt(key, 1);
    }
    
    private void FixedUpdate()
    {
        if (LanguageSelectionScript.instance != null && LanguageSelectionScript.instance.changeLevelLanguage)
        {
            LanguageSelectionScript.instance.ChangeLevelLanguage(TestXp.instance.levelText, CalculateLevel());
            TestXp.instance.ChangeLanguage();
            LanguageSelectionScript.instance.changeLevelLanguage = false;
        }
    }

    public void UI_Update()
    {
        if (!PlayerPrefs.HasKey(key))
        {
            if (XP_System.Instance == null)
                return;

            int levelNo = CalculateLevel();

            TestXp.instance.ShowLevelNoInUi(levelNo);
            if(LanguageSelectionScript.instance != null)
                LanguageSelectionScript.instance.ChangeLevelLanguage(TestXp.instance.levelText, levelNo);
        
            TestXp.instance.ShowFillAmountTextForUiUpdate(XP_System.Instance.XP_Point, levelNo);
            SetValuesInXpBeforePlaying();
            
        }
        else
        {
            int showAnim = PlayerPrefs.GetInt(key);
            if (showAnim == 0)
            {
                if (XP_System.Instance == null)
                    return;

                int levelNo = CalculateLevel();

                TestXp.instance.ShowLevelNoInUi(levelNo);
                if(LanguageSelectionScript.instance != null)
                    LanguageSelectionScript.instance.ChangeLevelLanguage(TestXp.instance.levelText, levelNo);
        
                TestXp.instance.ShowFillAmountTextForUiUpdate(XP_System.Instance.XP_Point, levelNo);
                SetValuesInXpBeforePlaying();
                
            }
        }

        
        

    }

    public void XpSavedForFirstTime()
    {
        if (PlayerPrefs.HasKey(XpSavedKey))
        {
            
        }
        else
        {
            SetValuesInXpBeforePlaying();
            PlayerPrefs.SetInt(XpSavedKey, 1);
        }
    }
    
    public void SetValuesInXpBeforePlaying()
    {
        if(XP_System.Instance != null)
        {
                int xp = XP_System.Instance.XP_Point;
                
                PlayerPrefs.SetInt(TestXp.instance.PreviousXpSaveKey, xp);
                

                int level = CalculateLevel();
                PlayerPrefs.SetInt(TestXp.instance.PreviousLevelSaveKey, level);
                
                
            
        }
        
    }
    
    public int CalculateLevel()
    {
        int pow = 1;
        int xp = XP_System.Instance.XP_Point;

        while (Mathf.Pow(2, pow) <= xp)
        {
            xp = xp - (int)Mathf.Pow(2, pow);
            pow++;
        }

        return pow;
    }

    
    public void XpFlyByAnimation()
    {
            KillXpSeq();
            float radius = 1;
            float spawDelay = 0.01f;
            StartCoroutine(_XpFlyByAnimataion(xpHolder.transform.position, numberOfXpElements, radius, spawDelay));
    }
        
    private IEnumerator _XpFlyByAnimataion(Vector3 startPosition,int numberOfXpElements, float radius = 2, float spawDelay = 0.01f)
    {
            
            TestXp.instance.ShowProgressBarAnim();
            xpSeq = new Sequence[numberOfXpElements];
            
            for (int i = 0; i < numberOfXpElements; i++)
            {
                Vector3 randomPos = Random.insideUnitCircle * radius;
                randomPos += startPosition;
                GameObject piece = Instantiate(xpPrefeb, startPosition, Quaternion.identity, xpHolder);

                piece.transform.SetAsFirstSibling();

                Vector3[] travelPath = new Vector3[3];
                travelPath[0] = randomPos;
                travelPath[1] = new Vector3((xpTargetPosition.position.x + startPosition.x) / Random.Range(1.2f, 1.5f), (xpTargetPosition.position.y + startPosition.y) / Random.Range(1.5f, 2.5f));
                travelPath[2] = xpTargetPosition.position;
                
                xpSeq[i].Append(piece.transform.DOMove(randomPos, 0.3f).SetEase(Ease.OutExpo));
                xpSeq[i].Append(piece.transform.DOPath(travelPath, 1f, PathType.CatmullRom).SetEase(Ease.InExpo).SetDelay(Random.Range(0.3f, 0.8f))
                    .OnComplete(() =>
                    {
                        xpReachCount++;
                        piece.transform.localScale = Vector3.zero;
                        if (xpReachCount == numberOfXpElements)
                        {
                            xpReachCount = 0;
                        }
                        
                    }));
                
                Destroy(piece, 5);

                yield return new WaitForSeconds(spawDelay);
            }
    }

    private void PlayerWinningCoinAndXpAnimation()
    {
        LobbyUIManager.instance.PlayerTotalWinningAmount();
        
        
        if (LobbyUIManager.instance.XpShouldWait)
        {
            if (XP_System.Instance != null)
            {
                Invoke("XpFlyByAnimation", 2.5f);
            }
        }
        else
        {
            if (XP_System.Instance != null)
            {
                Invoke("XpFlyByAnimation", 1.5f);
                
            }
            
        }
            
            
    }

    private void KillXpSeq()
    {
        if (xpSeq.Length > 0)
        {
            for (int j = 0; j < xpSeq.Length; j++)
            {
                if (xpSeq[j] != null && xpSeq[j].IsActive())
                {
                    xpSeq[j].Kill();
                }
            }
        }
        xpSeq = new Sequence[0];
    }

    public bool IsPlayerRewardAvailable()
    {
        if (Application.internetReachability != NetworkReachability.NotReachable && PlayFabClientAPI.IsClientLoggedIn())
        {
            return true;
        }

        return false;

    }
    

    
    private void OnDestroy()
    {
        KillXpSeq();
    }
}
