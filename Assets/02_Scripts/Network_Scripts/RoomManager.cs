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
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("[RoomManager] NetworkManager�� �������� �ʽ��ϴ�!");
        }

        Debug.Log($"[RoomManager] ���� NetworkManager ���� - IsServer: {NetworkManager.Singleton.IsServer}, IsClient: {NetworkManager.Singleton.IsClient}, IsHost: {NetworkManager.Singleton.IsHost}");

        // �ֽ� Unity 6���� Relay Ȱ��ȭ ���� Ȯ��
        //UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        //if (transport == null)
        //{
        //    Debug.LogError("[RoomManager] UnityTransport�� �������� �ʽ��ϴ�!");
        //    return;
        //}

        //// Relay ���� ������ Ȯ��
        //RelayServerData relayData;
        //if (!transport.TryGetRelayServerData(out relayData))
        //{
        //    Debug.LogError("[RoomManager] Relay ���� �����Ͱ� �������� �ʾҽ��ϴ�! ���� ������ �ߴ��մϴ�.");
        //    return;
        //}

        Debug.Log("[RoomManager] Relay ���� Ȯ�� �Ϸ�! StartHost() ���� ����");


        //while (NetworkManager.Singleton == null)
        //{
        //    Debug.LogWarning("RoomManager: NetworkManager.Singleton�� ���� �ʱ�ȭ���� ����, ��� ��...");
        //    await Task.Delay(100);
        //}

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

        // OnServerStarted �̺�Ʈ ���
        //NetworkManager.Singleton.OnServerStarted += () =>
        //{
        //    if (NetworkManager.Singleton == null || NetworkManager.Singleton.CustomMessagingManager == null)
        //    {
        //        Debug.LogError("RoomManager: NetworkManager �Ǵ� CustomMessagingManager�� �ʱ�ȭ���� �ʾҽ��ϴ�!");
        //        return;
        //    }

        //    Debug.Log("RoomManager: CustomMessagingManager �ʱ�ȭ �Ϸ�");
        //    NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("OnRelayMessage", OnRelayMessageReceived);
        //};

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

            // �� ������ ������Ʈ
            lobby = await LobbyService.Instance.UpdateLobbyAsync(lobby.Id, updateLobbyOptions);

            // NetworkManager ����
            NetworkManager.Singleton.StartHost();

            // �� �ڵ� ��ư Ȱ��ȭ �� �ڵ� ǥ��
            SetCurrentRoom(lobby.Id);

            //UpdateRoomUI(lobby); Debug.Log($"[CreateRoom] �� ���� ����! ID: {lobby.Id}");
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
            join_SelectRoomButton.interactable = false;
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

            // RelayJoinCode Ȯ�� �� ����
            if (!lobby.Data.ContainsKey("RelayJoinCode") || string.IsNullOrEmpty(lobby.Data["RelayJoinCode"].Value))
            {
                Debug.LogError("RelayJoinCode�� �������� �ʽ��ϴ�!");
                return;
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


            ////  ������ Ŭ���̾�Ʈ�� �����͸� `UpdatePlayerAsync()`�� ����Ͽ� �߰�
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

            // ȣ��Ʈ���� ���� Ȯ��
            if (NetworkManager.Singleton.IsHost)
            {
                Debug.Log("[JoinRoom] ���� �÷��̾�� HOST �Դϴ�.");
            }
            else
            {
                Debug.Log("[JoinRoom] ���� �÷��̾�� Ŭ���̾�Ʈ �Դϴ�.");
            }

            RequestRoomRefresh();
            UpdateRoomUI(lobby);

            Debug.Log($"�� ���� ����: {lobby.Id}");
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

            // ���õ� Room ID�� ����Ͽ� ����
            Lobby lobby = await LobbyService.Instance.JoinLobbyByIdAsync(currentRoom);

            string relayJoinCode = lobby.Data["RelayJoinCode"].Value;

            if (string.IsNullOrEmpty(relayJoinCode))
            {
                Debug.LogError("[JoinSelectedRoom] RelayJoinCode ���� ��� �ֽ��ϴ�!");
                return;
            }

            Debug.Log($"RelayJoinCode Ȯ�� �Ϸ�: {relayJoinCode}");

            bool relaySuccess = await RelayManager.Instance.JoinRelay(relayJoinCode);
            if (!relaySuccess)
            {
                Debug.LogError("Relay ���� ����!");
                return;
            }

            Debug.Log($"[JoinSelectedRoom] {lobby.Id}�� ���� ����!");

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
            Debug.LogError($"[JoinSelectedRoom] �� ���� ����: {e.Message}");
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
            join_SelectRoomButton.interactable = true;
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
            if (string.IsNullOrEmpty(playerId))
            {
                Debug.LogError("[Ready] Player ID�� ã�� �� �����ϴ�!");
                return;
            }

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

            Debug.Log("NotifyPlayersReady �����");
            // Ready ���� Ȯ�� �� �� ��ȯ
            await CheckAllPlayersReady();
            Debug.Log("CheckAllPlayersReady �����");
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



    public async Task CheckAllPlayersReady()
    {
        try
        {
            //  �ֽ� Lobby ������ ��������
            Lobby lobby = await LobbyService.Instance.GetLobbyAsync(currentRoom);

            if (lobby == null)
            {
                Debug.LogError("[Ready] Lobby ������ ������ �� �����ϴ�.");
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
                        Debug.Log($"[Ready] �÷��̾� {player.Id}�� ������ - Key: {dataKey}, Value: {player.Data[dataKey].Value}");
                    }

                    if (player.Data.ContainsKey("Ready") && player.Data["Ready"].Value == "True")
                    {
                        readyCount++;
                    }
                }
                else
                {
                    Debug.LogWarning($"[Ready] �÷��̾� {player.Id}�� �����Ͱ� null�Դϴ�!");
                }
            }

            Debug.Log($"[Ready] ���� Ready�� �÷��̾� ��: {readyCount}/{totalPlayers}");

            //  ��� ������ �÷��̾ Ready �����̸� �� ��ȯ
            if (readyCount == totalPlayers && totalPlayers > 1)
            {
                Debug.Log("[Ready] ��� �÷��̾ Ready �����Դϴ�! ���� ������ �̵�!");

                RequestSceneChange();
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"[Ready] Ready ���� Ȯ�� ����: {e.Message}");
        }
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

    public void RequestRoomRefresh()
    {
        if (!NetworkManager.Singleton.IsServer)  // Ŭ���̾�Ʈ�� ��û ����
        {
            Debug.Log("[Ŭ���̾�Ʈ] �� ���ΰ�ħ");
            NotifyAllClientsToRefreshRoomListServerRpc();
        }
        else
        {
            Debug.Log("[����] �� ���ΰ�ħ �ʿ� ����(�̹���)");
            //RefreshRoomListClientRpc();
        }
    }

    [ServerRpc]
    public void NotifyAllClientsToRefreshRoomListServerRpc()
    {
        RefreshRoomListClientRpc();
    }

    // ��� Ŭ���̾�Ʈ�� ������ RefreshRoomList RPC
    [ClientRpc]
    private void RefreshRoomListClientRpc()
    {
        Debug.Log("[Client] ��� Ŭ���̾�Ʈ���� RefreshRoomList ����!");
        RefreshRoomList();
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
        join_CodeRoomButton.interactable = true;
        join_SelectRoomButton.interactable = true;
        leaveRoomButton.interactable = false;
        codeButton.interactable = false;
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
            if (playerCountText.text == $"{maxPlayers}/{maxPlayers}")
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
                if (!roomList.ContainsKey(lobby.Id))
                {
                    // RoomItem ������ ����
                    GameObject newRoom = Instantiate(roomPrefab, roomListParent);
                    RoomItem roomItem = newRoom.GetComponent<RoomItem>();

                    // SetRoomInfo ȣ�� �� ����� �α� Ȯ��
                    roomItem.SetRoomInfo(lobby.Id, lobby.Name, lobby.Players.Count, lobby.MaxPlayers);

                    roomList.Add(lobby.Id, newRoom);
                }
                else
                {
                    // �̹� �����ϴ� ���� �÷��̾� �� ����
                    TMP_Text playerCountText = roomList[lobby.Id].transform.Find("roomPlayers").GetComponent<TMP_Text>();
                    playerCountText.text = $"{lobby.Players.Count}/{lobby.MaxPlayers}";
                }
            }

            //  ���� ȣ��Ʈ�� ���� ���� ���� ����
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
