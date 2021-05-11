using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class XpLevelAnim : MonoBehaviour
{

    public Text playerXpCountText;
    public Text levelNoText;
    
    void Start()
    {
        
    }
    void Update()
    {
        
    }

    public int CalculateLevel()
    {
        int pow = 1;
        
        while(Mathf.FloorToInt(Mathf.Pow(2,pow)) <= XP_System.Instance.XP_Point)
        {
            pow++;
        }
        return pow;
    }

    public void SetLevelText(int levelNo)
    {
        
    }

    public void PlayerXpCountText()
    {
        
    }
    
}
