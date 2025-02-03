using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

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

    private Dictionary<string, GameObject> roomList = new Dictionary<string, GameObject>(); // �� ��� ����
    private float roomSpacing = 40f;
    private string currentRoom = "";

    void Start()
    {
        createRoomButton.onClick.AddListener(CreateRoom);
        codeButton.interactable = false;
        leaveRoomButton.interactable = false;
    }

    public void CreateRoom()
    {
        string roomName = roomNameInput.text.Trim();
        if (string.IsNullOrEmpty(roomName)) return; // �� �� ����

        string roomCode = GenerateRoomCode(); // ������ �� �ڵ� ����
        if (roomList.ContainsKey(roomCode)) return; // �ߺ� �ڵ� ����

        // �� ����
        GameObject newRoom = Instantiate(roomPrefab, roomListParent);
        newRoom.transform.Find("roomName").GetComponent<TMP_Text>().text = roomName;
        newRoom.transform.Find("roomPlayers").GetComponent<TMP_Text>().text = "1/4";

        int roomCount = roomList.Count;

        roomList.Add(roomCode, newRoom); // Dictionary�� �� �߰�
        currentRoom = roomCode;
        // ��ũ�Ѻ� ũ�� ����
        UpdateScrollView();

        // UI ��Ȱ��ȭ
        roomNameInput.interactable = false; // ���� ��������Ƿ� �Է�â ��Ȱ��ȭ
        joinCodeInput.interactable=false;
        createRoomButton.interactable = false;
        joinRoomButton.interactable = false;
        leaveRoomButton.interactable = true;

        // Code ��ư Ȱ��ȭ �� �ڵ� ǥ��
        codeButton.interactable = true; // Code ��ư Ȱ��ȭ
        codeButton.GetComponentInChildren<TMP_Text>().text = roomCode; // ��ư�� �ڵ� ǥ��
    }
    // �� ����
    public void JoinRoom()
    {
        string roomCode = joinCodeInput.text;
        if (roomList.ContainsKey(roomCode))
        {
            TMP_Text playerCountText = roomList[roomCode].transform.Find("roomPlayers").GetComponent<TMP_Text>();
            string[] count = playerCountText.text.Split('/');
            int currentPlayers = int.Parse(count[0]);
            int maxPlayers = int.Parse(count[1]);

            if (currentPlayers < maxPlayers)
            {
                currentPlayers++;
                playerCountText.text = currentPlayers + "/4";

                currentRoom = roomCode;
                roomNameInput.interactable = false;
                joinCodeInput.interactable = false;
                createRoomButton.interactable = false;
                joinRoomButton.interactable = false;
                leaveRoomButton.interactable = true;
            }
        }
        else
        {
            Debug.Log("�߸��� �� �ڵ�");
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
}
