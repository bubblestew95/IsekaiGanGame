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

    public GameObject roomPrefab; // Roomstate ������
    public Transform roomListParent; // �� ����� ǥ���� �θ� ������Ʈ
    public TMP_InputField roomNameInput; // �� �̸� �Է� �ʵ�
    public TMP_InputField joinCodeInput; // �� �ڵ� �Է� �ʵ�
    public Button createRoomButton; // �� ���� ��ư
    public Button joinRoomButton; // Join ��ư
    public Button join_CodeRoomButton; // Join ��ư
    public Button leaveRoomButton; // Join ��ư
    public Button codeButton; // Code ���� ��ư
    public RectTransform contentRect; // Scroll View�� Content ũ�� ����
    public string gameSceneName = "GameTest"; // ���� �� �̸�

    private Dictionary<string, GameObject> roomList = new Dictionary<string, GameObject>(); // �� ��� ����
    private float roomSpacing = 40f;
    public string currentRoom { get; private set; }
    private int maxPlayers = 4;

    private string username;
    private bool isInitialized = false;

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
            // ���⼭ username�� ����Ͽ� ���ϴ� �۾� ����
        }

        //createRoomButton.onClick.AddListener(CreateRoom);
        //joinRoomButton.onClick.AddListener(JoinRoom);
        //leaveRoomButton.onClick.AddListener(LeaveRoom);
        codeButton.interactable = false;
        leaveRoomButton.interactable = false;

        

        //InvokeRepeating(nameof(RefreshRoomList), 0f, 15f); // 5�ʸ��� �ڵ� ����

        //while (true)
        //{
        //    await UpdateLobbyPlayerList();
        //    await Task.Delay(3000); // 3�ʸ��� �ֽ� �÷��̾� ����Ʈ Ȯ��
        //}
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

        //if (!string.IsNullOrEmpty(currentRoom))
        //{
        //    Debug.LogWarning("�̹� �濡 ������ �����Դϴ�.");
        //    return;
        //}

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
                Debug.Log($"Lobby ������: {lobby.LobbyCode}");
                

            }
            else
            {
                Debug.LogError("Lobby ���� �� �ڵ尡 ��ȯ���� �ʾҽ��ϴ�!");
            }

            // �ֽ� �����͸� �������� (�ִ� 3ȸ ��õ�, 2�� ����)
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
                    Debug.LogError($"Lobby �����͸� �������� �� ���� �߻�: {e.Message}");
                }
            }

            if (lobby.Data == null)
            {
                Debug.LogError("Lobby �����͸� �������� ���߽��ϴ�.");
                return;
            }



            UpdateRoomUI(lobby);
            Debug.Log($"�� ���� ����: {lobby.Id}");
            Debug.Log($"�� ������: {lobby.Data}");
            roomNameInput.interactable = false;
            joinCodeInput.interactable = false;
            createRoomButton.interactable = false;
            join_CodeRoomButton.interactable = false;
            codeButton.interactable = true;
            leaveRoomButton.interactable = true;

            // �� ���� �� 1�� ��� �� RefreshRoomList ����
            await Task.Delay(1000);
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
            Lobby lobby = null;
            int maxAttempts = 3;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                try
                {
                    lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(roomCode);
                    Debug.Log($"�� ���̵� ã�� : ({lobby.Id})");
                    Debug.Log($"�� ������ : ({lobby.Data})");
                    if (lobby != null) break;
                }
                catch (LobbyServiceException)
                {
                    Debug.LogWarning($"Lobby ��ȸ ����. ��õ� ��... ({attempt + 1}/{maxAttempts})");
                    await Task.Delay(1000);
                }
            }

            if (lobby == null)
            {
                Debug.LogError("Lobby�� ã�� �� �����ϴ�! Room Code�� ��Ȯ���� Ȯ���ϼ���.");
                return;
            }
            //if (!await WaitForRelayJoinCode(lobby.Id))
            //{
                
            //    Debug.LogError("RelayJoinCode�� ������ �� ���� �� ���� ����.");
            //    return;
            //}

            // lobby.Data�� null�̸� �ֽ� ������ �������� �õ�
            int retryDataFetch = 3;
            while (lobby.Data == null && retryDataFetch > 0)
            {
                Debug.LogWarning("Lobby �����Ͱ� null�Դϴ�. �ٽ� �õ��մϴ�...");
                await Task.Delay(1000);
                var newLobby = await LobbyService.Instance.GetLobbyAsync(lobby.Id);
                if (newLobby != null) lobby = newLobby;
                retryDataFetch--;
            }

            // ������ null�̸� ���� ó��
            if (lobby.Data == null)
            {
                Debug.LogError("Lobby �����͸� �ҷ����� ���߽��ϴ�. �ٽ� �õ��ϼ���.");
                return;
            }

            // �ű� �÷��̾� Ȯ�� �� �ֿܼ� ���
            foreach (var player in lobby.Players)
            {
                string username = "�� �� ����"; // �⺻�� ����

                //  `player.Data`�� null���� ���� Ȯ��
                if (player.Data == null)
                {
                    Debug.LogWarning($" �÷��̾� {player.Id}�� �����Ͱ� �����ϴ�! (player.Data == null)");
                    continue; // ���� �÷��̾�� �Ѿ
                }

                if (player.Data.ContainsKey("Username"))
                {
                    username = player.Data["Username"].Value;
                }
                else
                {
                    Debug.LogWarning($"�÷��̾� {player.Id}�� Username �����Ͱ� �����ϴ�!");
                }

                Debug.Log($"[Lobby] �÷��̾� ���� Ȯ��: Player ID = {player.Id}, Username = {username}");
            }



            // RelayJoinCode�� ������ ������ �ִ� 3�� ���
            int relayWaitAttempts = 3;
            while (relayWaitAttempts > 0)
            {
                if (lobby.Data.ContainsKey("RelayJoinCode") && !string.IsNullOrEmpty(lobby.Data["RelayJoinCode"].Value))
                {
                    break; // RelayJoinCode�� �����ϸ� ���� ����
                }

                Debug.LogWarning("RelayJoinCode�� ���� ����ȭ���� �ʾҽ��ϴ�. �ٽ� �õ��մϴ�...");
                await Task.Delay(1000);
                var updatedLobby = await LobbyService.Instance.GetLobbyAsync(lobby.Id);
                if (updatedLobby != null) lobby = updatedLobby;
                relayWaitAttempts--;
            }

            // RelayJoinCode�� ������ ����
            if (!lobby.Data.ContainsKey("RelayJoinCode") || string.IsNullOrEmpty(lobby.Data["RelayJoinCode"].Value))
            {
                Debug.LogError("RelayJoinCode�� �������� �ʰų� ��� �ֽ��ϴ�.");
                return;
            }

            string relayJoinCode = lobby.Data["RelayJoinCode"].Value;
            Debug.Log($"[JoinRoom] RelayJoinCode Ȯ�� �Ϸ�: {relayJoinCode}");

            bool relaySuccess = await RelayManager.Instance.JoinRelay(relayJoinCode);
            if (!relaySuccess)
            {
                Debug.LogError("Relay ���� ����!");
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
