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

    // ������ ���� �� Players �迭�� ����
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

    // ���� ��Ʈ��ũ ID ����
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

    // Players ����ȭ �����ִ� �ڵ� �ʿ�
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

    // �ε��� �������� �÷��̾� ����
    private void LoadingEnd()
    {
        SetPlayersClientRpc(objectId);
        settingEndCallback?.Invoke();
    }
}