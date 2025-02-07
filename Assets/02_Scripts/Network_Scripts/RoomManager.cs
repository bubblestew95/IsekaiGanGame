using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;
using Unity.Services.Core;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Netcode;
using System;
using Player = Unity.Services.Lobbies.Models.Player;

public class RoomManager : NetworkBehaviour
{
    public static RoomManager Instance { get; private set; }

    public GameObject roomPrefab; // Roomstate 프리팹
    public Transform roomListParent; // 방 목록을 표시할 부모 오브젝트
    public TMP_InputField roomNameInput; // 방 이름 입력 필드
    public TMP_InputField joinCodeInput; // 방 코드 입력 필드
    public Button createRoomButton; // 방 생성 버튼
    public Button joinRoomButton; // Join 버튼
    public Button join_CodeRoomButton; // Join 버튼
    public Button leaveRoomButton; // Join 버튼
    public Button codeButton; // Code 복사 버튼
    public Button readyButton; //ready 버튼
    public RectTransform contentRect; // Scroll View의 Content 크기 조절
    public string gameSceneName = "GameTest"; // 다음 씬 이름

    private Dictionary<string, GameObject> roomList = new Dictionary<string, GameObject>(); // 방 목록 관리
    private float roomSpacing = 40f;
    public string currentRoom { get; private set; }
    private int maxPlayers = 4;

    private string username;
    private bool isInitialized = false;

    //private bool IsHost = false;

    private HashSet<string> previousPlayerIDs = new HashSet<string>(); // 기존 플레이어 ID 저장


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    // 방 ID 설정 메서드
    public void SetCurrentRoom(string roomId)
    {
        currentRoom = roomId;
        Debug.Log($"현재 방 설정: {currentRoom}");
    }

    async void Start()
    {
        Debug.Log("RoomManager Start() 시작됨");

        if (RelayManager.Instance == null)
        {
            Debug.LogError("RoomManager: RelayManager 인스턴스가 존재하지 않습니다!");
            return;
        }
        while (NetworkManager.Singleton == null)
        {
            Debug.LogWarning("RoomManager: NetworkManager.Singleton이 아직 초기화되지 않음, 대기 중...");
            await Task.Delay(100);
        }
        // OnServerStarted 이벤트 등록
        NetworkManager.Singleton.OnServerStarted += () =>
        {
            if (NetworkManager.Singleton.CustomMessagingManager == null)
            {
                Debug.LogError("RoomManager: CustomMessagingManager가 초기화되지 않았습니다!");
                return;
            }

            Debug.Log("RoomManager: CustomMessagingManager 초기화 완료");
            NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("OnRelayMessage", OnRelayMessageReceived);
        };

        await InitializeServices(); // Unity Services 초기화
        // PlayerPrefs에서 username 불러오기
        username = PlayerPrefs.GetString("username");

        if (string.IsNullOrEmpty(username))
        {
            //Debug.LogError("Username not found!");
            username = "imsi";
            Debug.Log($"imsi - Welcome : , {username}!");
        }
        else
        {
            Debug.Log($"Welcome, {username}!");
            // 여기서 username을 사용하여 원하는 작업 수행
        }

        //createRoomButton.onClick.AddListener(CreateRoom);
        //joinRoomButton.onClick.AddListener(JoinRoom);
        //leaveRoomButton.onClick.AddListener(LeaveRoom);
        codeButton.interactable = false;
        leaveRoomButton.interactable = false;

    }

    // Relay에서 메시지를 수신하고 RoomList 갱신
    private void OnRelayMessageReceived(ulong clientId, FastBufferReader reader)
    {
        reader.ReadValueSafe(out string message);
        Debug.Log($"[Relay 메시지 수신] {message}");

        string[] messageParts = message.Split('|');
        if (messageParts.Length < 2) return;

        string command = messageParts[0];
        string roomId = messageParts[1];
        int playerCount = int.Parse(messageParts[2]);

        if (command == "UpdateRoom" && messageParts.Length == 3)
        {
            Debug.Log($"[Relay 메시지 처리] {roomId}의 인원 수 업데이트: {playerCount}/4");
            UpdateRoomPlayerCountClientRpc(roomId, playerCount);
        }
    }


    public async void CreateRoom()
    {

        if (!isInitialized)
        {
            Debug.LogError("Unity Services가 초기화되지 않았습니다!");
            return;
        }

        string roomName = roomNameInput.text.Trim();
        if (string.IsNullOrEmpty(roomName))
        {
            Debug.LogWarning("방 이름을 입력하세요.");
            return;
        }

        try
        {
            // Relay 생성 시 오류 방지를 위한 예외 처리 추가
            string relayJoinCode = await RelayManager.Instance.CreateRelay(maxPlayers);
            if (string.IsNullOrEmpty(relayJoinCode))
            {
                Debug.LogError("Relay 생성 실패");
                return;
            }
            Debug.Log($"Relay 생성 완료: {relayJoinCode}");

            // 올바르게 수정된 CreateLobbyOptions
            CreateLobbyOptions options = new CreateLobbyOptions();
            options.IsPrivate = false;
            options.Player = new Player(
                id: AuthenticationService.Instance.PlayerId,
                data: new Dictionary<string, PlayerDataObject>
                {
                    {
                        "Username", new PlayerDataObject(
                            visibility: PlayerDataObject.VisibilityOptions.Member,
                            value: username)
                    }
                }
            );
            options.Data = new Dictionary<string, DataObject>
            {
                { "RelayJoinCode", new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode) }
            };


            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(roomName, maxPlayers, options);
            // 방 코드 버튼 활성화 및 코드 표시
            SetCurrentRoom(lobby.Id);

            UpdateRoomUI(lobby); Debug.Log($"[CreateRoom] 방 생성 성공! ID: {lobby.Id}");
            Debug.Log($"[CreateRoom] 방 데이터 확인: RelayJoinCode = {lobby.Data["RelayJoinCode"].Value}");

            // 플레이어 데이터 확인 (방장)
            foreach (var player in lobby.Players)
            {
                string playerUsername = "알 수 없음";

                if (player.Data != null && player.Data.ContainsKey("Username"))
                {
                    playerUsername = player.Data["Username"].Value;
                }
                Debug.Log($"[CreateRoom] 플레이어 확인: Player ID = {player.Id}, Username = {playerUsername}");
            }

            readyButton.gameObject.SetActive(true);
            roomNameInput.interactable = false;
            joinCodeInput.interactable = false;
            createRoomButton.interactable = false;
            join_CodeRoomButton.interactable = false;
            codeButton.GetComponentInChildren<TMP_Text>().text = lobby.LobbyCode;
            codeButton.interactable = true;
            leaveRoomButton.interactable = true;

            // 방 생성자는 자동으로 Host가 됨
            if (NetworkManager.Singleton.IsHost)
            {
                Debug.Log("[CreateRoom] 현재 플레이어는 HOST 입니다.");
            }
            else
            {
                Debug.Log("[CreateRoom] 현재 플레이어는 클라이언트 입니다.");
            }

            // 방 생성 후 1초 대기 후 RefreshRoomList 실행
            await RefreshRoomList();
        }
        catch (Exception e)
        {
            Debug.LogError($"방 생성 실패: {e.Message}");
        }
    }

    // 방 입장
    public async void JoinRoom()
    {
        if (!isInitialized)
        {
            Debug.LogError("Unity Services가 초기화되지 않았습니다!");
            return;
        }

        string roomCode = joinCodeInput.text.Trim();
        if (string.IsNullOrEmpty(roomCode))
        {
            Debug.LogWarning("유효한 Room Code를 입력하세요.");
            return;
        }

        try
        {
            Lobby lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(roomCode);
            if (lobby == null)
            {
                Debug.LogError("[JoinRoom] Lobby를 찾을 수 없습니다!");
                return;
            }

            Debug.Log($"[JoinRoom] 방 참가 성공! Lobby ID: {lobby.Id}");

            //  참가한 클라이언트의 데이터를 `UpdatePlayerAsync()`를 사용하여 추가
            await LobbyService.Instance.UpdatePlayerAsync(lobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject>
            {
                {
                    "Username", new PlayerDataObject(
                        visibility: PlayerDataObject.VisibilityOptions.Member,
                        value: username)
                }
            }
            });

            Debug.Log($"[JoinRoom] 플레이어 데이터 업데이트 완료: {username}");

            // 최신 Lobby 정보 가져오기
            lobby = await LobbyService.Instance.GetLobbyAsync(lobby.Id);

            // 모든 플레이어의 `Username` 출력 (정상적으로 갱신되는지 확인)
            foreach (var player in lobby.Players)
            {
                string playerUsername = "알 수 없음";

                if (player.Data != null && player.Data.ContainsKey("Username"))
                {
                    playerUsername = player.Data["Username"].Value;
                }
                Debug.Log($"[CreateRoom] 플레이어 확인: Player ID = {player.Id}, Username = {playerUsername}");
            }

            //relay 참가
            string relayJoinCode = lobby.Data["RelayJoinCode"].Value;
            Debug.Log($"RelayJoinCode 확인 완료: {relayJoinCode}");

            bool relaySuccess = await RelayManager.Instance.JoinRelay(relayJoinCode);
            if (!relaySuccess)
            {
                Debug.LogError("Relay 참가 실패!");
                return;
            }

            readyButton.gameObject.SetActive(true);
            roomNameInput.interactable = false;
            joinCodeInput.interactable = false;
            createRoomButton.interactable = false;
            join_CodeRoomButton.interactable = false;
            codeButton.interactable = true;
            leaveRoomButton.interactable = true;
            //readyButton.SetActive(true);

            SetCurrentRoom(lobby.Id);

            // 호스트인지 여부 확인
            if (NetworkManager.Singleton.IsHost)
            {
                Debug.Log("[JoinRoom] 현재 플레이어는 HOST 입니다.");
            }
            else
            {
                Debug.Log("[JoinRoom] 현재 플레이어는 클라이언트 입니다.");
            }

            await RefreshRoomList();
            UpdateRoomUI(lobby);

            Debug.Log($"방 참가 성공: {lobby.Id}");
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"방 참가 실패: {e.Message}");
        }
    }


    // 방 퇴장
    public async void LeaveRoom()
    {
        if (string.IsNullOrEmpty(currentRoom))
        {
            Debug.LogWarning("현재 입장한 방이 없습니다.");
            return;
        }

        try
        {
            string currentScene = SceneManager.GetActiveScene().name;

            if (NetworkManager.Singleton.IsHost)
            {
                await RelayManager.Instance.TransferHostAndLeave(currentRoom);
            }
            else
            {
                await LobbyService.Instance.RemovePlayerAsync(currentRoom, AuthenticationService.Instance.PlayerId);
                Debug.Log("방에서 나갔으며, 새로운 유저가 입장 가능.");
            }

            roomNameInput.interactable = true;
            joinCodeInput.interactable = true;
            createRoomButton.interactable = true;
            join_CodeRoomButton.interactable = true;
            codeButton.interactable = false;
            leaveRoomButton.interactable = false;

            NetworkManager.Singleton.Shutdown();
            currentRoom = null;
        }
        catch (Exception e)
        {
            Debug.LogError($"방 나가기 실패: {e.Message}");
        }
    }

    //특정 방의 인원 수 업데이트
    private void UpdateRoomPlayerCount(string roomId, int playerCount)
    {
        if (roomList.ContainsKey(roomId))
        {
            TMP_Text playerCountText = roomList[roomId].transform.Find("roomPlayers").GetComponent<TMP_Text>();
            playerCountText.text = $"{playerCount}/{maxPlayers}";
        }
    }

    public async void OnReadyButtonClick()
    {
        if (string.IsNullOrEmpty(currentRoom))
        {
            Debug.LogError("[Ready] 현재 방이 없습니다!");
            return;
        }

        try
        {
            string playerId = AuthenticationService.Instance.PlayerId;

            // Ready 상태를 서버에 저장
            await LobbyService.Instance.UpdatePlayerAsync(currentRoom, playerId, new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject>
            {
                {
                    "Ready", new PlayerDataObject(
                        visibility: PlayerDataObject.VisibilityOptions.Member,
                        value: "True")
                }
            }
            });

            Debug.Log("[Ready] 플레이어 Ready 상태 업데이트 완료!");

            // 모든 클라이언트에게 Ready 상태 알리기
            NotifyPlayersReady(playerId);

            // Ready 상태 확인 후 씬 전환
            await CheckAllPlayersReady();
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"[Ready] Ready 상태 업데이트 실패: {e.Message}");
        }
    }

    [ClientRpc]
    private void NotifyPlayersReadyClientRpc(string playerId)
    {
        if (!IsServer) // 서버는 이미 상태를 알고 있으므로 클라이언트만 업데이트
        {
            Debug.Log($"[Ready] 플레이어 {playerId}가 Ready 상태가 되었습니다.");
            PlayerListManager.Instance.UpdatePlayerReadyState(playerId, true);
        }
    }

    private void NotifyPlayersReady(string playerId)
    {
        if (IsServer)
        {
            NotifyPlayersReadyClientRpc(playerId);
        }
    }



    public async Task CheckAllPlayersReady()
    {
        try
        {
            //  최신 Lobby 데이터 가져오기
            Lobby lobby = await LobbyService.Instance.GetLobbyAsync(currentRoom);

            int totalPlayers = lobby.Players.Count;
            int readyCount = 0;

            foreach (var player in lobby.Players)
            {
                if (player.Data.ContainsKey("Ready") && player.Data["Ready"].Value == "True")
                {
                    readyCount++;
                }
            }

            Debug.Log($"[Ready] 현재 Ready한 플레이어 수: {readyCount}/{totalPlayers}");

            //  모든 입장한 플레이어가 Ready 상태이면 씬 전환
            if (readyCount == totalPlayers && totalPlayers > 1)
            {
                Debug.Log("[Ready] 모든 플레이어가 Ready 상태입니다! 다음 씬으로 이동!");

                if (NetworkManager.Singleton.IsHost)
                {
                    Debug.Log("IsHost진입");
                    LoadNextSceneServerRpc();
                }
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"[Ready] Ready 상태 확인 실패: {e.Message}");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void LoadNextSceneServerRpc()
    {
        Debug.Log("[Ready] LoadNextSceneServerRpc() 실행됨");
        LoadNextSceneClientRpc();
    }

    [ClientRpc]
    private void LoadNextSceneClientRpc()
    {
        Debug.Log("[Ready] LoadNextSceneClientRpc() 실행됨");
        SceneManager.LoadScene(gameSceneName);
    }



    //private void LoadNextScene()
    //{
    //    if (IsHost)
    //    {
    //        Debug.Log("[Ready] 모든 플레이어가 Ready 상태! 네트워크 오브젝트 정리 후 씬 전환!");

    //        foreach (var playerObject in FindObjectsOfType<NetworkObject>())
    //        {
    //            if (playerObject.IsSpawned)
    //            {
    //                playerObject.Despawn();
    //                Debug.Log($"[Ready] 네트워크 오브젝트 제거: {playerObject.name}");
    //            }
    //        }
    //    }

    //    SceneManager.LoadScene("GameTest");
    //}



    // Room UI 업데이트
    private void UpdateRoomUI(Lobby lobby)
    {
        if (roomList.ContainsKey(lobby.Id))
        {
            TMP_Text playerCountText = roomList[lobby.Id].transform.Find("roomPlayers").GetComponent<TMP_Text>();
            playerCountText.text = $"{lobby.Players.Count}/{lobby.MaxPlayers}";
        }
        else
        {
            GameObject newRoom = Instantiate(roomPrefab, roomListParent);
            newRoom.transform.Find("roomName").GetComponent<TMP_Text>().text = lobby.Name;
            newRoom.transform.Find("roomPlayers").GetComponent<TMP_Text>().text = $"{lobby.Players.Count}/{lobby.MaxPlayers}";
            roomList.Add(lobby.Id, newRoom);
        }
    }

    // UI 리셋
    private void ResetRoomUI()
    {
        roomNameInput.interactable = true;
        joinCodeInput.interactable = true;
        createRoomButton.interactable = true;
        joinRoomButton.interactable = true;
        leaveRoomButton.interactable = false;
        codeButton.interactable = false;
    }


    [ServerRpc(RequireOwnership = false)]
    private void UpdateRoomPlayerCountServerRpc(string roomId, int playerCount)
    {
        UpdateRoomPlayerCountClientRpc(roomId, playerCount);
    }

    [ClientRpc]
    private void UpdateRoomPlayerCountClientRpc(string roomId, int playerCount)
    {
        if (roomList.ContainsKey(roomId))
        {
            TMP_Text playerCountText = roomList[roomId].transform.Find("roomPlayers").GetComponent<TMP_Text>();
            playerCountText.text = $"{playerCount}/4";
            Debug.Log($"[ClientRpc] {roomId}의 인원 수 업데이트: {playerCount}/4");
        }
    }

    // 서버에서 방 삭제 동기화
    [ServerRpc(RequireOwnership = false)]
    private void DestroyRoomServerRpc(string roomId)
    {
        DestroyRoomClientRpc(roomId);
    }

    // 모든 클라이언트에서 방 삭제
    [ClientRpc]
    private void DestroyRoomClientRpc(string roomId)
    {
        if (roomList.ContainsKey(roomId))
        {
            Destroy(roomList[roomId]);
            roomList.Remove(roomId);
        }
        RefreshRoomList();
    }



    public async void UpdatePlayerList()
    {
        if (string.IsNullOrEmpty(currentRoom)) return;

        try
        {
            // 서버에서 최신 방 정보 가져오기
            Lobby lobby = await LobbyService.Instance.GetLobbyAsync(currentRoom);

            // PlayerListManager를 통해 기존 플레이어 삭제
            PlayerListManager.Instance.ClearPlayers();

            // 새로 갱신된 플레이어 정보 추가
            foreach (var player in lobby.Players)
            {
                string playerName = player.Data["Username"].Value;
                bool isHost = player.Id == lobby.HostId;

                // 플레이어 추가
                PlayerListManager.Instance.AddPlayer(playerName, isHost);
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Failed to update player list: {e.Message}");
        }
    }


    public void UpdatePlayerStatus(string playerName, PlayerStatus status)
    {
        PlayerListManager.Instance.UpdatePlayerStatus(playerName, status);
    }

    private void CheckPlayerCount()
    {
        if (roomList.ContainsKey(currentRoom))
        {
            TMP_Text playerCountText = roomList[currentRoom].transform.Find("roomPlayers").GetComponent<TMP_Text>();
            if (playerCountText.text == "4/4")
            {
                Debug.Log("4명 입장 완료! 게임 씬으로 이동");
                SceneManager.LoadScene(gameSceneName);
            }
        }
    }

    private string GenerateRoomCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        char[] stringChars = new char[5];
        for (int i = 0; i < 5; i++)
        {
            stringChars[i] = chars[UnityEngine.Random.Range(0, chars.Length)];
        }
        return new string(stringChars);
    }

    private void UpdateScrollView()
    {
        // 방 개수에 따라 Content 크기 조절
        int roomCount = roomList.Count;
        float contentHeight = Mathf.Max(200f, (roomCount * roomSpacing) + 80f);
        contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, contentHeight);
    }

    // 중간 호출 메서드 추가
    public void OnRefreshRoomListButton()
    {
        // RefreshRoomList 호출
        _ = RefreshRoomList();
    }

    public async Task RefreshRoomList()
    {
        try
        {
            QueryLobbiesOptions options = new QueryLobbiesOptions
            {
                Filters = new List<QueryFilter>
            {
                new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
            }
            };

            QueryResponse response = await LobbyService.Instance.QueryLobbiesAsync(options);

            foreach (var lobby in response.Results)
            {
                if (roomList.ContainsKey(lobby.Id))
                {
                    TMP_Text playerCountText = roomList[lobby.Id].transform.Find("roomPlayers").GetComponent<TMP_Text>();
                    playerCountText.text = $"{lobby.Players.Count}/4";
                }
                else
                {
                    GameObject newRoom = Instantiate(roomPrefab, roomListParent);
                    newRoom.transform.Find("roomName").GetComponent<TMP_Text>().text = lobby.Name;
                    newRoom.transform.Find("roomPlayers").GetComponent<TMP_Text>().text = $"{lobby.Players.Count}/4";
                    roomList.Add(lobby.Id, newRoom);
                }
            }

            //  현재 호스트가 속한 방을 따로 갱신
            if (!string.IsNullOrEmpty(currentRoom))
            {
                Lobby myLobby = await LobbyService.Instance.GetLobbyAsync(currentRoom);
                if (myLobby != null)
                {
                    if (roomList.ContainsKey(currentRoom))
                    {
                        TMP_Text playerCountText = roomList[currentRoom].transform.Find("roomPlayers").GetComponent<TMP_Text>();
                        playerCountText.text = $"{myLobby.Players.Count}/4";
                    }
                }
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Failed to refresh room list: {e.Message}");
        }
    }



    private async Task InitializeServices()
    {
        try
        {

            await UnityServices.InitializeAsync();

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log($"AuthenticationService: 로그인 완료 (Player ID: {AuthenticationService.Instance.PlayerId})");
            }
            isInitialized = true;
            Debug.Log("Unity Services 초기화 완료 & Authentication 성공");
        }
        catch (Exception e)
        {
            Debug.LogError($"Unity Services Initialization Failed: {e.Message}");
        }
    }
    
    public async Task UpdateLobbyPlayerList()
    {
        if (string.IsNullOrEmpty(currentRoom))
        {
            Debug.LogWarning("현재 방이 설정되지 않았습니다.");
            return;
        }

        try
        {
            // 최신 Lobby 정보 가져오기
            Lobby lobby = await LobbyService.Instance.GetLobbyAsync(currentRoom);
            if (lobby == null)
            {
                Debug.LogError("Lobby 정보를 가져올 수 없습니다.");
                return;
            }

            // 현재 플레이어 ID 목록 생성
            HashSet<string> currentPlayerIDs = new HashSet<string>();
            foreach (var player in lobby.Players)
            {
                currentPlayerIDs.Add(player.Id);
            }

            // 새로운 플레이어 감지 및 로그 출력
            foreach (string playerId in currentPlayerIDs)
            {
                if (!previousPlayerIDs.Contains(playerId))
                {
                    Debug.Log($"[호스트] 새로운 플레이어 입장: Player ID = {playerId}");
                }
            }

            // 이전 플레이어 목록 업데이트
            previousPlayerIDs = new HashSet<string>(currentPlayerIDs);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Lobby 플레이어 목록 업데이트 실패: {e.Message}");
        }
    }



    // 방 삭제 처리
    private void DestroyRoom(string roomId)
    {
        if (roomList.ContainsKey(roomId))
        {
            Destroy(roomList[roomId]);
            roomList.Remove(roomId);
            Debug.Log($"Room {roomId} has been deleted.");
        }

        RefreshRoomList();
    }

    private async Task<bool> WaitForRelayJoinCode(string lobbyId)
    {
        int maxAttempts = 5;
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            try
            {
                Lobby lobby = await LobbyService.Instance.GetLobbyAsync(lobbyId);
                if (lobby.Data.ContainsKey("RelayJoinCode") && !string.IsNullOrEmpty(lobby.Data["RelayJoinCode"].Value))
                {
                    Debug.Log($" RelayJoinCode 확인 완료: {lobby.Data["RelayJoinCode"].Value}");
                    return true;
                }
            }
            catch (LobbyServiceException e)
            {
                Debug.LogWarning($"RelayJoinCode 조회 실패: {e.Message}");
            }

            Debug.Log($"RelayJoinCode가 아직 설정되지 않음. ({attempt + 1}/{maxAttempts}) 재시도 중...");
            await Task.Delay(2000); // 2초 대기 후 다시 시도
        }

        Debug.LogError(" 최대 재시도 후에도 RelayJoinCode를 가져오지 못함.");
        return false;
    }
}
