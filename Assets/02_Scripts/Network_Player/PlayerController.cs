using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    public float moveSpeed = 5f;


    private void Start()
    {
        NetworkObject networkObject = GetComponent<NetworkObject>();

        if (networkObject.OwnerClientId != NetworkManager.Singleton.LocalClientId)
        {
            Debug.LogWarning($"[PlayerController] 내 것이 아닌 오브젝트 감지됨. 조작 불가능: {gameObject.name}");
            enabled = false;
        }
        else
        {
            Debug.Log($"[PlayerController] 내 캐릭터 조작 가능: {gameObject.name}");
        }
    }

    private void Update()
    {
        // 내 것이 아니면 조작 불가능
        if (!IsOwner) return;

        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");

        if (moveX != 0 || moveY != 0)
        {
            RequestMoveServerRpc(new Vector2(moveX, moveY));
        }
    }


    [ServerRpc]
    private void RequestMoveServerRpc(Vector2 moveInput, ServerRpcParams rpcParams = default)
    {
        ApplyMovementClientRpc(moveInput);
    }

    [ClientRpc]
    private void ApplyMovementClientRpc(Vector2 moveInput)
    {
        if (!IsOwner) return; // 내 것이 아니면 이동 동기화 안 함
        transform.position += new Vector3(moveInput.x, 0, moveInput.y) * moveSpeed * Time.deltaTime;
    }

    [ClientRpc]
    private void EnableInputClientRpc()
    {
        if (IsOwner)
        {
            Debug.Log("입력 활성화됨!");
            this.enabled = true; // 입력 다시 활성화
        }
    }
}
