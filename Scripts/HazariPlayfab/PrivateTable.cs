using System;
using System.Collections;
using System.Collections.Generic;
using Lobby;
using PlayFabKit;
using UnityEngine;
using UnityEngine.UI;

public class PrivateTable : MonoBehaviour
{

    [SerializeField] private GameObject _parentContentHolder;
    [SerializeField] private GameObject _joinAndCreatePanel;
    [SerializeField] private GameObject _setTableCoinAmountPanel;
    [SerializeField] private GameObject _enterRoomIdPanel;

    [SerializeField] private InputField roomIdInputField;

    public void OnJoinButtonClick()
    {
        _joinAndCreatePanel.SetActive(false);
        _enterRoomIdPanel.SetActive(true);
    }

    public void OnEnterButtonClick()
    {
        if (roomIdInputField.text.Length > 6 || roomIdInputField.text.Length < 6)
        {
            ToastNotification.instance.Show("Invalid ID");
        }
        else
        {
            PlayfabConstants.Instance.RoomId = roomIdInputField.text.ToUpper();
            PlayfabConstants.Instance.IsPrivateTable = true;
            PlayfabConstants.Instance.IsCreateTable = false;
            PlayfabConstants.Instance.IsRandomMatchMaking = true;
            SceneLoader.instance.LoadMatchMakingScene("RandomMatchMaking");
        }


    }

    public void OnCreateButtonClick()
    {
        _setTableCoinAmountPanel.SetActive(true);
        _setTableCoinAmountPanel.transform.parent.gameObject.SetActive(true);
        PlayfabConstants.Instance.IsPrivateTable = true;

    }

    public void OnBackButtonClick()
    {
        if (GlobalSettings.instance.globalSettingsDataObject.MusicOn && AudioManager.instance != null)
        {
              AudioManager.instance.PlayAudioClip(CommonUIAudioClips.instance.BUTTON_CLICK);
        }
        roomIdInputField.text = "";
        _parentContentHolder.SetActive(false);
        _joinAndCreatePanel.SetActive(true);
        _enterRoomIdPanel.SetActive(false);
        LobbyUIManager.instance._privateTableCanvasGroup.alpha = 0f;
    }

    public void OnPasteButtonClick()
    {
        string roomId = "";
        roomId.GetFromClipboard(out roomId);
        if (roomId.Length > 6)
        {
            roomIdInputField.text = roomId.Substring(roomId.Length - 6);
        }
        else
        {
            roomIdInputField.text = roomId;
        }
        
    }
}

public static class ClipboardExtension
{
    public static void GetFromClipboard(this string input, out string output)
    {
        output = input + GUIUtility.systemCopyBuffer.ToString();
    }
}
