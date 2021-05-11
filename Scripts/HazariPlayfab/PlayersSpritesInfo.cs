using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayersSpritesInfo : MonoBehaviour
{
    public static PlayersSpritesInfo Instance { get; private set; }

    public Sprite[] CountryFlags;

    public Sprite[] LocalAvatars;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    public Sprite GetCountryFlag(int _code)
    {
        switch (_code)
        {
            case 18: //BD
                return CountryFlags[1];
            case 102: //IN
                return CountryFlags[2];
            case 134: //MY
                return CountryFlags[3];
            case 155: //NP
                return CountryFlags[4];
            case 235: //US
                return CountryFlags[5];
            case 234: //GB
                return CountryFlags[6];
            case 198: //SL
                return CountryFlags[7];
            case 135: //MV
                return CountryFlags[8];
            case 167: //PK
                return CountryFlags[9];
            //  case 0: //AF
            //      return CountryFlags[10];
            case 103: //ID
                return CountryFlags[11];
            case 199: //SG
                return CountryFlags[12];
            case 85: //GR
                return CountryFlags[13];
            case 109: //IT
                return CountryFlags[14];
            case 238: //UZ
                return CountryFlags[15];
            case 65: //EG
                return CountryFlags[16];
            case 17: //BH
                return CountryFlags[17];
            case 119: //KW
                return CountryFlags[18];
            case 104: //IR
                return CountryFlags[19];
            case 194: //SA
                return CountryFlags[20];
            case 152: //MM
                return CountryFlags[21];
            case 141: //MU
                return CountryFlags[22];
            case 227: //TR
                return CountryFlags[23];
            case 233: //AE
                return CountryFlags[24];
            default: //International
                return CountryFlags[0];
        }

    }

    public Sprite GetAvatarSprite(int index)
    {
        return LocalAvatars[index];
    }
}
