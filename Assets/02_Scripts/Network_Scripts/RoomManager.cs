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

public class RoomManager : MonoBehaviour
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
    public RectTransform contentRect; // Scroll View의 Content 크기 조절
    public string gameSceneName = "GameTest"; // 다음 씬 이름

    private Dictionary<string, GameObject> roomList = new Dictionary<string, GameObject>(); // 방 목록 관리
    private float roomSpacing = 40f;
    public string currentRoom { get; private set; }
    private int maxPlayers = 4;

    private string username;
    private bool isInitialized = false;

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

        //NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("OnRelayMessage", OnRelayMessageReceived);

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

        

        //InvokeRepeating(nameof(RefreshRoomList), 0f, 15f); // 5초마다 자동 갱신

        //while (true)
        //{
        //    await UpdateLobbyPlayerList();
        //    await Task.Delay(3000); // 3초마다 최신 플레이어 리스트 확인
        //}
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

        //if (!string.IsNullOrEmpty(currentRoom))
        //{
        //    Debug.LogWarning("이미 방에 입장한 상태입니다.");
        //    return;
        //}

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

            CreateLobbyOptions options = new CreateLobbyOptions
            {
                IsPrivate = false,
                Player = new Player(AuthenticationService.Instance.PlayerId),
                Data = new Dictionary<string, DataObject>
                {
                    { "Username", new DataObject(DataObject.VisibilityOptions.Member, username) },
                    { "RelayJoinCode", new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode) }
                }
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(roomName, 4, options);
            SetCurrentRoom(lobby.Id);

            if (!string.IsNullOrEmpty(lobby.LobbyCode))
            {
                codeButton.GetComponentInChildren<TMP_Text>().text = lobby.LobbyCode;
                Debug.Log($"Lobby 생성됨: {lobby.LobbyCode}");
                

            }
            else
            {
                Debug.LogError("Lobby 생성 후 코드가 반환되지 않았습니다!");
            }

            // 최신 데이터를 가져오기 (최대 3회 재시도, 2초 간격)
            int maxAttempts = 3;
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                await Task.Delay(2000);
                try
                {
                    lobby = await LobbyService.Instance.GetLobbyAsync(lobby.Id);
                    if (lobby.Data != null) break;
                }
                catch (LobbyServiceException e)
                {
                    Debug.LogError($"Lobby 데이터를 가져오는 중 오류 발생: {e.Message}");
                }
            }

            if (lobby.Data == null)
            {
                Debug.LogError("Lobby 데이터를 가져오지 못했습니다.");
                return;
            }



            UpdateRoomUI(lobby);
            Debug.Log($"방 생성 성공: {lobby.Id}");
            Debug.Log($"방 데이터: {lobby.Data}");
            roomNameInput.interactable = false;
            joinCodeInput.interactable = false;
            createRoomButton.interactable = false;
            join_CodeRoomButton.interactable = false;
            codeButton.interactable = true;
            leaveRoomButton.interactable = true;

            // 방 생성 후 1초 대기 후 RefreshRoomList 실행
            await Task.Delay(1000);
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
            Lobby lobby = null;
            int maxAttempts = 3;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                try
                {
                    lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(roomCode);
                    Debug.Log($"방 아이디 찾음 : ({lobby.Id})");
                    Debug.Log($"방 데이터 : ({lobby.Data})");
                    if (lobby != null) break;
                }
                catch (LobbyServiceException)
                {
                    Debug.LogWarning($"Lobby 조회 실패. 재시도 중... ({attempt + 1}/{maxAttempts})");
                    await Task.Delay(1000);
                }
            }

            if (lobby == null)
            {
                Debug.LogError("Lobby를 찾을 수 없습니다! Room Code가 정확한지 확인하세요.");
                return;
            }
            //if (!await WaitForRelayJoinCode(lobby.Id))
            //{
                
            //    Debug.LogError("RelayJoinCode를 가져올 수 없어 방 입장 실패.");
            //    return;
            //}

            // lobby.Data가 null이면 최신 데이터 가져오기 시도
            int retryDataFetch = 3;
            while (lobby.Data == null && retryDataFetch > 0)
            {
                Debug.LogWarning("Lobby 데이터가 null입니다. 다시 시도합니다...");
                await Task.Delay(1000);
                var newLobby = await LobbyService.Instance.GetLobbyAsync(lobby.Id);
                if (newLobby != null) lobby = newLobby;
                retryDataFetch--;
            }

            // 여전히 null이면 오류 처리
            if (lobby.Data == null)
            {
                Debug.LogError("Lobby 데이터를 불러오지 못했습니다. 다시 시도하세요.");
                return;
            }

            // 신규 플레이어 확인 및 콘솔에 출력
            foreach (var player in lobby.Players)
            {
                string username = "알 수 없음"; // 기본값 설정

                //  `player.Data`가 null인지 먼저 확인
                if (player.Data == null)
                {
                    Debug.LogWarning($" 플레이어 {player.Id}의 데이터가 없습니다! (player.Data == null)");
                    continue; // 다음 플레이어로 넘어감
                }

                if (player.Data.ContainsKey("Username"))
                {
                    username = player.Data["Username"].Value;
                }
                else
                {
                    Debug.LogWarning($"플레이어 {player.Id}의 Username 데이터가 없습니다!");
                }

                Debug.Log($"[Lobby] 플레이어 참가 확인: Player ID = {player.Id}, Username = {username}");
            }



            // RelayJoinCode가 설정될 때까지 최대 3초 대기
            int relayWaitAttempts = 3;
            while (relayWaitAttempts > 0)
            {
                if (lobby.Data.ContainsKey("RelayJoinCode") && !string.IsNullOrEmpty(lobby.Data["RelayJoinCode"].Value))
                {
                    break; // RelayJoinCode가 존재하면 루프 종료
                }

                Debug.LogWarning("RelayJoinCode가 아직 동기화되지 않았습니다. 다시 시도합니다...");
                await Task.Delay(1000);
                var updatedLobby = await LobbyService.Instance.GetLobbyAsync(lobby.Id);
                if (updatedLobby != null) lobby = updatedLobby;
                relayWaitAttempts--;
            }

            // RelayJoinCode가 없으면 종료
            if (!lobby.Data.ContainsKey("RelayJoinCode") || string.IsNullOrEmpty(lobby.Data["RelayJoinCode"].Value))
            {
                Debug.LogError("RelayJoinCode가 존재하지 않거나 비어 있습니다.");
                return;
            }

            string relayJoinCode = lobby.Data["RelayJoinCode"].Value;
            Debug.Log($"[JoinRoom] RelayJoinCode 확인 완료: {relayJoinCode}");

            bool relaySuccess = await RelayManager.Instance.JoinRelay(relayJoinCode);
            if (!relaySuccess)
            {
                Debug.LogError("Relay 참가 실패!");
                return;
            }

            roomNameInput.interactable = false;
            joinCodeInput.interactable = false;
            createRoomButton.interactable = false;
            join_CodeRoomButton.interactable = false;
            codeButton.interactable = true;
            leaveRoomButton.interactable = true;

            SetCurrentRoom(lobby.Id);
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
