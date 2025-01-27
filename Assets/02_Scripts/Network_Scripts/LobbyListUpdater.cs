using System.Threading.Tasks;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;


public class LobbyListUpdater : MonoBehaviour
{
    //public Transform sessionListParent;  // 방 목록을 표시할 부모 UI 오브젝트 (스크롤 뷰)
    //public GameObject roomItemPrefab;    // 방을 나타내는 프리팹 (RoomItem)

    //private float refreshTime = 5f;      // 자동 새로고침 간격 (5초)

    //private async void Start()
    //{
    //    InvokeRepeating(nameof(RefreshLobbyList), 0f, refreshTime); // 5초마다 갱신
    //}

    //public async void RefreshLobbyList()
    //{
    //    try
    //    {
    //        QueryLobbiesOptions options = new QueryLobbiesOptions(); // 방 검색 옵션
    //        QueryResponse lobbies = await LobbyService.Instance.QueryLobbiesAsync(options);

    //        foreach (Transform child in sessionListParent)
    //        {
    //            Destroy(child.gameObject);  // 기존 목록 삭제
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

    public Transform sessionListParent;  // 방 목록을 표시할 부모 UI 오브젝트 (스크롤 뷰)
    public GameObject roomItemPrefab;    // 방을 나타내는 프리팹 (RoomItem)

    private float refreshTime = 5f;      // 자동 새로고침 간격 (5초)

    private async void Start()
    {
        await InitializeUnityServices();
        InvokeRepeating(nameof(RefreshLobbyList), 0f, refreshTime); // 5초마다 갱신
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
                Destroy(child.gameObject);  // 기존 목록 삭제
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

