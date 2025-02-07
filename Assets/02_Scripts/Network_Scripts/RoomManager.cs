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

    public GameObject roomPrefab; // Roomstate ������
    public Transform roomListParent; // �� ����� ǥ���� �θ� ������Ʈ
    public TMP_InputField roomNameInput; // �� �̸� �Է� �ʵ�
    public TMP_InputField joinCodeInput; // �� �ڵ� �Է� �ʵ�
    public Button createRoomButton; // �� ���� ��ư
    public Button joinRoomButton; // Join ��ư
    public Button join_CodeRoomButton; // Join ��ư
    public Button leaveRoomButton; // Join ��ư
    public Button codeButton; // Code ���� ��ư
    public Button readyButton; //ready ��ư
    public RectTransform contentRect; // Scroll View�� Content ũ�� ����
    public string gameSceneName = "GameTest"; // ���� �� �̸�

    private Dictionary<string, GameObject> roomList = new Dictionary<string, GameObject>(); // �� ��� ����
    private float roomSpacing = 40f;
    public string currentRoom { get; private set; }
    private int maxPlayers = 4;

    private string username;
    private bool isInitialized = false;

    //private bool IsHost = false;

    private HashSet<string> previousPlayerIDs = new HashSet<string>(); // ���� �÷��̾� ID ����


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
        while (NetworkManager.Singleton == null)
        {
            Debug.LogWarning("RoomManager: NetworkManager.Singleton�� ���� �ʱ�ȭ���� ����, ��� ��...");
            await Task.Delay(100);
        }
        // OnServerStarted �̺�Ʈ ���
        NetworkManager.Singleton.OnServerStarted += () =>
        {
            if (NetworkManager.Singleton.CustomMessagingManager == null)
            {
                Debug.LogError("RoomManager: CustomMessagingManager�� �ʱ�ȭ���� �ʾҽ��ϴ�!");
                return;
            }

            Debug.Log("RoomManager: CustomMessagingManager �ʱ�ȭ �Ϸ�");
            NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("OnRelayMessage", OnRelayMessageReceived);
        };

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

        //createRoomButton.onClick.AddListener(CreateRoom);
        //joinRoomButton.onClick.AddListener(JoinRoom);
        //leaveRoomButton.onClick.AddListener(LeaveRoom);
        codeButton.interactable = false;
        leaveRoomButton.interactable = false;

    }

    // Relay���� �޽����� �����ϰ� RoomList ����
    private void OnRelayMessageReceived(ulong clientId, FastBufferReader reader)
    {
        reader.ReadValueSafe(out string message);
        Debug.Log($"[Relay �޽��� ����] {message}");

        string[] messageParts = message.Split('|');
        if (messageParts.Length < 2) return;

        string command = messageParts[0];
        string roomId = messageParts[1];
        int playerCount = int.Parse(messageParts[2]);

        if (command == "UpdateRoom" && messageParts.Length == 3)
        {
            Debug.Log($"[Relay �޽��� ó��] {roomId}�� �ο� �� ������Ʈ: {playerCount}/4");
            UpdateRoomPlayerCountClientRpc(roomId, playerCount);
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
            // Relay ���� �� ���� ������ ���� ���� ó�� �߰�
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
                    }
                }
            );
            options.Data = new Dictionary<string, DataObject>
            {
                { "RelayJoinCode", new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode) }
            };


            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(roomName, maxPlayers, options);
            // �� �ڵ� ��ư Ȱ��ȭ �� �ڵ� ǥ��
            SetCurrentRoom(lobby.Id);

            UpdateRoomUI(lobby); Debug.Log($"[CreateRoom] �� ���� ����! ID: {lobby.Id}");
            Debug.Log($"[CreateRoom] �� ������ Ȯ��: RelayJoinCode = {lobby.Data["RelayJoinCode"].Value}");

            // �÷��̾� ������ Ȯ�� (����)
            foreach (var player in lobby.Players)
            {
                string playerUsername = "�� �� ����";

                if (player.Data != null && player.Data.ContainsKey("Username"))
                {
                    playerUsername = player.Data["Username"].Value;
                }
                Debug.Log($"[CreateRoom] �÷��̾� Ȯ��: Player ID = {player.Id}, Username = {playerUsername}");
            }

            readyButton.gameObject.SetActive(true);
            roomNameInput.interactable = false;
            joinCodeInput.interactable = false;
            createRoomButton.interactable = false;
            join_CodeRoomButton.interactable = false;
            codeButton.GetComponentInChildren<TMP_Text>().text = lobby.LobbyCode;
            codeButton.interactable = true;
            leaveRoomButton.interactable = true;

            // �� �����ڴ� �ڵ����� Host�� ��
            if (NetworkManager.Singleton.IsHost)
            {
                Debug.Log("[CreateRoom] ���� �÷��̾�� HOST �Դϴ�.");
            }
            else
            {
                Debug.Log("[CreateRoom] ���� �÷��̾�� Ŭ���̾�Ʈ �Դϴ�.");
            }

            // �� ���� �� 1�� ��� �� RefreshRoomList ����
            await RefreshRoomList();
        }
        catch (Exception e)
        {
            Debug.LogError($"�� ���� ����: {e.Message}");
        }
    }

    // �� ����
    public async void JoinRoom()
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
            Lobby lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(roomCode);
            if (lobby == null)
            {
                Debug.LogError("[JoinRoom] Lobby�� ã�� �� �����ϴ�!");
                return;
            }

            Debug.Log($"[JoinRoom] �� ���� ����! Lobby ID: {lobby.Id}");

            //  ������ Ŭ���̾�Ʈ�� �����͸� `UpdatePlayerAsync()`�� ����Ͽ� �߰�
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

            Debug.Log($"[JoinRoom] �÷��̾� ������ ������Ʈ �Ϸ�: {username}");

            // �ֽ� Lobby ���� ��������
            lobby = await LobbyService.Instance.GetLobbyAsync(lobby.Id);

            // ��� �÷��̾��� `Username` ��� (���������� ���ŵǴ��� Ȯ��)
            foreach (var player in lobby.Players)
            {
                string playerUsername = "�� �� ����";

                if (player.Data != null && player.Data.ContainsKey("Username"))
                {
                    playerUsername = player.Data["Username"].Value;
                }
                Debug.Log($"[CreateRoom] �÷��̾� Ȯ��: Player ID = {player.Id}, Username = {playerUsername}");
            }

            //relay ����
            string relayJoinCode = lobby.Data["RelayJoinCode"].Value;
            Debug.Log($"RelayJoinCode Ȯ�� �Ϸ�: {relayJoinCode}");

            bool relaySuccess = await RelayManager.Instance.JoinRelay(relayJoinCode);
            if (!relaySuccess)
            {
                Debug.LogError("Relay ���� ����!");
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

            // ȣ��Ʈ���� ���� Ȯ��
            if (NetworkManager.Singleton.IsHost)
            {
                Debug.Log("[JoinRoom] ���� �÷��̾�� HOST �Դϴ�.");
            }
            else
            {
                Debug.Log("[JoinRoom] ���� �÷��̾�� Ŭ���̾�Ʈ �Դϴ�.");
            }

            await RefreshRoomList();
            UpdateRoomUI(lobby);

            Debug.Log($"�� ���� ����: {lobby.Id}");
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"�� ���� ����: {e.Message}");
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
            Debug.LogError($"�� ������ ����: {e.Message}");
        }
    }

    //Ư�� ���� �ο� �� ������Ʈ
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
            Debug.LogError("[Ready] ���� ���� �����ϴ�!");
            return;
        }

        try
        {
            string playerId = AuthenticationService.Instance.PlayerId;

            // Ready ���¸� ������ ����
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

            Debug.Log("[Ready] �÷��̾� Ready ���� ������Ʈ �Ϸ�!");

            // ��� Ŭ���̾�Ʈ���� Ready ���� �˸���
            NotifyPlayersReady(playerId);

            // Ready ���� Ȯ�� �� �� ��ȯ
            await CheckAllPlayersReady();
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"[Ready] Ready ���� ������Ʈ ����: {e.Message}");
        }
    }

    [ClientRpc]
    private void NotifyPlayersReadyClientRpc(string playerId)
    {
        if (!IsServer) // ������ �̹� ���¸� �˰� �����Ƿ� Ŭ���̾�Ʈ�� ������Ʈ
        {
            Debug.Log($"[Ready] �÷��̾� {playerId}�� Ready ���°� �Ǿ����ϴ�.");
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
            //  �ֽ� Lobby ������ ��������
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

            Debug.Log($"[Ready] ���� Ready�� �÷��̾� ��: {readyCount}/{totalPlayers}");

            //  ��� ������ �÷��̾ Ready �����̸� �� ��ȯ
            if (readyCount == totalPlayers && totalPlayers > 1)
            {
                Debug.Log("[Ready] ��� �÷��̾ Ready �����Դϴ�! ���� ������ �̵�!");

                if (NetworkManager.Singleton.IsHost)
                {
                    Debug.Log("IsHost����");
                    LoadNextSceneServerRpc();
                }
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"[Ready] Ready ���� Ȯ�� ����: {e.Message}");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void LoadNextSceneServerRpc()
    {
        Debug.Log("[Ready] LoadNextSceneServerRpc() �����");
        LoadNextSceneClientRpc();
    }

    [ClientRpc]
    private void LoadNextSceneClientRpc()
    {
        Debug.Log("[Ready] LoadNextSceneClientRpc() �����");
        SceneManager.LoadScene(gameSceneName);
    }



    //private void LoadNextScene()
    //{
    //    if (IsHost)
    //    {
    //        Debug.Log("[Ready] ��� �÷��̾ Ready ����! ��Ʈ��ũ ������Ʈ ���� �� �� ��ȯ!");

    //        foreach (var playerObject in FindObjectsOfType<NetworkObject>())
    //        {
    //            if (playerObject.IsSpawned)
    //            {
    //                playerObject.Despawn();
    //                Debug.Log($"[Ready] ��Ʈ��ũ ������Ʈ ����: {playerObject.name}");
    //            }
    //        }
    //    }

    //    SceneManager.LoadScene("GameTest");
    //}



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

    // UI ����
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
            Debug.Log($"[ClientRpc] {roomId}�� �ο� �� ������Ʈ: {playerCount}/4");
        }
    }

    // �������� �� ���� ����ȭ
    [ServerRpc(RequireOwnership = false)]
    private void DestroyRoomServerRpc(string roomId)
    {
        DestroyRoomClientRpc(roomId);
    }

    // ��� Ŭ���̾�Ʈ���� �� ����
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
            // �������� �ֽ� �� ���� ��������
            Lobby lobby = await LobbyService.Instance.GetLobbyAsync(currentRoom);

            // PlayerListManager�� ���� ���� �÷��̾� ����
            PlayerListManager.Instance.ClearPlayers();

            // ���� ���ŵ� �÷��̾� ���� �߰�
            foreach (var player in lobby.Players)
            {
                string playerName = player.Data["Username"].Value;
                bool isHost = player.Id == lobby.HostId;

                // �÷��̾� �߰�
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
                Debug.Log("4�� ���� �Ϸ�! ���� ������ �̵�");
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
        // �� ������ ���� Content ũ�� ����
        int roomCount = roomList.Count;
        float contentHeight = Mathf.Max(200f, (roomCount * roomSpacing) + 80f);
        contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, contentHeight);
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

            //  ���� ȣ��Ʈ�� ���� ���� ���� ����
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
    
    public async Task UpdateLobbyPlayerList()
    {
        if (string.IsNullOrEmpty(currentRoom))
        {
            Debug.LogWarning("���� ���� �������� �ʾҽ��ϴ�.");
            return;
        }

        try
        {
            // �ֽ� Lobby ���� ��������
            Lobby lobby = await LobbyService.Instance.GetLobbyAsync(currentRoom);
            if (lobby == null)
            {
                Debug.LogError("Lobby ������ ������ �� �����ϴ�.");
                return;
            }

            // ���� �÷��̾� ID ��� ����
            HashSet<string> currentPlayerIDs = new HashSet<string>();
            foreach (var player in lobby.Players)
            {
                currentPlayerIDs.Add(player.Id);
            }

            // ���ο� �÷��̾� ���� �� �α� ���
            foreach (string playerId in currentPlayerIDs)
            {
                if (!previousPlayerIDs.Contains(playerId))
                {
                    Debug.Log($"[ȣ��Ʈ] ���ο� �÷��̾� ����: Player ID = {playerId}");
                }
            }

            // ���� �÷��̾� ��� ������Ʈ
            previousPlayerIDs = new HashSet<string>(currentPlayerIDs);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Lobby �÷��̾� ��� ������Ʈ ����: {e.Message}");
        }
    }



    // �� ���� ó��
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
                    Debug.Log($" RelayJoinCode Ȯ�� �Ϸ�: {lobby.Data["RelayJoinCode"].Value}");
                    return true;
                }
            }
            catch (LobbyServiceException e)
            {
                Debug.LogWarning($"RelayJoinCode ��ȸ ����: {e.Message}");
            }

            Debug.Log($"RelayJoinCode�� ���� �������� ����. ({attempt + 1}/{maxAttempts}) ��õ� ��...");
            await Task.Delay(2000); // 2�� ��� �� �ٽ� �õ�
        }

        Debug.LogError(" �ִ� ��õ� �Ŀ��� RelayJoinCode�� �������� ����.");
        return false;
    }
}
