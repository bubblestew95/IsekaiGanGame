using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using EnumTypes;

public class NetworkGameManager : NetworkBehaviour
{
    public event Action loadingFinishCallback;
    public event UnityAction<ulong> playerDieCallback;

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

    // ��� ���� ����
    private int playerDieCnt = 0;

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
        (PlayerManager _damageReceiver, int _damage)
    {
        ulong clientId = _damageReceiver.PlayerNetworkManager.OwnerClientId;

        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            PlayerDamagedServerRpc(clientId, _damage);
        }
    }

    /// <summary>
    /// �÷��̾ �˹� ȿ���� �޾����� �������� �˷��ִ� RPC�� ȣ���Ѵ�.
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
            PlayerKnockbackServerRpc(clientId, _attackPos, _knockbackDist);
        }
    }

    public void OnPlayerDeath(ulong _clientId)
    {
        PlayerDeathServerRpc(_clientId);
    }

    #endregion

    #region [Private Funtions]

    // ������ ���� �� Players �迭�� ����
    private void SpawnPlayerControlledObjects()
    {
        int cnt = 0;

        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            // �ӽÿ� ������ ����
            GameObject playerObject = prefabs[cnt];
            
            // GameObject�� ���ҿ� �°� ����
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

    // �÷��̾� ���� �׾����� Check => �� �׾��ٸ� �й� �ڷ�ƾ ����
    private void CheckPlayerAllDie()
    {
        // �÷��̾� ���� ����� ���ѱ�
        if (playerDieCnt == NetworkManager.Singleton.ConnectedClients.Count)
        {
            if (IsServer)
            {
                FailClientRpc();
            }
        }
    }

    // ���� ������ ����Ǵ� �ڷ�ƾ
    private IEnumerator FailCoroutine()
    {
        yield return new WaitForSeconds(3f);

        FindAnyObjectByType<UIBattleUIManager>().FadeInResult(false);
        FindAnyObjectByType<BgmController>().PlayDefeat();

        // ���� �ִ� ��ư�� "GameResultFail" ������Ʈ�� �ڽ����� �ִ� ResultButton�� Ŭ��������"
        Button failBtn = GameObject.Find("GameResultFail").transform.Find("ResultButton").gameObject.GetComponent<Button>();
        failBtn.onClick.AddListener(ClickFailBtn);

        yield return new WaitForSeconds(15f);

        ClickFailBtn();
    }

    // ���� Ŭ����� ����Ǵ� �ڷ�ƾ
    private IEnumerator VictoryCoroutine()
    {
        yield return new WaitForSeconds(3f);

        FindAnyObjectByType<UIBattleUIManager>().FadeInResult(true);
        FindAnyObjectByType<BgmController>().PlayVictory();

        // ���� �ִ� ��ư�� "GameResultSuccess" ������Ʈ�� �ڽ����� �ִ� ResultButton�� Ŭ��������"
        Button successBtn = GameObject.Find("GameResultSuccess").transform.Find("ResultButton").gameObject.GetComponent<Button>();
        successBtn.onClick.AddListener(ClickSucessBtn);

        yield return new WaitForSeconds(15f);

        if (IsServer)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("LobbyTest", LoadSceneMode.Single);
        }
    }

    // Fail��ư ��������
    private void ClickFailBtn()
    {
        if (GameManager.Instance.IsGolem)
        {
            if (IsServer)
            {
                NetworkManager.Singleton.SceneManager.LoadScene("GolemSceneTest", LoadSceneMode.Single);
            }
        }
        else if (GameManager.Instance.IsMush)
        {
            if (IsServer)
            {
                NetworkManager.Singleton.SceneManager.LoadScene("MushRoomSceneTest", LoadSceneMode.Single);
            }
        }
    }

    private void ClickSucessBtn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("LobbyTest", LoadSceneMode.Single);
        }
    }

    #endregion

    #region RPC

    #region Client To Server RPC

    /// <summary>
    /// �÷��̾ �������� �޾����� �������� �˸���.
    /// </summary>
    /// <param name="_clientId"></param>
    /// <param name="_damage"></param>
    /// <param name="_attackPos"></param>
    /// <param name="_knockbackDist"></param>
    [Rpc(SendTo.Server)]
    private void PlayerDamagedServerRpc
        (ulong _clientId, int _damage)
    {
        // �������� �ٸ� Ŭ���̾�Ʈ�鿡�� Ư�� �÷��̾�� �������� �����϶�� ����Ѵ�.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           
        ApplyDamageToPlayerClientRpc(_clientId, _damage);
    }

    /// <summary>
    /// �÷��̾ �˹�������� �������� �˸���.
    /// </summary>
    /// <param name="_clientId"></param>
    /// <param name="_attackPos"></param>
    /// <param name="_knockbackDist"></param>
    [Rpc(SendTo.Server)]
    private void PlayerKnockbackServerRpc(ulong _clientId, Vector3 _attackPos, float _knockbackDist)
    {
        ApplyKnockbackClientRpc(_clientId, _attackPos, _knockbackDist);
    }

    [Rpc(SendTo.Server)]
    public void PlayerDeathServerRpc(ulong _clientId)
    {
        Debug.LogWarningFormat("Server Death!!! : {0}", _clientId);
        PlayerDieClientRpc(_clientId);
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
    private void ApplyDamageToPlayerClientRpc(ulong _cliendId, int _damage)
    {
        var obj = NetworkManager.ConnectedClients[_cliendId].PlayerObject;

        if(obj != null)
        {
            GameManager.Instance.ApplyDamageToPlayer(obj.GetComponent<PlayerManager>(), _damage);
        }

    }

    /// <summary>
    /// �������� Ư�� �÷��̾�� �˹� ȿ���� �ο��ϵ��� ��� Ŭ���̾�Ʈ���� ����Ѵ�.
    /// </summary>
    /// <param name="_cliendId"></param>
    /// <param name="_damage"></param>
    /// <param name="_attackPos"></param>
    /// <param name="_knockbackDist"></param>
    [Rpc(SendTo.Everyone)]
    private void ApplyKnockbackClientRpc(ulong _cliendId, Vector3 _attackPos, float _knockbackDist)
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

        if (IsServer)
        {
            BossStateManager Boss = FindAnyObjectByType<BossStateManager>();

            if (Boss != null)
            {
                Boss.bossDieCallback += VictoryClientRpc;
            }
        }

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
            SetPlayerDieCallbackClientRpc();
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

    // �÷��̾� ��� �ݹ� ���
    [ClientRpc]
    private void SetPlayerDieCallbackClientRpc()
    {
        for (int i = 0; i < 4; i++)
        {
            if (players[i] == null) continue;

            players[i].GetComponent<PlayerNetworkManager>().OnNetworkPlayerDeath += OnPlayerDeath;
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

    // �÷��̾� �׾����� ȣ��(��� Ŭ�� ȣ���ؾ���. -> ��� Ŭ�� �÷��̾� ������ ��������)
    [ClientRpc]
    private void PlayerDieClientRpc(ulong _clientId)
    {
        foreach(var playerObj in Players)
        {
            if(playerObj.GetComponent<PlayerManager>().PlayerNetworkManager.OwnerClientId
                == _clientId)
            {
                if(GameManager.Instance.IsGolem)
                {
                    GameManager.Instance.BattleLog.SetKillLog(BossType.Golem, 
                        playerObj.GetComponent<PlayerManager>().PlayerData.characterClass);
                }
                else
                {
                    GameManager.Instance.BattleLog.SetKillLog(BossType.Mushroom,
                        playerObj.GetComponent<PlayerManager>().PlayerData.characterClass);
                }

                GameManager.Instance.BattleLog.ShowLog();

                break;
            }
        }

        if (IsServer)
        {
            playerDieCallback?.Invoke(_clientId);
            playerDieCnt++;
            CheckPlayerAllDie();
        }
    }

    // �÷��̾� �� �׾����� Ŭ�� ȣ���
    [ClientRpc]
    private void FailClientRpc()
    {
        StartCoroutine(FailCoroutine());
    }

    // ���� �׿����� Ŭ�� ȣ��
    [ClientRpc]
    private void VictoryClientRpc()
    {
        StartCoroutine(VictoryCoroutine());
    }
    #endregion
}
