using System.Collections.Generic;
using UnityEngine;

public class PlayerListManager : MonoBehaviour
{
    public static PlayerListManager Instance { get; private set; }

    [SerializeField] private Transform playerListParent; // PlayerItem���� ��ġ�� �θ� ������Ʈ
    [SerializeField] private GameObject playerItemPrefab; // PlayerItem ������

    private Dictionary<string, PlayerItem> playerItems = new Dictionary<string, PlayerItem>(); // �÷��̾� ���

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddPlayer(string playerId, string username, PlayerStatus status, int selectedCharacter = -1)
    {
        Debug.Log($"[PlayerListManager] AddPlayer ȣ��� - {username} / {status}");

        if (playerItems.ContainsKey(playerId))
        {
            Debug.Log($"[PlayerListManager] �̹� �����ϴ� �÷��̾�: {playerId}");
            return; // �ߺ� �߰� ����
        }

        GameObject newItem = Instantiate(playerItemPrefab, playerListParent); //PlayerList�� �߰�
        PlayerItem playerItem = newItem.GetComponent<PlayerItem>();

        if (playerItem == null)
        {
            Debug.LogError("[PlayerListManager] PlayerItem ������Ʈ�� ����!");
            return;
        }
        playerItem.SetPlayerInfo(playerId, username, status);
        playerItem.SetCharacterSelection(selectedCharacter); // ������ ĳ���� ����
        playerItems.Add(playerId, playerItem);
        Debug.Log($"[PlayerListManager] Player �߰� �Ϸ�: {playerId} {username} ������ ĳ����: {selectedCharacter}");
    }
    // �÷��̾��� ĳ���� ���� ����
    public void UpdatePlayerCharacter(string playerId, int selectedCharacter)
    {
        if (playerItems.ContainsKey(playerId))
        {
            playerItems[playerId].SetCharacterSelection(selectedCharacter);
            Debug.Log($"[PlayerListManager] {playerId}�� ĳ���� ���� ���°� {selectedCharacter}�� �����.");
        }
    }

    public void UpdatePlayerStatus(string playerId, PlayerStatus status)
    {
        if (playerItems.ContainsKey(playerId))
        {
            playerItems[playerId].SetStatus(status);
        }
    }

    public void RemovePlayer(string playerId)
    {
        if (playerItems.ContainsKey(playerId))
        {
            Destroy(playerItems[playerId].gameObject);
            playerItems.Remove(playerId);
        }
    }

    public void ClearPlayers()
    {
        foreach (var item in playerItems.Values)
        {
            Destroy(item.gameObject);
        }
        playerItems.Clear();
    }

    public void UpdatePlayerReadyState(string playerId, bool isReady, bool isHost = false)
    {
        if (playerItems.ContainsKey(playerId))
        {
            playerItems[playerId].SetReadyState(isReady, isHost);
            Debug.Log($"[PlayerList] {playerId}�� Ready ���°� {isReady}�� �����.");
        }
    }
    public bool ContainsPlayer(string playerId)
    {
        return playerItems.ContainsKey(playerId);
    }



}
