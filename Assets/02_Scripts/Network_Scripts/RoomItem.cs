using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class RoomItem : MonoBehaviour
{
    public TMP_Text roomNameText;
    public TMP_Text playerCountText;
    private string roomId;

    // �� ������ ����
    public void SetRoomInfo(string roomId, string roomName, int currentPlayers, int maxPlayers)
    {
        this.roomId = roomId;
        roomNameText.text = $"{roomName} ({currentPlayers}/{maxPlayers})";
    }

    // Ŭ�� �̺�Ʈ ó��
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"�� Ŭ��: {roomId} - {roomNameText.text}");
        RoomManager.Instance.SetCurrentRoom(roomId); // RoomManager�� ���� ���õ� �� ID ����
    }
}
