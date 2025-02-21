using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using System.Collections.Generic;
using System.Linq;

public class Lobby_CharacterSelector : NetworkBehaviour
{
    public static Lobby_CharacterSelector Instance { get; private set; }

    public RawImage[] characterImages;  // 캐릭터 미리보기 UI
    public Transform[] characterModels; // 3D 캐릭터 모델
    public List<Button> characterButtons;   // UI 버튼 (각 슬롯)
    public Button readyButton;
    private bool isReady = false; // Ready 상태 저장

    private Dictionary<int, string> characterSelections = new Dictionary<int, string>(); // <캐릭터 번호, 플레이어 ID>


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        UpdateCharacterUI();
    }

    // 네트워크 데이터 기반으로 UI 업데이트
    public void UpdateCharacterUI()
    {
        Debug.Log("[Lobby_CharacterSelector] UI 업데이트 실행");
        string playerId = Unity.Services.Authentication.AuthenticationService.Instance.PlayerId;

        for (int i = 0; i < characterButtons.Count; i++)
        {
            Button button = characterButtons[i];
            string ownerId = characterSelections.ContainsKey(i) ? characterSelections[i] : null;

            if (ownerId == playerId)
            {
                // 본인이 선택한 캐릭터 (불투명 1.0, 선택 가능)
                SetButtonState(button, 1.0f, false);
                Debug.Log($"[Lobby_CharacterSelector] Player {playerId}가 선택한 캐릭터 {i} → 불투명 (1.0) / 선택 가능");
            }
            else if (ownerId == null)
            {
                // 아무도 선택하지 않은 캐릭터
                if (RoleManager.Instance.SelectedCharacter == "-1")
                {
                    // 본인이 선택하지 않은 경우 (불투명 1.0, 선택 가능)
                    SetButtonState(button, 1.0f, false);
                }
                else
                {
                    // 본인이 다른 캐릭터를 선택한 경우 (반투명 0.5, 선택 가능)
                    SetButtonState(button, 0.5f, false);
                }
                Debug.Log($"[Lobby_CharacterSelector] 선택 가능한 캐릭터 {i} → {(RoleManager.Instance.SelectedCharacter == "-1" ? "불투명 (1.0)" : "반투명 (0.5)")} / 선택 가능");
            }
            else
            {
                // 다른 플레이어가 선택한 캐릭터 (검은색 반투명 0.8, 선택 불가능)
                SetButtonState(button, 0.8f, true);
                Debug.Log($"[Lobby_CharacterSelector] Player {ownerId}가 선택한 캐릭터 {i} → 검은색 반투명 (0.8) / 선택 불가");
            }
        }
    }

    // 클라이언트에서 UI를 업데이트하는 메서드 추가
    public void UpdateCharacterSelection(ulong playerId, int selectedCharacter)
    {
        string playerIdStr = playerId.ToString();
        Debug.Log($"[Lobby_CharacterSelector] Player {playerIdStr} 선택한 캐릭터: {selectedCharacter}");

        // 기존 선택 해제
        foreach (var key in characterSelections.Keys.ToList())
        {
            if (characterSelections[key] == playerIdStr)
            {
                characterSelections.Remove(key);
            }
        }

        // 새로운 선택 저장
        characterSelections[selectedCharacter] = playerIdStr;

        // UI 갱신
        UpdateCharacterUI();
        Debug.Log($"[Lobby_CharacterSelector] UI 업데이트 완료 - 현재 선택 상태: {characterSelections.Count}");
    }


    // 버튼 상태 설정
    // 버튼 상태 설정
    private void SetButtonState(Button button, float alpha, bool disable = false)
    {
        ColorBlock colors = button.colors;
        Color newColor = colors.normalColor;
        newColor.a = alpha;

        if (disable)
        {
            // 다른 플레이어가 선택한 캐릭터 -> 검은색 반투명 (선택 불가능)
            newColor = Color.black * 0.8f;
        }

        colors.normalColor = newColor;
        button.colors = colors;

        // 검은색 반투명(선택 불가능)일 때만 interactable = false
        button.interactable = !disable;

        Debug.Log($"[Lobby_CharacterSelector] 버튼 상태 변경됨: Alpha={alpha}, Interactable={button.interactable}");
    }


    public void SelectCharacter(int characterIndex)
    {
        string playerId = Unity.Services.Authentication.AuthenticationService.Instance.PlayerId;

        if (characterSelections.ContainsKey(characterIndex) && characterSelections[characterIndex] == playerId)
        {
            // 본인이 선택한 캐릭터를 다시 선택 → 선택 해제
            RoleManager.Instance.SelectCharacter(-1);
            Debug.Log($"[Lobby_CharacterSelector] Player {playerId} 캐릭터 선택 해제");
        }
        else
        {
            // 새로운 캐릭터 선택
            RoleManager.Instance.SelectCharacter(characterIndex);
            Debug.Log($"[Lobby_CharacterSelector] Player {playerId}가 캐릭터 {characterIndex} 선택");
        }

        // UI 업데이트
        UpdateCharacterUI();
    }


}
