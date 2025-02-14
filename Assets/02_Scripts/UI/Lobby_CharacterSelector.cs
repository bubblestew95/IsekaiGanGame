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

    void Start()
    {
        for (int i = 0; i < characterButtons.Length; i++)
        {
            int index = i;
            characterButtons[i].onClick.AddListener(() => RequestCharacterSelection(index));
        }
    }

    // 캐릭터 선택 요청 (클라이언트 → 서버)
    private void RequestCharacterSelection(int index)
    {
        if (isReady) return; // Ready 상태에서는 변경 불가능

        if (NetworkManager.Singleton.IsServer)
        {
            RoomManager.Instance.SelectCharacterServerRpc(NetworkManager.Singleton.LocalClientId, index);
        }
        else
        {
            RoomManager.Instance.SelectCharacterServerRpc(NetworkManager.Singleton.LocalClientId, index);
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

        if (playerSelections.ContainsKey(clientId))
        {
            Debug.Log($"[Client] Player {clientId} 기존 선택 {playerSelections[clientId]} → {selectedCharacter}");
            playerSelections[clientId] = selectedCharacter;
        }
        else
        {
            playerSelections.Add(clientId, selectedCharacter);
        }

        for (int i = 0; i < characterButtons.Length; i++)
        {
            characterButtons[i].interactable = !playerSelections.ContainsValue(i);
            Debug.Log($"[Client] 버튼 {i}: {(characterButtons[i].interactable ? "활성화" : "비활성화")}");
        }

        Debug.Log($"[Client] UI 업데이트 완료: Player {clientId} -> 캐릭터 {selectedCharacter}");

        // Ready 버튼 활성화 (캐릭터 선택 후)
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            readyButton.interactable = true;
            Debug.Log($"[Client] Ready 버튼 활성화!");
        }
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
