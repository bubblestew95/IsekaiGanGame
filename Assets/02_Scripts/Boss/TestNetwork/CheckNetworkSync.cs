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
            Debug.Log("동기화 완료!");
            LoadingFinishClientRpc();
        }
    }

    [ClientRpc]
    private void LoadingFinishClientRpc()
    {
        // 서버에서 클라이언트들로 전달된 콜백을 실행
        loadingFinishCallback?.Invoke();
    }
}
