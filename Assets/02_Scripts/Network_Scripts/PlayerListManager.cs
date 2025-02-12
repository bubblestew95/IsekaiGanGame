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

    public void AddPlayer(string username, PlayerStatus status)
    {
        Debug.Log($"[PlayerListManager] AddPlayer 호출됨 - {username} / {status}");

        if (playerItems.ContainsKey(username))
        {
            Debug.Log($"[PlayerListManager] 이미 존재하는 플레이어: {username}");
            return; // 중복 추가 방지
        }

        GameObject newItem = Instantiate(playerItemPrefab, playerListParent); //PlayerList에 추가
        PlayerItem playerItem = newItem.GetComponent<PlayerItem>();

        if (playerItem == null)
        {
            Debug.LogError("[PlayerListManager] PlayerItem 컴포넌트가 없음!");
            return;
        }
        playerItem.SetPlayerInfo(username, status);
        playerItems.Add(username, playerItem);
        Debug.Log($"[PlayerListManager] Player 추가 완료: {username}");
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
            Debug.Log($"[PlayerList] {playerId}의 Ready 상태가 {isReady}로 변경됨.");
        }
    }
    public bool ContainsPlayer(string username)
    {
        return playerItems.ContainsKey(username);
    }


}
