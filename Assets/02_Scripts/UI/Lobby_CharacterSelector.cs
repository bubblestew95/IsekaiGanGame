using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using System.Collections.Generic;

public class Lobby_CharacterSelector : NetworkBehaviour
{
    public RawImage[] characterImages;  // ĳ���� �̸����� UI
    public Transform[] characterModels; // 3D ĳ���� ��
    public Button[] characterButtons;   // UI ��ư (�� ����)
    public Button readyButton;
    private bool isReady = false; // Ready ���� ����

    //���� �÷��̾ ������ ĳ���� (�������� ����ȭ)
    private NetworkVariable<int> selectedCharacter = new NetworkVariable<int>(-1);

    //��ü �÷��̾���� ���� ���¸� �����ϴ� ��ųʸ�
    private Dictionary<ulong, int> playerSelections = new Dictionary<ulong, int>();

    private void Start()
    {
        for (int i = 0; i < characterButtons.Length; i++)
        {
            int index = i;
            characterButtons[i].onClick.AddListener(() => RequestCharacterSelection(index));
        }
        RequestExistingSelectionsServerRpc(NetworkManager.Singleton.LocalClientId);

    }

    // ĳ���� ���� ��û (Ŭ���̾�Ʈ -> ����)
    private void RequestCharacterSelection(int index)
    {
        if (isReady) return; // Ready ���¿����� ���� �Ұ���

        // ������ ������ ĳ���͸� �ٽ� Ŭ���ϸ� ���� ����
        if (playerSelections.ContainsKey(NetworkManager.Singleton.LocalClientId) &&
            playerSelections[NetworkManager.Singleton.LocalClientId] == index)
        {
            Debug.Log($"[Client] Player {NetworkManager.Singleton.LocalClientId} ĳ���� ���� ����: {index}");
        }
        else
        {
            if (NetworkManager.Singleton.IsServer)
            {
                RoomManager.Instance.SelectCharacterServerRpc(NetworkManager.Singleton.LocalClientId, index);
            }
            else
            {
                RoomManager.Instance.SelectCharacterServerRpc(NetworkManager.Singleton.LocalClientId, index);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestExistingSelectionsServerRpc(ulong clientId)
    {
        Debug.Log($"[Server] Ŭ���̾�Ʈ {clientId} �� ���� ���� ������ ��û��.");

        // RoomManager �ν��Ͻ��� ã�Ƽ� ����
        RoomManager roomManager = Object.FindFirstObjectByType<RoomManager>();
        if (roomManager != null)
        {

        }
        else
        {
            Debug.LogError("[Server] RoomManager�� ã�� �� �����ϴ�. SyncExistingSelectionsToNewPlayer ���� ����.");
        }
    }

    // Ŭ���̾�Ʈ UI ������Ʈ (�������� ĳ���� ���� ����ȭ)
    public void UpdateCharacterSelection(ulong clientId, int selectedCharacter)
    {
        Debug.Log($"[Client] Player {clientId} ������ ĳ����: {selectedCharacter}");

        if (selectedCharacter == -1) // ���� ���� ��
        {
            if (playerSelections.ContainsKey(clientId))
            {
                playerSelections.Remove(clientId);
            }
        }
        else
        {
            playerSelections[clientId] = selectedCharacter;
        }

        // UI ������Ʈ
        UpdateCharacterUI();
    }
    private void UpdateCharacterUI()
    {
        bool isSelfSelected = playerSelections.ContainsKey(NetworkManager.Singleton.LocalClientId);

        for (int i = 0; i < characterButtons.Length; i++)
        {
            // �⺻ ���� (�������� �ʾ��� ��� ������)
            characterImages[i].color = new Color(1f, 1f, 1f, 1f);
            characterButtons[i].interactable = true;
        }

        // ������ ������ ĳ���� ó��
        if (isSelfSelected)
        {
            int selfSelected = playerSelections[NetworkManager.Singleton.LocalClientId];

            // ������ ������ ĳ���ʹ� ������ ����
            characterImages[selfSelected].color = new Color(1f, 1f, 1f, 1f);

            // �ٸ� ĳ���͵��� ������ ó��
            for (int i = 0; i < characterButtons.Length; i++)
            {
                if (i != selfSelected)
                {
                    characterImages[i].color = new Color(1f, 1f, 1f, 0.5f); // ������
                }
            }
        }

        // ���� ������ ĳ���� ó�� (������ ������)
        foreach (var entry in playerSelections)
        {
            if (entry.Key != NetworkManager.Singleton.LocalClientId) // ���� ����
            {
                characterImages[entry.Value].color = new Color(0f, 0f, 0f, 0.8f); // ������ ������
                characterButtons[entry.Value].interactable = false;
            }
        }

        Debug.Log("[Client] ĳ���� UI ������Ʈ �Ϸ�!");
    }
    // Ready ���� ���� �� UI ������Ʈ
    public void SetReadyState(bool ready)
    {
        isReady = ready;

        foreach (var button in characterButtons)
        {
            button.interactable = !isReady;
        }
    }
}
