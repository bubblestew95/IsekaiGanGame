using Unity.Services.Lobbies.Models;
using UnityEngine;

public class PlayerListManager : MonoBehaviour
{
    public Transform playerListParent;  // 플레이어 목록을 표시할 부모 오브젝트
    public GameObject playerPrefab;     // 플레이어 상태 프리팹 (Cube, Capsule, Sphere)

    public void UpdatePlayerList(Lobby lobby)
    {
        foreach (Transform child in playerListParent)
        {
            Destroy(child.gameObject);  // 기존 목록 삭제
        }

        foreach (Player player in lobby.Players)
        {
            bool isHost = player.Id == lobby.HostId;  // HostId와 비교하여 방장 확인
            GameObject playerItem = Instantiate(playerPrefab, playerListParent);
            playerItem.GetComponent<PlayerItem>().SetPlayerInfo(player.Data["username"].Value, isHost);
        }
    }

}

