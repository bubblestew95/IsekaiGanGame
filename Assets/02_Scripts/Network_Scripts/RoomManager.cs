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
using Unity.Collections;

public class RoomManager : NetworkBehaviour
{
    public static RoomManager Instance { get; private set; }

    public GameObject roomPrefab; // Roomstate 프리팹
    public Transform roomListParent; // 방 목록을 표시할 부모 오브젝트
    public TMP_InputField roomNameInput; // 방 이름 입력 필드
    public TMP_InputField joinCodeInput; // 방 코드 입력 필드
    public Button createRoomButton; // 방 생성 버튼
    public Button join_SelectRoomButton; // Select Join 버튼
    public Button join_CodeRoomButton; // Code Join 버튼
    public Button leaveRoomButton; // Join 버튼
    public Button codeButton; // Code 복사 버튼
    public Button readyButton; //ready 버튼
    public Button startButton; //start 버튼
    public RectTransform contentRect; // Scroll View의 Content 크기 조절
    public string gameSceneName = "GameTest"; // 다음 씬 이름

    private Dictionary<string, GameObject> roomList = new Dictionary<string, GameObject>(); // 방 목록 관리
    private float roomSpacing = 40f;
    public string currentRoom { get; private set; }
    private int maxPlayers = 4;

    private string username;
    private bool isInitialized = false;
    private Lobby currentLobby;
    private bool playerJoined = false; // 플레이어가 들어왔을때의 상태를 나타낼 변수
    private bool isReady = false; //Ready 상태를 저장하는 변수

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
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("[RoomManager] NetworkManager가 존재하지 않습니다!");
        }

        Debug.Log($"[RoomManager] 현재 NetworkManager 상태 - IsServer: {NetworkManager.Singleton.IsServer}, IsClient: {NetworkManager.Singleton.IsClient}, IsHost: {NetworkManager.Singleton.IsHost}");

        Debug.Log("[RoomManager] Relay 설정 확인 완료! StartHost() 실행 가능");


        // 서버가 이미 실행 중이라면 즉시 핸들러 등록
        if (NetworkManager.Singleton.IsServer)
        {
            Debug.Log("[RoomManager] 현재 서버 상태 - 이미 실행 중 (핸들러 즉시 등록)");
            RegisterRelayMessageHandler();
        }
        else
        {
            // 서버 시작 후 실행되도록 이벤트 등록
            Debug.Log("[RoomManager] 현재 서버가 아님. OnServerStarted 이벤트 등록 대기");
            NetworkManager.Singleton.OnServerStarted += () =>
            {
                Debug.Log("[RoomManager] OnServerStarted 이벤트 실행됨!");
                RegisterRelayMessageHandler();
            };
        }

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


        codeButton.interactable = false;
        leaveRoomButton.interactable = false;

    }

    void RegisterRelayMessageHandler()
    {
        if (NetworkManager.Singleton.CustomMessagingManager == null)
        {
            Debug.LogError("[RoomManager] CustomMessagingManager가 존재하지 않습니다!");
            return;
        }

        Debug.Log("[RoomManager] CustomMessagingManager 초기화 완료 - 메시지 핸들러 등록 중...");
        NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("OnRelayMessage", OnRelayMessageReceived);
    }

    // Relay에서 메시지를 수신하고 RoomList 갱신
    private void OnRelayMessageReceived(ulong clientId, FastBufferReader reader)
    {
        Debug.Log($"[Relay 메시지 수신 - 서버] 메시지 수신 시작 (clientId: {clientId})");

        ushort messageLength;
        reader.ReadValueSafe(out messageLength); // 문자열 길이 먼저 읽기

        byte[] messageBytes = new byte[messageLength];
        reader.ReadBytesSafe(ref messageBytes, messageLength); // 문자열 데이터를 읽기

        string message = System.Text.Encoding.UTF8.GetString(messageBytes); // UTF8로 변환

        Debug.Log($"[Relay 메시지 수신 - 서버] 클라이언트({clientId})로부터 메시지 수신: {message}");

        string[] messageParts = message.Split('|');
        if (messageParts.Length < 3) return;

        string command = messageParts[0];
        string roomId = messageParts[1];
        int playerCount = int.Parse(messageParts[2]);

        if (command == "UpdateRoom")
        {
            Debug.Log($"[Relay 메시지 처리] {roomId}의 인원 수 업데이트: {playerCount}/{maxPlayers}");
            UpdateRoomPlayerCountServerRpc(roomId, playerCount);
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
            // Relay 생성
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
                    },
                    
                }
            );
            options.Data = new Dictionary<string, DataObject>
            {
                { "RelayJoinCode", new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode) }
            };

            currentLobby = await LobbyService.Instance.CreateLobbyAsync(roomName, maxPlayers, options);

            UpdateLobbyOptions updateLobbyOptions = new UpdateLobbyOptions();
            updateLobbyOptions.Data = new Dictionary<string, DataObject>
            {
                { "LobbyCode", new DataObject(DataObject.VisibilityOptions.Member, currentLobby.LobbyCode) }
            };

            // 방 데이터 업데이트
            currentLobby = await LobbyService.Instance.UpdateLobbyAsync(currentLobby.Id, updateLobbyOptions);

            SetCurrentRoom(currentLobby.Id);

            // NetworkManager 시작
            NetworkManager.Singleton.StartHost();

            Debug.Log($"[CreateRoom] 방 데이터 확인: RelayJoinCode = {currentLobby.Data["RelayJoinCode"].Value}");

            // 플레이어 데이터 확인 (방장)
            foreach (var player in currentLobby.Players)
            {
                string playerUsername = "알 수 없음";

                if (player.Data != null && player.Data.ContainsKey("Username"))
                {
                    playerUsername = player.Data["Username"].Value;
                }
                Debug.Log($"[CreateRoom] 플레이어 확인: Player ID = {player.Id}, Username = {playerUsername}");
            }

            SetUIState(true);
            UpdateRoomUI(currentLobby);
            codeButton.GetComponentInChildren<TMP_Text>().text = currentLobby.LobbyCode;

            // 방 생성자는 자동으로 Host가 됨
            if (NetworkManager.Singleton.IsHost)
            {
                Debug.Log("[CreateRoom] 현재 플레이어는 HOST 입니다.");
            }
            else
            {
                Debug.Log("[CreateRoom] 현재 플레이어는 클라이언트 입니다.");
            }

            await SubscribeToLobbyEvents(currentLobby.Id);
            //RefreshRoomList 실행
            await RefreshRoomList();

        }
        catch (Exception e)
        {
            Debug.LogError($"방 생성 실패: {e.Message}");
        }
    }

    public async void JoinCodeRoom()
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
            Debug.Log($"[JoinRoom] 방 참가 요청: Room Code = {roomCode}");

            // 코드로 방 찾기
            currentLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(roomCode);

            if (currentLobby == null)
            {
                Debug.LogError("[JoinRoom] Lobby를 찾을 수 없습니다!");
                return;
            }

            Debug.Log($"[JoinRoom] 방 참가 성공! Lobby ID: {currentLobby.Id}");

            // 공통 처리 메서드 호출 (Relay 참가 및 UI 갱신)
            await JoinRoom(currentLobby.Id);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"방 참가 실패: {e.Message}");
        }
    }

    public async void JoinSelectedRoom()
    {
        if (string.IsNullOrEmpty(currentRoom))
        {
            Debug.LogError("선택된 방이 없습니다!");
            return;
        }

        try
        {
            Debug.Log($"[JoinSelectedRoom] 방 참가 요청: {currentRoom}");

            // 선택된 Room ID로 참가
            currentLobby = await LobbyService.Instance.JoinLobbyByIdAsync(currentRoom);

            if (currentLobby == null)
            {
                Debug.LogError("[JoinSelectedRoom] Lobby를 찾을 수 없습니다!");
                return;
            }

            Debug.Log($"[JoinSelectedRoom] 방 참가 성공! Lobby ID: {currentLobby.Id}");

            // 공통 처리 메서드 호출 (Relay 참가 및 UI 갱신)
            await JoinRoom(currentLobby.Id);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"[JoinSelectedRoom] 방 참가 실패: {e.Message}");
        }
    }

    // 중복된 로직을 처리하는 메서드 (공통 함수)
    private async Task JoinRoom(string lobbyId)
    {
        try
        {
            Debug.Log($"[JoinRoomById] 방 참가 처리 시작: Lobby ID = {lobbyId}");

            // 최신 Lobby 정보 가져오기
            currentLobby = await LobbyService.Instance.GetLobbyAsync(lobbyId);

            // RelayJoinCode 확인 후 참가
            if (!currentLobby.Data.ContainsKey("RelayJoinCode") || string.IsNullOrEmpty(currentLobby.Data["RelayJoinCode"].Value))
            {
                Debug.LogError("[JoinRoomById] RelayJoinCode가 존재하지 않습니다!");
                return;
            }

            string relayJoinCode = currentLobby.Data["RelayJoinCode"].Value;
            Debug.Log($"[JoinRoomById] RelayJoinCode 확인 완료: {relayJoinCode}");

            bool relaySuccess = await RelayManager.Instance.JoinRelay(relayJoinCode);
            if (!relaySuccess)
            {
                Debug.LogError("[JoinRoomById] Relay 참가 실패!");
                return;
            }

            Debug.Log($"[JoinRoomById] Relay 참가 성공!");

            // UI 상태 갱신
            SetUIState(true);
            SetCurrentRoom(currentLobby.Id);
            UpdateRoomUI(currentLobby);

            // 로비 이벤트 구독
            await SubscribeToLobbyEvents(currentLobby.Id);

            Debug.Log($"[JoinRoomById] 최종 방 참가 완료: {currentLobby.Id}");
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"[JoinRoomById] 처리 중 오류 발생: {e.Message}");
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

            SetUIState(false);
            codeButton.GetComponentInChildren<TMP_Text>().text = "Code";

            NetworkManager.Singleton.Shutdown();
            currentRoom = null;
            await SubscribeToLobbyEvents(currentLobby.Id);
        }
        catch (Exception e)
        {
            Debug.LogError($"방 나가기 실패: {e.Message}");
        }
    }

    private async Task SubscribeToLobbyEvents(string lobbyId)
    {
        var callbacks = new LobbyEventCallbacks();
        callbacks.LobbyChanged += OnLobbyChanged;
        //callbacks.LobbyEventConnectionStateChanged += OnLobbyEventConnectionStateChanged;
        await LobbyService.Instance.SubscribeToLobbyEventsAsync(lobbyId, callbacks);

        Debug.Log($"[RoomManager] 로비 이벤트 구독 완료: {lobbyId}");
    }
    private async void OnLobbyChanged(ILobbyChanges changes)
    {
        Debug.Log("[RoomManager] 로비 변경 감지!");

        // 로비 삭제 확인
        if (changes.LobbyDeleted)
        {
            Debug.Log("[RoomManager] 로비가 삭제됨! UI에서 제거해야 함.");
            return;
        }

        // 플레이어 입장 감지 → UI 갱신 필요
        if (changes.PlayerJoined.Changed)
        {
            foreach (var player in changes.PlayerJoined.Value)
            {
                Debug.Log($"[RoomManager] 플레이어 입장 감지: {player.Player.Id}");
                playerJoined = true;
                await RefreshRoomList();
            }
        }
        // 새로운 플레이어가 입장하면 Start 버튼 비활성화
        if (playerJoined && NetworkManager.Singleton.IsHost)
        {
            Debug.Log("[RoomManager] 새로운 플레이어가 입장하여 Start 버튼 비활성화");
            playerJoined = false;
            startButton.interactable = playerJoined;
            
        }

        // 플레이어 퇴장 감지 → UI 갱신 필요
        if (changes.PlayerLeft.Changed)
        {
            foreach (var playerId in changes.PlayerLeft.Value)
            {
                Debug.Log($"[RoomManager] 플레이어 퇴장 감지: {playerId}");
                await RefreshRoomList();
            }
        }
        // 플레이어 데이터 변경 감지 (캐릭터 정보/Ready 등)
        if (changes.PlayerData.Changed)
        {
            Debug.Log("Ready 이벤트 시작");
            foreach (var playerData in changes.PlayerData.Value)
            {
                int playerId = playerData.Key;
                var playerChanges = playerData.Value;

                if (playerChanges.ChangedData.Changed)
                {
                    var newData = playerChanges.ChangedData.Value;
                    if (newData.ContainsKey("Ready"))
                    {
                        var readyStatus = newData["Ready"];
                        if (readyStatus.Changed)
                        {
                            Debug.Log($"[RoomManager]  플레이어 {playerId} Ready 상태 변경: {readyStatus.Value}");
                        }
                    }
                }
                
            }
            CheckAllPlayersReady();

            Debug.Log("Ready 이벤트 끝");

            // 최신 로비 정보 적용
            if (currentLobby != null)
            {
                changes.ApplyToLobby(currentLobby);
                Debug.Log("[RoomManager] 로비 변경 사항이 적용됨.");
            }
            else
            {
                Debug.LogWarning("[RoomManager] 현재 로비 정보가 존재하지 않음.");
            }
        }
    }
    // 네트워크 연결 상태 변경 감지
    //private void OnLobbyEventConnectionStateChanged(LobbyEventConnectionState state)
    //{
    //    Debug.Log($"[RoomManager] 로비 이벤트 연결 상태 변경됨: {state}");

    //    if (state == LobbyEventConnectionState.Connected)
    //    {
    //        Debug.Log("[RoomManager] 로비 이벤트 시스템이 정상적으로 연결되었습니다.");
    //    }
    //    else if (state == LobbyEventConnectionState.Disconnected)
    //    {
    //        Debug.LogWarning("[RoomManager] 로비 이벤트 시스템이 끊어졌습니다. 재구독을 시도합니다...");
    //        if (currentLobby != null)
    //        {
    //            SubscribeToLobbyEvents(currentLobby.Id);
    //        }
    //    }
    //}

    public void SendMessageToServer(string message)
    {
        Debug.Log($"[Relay] 서버로 메시지 전송: {message}");

        using (FastBufferWriter writer = new FastBufferWriter(1024, Allocator.Temp))
        {
            writer.WriteValueSafe((ushort)message.Length);
            writer.WriteBytesSafe(System.Text.Encoding.UTF8.GetBytes(message));

            Debug.Log($"[Relay] 서버 ID: {NetworkManager.ServerClientId}");
            NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage("OnRelayMessage", NetworkManager.ServerClientId, writer);
        }
    }

    public void OnRoomItemClicked(RoomItem clickedRoom)
    {
        if (clickedRoom == null)
        {
            Debug.LogError("[RoomManager] 클릭된 RoomItem이 null입니다!");
            return;
        }

        string selectedRoomId = clickedRoom.GetRoomId();
        Debug.Log($"[RoomManager] 선택된 방 ID: {selectedRoomId}");

        // 현재 선택된 방 ID 업데이트
        SetCurrentRoom(selectedRoomId);
    }
    public async void OnReadyButtonClicked()
    {
        if (currentLobby == null) return;

        try
        {
            Debug.Log("[OnReadyButtonClicked] Ready 버튼 클릭!");

            isReady = !isReady;

            UpdatePlayerOptions options = new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject>
                {
                    { "Ready", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, isReady ? "true" : "false") }
                }
            };

            await LobbyService.Instance.UpdatePlayerAsync(currentLobby.Id, AuthenticationService.Instance.PlayerId, options);

            Debug.Log($"[OnReadyButtonClicked] Ready 상태 업데이트 완료! 현재 상태: {(isReady ? "Ready" : "Not Ready")}");

            // Ready 버튼 텍스트 변경
            readyButton.GetComponentInChildren<TMP_Text>().text = isReady ? "On Ready" : "Not Ready";

            CheckAllPlayersReady();
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"[OnReadyButtonClicked] Ready 상태 업데이트 실패: {e.Message}");
        }
    }


    [ClientRpc]
    private void NotifyPlayersReadyClientRpc(string playerId)
    {
        if (!IsServer) // 서버는 이미 상태를 알고 있으므로 클라이언트만 업데이트
        {
            Debug.Log($"[Ready] 플레이어 {playerId}가 Ready 상태가 되었습니다.");

            if (PlayerListManager.Instance == null)
            {
                Debug.LogError("[Ready] PlayerListManager 인스턴스가 존재하지 않습니다!");
                return;
            }
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

    private async void CheckAllPlayersReady()
    {
        if (!NetworkManager.Singleton.IsHost) return; // 호스트만 실행

        Debug.Log("[RoomManager] 모든 플레이어의 Ready 상태 확인 시작...");

        bool allReady = true;

        // 최신 Lobby 정보 가져오기 (Ready 상태 업데이트 반영)
        try
        {
            currentLobby = await LobbyService.Instance.GetLobbyAsync(currentLobby.Id);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"[RoomManager] 최신 Lobby 정보 가져오기 실패: {e.Message}");
            return;
        }

        foreach (var player in currentLobby.Players)
        {
            Debug.Log($"[RoomManager] 체크 중 - 플레이어 {player.Id}");

            // 호스트는 Ready 체크에서 제외
            if (player.Id == AuthenticationService.Instance.PlayerId)
            {
                Debug.Log($"[RoomManager] {player.Id} (호스트) - Ready 체크 제외");
                continue;
            }

            // player.Data가 null인지 확인 후 처리
            if (player.Data == null)
            {
                Debug.Log($"[RoomManager] {player.Id} - player.Data가 null! (동기화 지연 가능)");
                allReady = false;
                break;
            }

            if (!player.Data.ContainsKey("Ready"))
            {
                Debug.Log($"[RoomManager] {player.Id} - Ready 데이터 없음");
                allReady = false;
                break;
            }

            if (player.Data["Ready"].Value != "true") // Ready가 아닌 경우 즉시 종료
            {
                Debug.Log($"[RoomManager] {player.Id} - Ready 상태 아님: {player.Data["Ready"].Value}");
                allReady = false;
                break;
            }

            Debug.Log($"[RoomManager] {player.Id} - Ready 상태 확인 완료");
        }

        startButton.interactable = allReady;
        Debug.Log($"[RoomManager] Start 버튼 상태 업데이트: {startButton.interactable}");
    }

    public void OnStartButtonClicked()
    {
        if (!NetworkManager.Singleton.IsHost) return;

        Debug.Log("[OnStartButtonClicked] 게임 시작!");

        // 씬 전환 (네트워크 동기화 필요)
        NetworkManager.Singleton.SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single);
    }


    public void RequestSceneChange()
    {
        if (!NetworkManager.Singleton.IsServer)  // 클라이언트만 요청 가능
        {
            Debug.Log("[클라이언트] 씬 전환 요청을 서버로 보냅니다.");
            RequestSceneChangeServerRpc();
        }
        else
        {
            Debug.Log("[서버] 이미 서버이므로 직접 씬을 전환합니다.");
            LoadNextSceneServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestSceneChangeServerRpc()
    {
        Debug.Log("[ServerRpc] 클라이언트로부터 씬 전환 요청을 받았습니다.");
        LoadNextSceneServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void LoadNextSceneServerRpc()
    {
        Debug.Log("[Ready] LoadNextSceneServerRpc() 실행됨");

        if (NetworkManager.Singleton.IsServer)
        {
            Debug.Log("[Ready] 서버에서 씬을 전환합니다.");

            Debug.Log($"[Server] 현재 NetworkManager 상태 - IsServer: {NetworkManager.Singleton.IsServer}, IsClient: {NetworkManager.Singleton.IsClient}");

            NetworkManager.Singleton.SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single);
        }
        else
        {
            Debug.LogError("[Ready] 서버가 아닌 클라이언트가 씬을 전환하려고 합니다. 잘못된 요청!");
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
    private void SetUIState(bool isInRoom)
    {
        roomNameInput.interactable = !isInRoom;
        joinCodeInput.interactable = !isInRoom;
        createRoomButton.interactable = !isInRoom;
        join_CodeRoomButton.interactable = !isInRoom;
        join_SelectRoomButton.interactable = !isInRoom;
        codeButton.interactable = isInRoom;
        leaveRoomButton.interactable = isInRoom;
        SetUIForRole(isInRoom);
    }

    private void SetUIForRole(bool isInRoom)
    {
        if (NetworkManager.Singleton.IsHost)
        {
            startButton.gameObject.SetActive(isInRoom);
            startButton.interactable = !isInRoom; // 기본적으로 비활성화
            readyButton.gameObject.SetActive(!isInRoom); // 호스트는 Ready 버튼 필요 없음
        }
        else
        {
            startButton.gameObject.SetActive(!isInRoom); // 클라이언트는 Start 버튼 필요 없음
            readyButton.gameObject.SetActive(isInRoom);
        }
    }


    [ServerRpc(RequireOwnership = false)]
    private void UpdateRoomPlayerCountServerRpc(string roomId, int playerCount)
    {
        Debug.Log($"UpdateRoomPlayerCount [ServerRpc] {roomId}의 인원 수 업데이트: {playerCount}/{maxPlayers}");
        UpdateRoomPlayerCountClientRpc(roomId, playerCount);
    }

    [ClientRpc]
    private void UpdateRoomPlayerCountClientRpc(string roomId, int playerCount)
    {
        Debug.Log($"UpdateRoomPlayerCount [ClientRpc] {roomId}의 인원 수 업데이트 완료 (클라이언트 {NetworkManager.Singleton.LocalClientId}): {playerCount}/{maxPlayers}");
        if (roomList.ContainsKey(roomId))
        {
            TMP_Text playerCountText = roomList[roomId].transform.Find("roomPlayers").GetComponent<TMP_Text>();
            playerCountText.text = $"{playerCount}/{maxPlayers}";
            Debug.Log($"[ClientRpc] {roomId}의 인원 수 업데이트: {playerCount}/{maxPlayers}");
        }
    }

    //// 서버에서 방 삭제 동기화
    //[ServerRpc(RequireOwnership = false)]
    //private void DestroyRoomServerRpc(string roomId)
    //{
    //    DestroyRoomClientRpc(roomId);
    //}

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
            currentLobby = await LobbyService.Instance.GetLobbyAsync(currentRoom);

            // PlayerListManager를 통해 기존 플레이어 삭제
            PlayerListManager.Instance.ClearPlayers();

            // 새로 갱신된 플레이어 정보 추가
            foreach (var player in currentLobby.Players)
            {
                string playerName = player.Data["Username"].Value;
                bool isHost = player.Id == currentLobby.HostId;

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
            if (currentLobby != null)
            {
                // 현재 방에 입장한 상태라면 해당 방 UI만 갱신
                Debug.Log($"[RoomManager] 방 내 UI 갱신: {currentLobby.Id}");

                // 최신 방 정보 가져오기
                currentLobby = await LobbyService.Instance.GetLobbyAsync(currentLobby.Id);

                // 해당 방의 플레이어 수만 업데이트
                TMP_Text playerCountText = roomList[currentLobby.Id].transform.Find("roomPlayers").GetComponent<TMP_Text>();
                playerCountText.text = $"{currentLobby.Players.Count}/{currentLobby.MaxPlayers}";
            }
            else
            {
                // 방에 입장하지 않은 상태에서는 전체 방 리스트 UI 갱신
                Debug.Log("[RoomManager] 전체 방 목록 갱신 시작");

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
                    if (!roomList.ContainsKey(lobby.Id))
                    {
                        // 새로운 방이면 RoomItem 프리팹 생성
                        GameObject newRoom = Instantiate(roomPrefab, roomListParent);
                        RoomItem roomItem = newRoom.GetComponent<RoomItem>();

                        // 방 정보 설정
                        roomItem.SetRoomInfo(lobby.Id, lobby.Name, lobby.Players.Count, lobby.MaxPlayers);

                        roomList.Add(lobby.Id, newRoom);
                    }
                    else
                    {
                        // 이미 존재하는 방이면 플레이어 수만 업데이트
                        TMP_Text playerCountText = roomList[lobby.Id].transform.Find("roomPlayers").GetComponent<TMP_Text>();
                        playerCountText.text = $"{lobby.Players.Count}/{lobby.MaxPlayers}";
                    }
                }
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"[RoomManager] 로비 목록 갱신 실패: {e.Message}");
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
            currentLobby = await LobbyService.Instance.GetLobbyAsync(currentRoom);
            if (currentLobby == null)
            {
                Debug.LogError("Lobby 정보를 가져올 수 없습니다.");
                return;
            }

            // 현재 플레이어 ID 목록 생성
            HashSet<string> currentPlayerIDs = new HashSet<string>();
            foreach (var player in currentLobby.Players)
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
    //private void DestroyRoom(string roomId)
    //{
    //    if (roomList.ContainsKey(roomId))
    //    {
    //        Destroy(roomList[roomId]);
    //        roomList.Remove(roomId);
    //        Debug.Log($"Room {roomId} has been deleted.");
    //    }

    //    RefreshRoomList();
    //}

    //private async Task<bool> WaitForRelayJoinCode(string lobbyId)
    //{
    //    int maxAttempts = 5;
    //    for (int attempt = 0; attempt < maxAttempts; attempt++)
    //    {
    //        try
    //        {
    //            currentLobby = await LobbyService.Instance.GetLobbyAsync(lobbyId);
    //            if (currentLobby.Data.ContainsKey("RelayJoinCode") && !string.IsNullOrEmpty(currentLobby.Data["RelayJoinCode"].Value))
    //            {
    //                Debug.Log($" RelayJoinCode 확인 완료: {currentLobby.Data["RelayJoinCode"].Value}");
    //                return true;
    //            }
    //        }
    //        catch (LobbyServiceException e)
    //        {
    //            Debug.LogWarning($"RelayJoinCode 조회 실패: {e.Message}");
    //        }

    //        Debug.Log($"RelayJoinCode가 아직 설정되지 않음. ({attempt + 1}/{maxAttempts}) 재시도 중...");
    //        await Task.Delay(2000); // 2초 대기 후 다시 시도
    //    }

    //    Debug.LogError(" 최대 재시도 후에도 RelayJoinCode를 가져오지 못함.");
    //    return false;
    //}
}
