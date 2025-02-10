using System;
using Unity.Multiplayer.Center.NetcodeForGameObjectsExample;
using Unity.Netcode;
using UnityEngine;

public class TestNetwork : NetworkBehaviour
{
    public static event Action settingEndCallback;

    public GameObject[] Characters = new GameObject[4];
    public Transform[] SpwanTr = new Transform[4];
    public GameObject[] Players = new GameObject[2];

    private ulong[] objectId;

    private void Start()
    {
        if (IsServer)
        {
            SpawnPlayerControlledObjects();
            // GameManager.Instance.loadingFinishCallback += LoadingEnd;
        }

        
    }

    // 프리펩 생성 및 Players 배열에 저장
    private void SpawnPlayerControlledObjects()
    {
        int cnt = 0;

        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            GameObject playerObject = Characters[cnt];

            Players[cnt] = Instantiate(playerObject, SpwanTr[cnt].position, Quaternion.identity);

            Players[cnt].GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);

            cnt++;
        }

        objectId = GetNetworkId(cnt);

        SetPlayersClientRpc(objectId);
    }

    // 생성 네트워크 ID 리턴
    private ulong[] GetNetworkId(int _cnt)
    {
        ulong[] objectIds = new ulong[_cnt];

        _cnt = 0;

        foreach (GameObject player in Players)
        {
            objectId[_cnt++] = player.GetComponent<NetworkObject>().NetworkObjectId;
        }

        return objectIds;
    }

    // Players 동기화 시켜주는 코드 필요
    [ClientRpc]
    private void SetPlayersClientRpc(ulong[] _networkObjectIds)
    {
        int cnt = 0;

        foreach (ulong clientId in _networkObjectIds)
        {
            Players[cnt] = NetworkManager.Singleton.SpawnManager.SpawnedObjects[_networkObjectIds[cnt]].gameObject;

            Debug.Log("SetPlayer : " + Players[cnt]);

            cnt++;
        }
    }

    // 로딩이 끝났을때 플레이어 설정
    private void LoadingEnd()
    {
        SetPlayersClientRpc(objectId);
        settingEndCallback?.Invoke();
    }
}