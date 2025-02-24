using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using System.Collections.Generic;
using System.Linq;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;

public class Lobby_CharacterSelector : NetworkBehaviour
{
    public static Lobby_CharacterSelector Instance { get; private set; }

    public RawImage[] characterImages;  // ĳ���� �̸����� UI
    public Transform[] characterModels; // 3D ĳ���� ��
    public Button[] characterButtons;   // UI ��ư (�� ����)
    public Button readyButton;
    private bool isReady = false; // Ready ���� ����

    public Dictionary<int, string> characterSelections = new Dictionary<int, string>(); // <ĳ���� ��ȣ, �÷��̾� ID>


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

    // ��Ʈ��ũ ������ ������� UI ������Ʈ
    public void UpdateCharacterUI()
    {
        Debug.Log("[Lobby_CharacterSelector] UI ������Ʈ ����");

        string localPlayerId = AuthenticationService.Instance.PlayerId;
        Debug.Log($"[Lobby_CharacterSelector] localPlayerId Ȯ��: {localPlayerId}");

        int localSelectedCharacter = -1;

        // ���� �÷��̾ ������ ĳ���� ã��
        foreach (var entry in characterSelections)
        {
            Debug.Log($"[DEBUG] ���� characterSelections: {entry.Key} => {entry.Value}");

            if (entry.Value == localPlayerId)
            {
                localSelectedCharacter = entry.Key;
                Debug.Log($"[DEBUG] ������ ������ ĳ���� Ȯ�� �Ϸ�: {localSelectedCharacter}");
                break;
            }
        }
        if (!IsHost)
        {
            Debug.Log($"[ClientDEBUG] ���� characterSelections ����: {string.Join(", ", characterSelections.Select(kv => kv.Key + "=>" + kv.Value))}");
        }

        for (int i = 0; i < characterButtons.Length; i++)
        {
            if (!IsHost)
            {
                Debug.Log($"[ClientDEBUG] ���� Ȯ�� : {i}");
            }
            if (characterSelections.ContainsKey(i))
            {
                if(!IsHost)
                {
                    Debug.Log($"[ClientDEBUG]characterSelections[{i}] Ȯ�� : {characterSelections[i]}");
                }
                string ownerId = characterSelections[i];
                Debug.Log($"[Lobby_CharacterSelector] ownerId Ȯ��: {ownerId}, localPlayerId: {localPlayerId}");

                if (ownerId == localPlayerId)
                {
                    // ������ ������ ĳ���� �� ������ (1.0)
                    SetButtonState(characterButtons[i], 1.0f, false);
                    SetImageState(characterImages[i], 1.0f);
                    Debug.Log($"[Lobby_CharacterSelector] ������ ������ ĳ���� {i} �� ������ (1.0)");
                }
                else
                {
                    // �ٸ� �÷��̾ ������ ĳ���� �� ������ ������ (0.8)
                    SetButtonState(characterButtons[i], 0.8f, true);
                    SetImageState(characterImages[i], 0.8f);
                    Debug.Log($"[Lobby_CharacterSelector] �ٸ� �÷��̾ ������ ĳ���� {i} �� ������ ������ (0.8)");
                }
            }
            else
            {
                // ���õ��� ���� ĳ���� �� ������ (0.5)
                SetButtonState(characterButtons[i], 0.5f, false);
                SetImageState(characterImages[i], 0.5f);
                Debug.Log($"[Lobby_CharacterSelector] ���� ������ ĳ���� {i} �� ������ (0.5)");
            }
        }

        //if (localSelectedCharacter == -1)
        //{
        //    Debug.Log("[Lobby_CharacterSelector] ������ ���� ����, ��� ĳ���͸� �⺻ ����(0.5)�� ����");
        //    for (int i = 0; i < characterButtons.Length; i++)
        //    {
        //        SetButtonState(characterButtons[i], 1.0f, false);
        //        SetImageState(characterImages[i], 1.0f);
        //    }
        //}
    }





    public void UpdateCharacterSelection(string playerId, int selectedCharacter)
    {
        Debug.Log($"[Lobby_CharacterSelector] Player {playerId} ������ ĳ����: {selectedCharacter}");

        // ���� ���� ����
        foreach (var key in characterSelections.Keys.ToList())
        {
            if (characterSelections[key] == playerId)
            {
                characterSelections.Remove(key); // ���� ���� ����
            }
        }

        if (selectedCharacter == -1)
        {
            Debug.Log($"[Lobby_CharacterSelector] Player {playerId} ĳ���� ���� ���� (-1)");
        }
        else
        {
            // ���ο� ���� ����
            characterSelections[selectedCharacter] = playerId;
            Debug.Log($"���ο� ���� characterSelections[{selectedCharacter}]: {characterSelections[selectedCharacter]}");
        }

        // UI ���� ����
        Debug.Log("[Lobby_CharacterSelector] UI ���� ����");
        UpdateCharacterUI();
    }



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
    private void SetImageState(RawImage image, float alpha, bool isDisabled = false)
    {
        Color newColor = image.color;
        newColor.a = alpha;

        if (isDisabled)
        {
            // �ٸ� �÷��̾ ������ ĳ���� �� ������ ������ ó��
            newColor = Color.black * 0.8f;
        }

        image.color = newColor;
        Debug.Log($"[Lobby_CharacterSelector] �̹��� ���� �����: Alpha={alpha}");
    }


    public void SelectCharacter(int characterIndex)
    {
        string playerId = AuthenticationService.Instance.PlayerId;

        if (characterSelections.ContainsKey(characterIndex) && characterSelections[characterIndex] == playerId)
        {
            // ������ ������ ĳ���͸� �ٽ� ���� -> ���� ����
            RoleManager.Instance.SelectCharacter(-1);
            //characterSelections.Remove(characterIndex); // ��� �ݿ�
            Debug.Log($"[Lobby_CharacterSelector] Player {playerId} ĳ���� ���� ����");
        }
        else
        {
            // ���ο� ĳ���� ����
            RoleManager.Instance.SelectCharacter(characterIndex);
            //characterSelections[characterIndex] = playerId; // ��� �ݿ�
            Debug.Log($"[Lobby_CharacterSelector] Player {playerId}�� ĳ���� {characterIndex} ����");
        }

        // UI ������Ʈ
        UpdateCharacterUI();
    }


}
