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
        string labelKey = "LobbyPlayerIcons"; // 지정한 Label을 사용

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
                Debug.LogError($"[PlayerItem] {labelKey}에서 아이콘을 찾을 수 없습니다!");
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

        Debug.Log($"[PlayerItem] Ready 상태 변경 - 플레이어: {usernameText.text}, 상태: {(isReady ? "Ready" : "Not Ready")}");
    }

}
