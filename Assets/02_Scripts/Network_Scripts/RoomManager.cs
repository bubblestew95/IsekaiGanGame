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

    async void Start()
    {
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

        createRoomButton.onClick.AddListener(CreateRoom);
        codeButton.interactable = false;
        leaveRoomButton.interactable = false;

        InvokeRepeating(nameof(RefreshRoomList), 0f, 5f); // 5�ʸ��� �ڵ� ����
    }

    public async void CreateRoom()
    {
        string roomName = roomNameInput.text.Trim();
        if (string.IsNullOrEmpty(roomName)) return; // �� �� ����

        try
        {
            // Lobby Service�� ���� �� ����
            CreateLobbyOptions options = new CreateLobbyOptions
            {
                IsPrivate = false, // ���� ��
                Player = new Player(
                    id: AuthenticationService.Instance.PlayerId, // ���� �÷��̾� ID
                    data: new Dictionary<string, PlayerDataObject>
                    {
                    { "Username", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, username) }
                    }
                )
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(roomName, maxPlayers, options);

            // �� ���� ���� �� ó��
            GameObject newRoom = Instantiate(roomPrefab, roomListParent);
            newRoom.transform.Find("roomName").GetComponent<TMP_Text>().text = lobby.Name;
            newRoom.transform.Find("roomPlayers").GetComponent<TMP_Text>().text = $"{lobby.Players.Count}/{maxPlayers}";

            roomList.Add(lobby.Id, newRoom); // Dictionary�� �� �߰�
            currentRoom = lobby.Id;

            // �÷��̾� ����Ʈ ���� (������ �������� �߰�)
            PlayerListManager.Instance.AddPlayer(username, true);

            // UI ������Ʈ
            roomNameInput.interactable = false;
            joinCodeInput.interactable = false;
            createRoomButton.interactable = false;
            joinRoomButton.interactable = false;
            leaveRoomButton.interactable = true;

            // Code ��ư Ȱ��ȭ �� �ڵ� ǥ��
            codeButton.interactable = true;
            codeButton.GetComponentInChildren<TMP_Text>().text = lobby.LobbyCode;

            Debug.Log($"Lobby created successfully: {lobby.Id}");
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Failed to create lobby: {e.Message}");
        }
    }
    // �� ����
    public async void JoinRoom()
    {
        string roomCode = joinCodeInput.text;
        if (string.IsNullOrEmpty(roomCode)) return;

        try
        {
            // Lobby Service�� ���� �� ����
            Lobby lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(roomCode);

            // �� ���� ���� �� ó��
            if (roomList.ContainsKey(lobby.Id))
            {
                TMP_Text playerCountText = roomList[lobby.Id].transform.Find("roomPlayers").GetComponent<TMP_Text>();
                playerCountText.text = $"{lobby.Players.Count}/{maxPlayers}";

                currentRoom = lobby.Id;
                roomNameInput.interactable = false;
                joinCodeInput.interactable = false;
                createRoomButton.interactable = false;
                joinRoomButton.interactable = false;
                leaveRoomButton.interactable = true;

                // �÷��̾� �߰�
                PlayerListManager.Instance.AddPlayer(username, false);
                CheckPlayerCount();

                Debug.Log($"Joined lobby successfully: {lobby.Id}");
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Failed to join lobby: {e.Message}");
        }
    }

    // �� ����
    public void LeaveRoom()
    {
        if (string.IsNullOrEmpty(currentRoom)) return;

        if (roomList.ContainsKey(currentRoom))
        {
            TMP_Text playerCountText = roomList[currentRoom].transform.Find("roomPlayers").GetComponent<TMP_Text>();
            string[] count = playerCountText.text.Split('/');
            int currentPlayers = int.Parse(count[0]) - 1;

            // �÷��̾� ����
            PlayerListManager.Instance.RemovePlayer(username);

            if (currentPlayers <= 0)
            {
                Destroy(roomList[currentRoom]);
                roomList.Remove(currentRoom);
            }
            else
            {
                playerCountText.text = currentPlayers + "/4";
            }

            currentRoom = "";
            roomNameInput.interactable = true;
            joinCodeInput.interactable = true;
            createRoomButton.interactable = true;
            joinRoomButton.interactable = true;
            leaveRoomButton.interactable = false;
            // Code ��ư ��Ȱ��ȭ �� �ڵ� ǥ��
            codeButton.interactable = true; // Code ��ư Ȱ��ȭ
            codeButton.GetComponentInChildren<TMP_Text>().text = "Code"; // ��ư�� �ڵ� ǥ��
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
            stringChars[i] = chars[Random.Range(0, chars.Length)];
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

    public async void RefreshRoomList()
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

            foreach (Transform child in roomListParent)
            {
                Destroy(child.gameObject); // ���� �� ��� ����
            }

            roomList.Clear();

            foreach (var lobby in response.Results)
            {
                GameObject newRoom = Instantiate(roomPrefab, roomListParent);
                newRoom.transform.Find("roomName").GetComponent<TMP_Text>().text = lobby.Name;
                newRoom.transform.Find("roomPlayers").GetComponent<TMP_Text>().text = $"{lobby.Players.Count}/4";
                roomList.Add(lobby.Id, newRoom);
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Failed to refresh lobbies: {e.Message}");
        }
    }

    private async Task InitializeServices()
    {
        try
        {
            await UnityServices.InitializeAsync(); // Unity Services �ʱ�ȭ
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync(); // �͸� �α���
            }
            Debug.Log("Unity Services & Authentication Initialized Successfully.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Unity Services Initialization Failed: {e.Message}");
        }
    }

}
