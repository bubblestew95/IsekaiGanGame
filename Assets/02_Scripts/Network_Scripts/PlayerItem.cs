using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public enum PlayerStatus { Host, Ready, Not_Ready }

public class PlayerItem : MonoBehaviour
{
    private string playerId;
    private string username;
    private PlayerStatus status;

    public TMP_Text usernameText;
    public Image statusIcon;


    public void SetPlayerInfo(string id, string name, PlayerStatus playerStatus)
    {
        playerId = id;
        username = name;
        status = playerStatus;

        UpdateUI();
    }

    private void UpdateUI()
    {
        usernameText.text = username;
        SetStatus(status);
    }

    public void SetCharacterSelection(int selectedCharacter)
    {
        Debug.Log($"[PlayerItem] Player {playerId} ������ ĳ����: {selectedCharacter}");
    }

    public void SetStatus(PlayerStatus status)
    {
        string labelKey = "LobbyPlayerIcons"; // ������ Label ���

        Addressables.LoadAssetsAsync<Sprite>(labelKey, null).Completed += (handle) =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                foreach (var sprite in handle.Result)
                {
                    if (sprite.name.Contains(status.ToString()))
                    {
                        statusIcon.sprite = sprite;
                        return;
                    }
                }
                Debug.LogError($"[PlayerItem] {status.ToString()} �������� ã�� �� �����ϴ�!");
            }
            else
            {
                Debug.LogError($"[PlayerItem] {labelKey}���� ������ �ε� ����!");
            }
        };
    }

    public void SetReadyState(bool isReady, bool isHost = false)
    {
        string statusName = isHost ? "Host" : (isReady ? "Ready" : "Not_Ready");

        string labelKey = "LobbyPlayerIcons"; // ������ Label ���
        Addressables.LoadAssetsAsync<Sprite>(labelKey, null).Completed += (handle) =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                foreach (var sprite in handle.Result)
                {
                    if (sprite.name.Contains(statusName))
                    {
                        statusIcon.sprite = sprite;
                        return;
                    }
                }
                Debug.LogError($"[PlayerItem] {statusName} �������� ã�� �� �����ϴ�!");
            }
            else
            {
                Debug.LogError($"[PlayerItem] {labelKey}���� ������ �ε� ����!");
            }
        };

        Debug.Log($"[PlayerItem] Ready ���� ���� - �÷��̾�: {usernameText.text}, ����: {statusName}");
    }

}
