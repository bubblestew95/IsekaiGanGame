using TMPro;
using UnityEngine;

public class RoomItem : MonoBehaviour
{
    public TMP_Text roomNameText;
    public TMP_Text playerCountText;

    public void SetRoomInfo(string roomName, int currentPlayers, int maxPlayers)
    {
        roomNameText.text = $"{roomName} ({currentPlayers}/{maxPlayers})";
    }
}
