using TMPro;
using UnityEngine;

public class PlayerItem : MonoBehaviour
{
    public TMP_Text playerNameText;  // 플레이어 이름을 표시할 UI 텍스트
    public GameObject hostIcon;      // 방장 여부를 표시할 아이콘 (예: 왕관 이미지)

    public void SetPlayerInfo(string playerName, bool isHost)
    {
        playerNameText.text = playerName;  // 플레이어 이름 설정

        if (isHost)
        {
            hostIcon.SetActive(true);  // 방장인 경우 아이콘 활성화
        }
        else
        {
            hostIcon.SetActive(false); // 방장이 아니면 비활성화
        }
    }
}
