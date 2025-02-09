using Unity.Netcode;
using UnityEngine;
using System;

public class CheckNetworkSync : NetworkBehaviour
{
    public static event Action loadingFinishCallback;
    private int loadingCnt = 0;

    private void Start()
    {
        LoadingCheckServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void LoadingCheckServerRpc()
    {
        loadingCnt++;
        CheckPlayerNum();
    }

    private void CheckPlayerNum()
    {
        if (loadingCnt == NetworkManager.Singleton.ConnectedClients.Count)
        {
            Debug.Log("����ȭ �Ϸ�!");
            LoadingFinishClientRpc();
        }
    }

    [ClientRpc]
    private void LoadingFinishClientRpc()
    {
        // �������� Ŭ���̾�Ʈ��� ���޵� �ݹ��� ����
        loadingFinishCallback?.Invoke();
    }
}
