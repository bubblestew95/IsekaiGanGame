using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Unity.Netcode;


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
    public BossStateDelegate2 bossRandomTargetCallback;

    // ������Ƽ
    public GameObject Boss { get { return boss; } }
    public GameObject AggroPlayer { get { return aggroPlayer; } }
    public GameObject BossSkin { get { return bossSkin; } }
    public GameObject[] Players { get { return players; } }
    public BoxCollider HitCollider { get { return hitCollider; } }

    // ���� ���� ���� ������
    public List<BossChain> activeChain = new List<BossChain>();
    public bool[] hpCheck = new bool[9];
    public GameObject randomTarget;
    public float bestAggro = 0f;
    public bool isPhase2 = false;
    public int maxHp = 100;
    public float chainTime = 0f;
    public GameObject[] players;
    public GameObject aggroPlayer;

    // ��Ʈ��ũ�� ����ȭ �Ұ͵�
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

    private void Awake()
    {
        FindAnyObjectByType<NetworkGameManager>().loadingFinishCallback += InitMulti;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Wall")
        {
            bossWallTriggerCallback?.Invoke();
            Debug.Log("���̶� �ε�ħ");
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

        // Ŭ���̾�Ʈ ��� ��׷� �÷��̾� �����ϴ� �Լ�
        GetHighestAggroTarget();

        // Ŭ���̾�Ʈ ��� ���� �ǰ� ��ƼŬ ����
        DamageParticleClientRpc(_damage);

        // Ŭ���̾�Ʈ ��� ���� UI����
        UpdateBossUIClientRpc(_damage);

        // Ŭ���̾�Ʈ ��� ���� ��� ����
        ChangeExcitedLevelClientRpc();

        // ������ hp�ݹ�(���� �ǿ� ���� ���� ����)
        CheckHpCallback();
    }

    #region [ClientRPC]
    // aggroPlayer ����
    [ClientRpc]
    private void SetAggroPlayerClientRpc(int _num)
    {
        Debug.LogWarning("SetAggroPlayerClientRpc �����");

        Debug.LogWarning("SetAggroPlayerClientRpc�� num : " + _num);

        aggroPlayer = players[_num];
    }

    // ������ ��ƼŬ ����
    [ClientRpc]
    private void DamageParticleClientRpc(float _damage)
    {
        damageParticle.SetupAndPlayParticles(_damage);
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
    private void ResetAggro()
    {
        for (int i = 0; i < playerAggro.Count; i++)
        {
            playerAggro[i] = 0f;
        }
    }

    // �÷��̾��� ������, ��׷� ��ġ �����ϴ� �Լ�
    private void RegisterDamageAndAggro(ulong _clientId, int _damage, float _aggro)
    {
        if (_clientId == 100) return;

        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] == null) continue;

            if (players[i].GetComponent<NetworkObject>().OwnerClientId == _clientId)
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

        // ���� ��׷� 0�϶� -> ���� 1�� �ƹ��� aggro 10���� ����� aggroPlayer ���·�
        foreach (float aggro in playerAggro)
        {
            if (aggro != 0f)
            {
                allAggroZero = false;
            }
        }

        if (allAggroZero)
        {
            int len = 0;

            foreach (GameObject player in players)
            {
                if (player != null) len++;
            }

            int num = UnityEngine.Random.Range(0, len);
            playerAggro[num] = 10f;
            aggroPlayerIndex.Value = num;
            SetAggroPlayerClientRpc(aggroPlayerIndex.Value);
            return;
        }

        // ������ ��׷� �պ��� ��׷ΰ� 1.2�� ũ�� ��׷� �ٲ�
        for (int i = 0; i < players.Length; i++)
        {
            if (bestAggro * 1.2f <= playerAggro[i])
            {
                bestAggro = playerAggro[i];
                aggroPlayerIndex.Value = i;
            }
        }

        Debug.Log("�������� aggroPlayerIndex.Value :" + aggroPlayerIndex.Value);

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
    #endregion

    #region [Init]

    // �ʱ�ȭ �ؾ��Ұ͵�
    private void Init()
    {
        for (int i = 0; i < hpCheck.Length; i++)
        {
            hpCheck[i] = false;
        }
        bestAggro = 0f;

        // ���� ��������
        boss = transform.gameObject;
        bossSkin = boss.transform.GetChild(0).gameObject;
        hitCollider = boss.transform.GetChild(1).GetComponent<BoxCollider>();
        attackCollider = GetComponent<BossAttackCollider>();
        damageParticle = FindFirstObjectByType<DamageParticle>();
        bossHpUI = FindFirstObjectByType<UIBossHpsManager>();
        bgmController = FindFirstObjectByType<BgmController>();
        bossBT = FindAnyObjectByType<BossBT>();

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
        }

        Init();

        SetCallback();

        SetPlayerMulti();

        bossBT.curState = BossState.Chase;
    }

    // �ݹ� ����
    private void SetCallback()
    {
        attackCollider.rockCollisionCallback += BossStun;
        bossBT.phase2BehaviorStartCallback += ChangePhase2BGM;
    }

    // ��Ƽ �÷��̾� ����
    private void SetPlayerMulti()
    {
        players = FindFirstObjectByType<NetworkGameManager>().Players;

        // �ʹ� aggro 0�̿��� �����ϴ� �Լ�
        GetHighestAggroTarget();
    }
    #endregion

    #region [Etc.]
    // ������ ��׷� �÷��̾� ������ �Ÿ� ���
    public float GetDisWithoutY()
    {
        Vector2 bossPos2D = new Vector2(boss.transform.position.x, boss.transform.position.z);
        Vector2 playerPos2D = new Vector2(players[aggroPlayerIndex.Value].transform.position.x, players[aggroPlayerIndex.Value].transform.position.z);

        return Vector2.Distance(bossPos2D, playerPos2D);
    }

    // ������ �÷��̾ ȣ���ϴ� �Լ�
    public ulong RandomPlayer()
    {
        List<ulong> numList = new List<ulong>();

        for (int i = 0; i < 4; ++i)
        {
            if (Players[i] == null) continue;

            numList.Add(Players[i].GetComponent<NetworkObject>().OwnerClientId);
        }

        ulong randomNum = numList[Random.Range(0, numList.Count)];

        return randomNum;
    }
    #endregion


}