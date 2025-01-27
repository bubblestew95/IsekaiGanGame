using Unity.Services.Lobbies.Models;
using UnityEngine;

public class PlayerListManager : MonoBehaviour
{
    public Transform playerListParent;  // �÷��̾� ����� ǥ���� �θ� ������Ʈ
    public GameObject playerPrefab;     // �÷��̾� ���� ������ (Cube, Capsule, Sphere)

    public void UpdatePlayerList(Lobby lobby)
    {
        foreach (Transform child in playerListParent)
        {
            Destroy(child.gameObject);  // ���� ��� ����
        }

        foreach (Player player in lobby.Players)
        {
            bool isHost = player.Id == lobby.HostId;  // HostId�� ���Ͽ� ���� Ȯ��
            GameObject playerItem = Instantiate(playerPrefab, playerListParent);
            playerItem.GetComponent<PlayerItem>().SetPlayerInfo(player.Data["username"].Value, isHost);
        }
    }

}

