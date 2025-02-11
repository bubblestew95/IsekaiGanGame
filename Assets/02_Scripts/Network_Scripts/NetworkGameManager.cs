using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkGameManager : NetworkBehaviour
{
    public event Action loadingFinishCallback;

    /// <summary>
    /// 클라이언트 ID를 키로 가지고, 해당 클라이언트가 소유한 PlayerManger를 값으로 가지는 딕셔너리.
    /// </summary>
    private Dictionary<ulong, PlayerManager> multiPlayersMap = null;

    // 플레이어 로딩 동기화 관련 변수들
    private int playerCnt = 0;
    private int loadingCnt = 0;
    private bool spawnPlayer = false;
    private bool loadingScene = false;

    // 플레이어 생성 관련 변수들
    [SerializeField] private GameObject[] prefabs = new GameObject[4];
    [SerializeField] private Transform[] spwanTr = new Transform[4];
    public GameObject[] players = new GameObject[4];
    private ulong[] objectId = new ulong[4];

    public GameObject[] Players { get { return players; } }

    #region Public Functions

    public void OnPlayerDamaged
        (PlayerManager _damageReceiver, int _damage, Vector3 _attackPos, float _knockbackDist)
    {
        if(_damageReceiver.GetComponent<NetworkObject>().IsOwner)
        {
            ulong cliendId = _damageReceiver.GetComponent<NetworkObject>().OwnerClientId;
            PlayerDamagedRpc(cliendId, _damage, _attackPos, _knockbackDist);
        }
    }
    
    public void OnBossDamaged
        (PlayerManager _damageGiver, int _damage, float _aggro)
    {
        ulong cliendId = _damageGiver.GetComponent<NetworkObject>().OwnerClientId;
        BossDamagedRpc(cliendId, _damage, _aggro);
    }

    #endregion

    #region [Private Funtions]

    // 프리펩 생성 및 Players 배열에 저장
    private void SpawnPlayerControlledObjects()
    {
        int cnt = 0;

        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            GameObject playerObject = prefabs[cnt];
            
            // GameObject를 역할에 맞게 설정
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

    // 생성 네트워크 ID 리턴
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
    /// 플레이어가 데미지를 받았음을 서버에게 알린다.
    /// </summary>
    /// <param name="_cliendId"></param>
    /// <param name="_damage"></param>
    /// <param name="_attackPos"></param>
    /// <param name="_knockbackDist"></param>
    [Rpc(SendTo.Server)]
    private void PlayerDamagedRpc
        (ulong _cliendId, int _damage, Vector3 _attackPos, float _knockbackDist)
    {
        // 서버에서 다른 클라이언트들에게 특정 플레이어에게 데미지를 적용하라고 명령한다.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           
        ApplyDamageToPlayerRpc(_cliendId, _damage, _attackPos, _knockbackDist);
    }

    /// <summary>
    /// 보스가 데미지를 받았음을 서버에게 알린다.
    /// </summary>
    /// <param name="_cliendId"></param>
    /// <param name="_damage"></param>
    /// <param name="_attackPos"></param>
    /// <param name="_knockbackDist"></param>
    [Rpc(SendTo.Server)]
    private void BossDamagedRpc
        (ulong _cliendId, int _damage, float _aggro)
    {
        // 다른 클라이언트들에게 보스에게 데미지를 입히라고 명령한다.
        ApplyDamageToBossRpc(_cliendId, _damage, _aggro);
    }

        #endregion

        #region Server To Client RPC

    /// <summary>
    /// 서버에서 특정 플레이어에게 데미지를 적용하도록 모든 클라이언트에게 명령한다.
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

    /// <summary>
    /// 서버에서 보스에게 데미지를 적용하도록 모든 클라이언트에게 명령한다.
    /// </summary>
    /// <param name="_cliendId"></param>
    /// <param name="_damage"></param>
    /// <param name="_attackPos"></param>
    /// <param name="_knockbackDist"></param>
    [Rpc(SendTo.Everyone)]
    private void ApplyDamageToBossRpc
        (ulong _cliendId, int _damage, float _aggro)
    {
        var obj = NetworkManager.ConnectedClients[_cliendId].PlayerObject;

        if (obj != null)
        {
            // GameManager.Instance.ApplyDamageToBoss(playerManager, _damage, _aggro)
        }
    }

        #endregion

    #endregion

    #region Unity Callbacks

    private void Awake()
    {
        multiPlayersMap = new Dictionary<ulong, PlayerManager>(); 
    }

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

    // 플레이어 전부 네트워크 상에서 스폰됬는지 확인하는 함수
    [ServerRpc(RequireOwnership = false)]
    public void CheckPlayerSpawnServerRpc()
    {
        playerCnt++;

        if (playerCnt == NetworkManager.Singleton.ConnectedClients.Count)
        {
            spawnPlayer = true;
        }
    }

    // 플레이어 전부 네트워크 상에서 씬로딩이 됬는지 확인하는 함수
    [ServerRpc(RequireOwnership = false)]
    private void LoadingCheckServerRpc()
    {
        loadingCnt++;

        if (loadingCnt == NetworkManager.Singleton.ConnectedClients.Count)
        {
            loadingScene = true;
        }
    }

    // 로딩이 끝나면 모든 클라이언트에 실행이 끝났다고 콜백이 됨.
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
