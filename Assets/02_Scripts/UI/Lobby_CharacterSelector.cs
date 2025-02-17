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
            RoomManager.Instance.DeselectCharacterServerRpc(NetworkManager.Singleton.LocalClientId);
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

    //서버에서 캐릭터 선택 처리 (서버 RPC)
    [ServerRpc(RequireOwnership = false)]
    void ToggleCharacterSelectionServerRpc(ulong clientId, int index)
    {
        ToggleCharacterSelection(clientId, index);
    }

    //서버에서 캐릭터 선택 변경 및 동기화
    void ToggleCharacterSelection(ulong clientId, int index)
    {
        if (playerSelections.ContainsKey(clientId) && playerSelections[clientId] == index)
        {
            //이미 선택한 캐릭터를 다시 클릭하면 선택 해제
            playerSelections.Remove(clientId);
            if (clientId == OwnerClientId)
                selectedCharacter.Value = -1;  // 선택 해제 시 -1로 설정
        }
        else
        {
            //기존 선택한 캐릭터 해제 후 새 캐릭터 선택
            if (playerSelections.ContainsKey(clientId))
            {
                playerSelections.Remove(clientId);
            }
            playerSelections[clientId] = index;

            if (clientId == OwnerClientId)
                selectedCharacter.Value = index;  //현재 플레이어의 선택을 selectedCharacter에 저장
        }

        // 모든 클라이언트 UI 업데이트
        UpdateCharacterSelectionClientRpc();
    }

    //클라이언트에서 UI 업데이트 (서버 -> 클라이언트 동기화)
    [ClientRpc]
    void UpdateCharacterSelectionClientRpc()
    {
        //모든 캐릭터를 기본 상태(반투명)로 초기화
        for (int i = 0; i < characterModels.Length; i++)
        {
            characterModels[i].gameObject.SetActive(true);
            characterImages[i].color = new Color(1f, 1f, 1f, 0.5f);
            characterButtons[i].interactable = true;
        }

        //본인이 선택한 캐릭터 불투명 설정
        if (selectedCharacter.Value != -1)
        {
            characterImages[selectedCharacter.Value].color = new Color(1f, 1f, 1f, 1f);
            characterButtons[selectedCharacter.Value].interactable = true;
        }

        //남이 선택한 캐릭터는 검은색 반투명 & 선택 불가
        foreach (var entry in playerSelections)
        {
            if (entry.Key != OwnerClientId)
            {
                characterImages[entry.Value].color = new Color(0f, 0f, 0f, 0.5f);
                characterButtons[entry.Value].interactable = false;
            }
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
    // 기존 선택된 캐릭터 정보를 클라이언트에게 강제 적용
    [ClientRpc]
    private void ForceUpdateCharacterSelectionClientRpc(ulong targetClientId, ulong playerId, int characterIndex)
    {
        if (NetworkManager.Singleton.LocalClientId == targetClientId)
        {
            Debug.Log($"[Client] 기존 선택 정보 수신 - Player {playerId} 캐릭터 {characterIndex}");

            if (!playerSelections.ContainsKey(playerId))
            {
                playerSelections[playerId] = characterIndex;
            }

            UpdateCharacterUI();
        }
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
