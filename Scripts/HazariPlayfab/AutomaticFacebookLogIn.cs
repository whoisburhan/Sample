using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFabKit;
using PlayFab;
using PlayFab.ClientModels;
using LoginResult = PlayFab.ClientModels.LoginResult;
using System;
using TAAASH_KIT;
using Facebook.Unity;

public class AutomaticFacebookLogIn : FacebookLogIn
{
    protected override void FacebookData(IGraphResult result)
    {
        base.FacebookData(result);

        if(HazariPlayfabAuthentication.Instance != null)
        {
            HazariPlayfabAuthentication.Instance.LogInWithFacebookID();
        }
    }
}
