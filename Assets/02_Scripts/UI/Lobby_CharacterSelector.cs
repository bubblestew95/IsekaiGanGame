using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using System.Collections.Generic;

public class Lobby_CharacterSelector : NetworkBehaviour
{
    public RawImage[] characterImages;  // 캐릭터 미리보기 UI
    public Transform[] characterModels; // 3D 캐릭터 모델
    public Button[] characterButtons;   // UI 버튼 (각 슬롯)
    public Button readyButton;
    private bool isReady = false; // Ready 상태 저장

    //현재 플레이어가 선택한 캐릭터 (서버에서 동기화)
    private NetworkVariable<int> selectedCharacter = new NetworkVariable<int>(-1);

    //전체 플레이어들의 선택 상태를 저장하는 딕셔너리
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

    // 캐릭터 선택 요청 (클라이언트 -> 서버)
    private void RequestCharacterSelection(int index)
    {
        if (isReady) return; // Ready 상태에서는 변경 불가능

        // 본인이 선택한 캐릭터를 다시 클릭하면 선택 해제
        if (playerSelections.ContainsKey(NetworkManager.Singleton.LocalClientId) &&
            playerSelections[NetworkManager.Singleton.LocalClientId] == index)
        {
            Debug.Log($"[Client] Player {NetworkManager.Singleton.LocalClientId} 캐릭터 선택 해제: {index}");
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
        Debug.Log($"[Server] 클라이언트 {clientId} 가 기존 선택 정보를 요청함.");

        // RoomManager 인스턴스를 찾아서 실행
        RoomManager roomManager = Object.FindFirstObjectByType<RoomManager>();
        if (roomManager != null)
        {

        }
        else
        {
            Debug.LogError("[Server] RoomManager를 찾을 수 없습니다. SyncExistingSelectionsToNewPlayer 실행 실패.");
        }
    }

    // 클라이언트 UI 업데이트 (서버에서 캐릭터 선택 동기화)
    public void UpdateCharacterSelection(ulong clientId, int selectedCharacter)
    {
        Debug.Log($"[Client] Player {clientId} 선택한 캐릭터: {selectedCharacter}");

        if (selectedCharacter == -1) // 선택 해제 시
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

        // UI 업데이트
        UpdateCharacterUI();
    }
    private void UpdateCharacterUI()
    {
        bool isSelfSelected = playerSelections.ContainsKey(NetworkManager.Singleton.LocalClientId);

        for (int i = 0; i < characterButtons.Length; i++)
        {
            // 기본 상태 (선택하지 않았을 경우 불투명)
            characterImages[i].color = new Color(1f, 1f, 1f, 1f);
            characterButtons[i].interactable = true;
        }

        // 본인이 선택한 캐릭터 처리
        if (isSelfSelected)
        {
            int selfSelected = playerSelections[NetworkManager.Singleton.LocalClientId];

            // 본인이 선택한 캐릭터는 불투명 유지
            characterImages[selfSelected].color = new Color(1f, 1f, 1f, 1f);

            // 다른 캐릭터들은 반투명 처리
            for (int i = 0; i < characterButtons.Length; i++)
            {
                if (i != selfSelected)
                {
                    characterImages[i].color = new Color(1f, 1f, 1f, 0.5f); // 반투명
                }
            }
        }

        // 남이 선택한 캐릭터 처리 (검은색 반투명)
        foreach (var entry in playerSelections)
        {
            if (entry.Key != NetworkManager.Singleton.LocalClientId) // 본인 제외
            {
                characterImages[entry.Value].color = new Color(0f, 0f, 0f, 0.8f); // 검은색 반투명
                characterButtons[entry.Value].interactable = false;
            }
        }

        Debug.Log("[Client] 캐릭터 UI 업데이트 완료!");
    }
    // Ready 상태 변경 시 UI 업데이트
    public void SetReadyState(bool ready)
    {
        isReady = ready;

        foreach (var button in characterButtons)
        {
            button.interactable = !isReady;
        }
    }
}
