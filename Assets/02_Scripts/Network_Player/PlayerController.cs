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
            Debug.LogWarning($"[PlayerController] �� ���� �ƴ� ������Ʈ ������. ���� �Ұ���: {gameObject.name}");
            enabled = false;
        }
        else
        {
            Debug.Log($"[PlayerController] �� ĳ���� ���� ����: {gameObject.name}");
        }
    }

    private void Update()
    {
        // �� ���� �ƴϸ� ���� �Ұ���
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
        if (!IsOwner) return; // �� ���� �ƴϸ� �̵� ����ȭ �� ��
        transform.position += new Vector3(moveInput.x, 0, moveInput.y) * moveSpeed * Time.deltaTime;
    }

    [ClientRpc]
    private void EnableInputClientRpc()
    {
        if (IsOwner)
        {
            Debug.Log("�Է� Ȱ��ȭ��!");
            this.enabled = true; // �Է� �ٽ� Ȱ��ȭ
        }
    }
}
