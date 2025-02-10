using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class RoomItem : MonoBehaviour
{
    public TMP_Text roomNameText;
    public TMP_Text playerCountText;
    private string roomId;

    // 방 정보를 설정
    public void SetRoomInfo(string roomId, string roomName, int currentPlayers, int maxPlayers)
    {
        this.roomId = roomId;
        roomNameText.text = $"{roomName} ({currentPlayers}/{maxPlayers})";
    }

    // 클릭 이벤트 처리
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"방 클릭: {roomId} - {roomNameText.text}");
        RoomManager.Instance.SetCurrentRoom(roomId); // RoomManager에 현재 선택된 방 ID 전달
    }
}
