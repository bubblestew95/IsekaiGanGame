using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using System.Collections.Generic;
using System.Linq;

public class Lobby_CharacterSelector : NetworkBehaviour
{
    public static Lobby_CharacterSelector Instance { get; private set; }

    public RawImage[] characterImages;  // ĳ���� �̸����� UI
    public Transform[] characterModels; // 3D ĳ���� ��
    public List<Button> characterButtons;   // UI ��ư (�� ����)
    public Button readyButton;
    private bool isReady = false; // Ready ���� ����

    private Dictionary<int, string> characterSelections = new Dictionary<int, string>(); // <ĳ���� ��ȣ, �÷��̾� ID>


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

    // ��Ʈ��ũ ������ ������� UI ������Ʈ
    public void UpdateCharacterUI()
    {
        Debug.Log("[Lobby_CharacterSelector] UI ������Ʈ ����");
        string playerId = Unity.Services.Authentication.AuthenticationService.Instance.PlayerId;

        for (int i = 0; i < characterButtons.Count; i++)
        {
            Button button = characterButtons[i];
            string ownerId = characterSelections.ContainsKey(i) ? characterSelections[i] : null;

            if (ownerId == playerId)
            {
                // ������ ������ ĳ���� (������ 1.0, ���� ����)
                SetButtonState(button, 1.0f, false);
                Debug.Log($"[Lobby_CharacterSelector] Player {playerId}�� ������ ĳ���� {i} �� ������ (1.0) / ���� ����");
            }
            else if (ownerId == null)
            {
                // �ƹ��� �������� ���� ĳ����
                if (RoleManager.Instance.SelectedCharacter == "-1")
                {
                    // ������ �������� ���� ��� (������ 1.0, ���� ����)
                    SetButtonState(button, 1.0f, false);
                }
                else
                {
                    // ������ �ٸ� ĳ���͸� ������ ��� (������ 0.5, ���� ����)
                    SetButtonState(button, 0.5f, false);
                }
                Debug.Log($"[Lobby_CharacterSelector] ���� ������ ĳ���� {i} �� {(RoleManager.Instance.SelectedCharacter == "-1" ? "������ (1.0)" : "������ (0.5)")} / ���� ����");
            }
            else
            {
                // �ٸ� �÷��̾ ������ ĳ���� (������ ������ 0.8, ���� �Ұ���)
                SetButtonState(button, 0.8f, true);
                Debug.Log($"[Lobby_CharacterSelector] Player {ownerId}�� ������ ĳ���� {i} �� ������ ������ (0.8) / ���� �Ұ�");
            }
        }
    }

    // Ŭ���̾�Ʈ���� UI�� ������Ʈ�ϴ� �޼��� �߰�
    public void UpdateCharacterSelection(ulong playerId, int selectedCharacter)
    {
        string playerIdStr = playerId.ToString();
        Debug.Log($"[Lobby_CharacterSelector] Player {playerIdStr} ������ ĳ����: {selectedCharacter}");

        // ���� ���� ����
        foreach (var key in characterSelections.Keys.ToList())
        {
            if (characterSelections[key] == playerIdStr)
            {
                characterSelections.Remove(key);
            }
        }

        // ���ο� ���� ����
        characterSelections[selectedCharacter] = playerIdStr;

        // UI ����
        UpdateCharacterUI();
        Debug.Log($"[Lobby_CharacterSelector] UI ������Ʈ �Ϸ� - ���� ���� ����: {characterSelections.Count}");
    }


    // ��ư ���� ����
    // ��ư ���� ����
    private void SetButtonState(Button button, float alpha, bool disable = false)
    {
        ColorBlock colors = button.colors;
        Color newColor = colors.normalColor;
        newColor.a = alpha;

        if (disable)
        {
            // �ٸ� �÷��̾ ������ ĳ���� -> ������ ������ (���� �Ұ���)
            newColor = Color.black * 0.8f;
        }

        colors.normalColor = newColor;
        button.colors = colors;

        // ������ ������(���� �Ұ���)�� ���� interactable = false
        button.interactable = !disable;

        Debug.Log($"[Lobby_CharacterSelector] ��ư ���� �����: Alpha={alpha}, Interactable={button.interactable}");
    }


    public void SelectCharacter(int characterIndex)
    {
        string playerId = Unity.Services.Authentication.AuthenticationService.Instance.PlayerId;

        if (characterSelections.ContainsKey(characterIndex) && characterSelections[characterIndex] == playerId)
        {
            // ������ ������ ĳ���͸� �ٽ� ���� �� ���� ����
            RoleManager.Instance.SelectCharacter(-1);
            Debug.Log($"[Lobby_CharacterSelector] Player {playerId} ĳ���� ���� ����");
        }
        else
        {
            // ���ο� ĳ���� ����
            RoleManager.Instance.SelectCharacter(characterIndex);
            Debug.Log($"[Lobby_CharacterSelector] Player {playerId}�� ĳ���� {characterIndex} ����");
        }

        // UI ������Ʈ
        UpdateCharacterUI();
    }


}
