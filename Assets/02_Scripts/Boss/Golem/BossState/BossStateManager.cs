using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Unity.Netcode;
using System.Linq;



public class BossStateManager : NetworkBehaviour
{
    // ��������Ʈ
    public delegate void BossStateDelegate();
    public delegate void BossStateDelegate2(ulong _index);
    public BossStateDelegate bossDieCallback;
    public BossStateDelegate bossHp10Callback;
    public BossStateDelegate bossHpHalfCallback;
    public BossStateDelegate bossStunCallback;
    public BossStateDelegate bossWallTriggerCallback;
    public BossStateDelegate bossTimeOutCallback;
    public BossStateDelegate2 bossRandomTargetCallback;

    // ������Ƽ
    public GameObject Boss { get { return boss; } }
    public GameObject AggroPlayer { get { return aggroPlayer; } }
    public GameObject BossSkin { get { return bossSkin; } }
    public GameObject[] AlivePlayers { get { return alivePlayers; } }
    public BoxCollider HitCollider { get { return hitCollider; } }

    // ���� ���� ���� ������
    public List<BossChain> activeChain = new List<BossChain>();
    public bool[] hpCheck = new bool[9];
    public GameObject randomTarget;
    public bool isPhase2 = false;
    public int maxHp = 1000;
    public float chainTime = 0f;
    public float reduceAggro = 5f;
    public float reduceAggroTime = 10f;
    public GameObject[] allPlayers;
    public GameObject[] alivePlayers;
    public GameObject aggroPlayer;

    // ��Ʈ��ũ�� ����ȭ �Ұ͵�
    public NetworkVariable<float> bestAggro;
    public NetworkVariable<int> aggroPlayerIndex = new NetworkVariable<int>(-1);
    public NetworkVariable<int> curHp = new NetworkVariable<int>(-1);
    public NetworkList<float> playerDamage = new NetworkList<float>();
    public NetworkList<float> playerAggro = new NetworkList<float>();

    // �����ͼ� �ִ°�
    public DamageParticle damageParticle;
    public UIBossHpsManager bossHpUI;
    public BgmController bgmController;
    public BossAttackCollider attackCollider;
    public BossBT bossBT;
    public GameObject boss;
    public GameObject bossSkin;
    public BoxCollider hitCollider;
    public FollowCamera followCam;

    private void Awake()
    {
        FindAnyObjectByType<NetworkGameManager>().playerDieCallback += PlayerDieCallback;
        FindAnyObjectByType<NetworkGameManager>().loadingFinishCallback += () => 
        {
            InitMulti();
            Invoke("ChangeBossState", 3f);
        };
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Wall")
        {
            bossWallTriggerCallback?.Invoke();
            Debug.Log("���̶� �ε�ħ");
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            BossDamageReceiveServerRpc(100, 100, 0);
        }
    }

    // ���������� ������ �޴� �Լ� ����
    [ServerRpc(RequireOwnership = false)]
    public void BossDamageReceiveServerRpc(ulong _clientId, int _damage, float _aggro)
    {
        Debug.Log("������ ���� �����");

        if (curHp.Value <= 0) return;

        // ������, ��׷� ����(�����Ǵ� ������ ����)
        RegisterDamageAndAggro(_clientId, _damage, _aggro);

        // ������ �����̻� �߰�(�����Ǵ� ������ ����)
        // AddChainList();

        // �������� ����(�����Ǵ� ������ ����)
        TakeDamage(_damage);

        // Ŭ���̾�Ʈ ��� ��׷� �÷��̾� �����ϴ� �Լ� (��Ʈ��ũ���� ����� ����)
        if (IsServer)
        {
            GetHighestAggroTarget();
        }

        // Ŭ���̾�Ʈ ��� ���� �ǰ� ��ƼŬ ����
        DamageParticleClientRpc(_damage, _clientId);

        // Ŭ���̾�Ʈ ��� ���� UI����
        UpdateBossUIClientRpc(_damage);

        // Ŭ���̾�Ʈ ��� ���� ��� ����
        ChangeExcitedLevelClientRpc();

        // �÷��̾� ī�޶� ��鸲 ����
        ShakePlayerCamClientRpc(_clientId);

        // �´� ����� ���
        GetHitSoundClientRpc(_clientId);

        // ������ hp�ݹ�(���� �ǿ� ���� ���� ����)
        CheckHpCallback();
    }

    #region [ClientRPC]
    // aggroPlayer ����
    [ClientRpc]
    private void SetAggroPlayerClientRpc(int _num)
    {
        aggroPlayer = alivePlayers[_num];
    }

    // ������ ��ƼŬ ����
    [ClientRpc]
    private void DamageParticleClientRpc(float _damage, ulong _clientId)
    {
        // ������ ��Ʈ
        if (NetworkManager.Singleton.LocalClientId == _clientId)
        {
            // ������ ��ƼŬ
            damageParticle.SetupAndPlayParticlesMine(_damage);

            // ��Ʈ ��ƼŬ
            Vector3 pos = new Vector3(Boss.transform.position.x, 3f, Boss.transform.position.z);
            ParticleManager.Instance.PlayParticle(ParticleManager.Instance.hitParticle, pos);
        }
        else
        {
            damageParticle.SetupAndPlayParticles(_damage);
        }
    }

    // ���� UI ������Ʈ
    [ClientRpc]
    private void UpdateBossUIClientRpc(int _damage)
    {
        bossHpUI.BossDamage(_damage);
        bossHpUI.HpBarUIUpdate();
    }

    // ExcitedLevel ����
    [ClientRpc]
    private void ChangeExcitedLevelClientRpc()
    {
        bgmController.ExcitedLevel(ChangeHpToExciteLevel());
    }

    [ClientRpc]
    private void RandomPlayerClientRpc(ulong _num)
    {
        bossRandomTargetCallback?.Invoke(_num);
    }

    [ClientRpc]
    private void SetAlivePlayerClientRpc(int _index)
    {
        alivePlayers[_index] = null;
    }

    [ClientRpc]
    private void ShakePlayerCamClientRpc(ulong _clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == _clientId)
        {
            // ī�޶� ��鸮�� ����
            StartCoroutine(followCam.ShakeCam());
        }
    }
    #endregion

    #region [BossState]
    // chain ���¸� �����ϴ� List�� ����
    private void AddChainList(BossChain _chainType)
    {
        activeChain.Add(_chainType);
        StartCoroutine(RemoveChainList(_chainType));
    }

    // chain Ư�� �ð��� ����
    private IEnumerator RemoveChainList(BossChain _chainType)
    {
        yield return new WaitForSeconds(chainTime);

        if (activeChain.Contains(_chainType))
        {
            activeChain.Remove(_chainType);
        }
    }

    // Chain ���� Ȯ��
    public bool HasChain(BossChain _chainType)
    {
        return activeChain.Contains(_chainType);
    }

    //������ ���ϰɷ����� ȣ��Ǵ� �Լ�
    public void BossStun()
    {
        bossStunCallback?.Invoke();
    }

    // Ư�� hp�����϶� ���� �ݹ��� ����
    private void CheckHpCallback()
    {
        float hp = ((float)curHp.Value / (float)maxHp) * 100f;

        Debug.Log("���� hp" + hp);

        if (hp <= 90f && !hpCheck[0])
        {
            hpCheck[0] = true;
            bossHp10Callback?.Invoke();

            RandomPlayerClientRpc(RandomPlayer());
        }
        else if (hp <= 80f && !hpCheck[1])
        {
            hpCheck[1] = true;
            bossHp10Callback?.Invoke();

            RandomPlayerClientRpc(RandomPlayer());
        }
        else if (hp <= 70f && !hpCheck[2])
        {
            hpCheck[2] = true;
            bossHp10Callback?.Invoke();

            RandomPlayerClientRpc(RandomPlayer());
        }
        else if (hp <= 60f && !hpCheck[3])
        {
            hpCheck[3] = true;
            bossHp10Callback?.Invoke();

            RandomPlayerClientRpc(RandomPlayer());
        }
        else if (hp <= 50f && !hpCheck[4])
        {
            hpCheck[4] = true;
            bossHpHalfCallback?.Invoke();
        }
        else if (hp <= 40f && !hpCheck[5])
        {
            hpCheck[5] = true;
            bossHp10Callback?.Invoke();

            RandomPlayerClientRpc(RandomPlayer());
        }
        else if (hp <= 30f && !hpCheck[6])
        {
            hpCheck[6] = true;
            bossHp10Callback?.Invoke();

            RandomPlayerClientRpc(RandomPlayer());
        }
        else if (hp <= 20f && !hpCheck[7])
        {
            hpCheck[7] = true;
            bossHp10Callback?.Invoke();

            RandomPlayerClientRpc(RandomPlayer());
        }
        else if (hp <= 10f && !hpCheck[8])
        {
            hpCheck[8] = true;
            bossHp10Callback?.Invoke();

            RandomPlayerClientRpc(RandomPlayer());
        }
    }
    #endregion

    #region [Damage && Aggro]
    // ��׷� ��ġ �ʱ�ȭ �ϴ� �Լ�
    public void ResetAggro()
    {
        for (int i = 0; i < playerAggro.Count; i++)
        {
            playerAggro[i] = 0f;
        }

        bestAggro.Value = 0f;

        GetHighestAggroTarget();
    }

    // �÷��̾��� ������, ��׷� ��ġ �����ϴ� �Լ�
    private void RegisterDamageAndAggro(ulong _clientId, int _damage, float _aggro)
    {
        if (_clientId == 100) return;

        for (int i = 0; i < alivePlayers.Length; i++)
        {
            if (alivePlayers[i] == null) continue;

            if (alivePlayers[i].GetComponent<NetworkObject>().OwnerClientId == _clientId)
            {
                playerDamage[i] += _damage;
                playerAggro[i] += _aggro;
            }
        }
    }

    // ���� ��׷� ��ġ �������� ��׷� ��� �Ǻ��ϴ� �Լ�
    private void GetHighestAggroTarget()
    {
        bool allAggroZero = true;

        // ���� ��׷� 0�϶�
        foreach (float aggro in playerAggro)
        {
            if (aggro != 0f)
            {
                allAggroZero = false;
            }
        }

        // ���� ��׷� 0�϶� -> ���� 1�� �ƹ��� aggro 10���� ����� aggroPlayer ���·�
        if (allAggroZero)
        {
            // ���� �÷��̾� ����
            ulong randomIndex = RandomPlayer();
            GameObject randomPlayer = alivePlayers.FirstOrDefault(p => p != null && p.GetComponent<NetworkObject>().OwnerClientId == randomIndex);

            for (int i = 0; i < 4; i++)
            {
                if (alivePlayers[i] == null) continue;

                if (alivePlayers[i] == randomPlayer)
                {
                    playerAggro[i] = 10f;
                    bestAggro.Value = playerAggro[i];
                    aggroPlayerIndex.Value = i;
                }
            }

            SetAggroPlayerClientRpc(aggroPlayerIndex.Value);
            return;
        }

        // ������ ��׷� �պ��� ��׷ΰ� 1.2�� ũ�� ��׷� �ٲ�
        for (int i = 0; i < alivePlayers.Length; i++)
        {
            if (bestAggro.Value * 1.2f <= playerAggro[i])
            {
                bestAggro.Value = playerAggro[i];
                aggroPlayerIndex.Value = i;
            }
        }

        // ��׷� �÷��̾� ����(Ŭ�� ���)
        SetAggroPlayerClientRpc(aggroPlayerIndex.Value);
    }

    // ������ �޴� �Լ�
    private void TakeDamage(int _damage)
    {
        curHp.Value -= _damage;

        if (curHp.Value <= 0)
        {
            curHp.Value = 0;
            bossDieCallback?.Invoke();
        }
    }

    // ��׷� ��ġ ���ҽ�Ű�� �ڷ�ƾ
    private IEnumerator ReduceAggroCoroutine()
    {
        float elapseTime = 0f;

        while (true)
        {
            elapseTime += Time.deltaTime;

            if (elapseTime >= reduceAggroTime)
            {
                for (int i = 0; i < 4; ++i)
                {
                    playerAggro[i] -= reduceAggro;

                    if (playerAggro[i] <= 0)
                    {
                        playerAggro[i] = 0;
                    }
                }

                bestAggro.Value -= reduceAggro;

                if (bestAggro.Value <= 0) bestAggro.Value = 0;

                elapseTime = 0f;
            }
            yield return null;
        }
    }
    #endregion

    #region [BGM]
    // hp 1�������϶� 0~1, 2������ �϶� 0~1
    private float ChangeHpToExciteLevel()
    {
        float hp = ((float)curHp.Value / (float)maxHp);

        if (!isPhase2)
        {
            if (((1 - hp) * 2) >= 1f) return 1f;

            return (1 - hp) * 2;
        }
        else
        {
            return 1 - hp * 2;
        }
    }

    // ������ �ٲ� ��� �ٲٰ� ExcitedLevel �ٲ�.
    private void ChangePhase2BGM()
    {
        bgmController.ExcitedLevel(0);
        bgmController.PlayBossRageBgm();
        isPhase2 = true;
    }

    // �´� ����� ���
    [ClientRpc]
    private void GetHitSoundClientRpc(ulong _clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == _clientId)
        {
            BossAudioManager.Instance.AudioPlay(BossAudioManager.Instance.GetHit);
        }
    }
    #endregion

    #region [Init]

    // �ʱ�ȭ �ؾ��Ұ͵�
    private void Init()
    {
        for (int i = 0; i < hpCheck.Length; i++)
        {
            hpCheck[i] = false;
        }

        // ���� ��������
        boss = transform.gameObject;
        bossSkin = boss.transform.GetChild(0).gameObject;
        hitCollider = boss.transform.GetChild(1).GetComponent<BoxCollider>();
        attackCollider = GetComponent<BossAttackCollider>();
        damageParticle = FindFirstObjectByType<DamageParticle>();
        bossHpUI = FindFirstObjectByType<UIBossHpsManager>();
        bgmController = FindFirstObjectByType<BgmController>();
        bossBT = FindAnyObjectByType<BossBT>();
        followCam = FindAnyObjectByType<FollowCamera>();

        // ui�ʱ� ����
        bossHpUI.SetMaxHp(maxHp);
        bossHpUI.HpBarUIUpdate();
    }

    // ��Ƽ���� �ʱ�ȭ �ؾ��Ұ͵�
    private void InitMulti()
    {
        if (IsServer)
        {
            for (int i = 0; i < 4; i++)
            {
                playerDamage.Add(0f);
                playerAggro.Add(0f);
            }

            curHp.Value = maxHp;
            aggroPlayerIndex.Value = 0;
            bestAggro.Value = 0f;
        }

        Init();

        SetCallback();

        SetPlayerMulti();

        if (IsServer)
        {
            StartCoroutine(ReduceAggroCoroutine());
            StartCoroutine(CheckTimeOut());
        }
    }

    // �ݹ� ����
    private void SetCallback()
    {
        BossBT.SpecialAttackEndCallback += ResetAggro;
        attackCollider.rockCollisionCallback += BossStun;
        bossBT.phase2BehaviorStartCallback += ChangePhase2BGM;
    }

    // ��Ƽ �÷��̾� ����
    private void SetPlayerMulti()
    {
        allPlayers = FindFirstObjectByType<NetworkGameManager>().Players;
        alivePlayers = (GameObject[])allPlayers.Clone();

        // �ʹ� aggro 0�̿��� �����ϴ� �Լ�
        if (IsServer)
        {
            GetHighestAggroTarget();
        }
    }

    // ���� ���� ����
    private void ChangeBossState()
    {
        bossBT.curState = BossState.Chase;
    }
    #endregion

    #region [Etc.]
    // ������ ��׷� �÷��̾� ������ �Ÿ� ���
    public float GetDisWithoutY()
    {
        Vector2 bossPos2D = new Vector2(boss.transform.position.x, boss.transform.position.z);
        Vector2 playerPos2D = new Vector2(alivePlayers[aggroPlayerIndex.Value].transform.position.x, alivePlayers[aggroPlayerIndex.Value].transform.position.z);

        return Vector2.Distance(bossPos2D, playerPos2D);
    }

    // ������ �÷��̾ ȣ���ϴ� �Լ�
    public ulong RandomPlayer()
    {
        List<ulong> numList = new List<ulong>();
        ulong randomNum = 0;

        for (int i = 0; i < 4; ++i)
        {
            if (alivePlayers[i] == null) continue;

            numList.Add(alivePlayers[i].GetComponent<NetworkObject>().OwnerClientId);
        }

        if (numList.Count != 0)
        {
            randomNum = numList[Random.Range(0, numList.Count)];
        }
        else
        {
            randomNum = 0;
        }


        return randomNum;
    }

    // �÷��̾ �׾����� ȣ��Ǵ� �Լ�
    private void PlayerDieCallback(ulong _clientId)
    {
        Debug.LogWarning("�ش� �÷��̾ ���� : " + _clientId);

        int playerIndex = -1;
        bool IsAggroDie = false;

        // ���� �÷��̾��� Aggro��ġ ���� && Player����Ʈ���� ����
        for (int i = 0; i < 4; ++i)
        {
            if (alivePlayers[i] == null) continue;

            if (alivePlayers[i].GetComponent<NetworkObject>().OwnerClientId == _clientId)
            {
                // ���� ���� �÷��̾ bestAggro���ٸ� ����Ʈ ��׷ε� �ʱ�ȭ
                if (i == aggroPlayerIndex.Value)
                {
                    bestAggro.Value = 0f;
                    IsAggroDie = true;
                }
                playerAggro[i] = 0f;
                alivePlayers[i] = null;
                playerIndex = i;

                // Aggro�÷��̾ �׾��ٸ�, bestAggro �缳��
                if (IsAggroDie)
                {
                    // bestAggro �缳��
                    for (int j = 0; j < alivePlayers.Length; j++)
                    {
                        if (bestAggro.Value <= playerAggro[j])
                        {
                            bestAggro.Value = playerAggro[j];
                            aggroPlayerIndex.Value = j;
                        }
                    }
                }
            }
        }

        // alivePlayer ����ȭ
        SetAlivePlayerClientRpc(playerIndex);

        // ��׷� �÷��̾� ����(Ŭ�� ���)
        SetAggroPlayerClientRpc(aggroPlayerIndex.Value);

        // ��׷� �缳���� ���� ��׷� ���� �Լ� ȣ��
        GetHighestAggroTarget();
    }

    // �÷��̾� ��Ƴ����� ȣ��
    private void PlayerReviveCallback(ulong _clientId)
    {
        // ���� �÷��̾ alivePlayers�迭�� �߰�
        for (int i = 0; i < 4; ++i)
        {
            if (allPlayers[i] == null) continue;

            if (allPlayers[i].GetComponent<NetworkObject>().OwnerClientId == _clientId)
            {
                alivePlayers[i] = allPlayers[i];
            }
        }
    }

    // Ÿ�Ӿƿ� ������ ȣ��Ǵ� �Լ�
    private void TimeOutCallback()
    {
        bossTimeOutCallback?.Invoke();
    }

    // Ÿ�Ӿƿ� üũ�ϴ� �ڷ�ƾ
    private IEnumerator CheckTimeOut()
    {
        float elapseTime = 0f;

        while (true)
        {
            elapseTime += Time.deltaTime;

            if (elapseTime >= 600)
            {
                TimeOutCallback();
                break;
            }

            yield return null;
        }
    }
    #endregion


}