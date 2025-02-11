using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkGameManager : NetworkBehaviour
{
    public event Action loadingFinishCallback;

    // �÷��̾� �ε� ����ȭ ���� ������
    private int playerCnt = 0;
    private int loadingCnt = 0;
    private bool spawnPlayer = false;
    private bool loadingScene = false;

    // �÷��̾� ���� ���� ������
    [SerializeField] private GameObject[] prefabs = new GameObject[4];
    [SerializeField] private Transform[] spwanTr = new Transform[4];
    public GameObject[] players = new GameObject[4];
    private ulong[] objectId = new ulong[4];

    public GameObject[] Players { get { return players; } }

    #region Public Functions

    /// <summary>
    /// �÷��̾ �������� �޾����� �������� �˷��ִ� RPC�� ȣ���Ѵ�.
    /// </summary>
    /// <param name="_damageReceiver"></param>
    /// <param name="_damage"></param>
    /// <param name="_attackPos"></param>
    /// <param name="_knockbackDist"></param>
    public void OnPlayerDamaged
        (PlayerManager _damageReceiver, int _damage, Vector3 _attackPos, float _knockbackDist)
    {
        ulong clientId = _damageReceiver.PlayerNetworkManager.OwnerClientId;

        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            PlayerDamagedRpc(clientId, _damage, _attackPos, _knockbackDist);
        }
    }

    #endregion

    #region [Private Funtions]

    // ������ ���� �� Players �迭�� ����
    private void SpawnPlayerControlledObjects()
    {
        int cnt = 0;

        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            GameObject playerObject = prefabs[cnt];
            
            // GameObject�� ���ҿ� �°� ����
            //string role = RoleManager.Instance.GetPlayerRole(clientId);
            //foreach (GameObject prefab in prefabs)
            //{
            //    if (prefab.name == role) playerObject = prefab;
            //}


            players[cnt] = Instantiate(playerObject, spwanTr[cnt].position, Quaternion.identity);

            players[cnt].GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);

            cnt++;
        }

        objectId = GetNetworkId(cnt);
    }

    // ���� ��Ʈ��ũ ID ����
    private ulong[] GetNetworkId(int _cnt)
    {
        ulong[] objectIds = new ulong[_cnt];

        _cnt = 0;

        foreach (GameObject player in players)
        {
            if (player == null) continue;

            objectIds[_cnt++] = player.GetComponent<NetworkObject>().NetworkObjectId;
            
        }

        return objectIds;
    }

    #endregion

    #region RPC

        #region Client To Server RPC

    /// <summary>
    /// �÷��̾ �������� �޾����� �������� �˸���.
    /// </summary>
    /// <param name="_cliendId"></param>
    /// <param name="_damage"></param>
    /// <param name="_attackPos"></param>
    /// <param name="_knockbackDist"></param>
    [Rpc(SendTo.Server)]
    private void PlayerDamagedRpc
        (ulong _cliendId, int _damage, Vector3 _attackPos, float _knockbackDist)
    {
        // �������� �ٸ� Ŭ���̾�Ʈ�鿡�� Ư�� �÷��̾�� �������� �����϶�� ����Ѵ�.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           
        ApplyDamageToPlayerRpc(_cliendId, _damage, _attackPos, _knockbackDist);
    }


        #endregion

        #region Server To Client RPC

    /// <summary>
    /// �������� Ư�� �÷��̾�� �������� �����ϵ��� ��� Ŭ���̾�Ʈ���� ����Ѵ�.
    /// </summary>
    /// <param name="_cliendId"></param>
    /// <param name="_damage"></param>
    /// <param name="_attackPos"></param>
    /// <param name="_knockbackDist"></param>
    [Rpc(SendTo.Everyone)]
    private void ApplyDamageToPlayerRpc
        (ulong _cliendId, int _damage, Vector3 _attackPos, float _knockbackDist)
    {
        var obj = NetworkManager.ConnectedClients[_cliendId].PlayerObject;

        if(obj != null)
        {
            GameManager.Instance.ApplyDamageToPlayer(
                obj.GetComponent<PlayerManager>(),
                _damage,
                _attackPos,
                _knockbackDist);
        }

    }
        #endregion

    #endregion

    #region Unity Callbacks

    private void Start()
    {
       LoadingCheckServerRpc();

        if (IsServer)
        {
            SpawnPlayerControlledObjects();
        }
    }

    private void Update()
    {
        if (spawnPlayer && loadingScene)
        {
            SynPlayerClientRpc(objectId);
            LoadingFinishClientRpc();
            spawnPlayer = false;
            loadingScene = false;
        }
    }

    #endregion

    #region [ServerStart]

    // �÷��̾� ���� ��Ʈ��ũ �󿡼� ��������� Ȯ���ϴ� �Լ�
    [ServerRpc(RequireOwnership = false)]
    public void CheckPlayerSpawnServerRpc()
    {
        playerCnt++;

        if (playerCnt == NetworkManager.Singleton.ConnectedClients.Count)
        {
            spawnPlayer = true;
        }
    }

    // �÷��̾� ���� ��Ʈ��ũ �󿡼� ���ε��� ����� Ȯ���ϴ� �Լ�
    [ServerRpc(RequireOwnership = false)]
    private void LoadingCheckServerRpc()
    {
        loadingCnt++;

        if (loadingCnt == NetworkManager.Singleton.ConnectedClients.Count)
        {
            loadingScene = true;
        }
    }

    // �ε��� ������ ��� Ŭ���̾�Ʈ�� ������ �����ٰ� �ݹ��� ��.
    [ClientRpc]
    private void LoadingFinishClientRpc()
    {
        loadingFinishCallback?.Invoke();
    }

    #endregion

    #region [ClientRpc]

    [ClientRpc]
    private void SynPlayerClientRpc(ulong[] _networkObjectIds)
    {
        int cnt = 0;

        foreach (ulong clientId in _networkObjectIds)
        {
            players[cnt] = NetworkManager.Singleton.SpawnManager.SpawnedObjects[_networkObjectIds[cnt]].gameObject;

            Debug.Log("SetPlayer : " + players[cnt]);

            cnt++;
        }
    }
    #endregion
}
