using TMPro;
using UnityEngine;

public class PlayerItem : MonoBehaviour
{
    public TMP_Text playerNameText;  // �÷��̾� �̸��� ǥ���� UI �ؽ�Ʈ
    public GameObject hostIcon;      // ���� ���θ� ǥ���� ������ (��: �հ� �̹���)

    public void SetPlayerInfo(string playerName, bool isHost)
    {
        playerNameText.text = playerName;  // �÷��̾� �̸� ����

        if (isHost)
        {
            hostIcon.SetActive(true);  // ������ ��� ������ Ȱ��ȭ
        }
        else
        {
            hostIcon.SetActive(false); // ������ �ƴϸ� ��Ȱ��ȭ
        }
    }
}
