using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class RoomManager : MonoBehaviour
{
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

    private Dictionary<string, GameObject> roomList = new Dictionary<string, GameObject>(); // 방 목록 관리
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
        if (string.IsNullOrEmpty(roomName)) return; // 빈 값 방지

        string roomCode = GenerateRoomCode(); // 랜덤한 방 코드 생성
        if (roomList.ContainsKey(roomCode)) return; // 중복 코드 방지

        // 방 생성
        GameObject newRoom = Instantiate(roomPrefab, roomListParent);
        newRoom.transform.Find("roomName").GetComponent<TMP_Text>().text = roomName;
        newRoom.transform.Find("roomPlayers").GetComponent<TMP_Text>().text = "1/4";

        int roomCount = roomList.Count;

        roomList.Add(roomCode, newRoom); // Dictionary에 방 추가
        currentRoom = roomCode;
        // 스크롤뷰 크기 조정
        UpdateScrollView();

        // UI 비활성화
        roomNameInput.interactable = false; // 방을 만들었으므로 입력창 비활성화
        joinCodeInput.interactable=false;
        createRoomButton.interactable = false;
        joinRoomButton.interactable = false;
        leaveRoomButton.interactable = true;

        // Code 버튼 활성화 및 코드 표시
        codeButton.interactable = true; // Code 버튼 활성화
        codeButton.GetComponentInChildren<TMP_Text>().text = roomCode; // 버튼에 코드 표시
    }
    // 방 입장
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
            Debug.Log("잘못된 방 코드");
        }
    }

    // 방 퇴장
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
            // Code 버튼 비활성화 및 코드 표시
            codeButton.interactable = true; // Code 버튼 활성화
            codeButton.GetComponentInChildren<TMP_Text>().text = "Code"; // 버튼에 코드 표시
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
        // 방 개수에 따라 Content 크기 조절
        int roomCount = roomList.Count;
        float contentHeight = Mathf.Max(200f, (roomCount * roomSpacing) + 80f);
        contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, contentHeight);
    }
}
