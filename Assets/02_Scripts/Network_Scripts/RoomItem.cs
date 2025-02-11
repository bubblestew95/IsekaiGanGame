using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class RoomItem : MonoBehaviour, IPointerClickHandler
{
    public TMP_Text roomNameText;
    public TMP_Text playerCountText;
    private string roomId;

    // �� ������ ����
    public void SetRoomInfo(string roomId, string roomName, int currentPlayers, int maxPlayers)
    {
        this.roomId = roomId;
        roomNameText.text = $"{roomName}";
        playerCountText.text = $"{currentPlayers}/{maxPlayers}";

        // ����� �α� �߰�
        Debug.Log($"[RoomItem] SetRoomInfo ȣ���: roomId={roomId}, roomName={roomName}, players={currentPlayers}/{maxPlayers}");
    }

    // Ŭ�� �̺�Ʈ ó��
    public void OnPointerClick(PointerEventData eventData)
    {
        // Ŭ���� �� ���� �α�
        Debug.Log($"[RoomItem] �� Ŭ����: roomId={roomId}, roomName={roomNameText.text}, players={playerCountText.text}");

        // RoomManager�� �̺�Ʈ ����
        RoomManager.Instance.OnRoomItemClicked(this);
    }

    // RoomItem�� RoomId�� ��ȯ
    public string GetRoomId()
    {
        return roomId;
    }
}
