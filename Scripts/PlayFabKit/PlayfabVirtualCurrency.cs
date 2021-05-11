#region Library
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System;
using TAAASH_KIT;
using Lobby;
#endregion


namespace PlayFabKit
{
    public static class PlayfabVirtualCurrency 
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="onSuccessAction"></param>
        /// <param name="onFailed"></param>
        public static void GetAllVirtualCurrency(Action onSuccessAction = null, Action onFailed = null)
        {
            PlayFabClientAPI.GetUserInventory(
           new GetUserInventoryRequest { },
           GetResult => 
           {
               var vc = GetResult.VirtualCurrency;
               if (vc.ContainsKey("GD"))
               {
                   if (CoinSystem.instance != null)
                   {
                       CoinSystem.instance.SetBalance(vc["GD"]);
                   }
                   if (LobbyUIManager.instance != null)
                   {
                       LobbyUIManager.instance.CoinTextUpdateFromServer();
                   }

                   PlayfabLeaderboard.SendLeaderboard(vc["GD"]);

                   if (onSuccessAction != null)
                       onSuccessAction?.Invoke();
               }

               if (vc.ContainsKey("XP"))
               {
                   if (XP_System.Instance != null)
                   {
                       XP_System.Instance.XP_Point = vc["XP"];
                   }
                   XP_LevelUI.instance.UI_Update();
               }
               else Debug.Log("123 XP nai");

               LobbyUIManager.instance.SetCoinAndProfileSync(true);

           },
           onError => 
           {
               if (onFailed != null) onFailed();
               
           }
           );
        }
        
    }      
}