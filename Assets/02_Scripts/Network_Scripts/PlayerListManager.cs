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

    public void AddPlayer(string username, PlayerStatus status)
    {
        Debug.Log($"[PlayerListManager] AddPlayer ȣ��� - {username} / {status}");

        if (playerItems.ContainsKey(username))
        {
            Debug.Log($"[PlayerListManager] �̹� �����ϴ� �÷��̾�: {username}");
            return; // �ߺ� �߰� ����
        }

        GameObject newItem = Instantiate(playerItemPrefab, playerListParent); //PlayerList�� �߰�
        PlayerItem playerItem = newItem.GetComponent<PlayerItem>();

        if (playerItem == null)
        {
            Debug.LogError("[PlayerListManager] PlayerItem ������Ʈ�� ����!");
            return;
        }
        playerItem.SetPlayerInfo(username, status);
        playerItems.Add(username, playerItem);
        Debug.Log($"[PlayerListManager] Player �߰� �Ϸ�: {username}");
    }

    public void UpdatePlayerStatus(string username, PlayerStatus status)
    {
        if (playerItems.ContainsKey(username))
        {
            playerItems[username].SetStatus(status);
        }
    }

    public void RemovePlayer(string username)
    {
        if (playerItems.ContainsKey(username))
        {
            Destroy(playerItems[username].gameObject);
            playerItems.Remove(username);
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
    public bool ContainsPlayer(string username)
    {
        return playerItems.ContainsKey(username);
    }


}
