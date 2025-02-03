using System.Collections.Generic;
using UnityEngine;

public class PlayerListManager : MonoBehaviour
{
    public static PlayerListManager Instance { get; private set; }

    [SerializeField] private Transform playerListParent; // PlayerItem들이 배치될 부모 오브젝트
    [SerializeField] private GameObject playerItemPrefab; // PlayerItem 프리팹

    private Dictionary<string, PlayerItem> playerItems = new Dictionary<string, PlayerItem>(); // 플레이어 목록

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

    public void AddPlayer(string username, bool isHost = false)
    {
        if (playerItems.ContainsKey(username)) return; // 중복 추가 방지

        GameObject newItem = Instantiate(playerItemPrefab, playerListParent);
        PlayerItem playerItem = newItem.GetComponent<PlayerItem>();
        playerItem.SetPlayerInfo(username, isHost ? PlayerStatus.Host : PlayerStatus.NotReady);

        playerItems.Add(username, playerItem);
    }

    public void RemovePlayer(string username)
    {
        if (playerItems.ContainsKey(username))
        {
            Destroy(playerItems[username].gameObject);
            playerItems.Remove(username);
        }
    }

    public void UpdatePlayerStatus(string username, PlayerStatus status)
    {
        if (playerItems.ContainsKey(username))
        {
            playerItems[username].SetStatus(status);
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
}
