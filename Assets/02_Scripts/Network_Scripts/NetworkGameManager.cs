using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class NetworkGameManager : NetworkBehaviour
{
    public event Action loadingFinishCallback;
    public event UnityAction<ulong> playerDieCallback;

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

    // 사망 관련 변수
    private int playerDieCnt = 0;

    public GameObject[] Players { get { return players; } }

    #region Public Functions

    /// <summary>
    /// 플레이어가 데미지를 받았음을 서버에게 알려주는 RPC를 호출한다.
    /// </summary>
    /// <param name="_damageReceiver"></param>
    /// <param name="_damage"></param>
    /// <param name="_attackPos"></param>
    /// <param name="_knockbackDist"></param>
    public void OnPlayerDamaged
        (PlayerManager _damageReceiver, int _damage)
    {
        ulong clientId = _damageReceiver.PlayerNetworkManager.OwnerClientId;

        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            PlayerDamagedRpc(clientId, _damage);
        }
    }

    /// <summary>
    /// 플레이어가 넉백 효과를 받았음을 서버에게 알려주는 RPC를 호출한다.
    /// </summary>
    /// <param name="_damageReceiver"></param>
    /// <param name="_damage"></param>
    /// <param name="_attackPos"></param>
    /// <param name="_knockbackDist"></param>
    public void OnPlayerKnockback
        (PlayerManager _target, Vector3 _attackPos, float _knockbackDist)
    {
        ulong clientId = _target.PlayerNetworkManager.OwnerClientId;

        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            PlayerKnockbackRpc(clientId, _attackPos, _knockbackDist);
        }
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

            players[cnt].GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);

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

    // 플레이어 죽었을때 호출
    private void PlayerDie(ulong _clientId)
    {
        playerDieCallback?.Invoke(_clientId);
        playerDieCnt++;

        // 플레이어 전부 사망시 씬넘김
        if (playerDieCnt == NetworkManager.Singleton.ConnectedClients.Count)
        {
            if (IsServer)
            {
                NetworkManager.Singleton.SceneManager.LoadScene("LobbyTest", LoadSceneMode.Single);
            }
        }
    }

    #endregion

    #region RPC

        #region Client To Server RPC

    /// <summary>
    /// 플레이어가 데미지를 받았음을 서버에게 알린다.
    /// </summary>
    /// <param name="_clientId"></param>
    /// <param name="_damage"></param>
    /// <param name="_attackPos"></param>
    /// <param name="_knockbackDist"></param>
    [Rpc(SendTo.Server)]
    private void PlayerDamagedRpc
        (ulong _clientId, int _damage)
    {
        // 서버에서 다른 클라이언트들에게 특정 플레이어에게 데미지를 적용하라고 명령한다.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           
        ApplyDamageToPlayerRpc(_clientId, _damage);
    }

    /// <summary>
    /// 플레이어가 넉백상태임을 서버에게 알린다.
    /// </summary>
    /// <param name="_clientId"></param>
    /// <param name="_attackPos"></param>
    /// <param name="_knockbackDist"></param>
    [Rpc(SendTo.Server)]
    private void PlayerKnockbackRpc(ulong _clientId, Vector3 _attackPos, float _knockbackDist)
    {
        ApplyKnockbackRpc(_clientId, _attackPos, _knockbackDist);
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
    private void ApplyDamageToPlayerRpc(ulong _cliendId, int _damage)
    {
        var obj = NetworkManager.ConnectedClients[_cliendId].PlayerObject;

        if(obj != null)
        {
            GameManager.Instance.ApplyDamageToPlayer(obj.GetComponent<PlayerManager>(), _damage);
        }

    }

    /// <summary>
    /// 서버에서 특정 플레이어에게 넉백 효과를 부여하도록 모든 클라이언트에게 명령한다.
    /// </summary>
    /// <param name="_cliendId"></param>
    /// <param name="_damage"></param>
    /// <param name="_attackPos"></param>
    /// <param name="_knockbackDist"></param>
    [Rpc(SendTo.Everyone)]
    private void ApplyKnockbackRpc(ulong _cliendId, Vector3 _attackPos, float _knockbackDist)
    {
        var obj = NetworkManager.ConnectedClients[_cliendId].PlayerObject;

        if (obj != null)
        {
            GameManager.Instance.ApplyKnockbackToPlayer
                (obj.GetComponent<PlayerManager>(), _attackPos, _knockbackDist);
        }
    }

        #endregion

    #endregion

    #region Unity Callbacks

    private void Start()
    {
        if (GameManager.Instance.IsLocalGame)
            return;

       LoadingCheckServerRpc();

        if (IsServer)
        {
            SpawnPlayerControlledObjects();
        }
    }

    private void Update()
    {
        if (GameManager.Instance.IsLocalGame)
            return;

        if (spawnPlayer && loadingScene)
        {
            SynPlayerClientRpc(objectId);
            LoadingFinishClientRpc();
            SetPlayerDieCallback();
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

    // 플레이어 사망 콜백 등록
    private void SetPlayerDieCallback()
    {
        for (int i = 0; i < 4; i++)
        {
            if (players[i] == null) continue;

            players[i].GetComponent<PlayerNetworkManager>().OnNetworkPlayerDeath += PlayerDie;
        }
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
