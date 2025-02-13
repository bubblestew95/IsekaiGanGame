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
using System.Linq;

public class RoomManager : NetworkBehaviour
{
    public static RoomManager Instance { get; private set; }

    public GameObject roomPrefab; // Roomstate ������
    public Transform roomListParent; // �� ����� ǥ���� �θ� ������Ʈ
    public TMP_InputField roomNameInput; // �� �̸� �Է� �ʵ�
    public TMP_InputField joinCodeInput; // �� �ڵ� �Է� �ʵ�
    public Button createRoomButton; // �� ���� ��ư
    public Button join_SelectRoomButton; // Select Join ��ư
    public Button join_CodeRoomButton; // Code Join ��ư
    public Button leaveRoomButton; // Join ��ư
    public Button codeButton; // Code ���� ��ư
    public Button readyButton; //ready ��ư
    public Button startButton; //start ��ư
    public RectTransform contentRect; // Scroll View�� Content ũ�� ����
    public string gameSceneName = "GameTest"; // ���� �� �̸�
    public GameObject JobSelect_Panel; //ĳ���� ����â

    private Dictionary<string, GameObject> roomList = new Dictionary<string, GameObject>(); // �� ��� ����
    private float roomSpacing = 40f;
    public string currentRoom { get; private set; }
    private int maxPlayers = 4;

    private string username;
    private bool isInitialized = false;
    private Lobby currentLobby;
    private bool playerJoined = false; // �÷��̾ ���������� ���¸� ��Ÿ�� ����
    private bool isReady = false; //Ready ���¸� �����ϴ� ����

    private HashSet<string> previousPlayerIDs = new HashSet<string>(); // ���� �÷��̾� ID ����

    private void Awake()
    {
        if(IsHost)
        {
            NetworkManager.Singleton.Shutdown();
        }

        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    // �� ID ���� �޼���
    public void SetCurrentRoom(string roomId)
    {
        currentRoom = roomId;
        Debug.Log($"���� �� ����: {currentRoom}");

    }
    async void Start()
    {
        Debug.Log("RoomManager Start() ���۵�");

        if (RelayManager.Instance == null)
        {
            Debug.LogError("RoomManager: RelayManager �ν��Ͻ��� �������� �ʽ��ϴ�!");
            return;
        }
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("[RoomManager] NetworkManager�� �������� �ʽ��ϴ�!");
        }

        Debug.Log($"[RoomManager] ���� NetworkManager ���� - IsServer: {NetworkManager.Singleton.IsServer}, IsClient: {NetworkManager.Singleton.IsClient}, IsHost: {NetworkManager.Singleton.IsHost}");

        Debug.Log("[RoomManager] Relay ���� Ȯ�� �Ϸ�! StartHost() ���� ����");


        // ������ �̹� ���� ���̶�� ��� �ڵ鷯 ���
        if (NetworkManager.Singleton.IsServer)
        {
            Debug.Log("[RoomManager] ���� ���� ���� - �̹� ���� �� (�ڵ鷯 ��� ���)");
            RegisterRelayMessageHandler();
        }
        else
        {
            // ���� ���� �� ����ǵ��� �̺�Ʈ ���
            Debug.Log("[RoomManager] ���� ������ �ƴ�. OnServerStarted �̺�Ʈ ��� ���");
            NetworkManager.Singleton.OnServerStarted += () =>
            {
                Debug.Log("[RoomManager] OnServerStarted �̺�Ʈ �����!");
                RegisterRelayMessageHandler();
            };
        }

        await InitializeServices(); // Unity Services �ʱ�ȭ
        // PlayerPrefs���� username �ҷ�����
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
            // ���⼭ username�� ����Ͽ� ���ϴ� �۾� ����
        }


        codeButton.interactable = false;
        leaveRoomButton.interactable = false;
        JobSelect_Panel.SetActive(false);

    }

    void RegisterRelayMessageHandler()
    {
        if (NetworkManager.Singleton.CustomMessagingManager == null)
        {
            Debug.LogError("[RoomManager] CustomMessagingManager�� �������� �ʽ��ϴ�!");
            return;
        }

        Debug.Log("[RoomManager] CustomMessagingManager �ʱ�ȭ �Ϸ� - �޽��� �ڵ鷯 ��� ��...");
        NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("OnRelayMessage", OnRelayMessageReceived);
    }

    // Relay���� �޽����� �����ϰ� RoomList ����
    private void OnRelayMessageReceived(ulong clientId, FastBufferReader reader)
    {
        Debug.Log($"[Relay �޽��� ���� - ����] �޽��� ���� ���� (clientId: {clientId})");

        ushort messageLength;
        reader.ReadValueSafe(out messageLength); // ���ڿ� ���� ���� �б�

        byte[] messageBytes = new byte[messageLength];
        reader.ReadBytesSafe(ref messageBytes, messageLength); // ���ڿ� �����͸� �б�

        string message = System.Text.Encoding.UTF8.GetString(messageBytes); // UTF8�� ��ȯ

        Debug.Log($"[Relay �޽��� ���� - ����] Ŭ���̾�Ʈ({clientId})�κ��� �޽��� ����: {message}");

        string[] messageParts = message.Split('|');
        if (messageParts.Length < 3) return;

        string command = messageParts[0];
        string roomId = messageParts[1];
        int playerCount = int.Parse(messageParts[2]);

        if (command == "UpdateRoom")
        {
            Debug.Log($"[Relay �޽��� ó��] {roomId}�� �ο� �� ������Ʈ: {playerCount}/{maxPlayers}");
            UpdateRoomPlayerCountServerRpc(roomId, playerCount);
        }
    }

    public async void CreateRoom()
    {

        if (!isInitialized)
        {
            Debug.LogError("Unity Services�� �ʱ�ȭ���� �ʾҽ��ϴ�!");
            return;
        }

        string roomName = roomNameInput.text.Trim();
        if (string.IsNullOrEmpty(roomName))
        {
            Debug.LogWarning("�� �̸��� �Է��ϼ���.");
            return;
        }

        try
        {
            // Relay ����
            string relayJoinCode = await RelayManager.Instance.CreateRelay(maxPlayers);
            if (string.IsNullOrEmpty(relayJoinCode))
            {
                Debug.LogError("Relay ���� ����");
                return;
            }
            Debug.Log($"Relay ���� �Ϸ�: {relayJoinCode}");

            // �ùٸ��� ������ CreateLobbyOptions
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

            // �� ������ ������Ʈ
            currentLobby = await LobbyService.Instance.UpdateLobbyAsync(currentLobby.Id, updateLobbyOptions);

            SetCurrentRoom(currentLobby.Id);

            // NetworkManager ����
            NetworkManager.Singleton.StartHost();

            Debug.Log($"[CreateRoom] �� ������ Ȯ��: RelayJoinCode = {currentLobby.Data["RelayJoinCode"].Value}");

            // �÷��̾� ������ Ȯ�� (����)
            foreach (var player in currentLobby.Players)
            {
                string playerUsername = "�� �� ����";

                //if (player.Data != null && player.Data.ContainsKey("Username"))
                //{
                //    playerUsername = player.Data["Username"].Value;
                //}                
                if (player.Data != null && player.Data.ContainsKey("Username"))
                {
                    playerUsername = player.Data["Username"].Value;
                }
                Debug.Log($"[CreateRoom] �÷��̾� Ȯ��: Player ID = {player.Id}, Username = {playerUsername}");
            }

            SetUIState(true);
            UpdateRoomUI(currentLobby);
            codeButton.GetComponentInChildren<TMP_Text>().text = currentLobby.LobbyCode;

            // �� �����ڴ� �ڵ����� Host�� ��
            if (NetworkManager.Singleton.IsHost)
            {
                Debug.Log("[CreateRoom] ���� �÷��̾�� HOST �Դϴ�.");
            }
            else
            {
                Debug.Log("[CreateRoom] ���� �÷��̾�� Ŭ���̾�Ʈ �Դϴ�.");
            }

            await SubscribeToLobbyEvents(currentLobby.Id);
            //PlayerListManager.Instance.AddPlayer(AuthenticationService.Instance.PlayerId, username, PlayerStatus.Host);
            PlayerListManager.Instance.AddPlayer(AuthenticationService.Instance.PlayerId, username, PlayerStatus.Host);
            await RefreshRoomList();

        }
        catch (Exception e)
        {
            Debug.LogError($"�� ���� ����: {e.Message}");
        }
    }

    public async void JoinCodeRoom()
    {
        if (!isInitialized)
        {
            Debug.LogError("Unity Services�� �ʱ�ȭ���� �ʾҽ��ϴ�!");
            return;
        }

        string roomCode = joinCodeInput.text.Trim();
        if (string.IsNullOrEmpty(roomCode))
        {
            Debug.LogWarning("��ȿ�� Room Code�� �Է��ϼ���.");
            return;
        }

        try
        {
            Debug.Log($"[JoinRoom] �� ���� ��û: Room Code = {roomCode}");

            // �ڵ�� �� ã��
            currentLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(roomCode);

            if (currentLobby == null)
            {
                Debug.LogError("[JoinRoom] Lobby�� ã�� �� �����ϴ�!");
                return;
            }

            Debug.Log($"[JoinRoom] �� ���� ����! Lobby ID: {currentLobby.Id}");

            // ���� ó�� �޼��� ȣ�� (Relay ���� �� UI ����)
            await JoinRoom(currentLobby.Id);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"�� ���� ����: {e.Message}");
        }
    }

    public async void JoinSelectedRoom()
    {
        if (string.IsNullOrEmpty(currentRoom))
        {
            Debug.LogError("���õ� ���� �����ϴ�!");
            return;
        }

        try
        {
            Debug.Log($"[JoinSelectedRoom] �� ���� ��û: {currentRoom}");

            // ���õ� Room ID�� ����
            currentLobby = await LobbyService.Instance.JoinLobbyByIdAsync(currentRoom);

            if (currentLobby == null)
            {
                Debug.LogError("[JoinSelectedRoom] Lobby�� ã�� �� �����ϴ�!");
                return;
            }

            Debug.Log($"[JoinSelectedRoom] �� ���� ����! Lobby ID: {currentLobby.Id}");

            // ���� ó�� �޼��� ȣ�� (Relay ���� �� UI ����)
            await JoinRoom(currentLobby.Id);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"[JoinSelectedRoom] �� ���� ����: {e.Message}");
        }
    }

    // �ߺ��� ������ ó���ϴ� �޼��� (���� �Լ�)
    private async Task JoinRoom(string lobbyId)
    {
        try
        {
            Debug.Log($"[JoinRoomById] �� ���� ó�� ����: Lobby ID = {lobbyId}");

            // RelayJoinCode Ȯ�� �� ����
            if (!currentLobby.Data.ContainsKey("RelayJoinCode") || string.IsNullOrEmpty(currentLobby.Data["RelayJoinCode"].Value))
            {
                Debug.LogError("[JoinRoomById] RelayJoinCode�� �������� �ʽ��ϴ�!");
                return;
            }

            string relayJoinCode = currentLobby.Data["RelayJoinCode"].Value;
            Debug.Log($"[JoinRoomById] RelayJoinCode Ȯ�� �Ϸ�: {relayJoinCode}");

            bool relaySuccess = await RelayManager.Instance.JoinRelay(relayJoinCode);
            if (!relaySuccess)
            {
                Debug.LogError("[JoinRoomById] Relay ���� ����!");
                return;
            }

            Debug.Log($"[JoinRoomById] Relay ���� ����!");

            UpdatePlayerOptions options = new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject>
                {
                    { "Username", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, username) },
                    { "Ready", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "false") }
                }
            };
            await LobbyService.Instance.UpdatePlayerAsync(currentLobby.Id, AuthenticationService.Instance.PlayerId, options);

            // �߰�: ��� �÷��̾� ���� ��������
            await UpdatePlayerListUI();

            // UI ���� ����
            SetUIState(true);
            SetCurrentRoom(currentLobby.Id);
            UpdateRoomUI(currentLobby);
            codeButton.GetComponentInChildren<TMP_Text>().text = currentLobby.LobbyCode;

            // �κ� �̺�Ʈ ����
            await SubscribeToLobbyEvents(currentLobby.Id);
            // �߰�: ������ PlayerList ���� ��û
            RequestPlayerListUpdateServerRpc();


            Debug.Log($"[JoinRoomById] ���� �� ���� �Ϸ�: {currentLobby.Id}");
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"[JoinRoomById] ó�� �� ���� �߻�: {e.Message}");
        }
    }

    // �� ����
    public async void LeaveRoom()
    {
        if (string.IsNullOrEmpty(currentRoom))
        {
            Debug.LogWarning("���� ������ ���� �����ϴ�.");
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
                Debug.Log("�濡�� ��������, ���ο� ������ ���� ����.");
            }

            SetUIState(false);
            codeButton.GetComponentInChildren<TMP_Text>().text = "Code";

            NetworkManager.Singleton.Shutdown();
            currentRoom = null;
            await SubscribeToLobbyEvents(currentLobby.Id);
        }
        catch (Exception e)
        {
            Debug.LogError($"�� ������ ����: {e.Message}");
        }
    }

    private async Task SubscribeToLobbyEvents(string lobbyId)
    {
        var callbacks = new LobbyEventCallbacks();
        callbacks.LobbyChanged += OnLobbyChanged;
        await LobbyService.Instance.SubscribeToLobbyEventsAsync(lobbyId, callbacks);

        Debug.Log($"[RoomManager] �κ� �̺�Ʈ ���� �Ϸ�: {lobbyId}");
    }
    private async void OnLobbyChanged(ILobbyChanges changes)
    {
        Debug.Log("[RoomManager] �κ� ���� ����!");

        // �κ� ���� ����
        if (changes.LobbyDeleted)
        {
            Debug.Log("[RoomManager] �κ� ������! UI���� �����ؾ� ��.");
            return;
        }

        // �ʹ� ���� ȣ������ �ʵ��� ����
        if (changes.PlayerJoined.Changed || changes.PlayerLeft.Changed || changes.PlayerData.Changed)
        {
            Debug.Log("[RoomManager] �κ� ������Ʈ �ʿ�. �ֽ� ���� ��û");
            currentLobby = await LobbyService.Instance.GetLobbyAsync(currentLobby.Id);
            await UpdatePlayerListUI();
        }

        // �÷��̾� ���� ���� (ȣ��Ʈ �� Ŭ���̾�Ʈ)
        if (changes.PlayerJoined.Changed)
        {
            Debug.Log($"[RoomManager] PlayerJoined �̺�Ʈ ������. �� {changes.PlayerJoined.Value.Count}���� �÷��̾� �߰���.");

            foreach (var player in changes.PlayerJoined.Value)
            {
                Debug.Log($"[RoomManager] ������ �÷��̾� ID: {player.Player.Id}");
            }

            await UpdatePlayerListUI(); // PlayerList UI ����
        }

        // �÷��̾� ���� ���� (ȣ��Ʈ �� Ŭ���̾�Ʈ)
        if (changes.PlayerLeft.Changed)
        {
            Debug.Log($"[RoomManager] PlayerLeft �̺�Ʈ ������. �� {changes.PlayerLeft.Value.Count}���� �÷��̾� ������.");

            foreach (var playerId in changes.PlayerLeft.Value)
            {
                Debug.Log($"[RoomManager] ������ �÷��̾� ID: {playerId}");
            }

            await UpdatePlayerListUI(); // PlayerList UI ����
        }

        // �� �ο� ��ȭ ���� (UI ����)
        if (changes.PlayerJoined.Changed || changes.PlayerLeft.Changed)
        {
            Debug.Log("[RoomManager] �� �ο� ���� ���� - Room UI ������Ʈ");
            UpdateRoomUI(currentLobby);
            UpdateRoomUICallClientRpc(currentLobby.Players.Count);
        }

        // �÷��̾� ���� ���� ���� (Ready ���� ��)
        if (changes.PlayerData.Changed)
        {
            Debug.Log("Ready �̺�Ʈ ����");
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
                            Debug.Log($"[RoomManager] �÷��̾� {playerId} Ready ���� ����: {readyStatus.Value}");
                        }
                    }
                }
            }
            CheckAllPlayersReady();
            await UpdatePlayerListUI();
            Debug.Log("Ready �̺�Ʈ ��");

            if (currentLobby != null)
            {
                changes.ApplyToLobby(currentLobby);
                Debug.Log("[RoomManager] �κ� ���� ������ �����.");
            }
            else
            {
                Debug.LogWarning("[RoomManager] ���� �κ� ������ �������� ����.");
            }
        }
    }

    // ��Ʈ��ũ ���� ���� ���� ����
    //private void OnLobbyEventConnectionStateChanged(LobbyEventConnectionState state)
    //{
    //    Debug.Log($"[RoomManager] �κ� �̺�Ʈ ���� ���� �����: {state}");

    //    if (state == LobbyEventConnectionState.Connected)
    //    {
    //        Debug.Log("[RoomManager] �κ� �̺�Ʈ �ý����� ���������� ����Ǿ����ϴ�.");
    //    }
    //    else if (state == LobbyEventConnectionState.Disconnected)
    //    {
    //        Debug.LogWarning("[RoomManager] �κ� �̺�Ʈ �ý����� ���������ϴ�. �籸���� �õ��մϴ�...");
    //        if (currentLobby != null)
    //        {
    //            SubscribeToLobbyEvents(currentLobby.Id);
    //        }
    //    }
    //}

    [ServerRpc(RequireOwnership = false)]
    public void RequestPlayerListUpdateServerRpc()
    {
        Debug.Log("[ServerRpc] ��� Ŭ���̾�Ʈ���� PlayerList ������Ʈ ��û");
        RefreshPlayerListClientRpc();
    }

    [ClientRpc]
    private void RefreshPlayerListClientRpc()
    {
        Debug.Log("[ClientRpc] PlayerList ������Ʈ ���� - ���ο� �÷��̾� ����ȭ ����");

        _ = UpdatePlayerListUI();

        Debug.Log("[ClientRpc] PlayerList ������Ʈ �Ϸ�");
    }



    public void SendMessageToServer(string message)
    {
        Debug.Log($"[Relay] ������ �޽��� ����: {message}");

        using (FastBufferWriter writer = new FastBufferWriter(1024, Allocator.Temp))
        {
            writer.WriteValueSafe((ushort)message.Length);
            writer.WriteBytesSafe(System.Text.Encoding.UTF8.GetBytes(message));

            Debug.Log($"[Relay] ���� ID: {NetworkManager.ServerClientId}");
            NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage("OnRelayMessage", NetworkManager.ServerClientId, writer);
        }
    }

    public void OnRoomItemClicked(RoomItem clickedRoom)
    {
        if (clickedRoom == null)
        {
            Debug.LogError("[RoomManager] Ŭ���� RoomItem�� null�Դϴ�!");
            return;
        }

        string selectedRoomId = clickedRoom.GetRoomId();
        Debug.Log($"[RoomManager] ���õ� �� ID: {selectedRoomId}");

        // ���� ���õ� �� ID ������Ʈ
        SetCurrentRoom(selectedRoomId);
    }
    public async void OnReadyButtonClicked()
    {
        if (currentLobby == null) return;

        try
        {
            Debug.Log("[OnReadyButtonClicked] Ready ��ư Ŭ��!");

            isReady = !isReady;

            UpdatePlayerOptions options = new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject>
                {
                    { "Ready", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, isReady ? "true" : "false") }
                }
            };

            await LobbyService.Instance.UpdatePlayerAsync(currentLobby.Id, AuthenticationService.Instance.PlayerId, options);

            Debug.Log($"[OnReadyButtonClicked] Ready ���� ������Ʈ �Ϸ�! ���� ����: {(isReady ? "Ready" : "Not Ready")}");

            // Ready ��ư �ؽ�Ʈ ����
            readyButton.GetComponentInChildren<TMP_Text>().text = isReady ? "On Ready" : "Not Ready";

            CheckAllPlayersReady();
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"[OnReadyButtonClicked] Ready ���� ������Ʈ ����: {e.Message}");
        }
    }

    [ClientRpc]
    private void NotifyPlayersReadyClientRpc(string playerId)
    {
        if (!IsServer) // ������ �̹� ���¸� �˰� �����Ƿ� Ŭ���̾�Ʈ�� ������Ʈ
        {
            Debug.Log($"[Ready] �÷��̾� {playerId}�� Ready ���°� �Ǿ����ϴ�.");

            if (PlayerListManager.Instance == null)
            {
                Debug.LogError("[Ready] PlayerListManager �ν��Ͻ��� �������� �ʽ��ϴ�!");
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
        if (!NetworkManager.Singleton.IsHost) return; // ȣ��Ʈ�� ����

        Debug.Log("[RoomManager] ��� �÷��̾��� Ready ���� Ȯ�� ����...");

        bool allReady = true;

        foreach (var player in currentLobby.Players)
        {
            Debug.Log($"[RoomManager] üũ �� - �÷��̾� {player.Id}");

            // ȣ��Ʈ�� Ready üũ���� ����
            if (player.Id == AuthenticationService.Instance.PlayerId)
            {
                Debug.Log($"[RoomManager] {player.Id} (ȣ��Ʈ) - Ready üũ ����");
                continue;
            }

            // player.Data�� null���� Ȯ�� �� ó��
            if (player.Data == null)
            {
                Debug.Log($"[RoomManager] {player.Id} - player.Data�� null! (����ȭ ���� ����)");
                allReady = false;
                break;
            }

            if (!player.Data.ContainsKey("Ready"))
            {
                Debug.Log($"[RoomManager] {player.Id} - Ready ������ ����");
                allReady = false;
                break;
            }

            if (player.Data["Ready"].Value != "true") // Ready�� �ƴ� ��� ��� ����
            {
                Debug.Log($"[RoomManager] {player.Id} - Ready ���� �ƴ�: {player.Data["Ready"].Value}");
                allReady = false;
                break;
            }

            Debug.Log($"[RoomManager] {player.Id} - Ready ���� Ȯ�� �Ϸ�");
        }

        startButton.interactable = allReady;
        Debug.Log($"[RoomManager] Start ��ư ���� ������Ʈ: {startButton.interactable}");
    }

    public void OnStartButtonClicked()
    {
        if (!NetworkManager.Singleton.IsHost) return;

        Debug.Log("[OnStartButtonClicked] ���� ����!");

        // �� ��ȯ (��Ʈ��ũ ����ȭ �ʿ�)
        NetworkManager.Singleton.SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single);
    }


    public void RequestSceneChange()
    {
        if (!NetworkManager.Singleton.IsServer)  // Ŭ���̾�Ʈ�� ��û ����
        {
            Debug.Log("[Ŭ���̾�Ʈ] �� ��ȯ ��û�� ������ �����ϴ�.");
            RequestSceneChangeServerRpc();
        }
        else
        {
            Debug.Log("[����] �̹� �����̹Ƿ� ���� ���� ��ȯ�մϴ�.");
            LoadNextSceneServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestSceneChangeServerRpc()
    {
        Debug.Log("[ServerRpc] Ŭ���̾�Ʈ�κ��� �� ��ȯ ��û�� �޾ҽ��ϴ�.");
        LoadNextSceneServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void LoadNextSceneServerRpc()
    {
        Debug.Log("[Ready] LoadNextSceneServerRpc() �����");

        if (NetworkManager.Singleton.IsServer)
        {
            Debug.Log("[Ready] �������� ���� ��ȯ�մϴ�.");

            Debug.Log($"[Server] ���� NetworkManager ���� - IsServer: {NetworkManager.Singleton.IsServer}, IsClient: {NetworkManager.Singleton.IsClient}");

            NetworkManager.Singleton.SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single);
        }
        else
        {
            Debug.LogError("[Ready] ������ �ƴ� Ŭ���̾�Ʈ�� ���� ��ȯ�Ϸ��� �մϴ�. �߸��� ��û!");
        }
    }

    // Room UI ������Ʈ
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

    [ClientRpc]
    private void UpdateRoomUICallClientRpc(int playerCount)
    {
        if (currentLobby != null && roomList.ContainsKey(currentLobby.Id))
        {
            TMP_Text playerCountText = roomList[currentLobby.Id].transform.Find("roomPlayers").GetComponent<TMP_Text>();
            playerCountText.text = $"{playerCount}/{currentLobby.MaxPlayers}";

            Debug.Log($"[ClientRpc] �� UI ���� �Ϸ�: {playerCount}/{maxPlayers}");
        }
    }



    // UI ����
    private void SetUIState(bool isInRoom)
    {
        roomNameInput.interactable = !isInRoom;
        joinCodeInput.interactable = !isInRoom;
        createRoomButton.interactable = !isInRoom;
        join_CodeRoomButton.interactable = !isInRoom;
        join_SelectRoomButton.interactable = !isInRoom;
        codeButton.interactable = isInRoom;
        leaveRoomButton.interactable = isInRoom;
        JobSelect_Panel.SetActive(isInRoom);
        SetUIForRole(isInRoom);
    }

    private void SetUIForRole(bool isInRoom)
    {
        if (NetworkManager.Singleton.IsHost)
        {
            startButton.gameObject.SetActive(isInRoom);
            startButton.interactable = !isInRoom; // �⺻������ ��Ȱ��ȭ
            readyButton.gameObject.SetActive(!isInRoom); // ȣ��Ʈ�� Ready ��ư �ʿ� ����
        }
        else
        {
            startButton.gameObject.SetActive(!isInRoom); // Ŭ���̾�Ʈ�� Start ��ư �ʿ� ����
            readyButton.gameObject.SetActive(isInRoom);
        }
    }


    [ServerRpc(RequireOwnership = false)]
    private void UpdateRoomPlayerCountServerRpc(string roomId, int playerCount)
    {
        Debug.Log($"UpdateRoomPlayerCount [ServerRpc] {roomId}�� �ο� �� ������Ʈ: {playerCount}/{maxPlayers}");
        UpdateRoomPlayerCountClientRpc(roomId, playerCount);
    }

    [ClientRpc]
    private void UpdateRoomPlayerCountClientRpc(string roomId, int playerCount)
    {
        Debug.Log($"UpdateRoomPlayerCount [ClientRpc] {roomId}�� �ο� �� ������Ʈ �Ϸ� (Ŭ���̾�Ʈ {NetworkManager.Singleton.LocalClientId}): {playerCount}/{maxPlayers}");
        if (roomList.ContainsKey(roomId))
        {
            TMP_Text playerCountText = roomList[roomId].transform.Find("roomPlayers").GetComponent<TMP_Text>();
            playerCountText.text = $"{playerCount}/{maxPlayers}";
            Debug.Log($"[ClientRpc] {roomId}�� �ο� �� ������Ʈ: {playerCount}/{maxPlayers}");
        }
    }

    private async Task UpdatePlayerListUI()
    {
        if (string.IsNullOrEmpty(currentRoom)) return;

        Debug.Log($"[PlayerListManager] �ֽ� �κ� ���� Ȯ�� - �� {currentLobby.Players.Count}�� ����");

        foreach (var player in currentLobby.Players)
        {
            string playerName = "�� �� ����";
            bool isHost = false;
            bool isReady = false;

            // �߰�: �÷��̾� �����Ͱ� ��� ������ 3ȸ ��õ�
            int retryCount = 0;
            while ((player.Data == null || !player.Data.ContainsKey("Username")) && retryCount < 3)
            {
                Debug.LogWarning($"[PlayerListManager] �÷��̾� �����Ͱ� ��� ����. ��õ� ({retryCount + 1}/3) ����: {player.Id}");
                await Task.Delay(500);
                retryCount++;
            }

            if (player.Data != null && player.Data.ContainsKey("Username"))
            {
                playerName = player.Data["Username"].Value;
                isHost = player.Id == currentLobby.HostId;
                isReady = player.Data.ContainsKey("Ready") && player.Data["Ready"].Value == "true";
            }
            else
            {
                Debug.LogError($"[PlayerListManager] �÷��̾� ������ �ε� ����: {player.Id}");
                continue;
            }

            PlayerStatus status = isHost ? PlayerStatus.Host : (isReady ? PlayerStatus.Ready : PlayerStatus.Not_Ready);

            if (!PlayerListManager.Instance.ContainsPlayer(player.Id))
            {
                Debug.Log($"[PlayerListManager] �� �÷��̾� �߰�: {playerName} / ����: {status}");
                PlayerListManager.Instance.AddPlayer(player.Id,playerName, status);
            }
            else
            {
                Debug.Log($"[PlayerListManager] ���� �÷��̾� ���� ������Ʈ: {playerName} -> {status}");
                PlayerListManager.Instance.UpdatePlayerStatus(player.Id, status);
            }
        }

        // �߰�: PlayerList ����
        PlayerListManager.Instance.SortPlayerList();
        Debug.Log("[PlayerListManager] PlayerList UI ���� �Ϸ�");
    }


    public void UpdatePlayerStatus(string playerId, PlayerStatus status)
    {
        PlayerListManager.Instance.UpdatePlayerStatus(playerId, status);
    }


    // �߰� ȣ�� �޼��� �߰�
    public void OnRefreshRoomListButton()
    {
        // RefreshRoomList ȣ��
        _ = RefreshRoomList();
    }

    public async Task RefreshRoomList()
    {
        try
        {
            if (currentLobby != null)
            {
                // ���� �濡 ������ ���¶�� �ش� �� UI�� ����
                Debug.Log($"[RoomManager] �� �� UI ����: {currentLobby.Id}");

                // �ش� ���� �÷��̾� ���� ������Ʈ
                TMP_Text playerCountText = roomList[currentLobby.Id].transform.Find("roomPlayers").GetComponent<TMP_Text>();
                playerCountText.text = $"{currentLobby.Players.Count}/{currentLobby.MaxPlayers}";
            }
            else
            {
                // �濡 �������� ���� ���¿����� ��ü �� ����Ʈ UI ����
                Debug.Log("[RoomManager] ��ü �� ��� ���� ����");

                QueryLobbiesOptions options = new QueryLobbiesOptions
                {
                    Filters = new List<QueryFilter>
                {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
                }
                };

                QueryResponse response = await LobbyService.Instance.QueryLobbiesAsync(options);
                // �� ���� (�ֽ� ������ ������)
                List<Lobby> sortedLobbies = response.Results.OrderByDescending(lobby => lobby.Created).ToList();

                foreach (var lobby in sortedLobbies)
                {
                    if (!roomList.ContainsKey(lobby.Id))
                    {
                        // ���ο� ���̸� RoomItem ������ ����
                        GameObject newRoom = Instantiate(roomPrefab, roomListParent);
                        RoomItem roomItem = newRoom.GetComponent<RoomItem>();

                        // �� ���� ����
                        roomItem.SetRoomInfo(lobby.Id, lobby.Name, lobby.Players.Count, lobby.MaxPlayers);

                        roomList.Add(lobby.Id, newRoom);
                    }
                    else
                    {
                        // �̹� �����ϴ� ���̸� �÷��̾� ���� ������Ʈ
                        TMP_Text playerCountText = roomList[lobby.Id].transform.Find("roomPlayers").GetComponent<TMP_Text>();
                        playerCountText.text = $"{lobby.Players.Count}/{lobby.MaxPlayers}";
                    }
                }
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"[RoomManager] �κ� ��� ���� ����: {e.Message}");
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
                Debug.Log($"AuthenticationService: �α��� �Ϸ� (Player ID: {AuthenticationService.Instance.PlayerId})");
            }
            isInitialized = true;
            Debug.Log("Unity Services �ʱ�ȭ �Ϸ� & Authentication ����");
        }
        catch (Exception e)
        {
            Debug.LogError($"Unity Services Initialization Failed: {e.Message}");
        }
    }
}
