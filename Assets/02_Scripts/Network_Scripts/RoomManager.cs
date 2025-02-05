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
    private string currentRoom = "";
    private int maxPlayers = 4;

    private string username;
    private bool isInitialized = false;

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

        InvokeRepeating(nameof(RefreshRoomList), 0f, 15f); // 5�ʸ��� �ڵ� ����
    }

    // Relay���� �޽����� �����ϰ� RoomList ����
    private void OnRelayMessageReceived(ulong clientId, FastBufferReader reader)
    {
        reader.ReadValueSafe(out string message);
        Debug.Log($"Relay Message Received: {message}");

        string[] messageParts = message.Split('|');
        if (messageParts.Length < 2) return;

        string command = messageParts[0];
        string roomId = messageParts[1];
        int playerCount = int.Parse(messageParts[2]);

        if (command == "UpdateRoom" && messageParts.Length == 3)
        {
            UpdateRoomPlayerCount(roomId, playerCount);
        }
        else if (command == "DeleteRoom")
        {
            DestroyRoom(roomId);
        }
    }

    public async void CreateRoom()
    {
        if (!isInitialized)
        {
            Debug.LogError("Unity Services�� �ʱ�ȭ���� �ʾҽ��ϴ�!");
            return;
        }

        if (!string.IsNullOrEmpty(currentRoom))
        {
            Debug.LogWarning("�̹� �濡 ������ �����Դϴ�.");
            return;
        }

        string roomName = roomNameInput.text.Trim();
        if (string.IsNullOrEmpty(roomName))
        {
            Debug.LogWarning("�� �̸��� �Է��ϼ���.");
            return;
        }

        createRoomButton.interactable = false;

        try
        {
            // Relay ���� �� ���� ������ ���� ���� ó�� �߰�
            string relayJoinCode = await RelayManager.Instance.CreateRelay(4);
            if (string.IsNullOrEmpty(relayJoinCode))
            {
                Debug.LogError("Relay ���� ����");
                createRoomButton.interactable = true;
                return;
            }

            CreateLobbyOptions options = new CreateLobbyOptions
            {
                IsPrivate = false,
                Player = new Player(
                    id: AuthenticationService.Instance.PlayerId,
                    data: new Dictionary<string, PlayerDataObject>
                    {
                        { "Username", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, username) },
                        { "RelayJoinCode", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, relayJoinCode) }
                    }
                )
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(roomName, 4, options);
            currentRoom = lobby.Id;

            if (!string.IsNullOrEmpty(lobby.LobbyCode))
            {
                codeButton.interactable = true;
                codeButton.GetComponentInChildren<TMP_Text>().text = lobby.LobbyCode;
                Debug.Log($"Lobby ������: {lobby.LobbyCode}");
            }
            else
            {
                Debug.LogError("Lobby ���� �� �ڵ尡 ��ȯ���� �ʾҽ��ϴ�!");
            }

            UpdateRoomUI(lobby);
            Debug.Log($"�� ���� ����: {lobby.Id}");
        }
        catch (Exception e)
        {
            Debug.LogError($"�� ���� ����: {e.Message}");
        }
        finally
        {
            createRoomButton.interactable = true;
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

            // RelayJoinCode�� ����ȭ�� ������ ��� (�ִ� 3��)
            int relayWaitAttempts = 3;
            while ((!lobby.Data.ContainsKey("RelayJoinCode") || string.IsNullOrEmpty(lobby.Data["RelayJoinCode"].Value)) && relayWaitAttempts > 0)
            {
                Debug.LogWarning("RelayJoinCode�� ���� ����ȭ���� �ʾҽ��ϴ�. �ٽ� �õ��մϴ�...");
                await Task.Delay(1000);
                lobby = await LobbyService.Instance.GetLobbyAsync(lobby.Id);
                relayWaitAttempts--;
            }

            if (!lobby.Data.ContainsKey("RelayJoinCode") || string.IsNullOrEmpty(lobby.Data["RelayJoinCode"].Value))
            {
                Debug.LogError("RelayJoinCode�� �������� �ʰų� ��� �ֽ��ϴ�.");
                return;
            }

            string relayJoinCode = lobby.Data["RelayJoinCode"].Value;

            bool relaySuccess = await RelayManager.Instance.JoinRelay(relayJoinCode);
            if (!relaySuccess)
            {
                Debug.LogError("Relay ���� ����!");
                return;
            }

            currentRoom = lobby.Id;
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


    // �������� Ŭ���̾�Ʈ���� �ο� �� ����ȭ
    [ServerRpc(RequireOwnership = false)]
    private void UpdateRoomPlayerCountServerRpc(string roomId, int playerCount)
    {
        UpdateRoomPlayerCountClientRpc(roomId, playerCount);
    }

    // ��� Ŭ���̾�Ʈ���� RoomList ������Ʈ
    [ClientRpc]
    private void UpdateRoomPlayerCountClientRpc(string roomId, int playerCount)
    {
        if (roomList.ContainsKey(roomId))
        {
            TMP_Text playerCountText = roomList[roomId].transform.Find("roomPlayers").GetComponent<TMP_Text>();
            playerCountText.text = $"{playerCount}/4";
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
                // ���� ������ ���� ���ΰ�ħ���� ����
                if (currentRoom != null && lobby.Id == currentRoom)
                {
                    Debug.Log($" ���� ������ ��({lobby.Id})�� RefreshRoomList���� ���ܵ�.");
                    continue;
                }

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
    //private async Task InitializeServices()
    //{
    //    try
    //    {
    //        await UnityServices.InitializeAsync(); // Unity Services �ʱ�ȭ
    //        if (!AuthenticationService.Instance.IsSignedIn)
    //        {
    //            await AuthenticationService.Instance.SignInAnonymouslyAsync(); // �͸� �α���
    //        }
    //        Debug.Log("Unity Services & Authentication Initialized Successfully.");
    //    }
    //    catch (System.Exception e)
    //    {
    //        Debug.LogError($"Unity Services Initialization Failed: {e.Message}");
    //    }
    //}

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
}
