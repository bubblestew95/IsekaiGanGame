using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;


public class LobbyListUpdater : MonoBehaviour
{
    public Transform sessionListParent;  // �� ����� ǥ���� �θ� UI ������Ʈ (��ũ�� ��)
    public GameObject roomItemPrefab;    // ���� ��Ÿ���� ������ (RoomItem)

    private float refreshTime = 5f;      // �ڵ� ���ΰ�ħ ���� (5��)

    private async void Start()
    {
        InvokeRepeating(nameof(RefreshLobbyList), 0f, refreshTime); // 5�ʸ��� ����
    }

    public async void RefreshLobbyList()
    {
        try
        {
            QueryLobbiesOptions options = new QueryLobbiesOptions(); // �� �˻� �ɼ�
            QueryResponse lobbies = await LobbyService.Instance.QueryLobbiesAsync(options);

            foreach (Transform child in sessionListParent)
            {
                Destroy(child.gameObject);  // ���� ��� ����
            }

            foreach (var lobby in lobbies.Results)
            {
                GameObject roomItem = Instantiate(roomItemPrefab, sessionListParent);
                roomItem.GetComponent<RoomItem>().SetRoomInfo(lobby.Name, lobby.Players.Count, lobby.MaxPlayers);
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("Failed to refresh lobbies: " + e.Message);
        }
    }
}

