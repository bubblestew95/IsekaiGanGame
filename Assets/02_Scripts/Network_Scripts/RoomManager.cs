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
using Unity.Services.Matchmaker.Models;
using UnityEditor.PackageManager;
using Unity.Collections;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;

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
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("[RoomManager] NetworkManager가 존재하지 않습니다!");
        }

        Debug.Log($"[RoomManager] 현재 NetworkManager 상태 - IsServer: {NetworkManager.Singleton.IsServer}, IsClient: {NetworkManager.Singleton.IsClient}, IsHost: {NetworkManager.Singleton.IsHost}");

        // 최신 Unity 6에서 Relay 활성화 여부 확인
        //UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        //if (transport == null)
        //{
        //    Debug.LogError("[RoomManager] UnityTransport가 존재하지 않습니다!");
        //    return;
        //}

        //// Relay 서버 데이터 확인
        //RelayServerData relayData;
        //if (!transport.TryGetRelayServerData(out relayData))
        //{
        //    Debug.LogError("[RoomManager] Relay 서버 데이터가 설정되지 않았습니다! 서버 시작을 중단합니다.");
        //    return;
        //}

        Debug.Log("[RoomManager] Relay 설정 확인 완료! StartHost() 실행 가능");


        //while (NetworkManager.Singleton == null)
        //{
        //    Debug.LogWarning("RoomManager: NetworkManager.Singleton이 아직 초기화되지 않음, 대기 중...");
        //    await Task.Delay(100);
        //}

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

        // OnServerStarted 이벤트 등록
        //NetworkManager.Singleton.OnServerStarted += () =>
        //{
        //    if (NetworkManager.Singleton == null || NetworkManager.Singleton.CustomMessagingManager == null)
        //    {
        //        Debug.LogError("RoomManager: NetworkManager 또는 CustomMessagingManager가 초기화되지 않았습니다!");
        //        return;
        //    }

        //    Debug.Log("RoomManager: CustomMessagingManager 초기화 완료");
        //    NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("OnRelayMessage", OnRelayMessageReceived);
        //};

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
                    },
                    
                }
            );
            options.Data = new Dictionary<string, DataObject>
            {
                { "RelayJoinCode", new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode) }
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(roomName, maxPlayers, options);

            UpdateLobbyOptions updateLobbyOptions = new UpdateLobbyOptions();
            updateLobbyOptions.Data = new Dictionary<string, DataObject>
            {
                { "LobbyCode", new DataObject(DataObject.VisibilityOptions.Member, lobby.LobbyCode) }
            };

            // 방 데이터 업데이트
            lobby = await LobbyService.Instance.UpdateLobbyAsync(lobby.Id, updateLobbyOptions);

            // NetworkManager 시작
            NetworkManager.Singleton.StartHost();

            // 방 코드 버튼 활성화 및 코드 표시
            SetCurrentRoom(lobby.Id);

            //UpdateRoomUI(lobby); Debug.Log($"[CreateRoom] 방 생성 성공! ID: {lobby.Id}");
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
            join_SelectRoomButton.interactable = false;
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

            // RelayJoinCode 확인 후 참가
            if (!lobby.Data.ContainsKey("RelayJoinCode") || string.IsNullOrEmpty(lobby.Data["RelayJoinCode"].Value))
            {
                Debug.LogError("RelayJoinCode가 존재하지 않습니다!");
                return;
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


            ////  참가한 클라이언트의 데이터를 `UpdatePlayerAsync()`를 사용하여 추가
            //await LobbyService.Instance.UpdatePlayerAsync(lobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions
            //{
            //    Data = new Dictionary<string, PlayerDataObject>
            //    {
            //        {
            //            "Username", new PlayerDataObject(
            //                visibility: PlayerDataObject.VisibilityOptions.Member,
            //                value: username)
            //        }
            //    }
            //});

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

            //SendMessageToServer($"UpdateRoom|{lobby.Id}|{lobby.Players.Count}");

            readyButton.gameObject.SetActive(true);
            roomNameInput.interactable = false;
            joinCodeInput.interactable = false;
            createRoomButton.interactable = false;
            join_CodeRoomButton.interactable = false;
            join_SelectRoomButton.interactable = false;
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

            RequestRoomRefresh();
            UpdateRoomUI(lobby);

            Debug.Log($"방 참가 성공: {lobby.Id}");
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

            // 선택된 Room ID를 사용하여 입장
            Lobby lobby = await LobbyService.Instance.JoinLobbyByIdAsync(currentRoom);

            string relayJoinCode = lobby.Data["RelayJoinCode"].Value;

            if (string.IsNullOrEmpty(relayJoinCode))
            {
                Debug.LogError("[JoinSelectedRoom] RelayJoinCode 값이 비어 있습니다!");
                return;
            }

            Debug.Log($"RelayJoinCode 확인 완료: {relayJoinCode}");

            bool relaySuccess = await RelayManager.Instance.JoinRelay(relayJoinCode);
            if (!relaySuccess)
            {
                Debug.LogError("Relay 참가 실패!");
                return;
            }

            Debug.Log($"[JoinSelectedRoom] {lobby.Id}로 입장 성공!");

            readyButton.gameObject.SetActive(true);
            roomNameInput.interactable = false;
            joinCodeInput.interactable = false;
            createRoomButton.interactable = false;
            join_CodeRoomButton.interactable = false;
            join_SelectRoomButton.interactable = false;
            codeButton.interactable = true;
            leaveRoomButton.interactable = true;

            RequestRoomRefresh();
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"[JoinSelectedRoom] 방 참가 실패: {e.Message}");
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
            join_SelectRoomButton.interactable = true;
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
            if (string.IsNullOrEmpty(playerId))
            {
                Debug.LogError("[Ready] Player ID를 찾을 수 없습니다!");
                return;
            }

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

            Debug.Log("NotifyPlayersReady 실행완");
            // Ready 상태 확인 후 씬 전환
            await CheckAllPlayersReady();
            Debug.Log("CheckAllPlayersReady 실행완");
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



    public async Task CheckAllPlayersReady()
    {
        try
        {
            //  최신 Lobby 데이터 가져오기
            Lobby lobby = await LobbyService.Instance.GetLobbyAsync(currentRoom);

            if (lobby == null)
            {
                Debug.LogError("[Ready] Lobby 정보를 가져올 수 없습니다.");
                return;
            }

            int totalPlayers = lobby.Players.Count;
            int readyCount = 0;

            foreach (var player in lobby.Players)
            {
                if (player.Data != null)
                {
                    foreach (var dataKey in player.Data.Keys)
                    {
                        Debug.Log($"[Ready] 플레이어 {player.Id}의 데이터 - Key: {dataKey}, Value: {player.Data[dataKey].Value}");
                    }

                    if (player.Data.ContainsKey("Ready") && player.Data["Ready"].Value == "True")
                    {
                        readyCount++;
                    }
                }
                else
                {
                    Debug.LogWarning($"[Ready] 플레이어 {player.Id}의 데이터가 null입니다!");
                }
            }

            Debug.Log($"[Ready] 현재 Ready한 플레이어 수: {readyCount}/{totalPlayers}");

            //  모든 입장한 플레이어가 Ready 상태이면 씬 전환
            if (readyCount == totalPlayers && totalPlayers > 1)
            {
                Debug.Log("[Ready] 모든 플레이어가 Ready 상태입니다! 다음 씬으로 이동!");

                RequestSceneChange();
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"[Ready] Ready 상태 확인 실패: {e.Message}");
        }
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

    public void RequestRoomRefresh()
    {
        if (!NetworkManager.Singleton.IsServer)  // 클라이언트만 요청 가능
        {
            Debug.Log("[클라이언트] 방 새로고침");
            NotifyAllClientsToRefreshRoomListServerRpc();
        }
        else
        {
            Debug.Log("[서버] 방 새로고침 필요 없음(이미함)");
            //RefreshRoomListClientRpc();
        }
    }

    [ServerRpc]
    public void NotifyAllClientsToRefreshRoomListServerRpc()
    {
        RefreshRoomListClientRpc();
    }

    // 모든 클라이언트가 실행할 RefreshRoomList RPC
    [ClientRpc]
    private void RefreshRoomListClientRpc()
    {
        Debug.Log("[Client] 모든 클라이언트에서 RefreshRoomList 실행!");
        RefreshRoomList();
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
        join_CodeRoomButton.interactable = true;
        join_SelectRoomButton.interactable = true;
        leaveRoomButton.interactable = false;
        codeButton.interactable = false;
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
            if (playerCountText.text == $"{maxPlayers}/{maxPlayers}")
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
                if (!roomList.ContainsKey(lobby.Id))
                {
                    // RoomItem 프리팹 생성
                    GameObject newRoom = Instantiate(roomPrefab, roomListParent);
                    RoomItem roomItem = newRoom.GetComponent<RoomItem>();

                    // SetRoomInfo 호출 및 디버그 로그 확인
                    roomItem.SetRoomInfo(lobby.Id, lobby.Name, lobby.Players.Count, lobby.MaxPlayers);

                    roomList.Add(lobby.Id, newRoom);
                }
                else
                {
                    // 이미 존재하는 방의 플레이어 수 갱신
                    TMP_Text playerCountText = roomList[lobby.Id].transform.Find("roomPlayers").GetComponent<TMP_Text>();
                    playerCountText.text = $"{lobby.Players.Count}/{lobby.MaxPlayers}";
                }
            }

            //  현재 호스트가 속한 방을 따로 갱신
            //if (!string.IsNullOrEmpty(currentRoom))
            //{
            //    Lobby myLobby = await LobbyService.Instance.GetLobbyAsync(currentRoom);
            //    if (myLobby != null)
            //    {
            //        if (roomList.ContainsKey(currentRoom))
            //        {
            //            TMP_Text playerCountText = roomList[currentRoom].transform.Find("roomPlayers").GetComponent<TMP_Text>();
            //            playerCountText.text = $"{myLobby.Players.Count}/4";
            //        }
            //    }
            //}
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
