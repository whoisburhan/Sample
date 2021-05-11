using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OnlineFriendGameObject : MonoBehaviour
{
    public Text FriendNo;
    public Text FriendName;
    [HideInInspector] public string FriendsPlayfabID;
    public Text LogInStatus;
    public Image FriendProfilePic;
    public Image ActivityIcon;
    public Button GiftButton;
    public Button ChallengeButton;
    public Color OnlineColor;
    public Color OfflineColor;

    private void OnEnable()
    {
        PlayFabScript.OnFriendReload += AutoDestroy;
    }

    private void OnDisable()
    {
        PlayFabScript.OnFriendReload -= AutoDestroy;
    }


    public void AutoDestroy()
    {
        Destroy(this.gameObject);
    }
}
