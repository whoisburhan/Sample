using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayfabCurrencyHandler : MonoBehaviour
{
    public static PlayfabCurrencyHandler Instance { get; private set; }

    int[] hazariGameCost = new int[] { 50, 100, 200, 500, 1000, 2000 };
    int[] nineCardGameCost = new int[] { 10, 20, 50, 100, 200 };

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

   

}
