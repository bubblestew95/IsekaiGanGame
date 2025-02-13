using UnityEngine;
using UnityEngine.UI;

public class Lobby_CharacterSelector : MonoBehaviour
{
    public RawImage[] characterImages;  // 4개의 캐릭터 미리보기 UI (Raw Image)
    public Transform[] characterModels; // 3D 캐릭터 모델
    public Button[] characterButtons;   // UI 버튼 (각 슬롯)

    private int selectedCharacter = -1; // 선택된 캐릭터 인덱스

    void Start()
    {
        for (int i = 0; i < characterButtons.Length; i++)
        {
            int index = i;  // 클로저 문제 방지
            characterButtons[i].onClick.AddListener(() => ToggleCharacterSelection(index));
        }
    }

    void ToggleCharacterSelection(int index)
    {
        // 이미 선택한 캐릭터를 다시 클릭하면 선택 해제
        if (selectedCharacter == index)
        {
            DeselectAllCharacters();
            selectedCharacter = -1;
        }
        else
        {
            SelectCharacter(index);
        }
    }


    void SelectCharacter(int index)
    {
        selectedCharacter = index;

        for (int i = 0; i < characterModels.Length; i++)
        {
            // 선택된 캐릭터는 원래 Alpha 값 유지 (불투명)
            if (i == selectedCharacter)
            {
                //characterModels[i].gameObject.SetActive(true);
                characterImages[i].color = new Color(1f, 1f, 1f, 1f); // 완전 불투명 (Alpha = 1)
            }
            else
            {
                //characterModels[i].gameObject.SetActive(true);
                characterImages[i].color = new Color(1f, 1f, 1f, 0.5f); // Alpha = 0.5 (약간 투명)
            }
        }

        Debug.Log("선택한 캐릭터: " + index);
    }
    void DeselectAllCharacters()
    {
        for (int i = 0; i < characterModels.Length; i++)
        {
            characterModels[i].gameObject.SetActive(true);
            characterImages[i].color = new Color(1f, 1f, 1f, 1f); // 모든 캐릭터 원래 색상으로 복귀
        }

        Debug.Log("선택 해제됨!");
    }
}
