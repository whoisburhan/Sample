using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using PlayFab;
using TAAASH_KIT;
using Random = UnityEngine.Random;

public class TestXp : MonoBehaviour
{
    public static TestXp instance;


    public float previousXp = 0;

    public int previousLevel = 0;
    public int savingPreviousLevelForLevelUpAnim = 0;

    public Text levelText;
    public Text fillAmountText;

    public Slider progressBar;

    //Keys
    public string PreviousXpSaveKey = "PreviousXpSaveKey";
    public string PreviousLevelSaveKey = "PreviousLevelSaveKey";

    [Header("Level Up Animation")] [SerializeField]
    private GameObject levelUpAnim;
    [SerializeField] private Text crowmTextOne;
    [SerializeField] private Text coinAmountText;
    private bool showLevelUpAnim = true;
    private bool playerServerXpReceived = false;
    private int totalWinningCoinAmount = 0;
    [SerializeField] private Button collectButton = null;
    [SerializeField] private GameObject[] leftCoinStack;
    [SerializeField] private GameObject[] rightCoinStack;
    //[SerializeField] private Button closeButton;
    [SerializeField] private Text levelUpText;
    [SerializeField] private Text collectButtonText;
    [SerializeField] AudioSource coinAudioSrc = null;

    [Header("Coin Fly Animation")] 
    [SerializeField] private RectTransform coinTargetPosition = null;
    [SerializeField] private GameObject coinPrefeb;
    [SerializeField] Transform coinHolderOne = null;
    [SerializeField] Transform coinHolderTwo= null;
    Sequence[] coinSeqOne = new Sequence[0];
    Sequence[] coinSeqTwo = new Sequence[0];
    private int numberOfCoinElements = 20;
    [SerializeField] private Text playerTotalCoinText = null;
    [SerializeField] private int initialCoinAmount = 0;
    [SerializeField] private int playerWinningCoinAmountInLevelUpAnim = 0;
    [SerializeField] private int playerWinningCoinAmount = 0;
    [SerializeField] private int loopFlag = 1;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    private void Start()
    {
        ChangeLanguage();
        GetValueFromKey();
        SetLevelAndXpPointTextAtStart();
    }


    public void GetValueFromKey()
    {

        if (IsPlayerRewardAvailable())
        {
            playerServerXpReceived = true;
        }
        if (PlayerPrefs.HasKey(PreviousXpSaveKey))
        {
            previousXp = PlayerPrefs.GetInt(PreviousXpSaveKey);
       
        }
        else
        {
            previousXp = 0;
        }

        if (PlayerPrefs.HasKey(PreviousLevelSaveKey))
        {
            previousLevel = PlayerPrefs.GetInt(PreviousLevelSaveKey);
            
            savingPreviousLevelForLevelUpAnim = previousLevel;
        }
        else
        {
            previousLevel = 0;
            savingPreviousLevelForLevelUpAnim = previousLevel;
        }
    }

    public void CallAtStart(int playerTotalXp)
    {
        ShowLevelNoInUi(CalculateLevel((int) (playerTotalXp)));
        ShowFillAmountText(MyGainPoint((int) playerTotalXp), CalculateLevel((int) playerTotalXp));
    }

    public void SetLevelAndXpPointTextAtStart()
    {
        levelText.text = "Level " + previousLevel;
        int currentLevelTarget = Leveltarget(previousLevel);
        fillAmountText.text = MyGainPoint((int)previousXp).ToString("0") + "/" + currentLevelTarget.ToString();
        
        // Adding This Line For FillUp Bar
        float myGP = MyGainPoint((int) previousXp);
        float t = currentLevelTarget;
        float previousFillAmount = myGP / t;
        progressBar.value = previousFillAmount;
        
    }


    public void ShowProgressBarAnim()
    {
        levelText.text = "Level " + previousLevel;
        LanguageSelectionScript.instance.ChangeLevelLanguage(levelText, previousLevel);
        int xpGain = XP_System.Instance.XP_Point - (int) previousXp;
        Debug.Log("xxxxx xpGain " + xpGain);


        float myGainPoint = (float) (MyGainPoint((int) previousXp));
        float levelTarget = (float) (Leveltarget(previousLevel));

        float myGainPointBeforeStartAnim = myGainPoint;

        float previousFillAmount = myGainPoint / levelTarget;
        progressBar.value = previousFillAmount;

        float newFillAmount = 0;

        int count = 0;

        while (xpGain != 0)
        {
            count++;
            previousXp = previousXp + 1;

            xpGain--;
            myGainPoint = (MyGainPoint((int) previousXp));
            if (myGainPoint == 0)
            {
                myGainPoint = (Leveltarget(previousLevel));
            }


            levelTarget = (Leveltarget(previousLevel));


            newFillAmount = (float) myGainPoint / (float) levelTarget;

            if (newFillAmount >= 1)
            {
                FillBarAnim(previousFillAmount, newFillAmount, levelTarget, myGainPointBeforeStartAnim, true);
                return;
            }
        }

        FillBarAnim(previousFillAmount, newFillAmount, levelTarget, myGainPointBeforeStartAnim, count);
    }


    public int CalculateLevel(int xp)
    {
        int pow = 1;

        while (Mathf.Pow(2, pow) <= xp)
        {
            xp = xp - (int) Mathf.Pow(2, pow);
            pow++;
        }

        return pow;
    }

    public int MyGainPoint(int xp)
    {
        int pow = 1;

        while (Mathf.Pow(2, pow) <= xp)
        {
            xp = xp - (int) Mathf.Pow(2, pow);
            pow++;
        }

        return xp;
    }


    public int Leveltarget(int level)
    {
        return (int) Mathf.Pow(2, level);
    }

    public void ShowLevelNoInUi(int level)
    {
        levelText.text = "Level " + level.ToString();
        LanguageSelectionScript.instance.ChangeLevelLanguage(levelText, level);
    }

    public void ShowFillAmountText(int myXpPoint, int level)
    {
        fillAmountText.text = myXpPoint.ToString() + "/" + Leveltarget(level).ToString();
    }

    public void ShowFillAmountTextForUiUpdate(int totalXp, int level)
    {
        int xp = MyGainPoint(totalXp);
        fillAmountText.text = xp.ToString() + "/" + Leveltarget(level).ToString();

        float target = (float) Leveltarget(level);
        float floatXp = (float) xp;
        progressBar.value = floatXp / target;
    }


    public void FillBarAnim(float previousFillAmount, float newFillAmount, float leveltarget,
        float myGainPointBeforeStartAnim, bool levelComplete)
    {
        progressBar.value = previousFillAmount;
        progressBar.DOValue(newFillAmount, 2, false).OnComplete(() =>
        {
            if (showLevelUpAnim == true && XP_LevelUI.instance.IsPlayerRewardAvailable() && playerServerXpReceived == true)
            {
                ShowLevelUpAnim(savingPreviousLevelForLevelUpAnim);
                showLevelUpAnim = false;
                playerServerXpReceived = false;
            }

            previousLevel++;
            progressBar.value = 0;
            ShowProgressBarAnim();
        });

        float totalXpGain = leveltarget - myGainPointBeforeStartAnim;
        float totalWaitAndAddXp = totalXpGain / 10;

        StartCoroutine(PlayerXpAmountAnim(totalWaitAndAddXp, totalXpGain, leveltarget, myGainPointBeforeStartAnim));
    }

    public void FillBarAnim(float previousFillAmount, float newFillAmount, float leveltarget,
        float myGainPointBeforeStartAnim, int xpGain)
    {
        progressBar.value = previousFillAmount;
        progressBar.DOValue(newFillAmount, 2, false);


        float totalXpGain = xpGain;
        float totalWaitAndAddXp = totalXpGain / 10;


        StartCoroutine(PlayerXpAmountAnim(totalWaitAndAddXp, totalXpGain, leveltarget, myGainPointBeforeStartAnim));
    }

    IEnumerator PlayerXpAmountAnim(float AddXpAmount, float totalXpGain, float levelTarget,
        float myGainPointBeforeStartAnim)
    {
        while (totalXpGain >= 0)
        {
            yield return new WaitForSeconds(0.2f);
            myGainPointBeforeStartAnim = myGainPointBeforeStartAnim + AddXpAmount;
            fillAmountText.text = myGainPointBeforeStartAnim.ToString("0") + "/" + levelTarget.ToString();
            totalXpGain = totalXpGain - AddXpAmount;
        }
    }

    public void ShowLevelUpAnim(int previousLevelForAnim)
    {
        int levelUpAmount = ChangePlayerLevelInLevelUpAnim();

        if (levelUpAmount > 0)
        {
            crowmTextOne.text = (previousLevelForAnim - 1).ToString();
            levelUpAnim.SetActive(true);
            LevelUpAnimSound();
            StartCoroutine(LevelUpAnimLevelAndCoinAmountChange(previousLevelForAnim, levelUpAmount));
        }
    }

    IEnumerator LevelUpAnimLevelAndCoinAmountChange(int previousLevelForAnim, int levelUpAmount)
    {
        int coinGiftAmount = 0;
        while (levelUpAmount != 0)
        {
            yield return new WaitForSeconds(1);
            crowmTextOne.text = (previousLevelForAnim).ToString();
            coinGiftAmount = coinGiftAmount + LevelPassGiftCoinAmount(previousLevelForAnim);
            coinAmountText.text = coinGiftAmount.ToString();
            previousLevelForAnim++;
            levelUpAmount--;
        }

        playerWinningCoinAmount = coinGiftAmount;
        playerWinningCoinAmountInLevelUpAnim = playerWinningCoinAmount;
        crowmTextOne.text = (previousLevelForAnim).ToString();
        initialCoinAmount = CoinSystem.instance.GetBalance();
        collectButton.gameObject.SetActive(true);
    }

    int LevelPassGiftCoinAmount(int levelNo)
    {
        int giftAmount = 1000 + (200 * (levelNo - 1));
        return giftAmount;
    }

    int ChangePlayerLevelInLevelUpAnim()
    {
        int currentLevel = CalculateLevel(XP_System.Instance.XP_Point);
        int levelUp = currentLevel - savingPreviousLevelForLevelUpAnim;
        return levelUp;
    }

    public bool IsPlayerRewardAvailable()
    {
        if (Application.internetReachability != NetworkReachability.NotReachable && PlayFabClientAPI.IsClientLoggedIn())
        {
            return true;
        }

        return false;
    }
    
    public void CoinFlyByAnimation()
    {
        if (GlobalSettings.instance.globalSettingsDataObject.MusicOn && AudioManager.instance != null)
        {
            AudioManager.instance.PlayAudioClip(CommonUIAudioClips.instance.BUTTON_CLICK);
        }
        collectButton.interactable = false;
        //closeButton.gameObject.SetActive(true);
            KillCoinSeq();
            CoinSystem.instance.AddCoins(playerWinningCoinAmount);
            float radius = 1;
            float spawDelay = 0.01f;
            StartCoroutine(_CoinFlyByAnimation(coinSeqOne,coinHolderOne,coinHolderOne.transform.position, radius, spawDelay, true));
            StartCoroutine(_CoinFlyByAnimation(coinSeqTwo,coinHolderTwo,coinHolderTwo.transform.position, radius, spawDelay, false));
    }
        
    private IEnumerator _CoinFlyByAnimation(Sequence[] coinSeq ,Transform coinHolder,Vector3 startPosition, float radius = 2, float spawDelay = 0.01f, bool showCoinInUi = false)
    {

            int perCoinValue = playerWinningCoinAmount / numberOfCoinElements;
            coinSeq = new Sequence[numberOfCoinElements];
            

            int coinStackCounterFlag = 0;
            
            
            for (int i = 0; i < numberOfCoinElements; i++)
            {
                Vector3 randomPos = Random.insideUnitCircle * radius;
                randomPos += startPosition;
                GameObject piece = Instantiate(coinPrefeb, startPosition, Quaternion.identity, coinHolder);

                piece.transform.SetAsFirstSibling();

                Vector3[] travelPath = new Vector3[3];
                travelPath[0] = randomPos;
                travelPath[1] = new Vector3((coinTargetPosition.position.x + startPosition.x) / Random.Range(1.2f, 1.5f), (coinTargetPosition.position.y + startPosition.y) / Random.Range(1.5f, 2.5f));
                travelPath[2] = coinTargetPosition.position;
                
                coinSeq[i].Append(piece.transform.DOMove(randomPos, 0.3f).SetEase(Ease.OutExpo));
                coinSeq[i].Append(piece.transform.DOPath(travelPath, 1f, PathType.CatmullRom).SetEase(Ease.InExpo).SetDelay(Random.Range(0.3f, 0.8f))
                    .OnComplete(() =>
                    {
                        if (showCoinInUi)
                        {
                            if (!coinAudioSrc.isPlaying)
                            {
                                if (GlobalSettings.instance != null && GlobalSettings.instance.globalSettingsDataObject.MusicOn)
                                    coinAudioSrc.Play();
                            }

                            if (loopFlag == numberOfCoinElements)
                            {
                                loopFlag++;
                                Debug.Log("ccccc Final Coin Balacnce : " + CoinSystem.instance.GetBalance());
                                playerTotalCoinText.text = CoinSystem.instance.GetBalance().ToString();
                                coinAmountText.text = "0";
                                HideLevelUpAnim();
                            }
                            else
                            {
                                ShowTotalCoinInUi(perCoinValue);
                                loopFlag++;
                            }
                            
                        }
                        
                    }));
                
                    
                

                if (numberOfCoinElements % 6 == 0)
                {
                    if (coinStackCounterFlag < 3)
                    {
                        leftCoinStack[coinStackCounterFlag].SetActive(false);
                        rightCoinStack[coinStackCounterFlag].SetActive(false);
                        coinStackCounterFlag++;
                    }
                }
                
                Destroy(piece, 5);

                yield return new WaitForSeconds(spawDelay);
            }
    }
    
    private void KillCoinSeq()
    {
        if (coinSeqOne.Length > 0)
        {
            for (int j = 0; j < coinSeqOne.Length; j++)
            {
                if (coinSeqOne[j] != null && coinSeqOne[j].IsActive())
                {
                    coinSeqOne[j].Kill();
                }
            }
        }
        if (coinSeqTwo.Length > 0)
        {
            for (int j = 0; j < coinSeqTwo.Length; j++)
            {
                if (coinSeqTwo[j] != null && coinSeqTwo[j].IsActive())
                {
                    coinSeqTwo[j].Kill();
                }
            }
        }
        coinSeqTwo = new Sequence[0];
    }
    
    private void OnDestroy()
    {
        KillCoinSeq();
    }

    private void ShowTotalCoinInUi(int coinAmount)
    {
        Debug.Log("cccccc Initial Amount : " + initialCoinAmount);
        initialCoinAmount = initialCoinAmount + coinAmount;
        playerTotalCoinText.text = initialCoinAmount.ToString();

       
        if (playerWinningCoinAmountInLevelUpAnim > coinAmount)
        {
            playerWinningCoinAmountInLevelUpAnim = playerWinningCoinAmountInLevelUpAnim - coinAmount;
            coinAmountText.text = playerWinningCoinAmountInLevelUpAnim.ToString();   
        }

    }

    public void HideLevelUpAnim()
    { 
        loopFlag = 0;
        levelUpAnim.SetActive(false);
        
    }

    public void ChangeLanguage()
    {
        if (LanguageSelectionScript.instance != null)
        {
            LanguageSelectionScript.instance.ChangeNumberLanguage(crowmTextOne);
            LanguageSelectionScript.instance.ChangeNumberLanguage(coinAmountText);
            LanguageSelectionScript.instance.ChangeNumberLanguage(playerTotalCoinText);
            LanguageSelectionScript.instance.ChangeCollectButtonText(collectButtonText);
            LanguageSelectionScript.instance.ChangeNumberLanguage(fillAmountText);
            LanguageSelectionScript.instance.ChangeLevelUpLanguage(levelUpText);
        }

    }

    public void LevelUpAnimSound()
    {
        if (GlobalSettings.instance.globalSettingsDataObject.MusicOn && AudioManager.instance != null)
        {
            AudioManager.instance.PlayAudioClip(CommonUIAudioClips.instance.LEVELUP_ANIM_SOUND);
            AudioManager.instance.PlayAudioClip(CommonUIAudioClips.instance.LEVELUP_BADGE_SOUND);
        }
    }
}