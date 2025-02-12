using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public enum PlayerStatus { Host, Ready, Not_Ready }

public class PlayerItem : MonoBehaviour
{
    public TMP_Text usernameText;
    public Image statusIcon;


    public void SetPlayerInfo(string username, PlayerStatus status)
    {
        usernameText.text = username;
        SetStatus(status);
    }

    public void SetStatus(PlayerStatus status)
    {
        string labelKey = "LobbyPlayerIcons"; // ������ Label�� ���

        Addressables.LoadAssetsAsync<Sprite>(labelKey, null).Completed += (handle) =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                foreach (var sprite in handle.Result)
                {
                    if (sprite.name.Contains(status.ToString()))
                    {
                        statusIcon.sprite = sprite;
                        break;
                    }
                }
            }
            else
            {
                Debug.LogError($"[PlayerItem] {labelKey}���� �������� ã�� �� �����ϴ�!");
            }
        };
    }

    public void SetReadyState(bool isReady, bool isHost = false)
    {
        if (isHost)
        {
            statusIcon.sprite = Resources.Load<Sprite>("05_UI/Images/Lobby_Player/HostIcon");
        }
        else
        {
            string spritePath = isReady ? "05_UI/Images/Lobby_Player/ReadyIcon" : "05_UI/Images/Lobby_Player/Not_ReadyIcon";
            statusIcon.sprite = Resources.Load<Sprite>(spritePath);
        }

        Debug.Log($"[PlayerItem] Ready ���� ���� - �÷��̾�: {usernameText.text}, ����: {(isReady ? "Ready" : "Not Ready")}");
    }

}
