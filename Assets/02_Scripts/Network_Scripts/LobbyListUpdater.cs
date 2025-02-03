using System.Threading.Tasks;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;


public class LobbyListUpdater : MonoBehaviour
{
    //public Transform sessionListParent;  // �� ����� ǥ���� �θ� UI ������Ʈ (��ũ�� ��)
    //public GameObject roomItemPrefab;    // ���� ��Ÿ���� ������ (RoomItem)

    //private float refreshTime = 5f;      // �ڵ� ���ΰ�ħ ���� (5��)

    //private async void Start()
    //{
    //    InvokeRepeating(nameof(RefreshLobbyList), 0f, refreshTime); // 5�ʸ��� ����
    //}

    //public async void RefreshLobbyList()
    //{
    //    try
    //    {
    //        QueryLobbiesOptions options = new QueryLobbiesOptions(); // �� �˻� �ɼ�
    //        QueryResponse lobbies = await LobbyService.Instance.QueryLobbiesAsync(options);

    //        foreach (Transform child in sessionListParent)
    //        {
    //            Destroy(child.gameObject);  // ���� ��� ����
    //        }

    //        foreach (var lobby in lobbies.Results)
    //        {
    //            GameObject roomItem = Instantiate(roomItemPrefab, sessionListParent);
    //            roomItem.GetComponent<RoomItem>().SetRoomInfo(lobby.Name, lobby.Players.Count, lobby.MaxPlayers);
    //        }
    //    }
    //    catch (LobbyServiceException e)
    //    {
    //        Debug.LogError("Failed to refresh lobbies: " + e.Message);
    //    }
    //}

    public Transform sessionListParent;  // �� ����� ǥ���� �θ� UI ������Ʈ (��ũ�� ��)
    public GameObject roomItemPrefab;    // ���� ��Ÿ���� ������ (RoomItem)

    private float refreshTime = 5f;      // �ڵ� ���ΰ�ħ ���� (5��)

    private async void Start()
    {
        await InitializeUnityServices();
        InvokeRepeating(nameof(RefreshLobbyList), 0f, refreshTime); // 5�ʸ��� ����
    }

    private async Task InitializeUnityServices()
    {
        try
        {
            await Unity.Services.Core.UnityServices.InitializeAsync();
            Debug.Log("Unity Services Initialized");

            if (!Unity.Services.Authentication.AuthenticationService.Instance.IsSignedIn)
            {
                await Unity.Services.Authentication.AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log("Signed in anonymously");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to initialize Unity services: {e.Message}");
        }
    }

    public async void RefreshLobbyList()
    {
        try
        {
            QueryLobbiesOptions options = new QueryLobbiesOptions();
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

