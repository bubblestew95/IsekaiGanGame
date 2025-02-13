using UnityEngine;
using UnityEngine.UI;

public class Lobby_CharacterSelector : MonoBehaviour
{
    public RawImage[] characterImages;  // 4���� ĳ���� �̸����� UI (Raw Image)
    public Transform[] characterModels; // 3D ĳ���� ��
    public Button[] characterButtons;   // UI ��ư (�� ����)

    private int selectedCharacter = -1; // ���õ� ĳ���� �ε���

    void Start()
    {
        for (int i = 0; i < characterButtons.Length; i++)
        {
            int index = i;  // Ŭ���� ���� ����
            characterButtons[i].onClick.AddListener(() => SelectCharacter(index));
        }
    }

    void SelectCharacter(int index)
    {
        selectedCharacter = index;

        for (int i = 0; i < characterModels.Length; i++)
        {
            // ���õ� ĳ���ʹ� ���� Alpha �� ���� (������)
            if (i == selectedCharacter)
            {
                //characterModels[i].gameObject.SetActive(true);
                characterImages[i].color = new Color(1f, 1f, 1f, 1f); // ���� ������ (Alpha = 1)
            }
            else
            {
                //characterModels[i].gameObject.SetActive(true);
                characterImages[i].color = new Color(1f, 1f, 1f, 0.5f); // Alpha = 0.5 (�ణ ����)
            }
        }

        Debug.Log("������ ĳ����: " + index);
    }
}
