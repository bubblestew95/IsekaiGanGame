using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using static BossStateManager;

// ������ ���¸� ����
public class MushStateManager : NetworkBehaviour
{
    // ��������Ʈ
    public delegate void MushStateDelegate();
    public delegate void MushStateDelegate2(ulong _index);
    public MushStateDelegate bossDieCallback;
    public MushStateDelegate bossChangeStateCallback;
    public MushStateDelegate bossHp25Callback;
    public MushStateDelegate2 bossRandomTargetCallback;

    // ��Ʈ��ũ�� ����ȭ �Ұ͵�
    public NetworkVariable<int> aggroPlayerIndex = new NetworkVariable<int>(-1);
    public NetworkVariable<int> curHp = new NetworkVariable<int>(-1);
    public NetworkVariable<float> bestAggro = new NetworkVariable<float>(-1f);
    public NetworkList<float> playerDamage = new NetworkList<float>();
    public NetworkList<float> playerAggro = new NetworkList<float>();

    // ������ ����Ǵ� ������
    public GameObject[] allPlayers;
    public GameObject[] alivePlayers;
    public GameObject aggroPlayer;
    public GameObject randomPlayer;
    public int maxHp;
    public float reduceAggro;
    public float reduceAggroTime;
    public bool[] hpCheck;

    // ���� ���
    public MushDamageParticle damageParticle;
    public UIBossHpsManager bossHpUI;
    public BgmController bgmController;
    public MushBT mushBT;
    public GameObject boss;


    // ������Ƽ
    public GameObject Boss { get { return boss; } }
    public GameObject AggroPlayer { get { return aggroPlayer; } }
    public GameObject[] AlivePlayers { get { return alivePlayers; } }
    public GameObject RandomPlayer { get { return randomPlayer; } }

    private void Awake()
    {
        FindAnyObjectByType<NetworkGameManager>().playerDieCallback += PlayerDieReceive;
        FindAnyObjectByType<NetworkGameManager>().loadingFinishCallback += Init;
    }

    #region [Damage]

    // ���������� ������ �޴� �Լ� ����
    [ServerRpc(RequireOwnership = false)]
    public void BossDamageReceiveServerRpc(ulong _clientId, int _damage, float _aggro)
    {
        if (curHp.Value <= 0) return;

        // ������, ��׷� ����(�����Ǵ� ������ ����)
        RegisterDamageAndAggro(_clientId, _damage, _aggro);

        // �������� ����(�����Ǵ� ������ ����)
        TakeDamage(_damage);

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

        // ������ hp�ݹ�(���� �ǿ� ���� ���� ����)
        CheckHpCallback();
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

    #region [Aggro]
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
            ulong randomIndex = RandomPlayerId();
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

    // �÷��̾��� ������, ��׷� ��ġ �����ϴ� �Լ�
    private void RegisterDamageAndAggro(ulong _clientId, int _damage, float _aggro)
    {
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

    // aggroPlayer ����
    [ClientRpc]
    private void SetAggroPlayerClientRpc(int _num)
    {
        aggroPlayer = alivePlayers[_num];
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

    #region [Particle]

    // ������ ��ƼŬ ����
    [ClientRpc]
    private void DamageParticleClientRpc(float _damage, ulong _clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == _clientId)
        {
            damageParticle.SetupAndPlayParticlesMine(_damage);
        }
        else
        {
            damageParticle.SetupAndPlayParticles(_damage);
        }
    }


    #endregion

    #region [BossUI]

    // ���� UI ������Ʈ
    [ClientRpc]
    private void UpdateBossUIClientRpc(int _damage)
    {
        bossHpUI.BossDamage(_damage);
        bossHpUI.HpBarUIUpdate();
    }

    #endregion

    #region [BGM]

    // ExcitedLevel ����
    [ClientRpc]
    private void ChangeExcitedLevelClientRpc()
    {
        bgmController.ExcitedLevel(ChangeHpToExciteLevel());
    }

    // hp ����
    private float ChangeHpToExciteLevel()
    {
        float hp = ((float)curHp.Value / (float)maxHp);

        return hp;
    }

    #endregion

    #region [Callback]

    // �÷��̾ �׾����� ȣ��Ǵ� �Լ�
    private void PlayerDieReceive(ulong _clientId)
    {
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

    // �÷��̾� ��Ƴ����� ����Ǵ� �Լ�
    private void PlayerReviveReceive(ulong _clientId)
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

    // Ư�� hp�����϶� ���� �ݹ��� ����
    private void CheckHpCallback()
    {
        float hp = ((float)curHp.Value / (float)maxHp) * 100f;

        Debug.Log("���� hp" + hp);

        if (hp <= 75f && !hpCheck[0])
        {
            hpCheck[0] = true;
            bossHp25Callback?.Invoke();
        }
        else if (hp <= 50f && !hpCheck[1])
        {
            hpCheck[1] = true;
            bossHp25Callback?.Invoke();
        }
        else if (hp <= 25f && !hpCheck[2])
        {
            hpCheck[2] = true;
            bossHp25Callback?.Invoke();
        }
    }

    #endregion

    #region [Function]

    // �ʱ�ȭ
    private void Init()
    {
        // �ʱ� �� ����
        maxHp = 30000;
        reduceAggro = 5f;
        reduceAggroTime = 10f;
        hpCheck = new bool[3];
        hpCheck[0] = false;
        hpCheck[1] = false;
        hpCheck[2] = false;

        // �������� �����Ұ� ����
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

        // ���� ��������
        boss = transform.gameObject;
        damageParticle = FindFirstObjectByType<MushDamageParticle>();
        bossHpUI = FindFirstObjectByType<UIBossHpsManager>();
        bgmController = FindFirstObjectByType<BgmController>();
        mushBT = FindAnyObjectByType<MushBT>();

        // ui�ʱ� ����
        bossHpUI.SetMaxHp(maxHp);
        bossHpUI.HpBarUIUpdate();

        // �÷��̾� ���� ����
        allPlayers = (GameObject[])FindFirstObjectByType<NetworkGameManager>().Players.Clone();
        alivePlayers = (GameObject[])allPlayers.Clone();

        if (IsServer)
        {
            // ��׷� �÷��̾� ����
            GetHighestAggroTarget();
        }

        // ���� �������� ���� ���� �ٲٶ�� �ݹ�
        bossChangeStateCallback?.Invoke();

        if (IsServer)
        {
            StartCoroutine(ReduceAggroCoroutine());
        }
    }

    // ������ �÷��̾ ȣ���ϴ� �Լ�
    public ulong RandomPlayerId()
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

    // Ŭ�� ��ΰ� ���� �÷��̾� ����
    public void SetRandomPlayer()
    {
        SetRandomPlayerClientRpc(RandomPlayerId());
    }

    [ClientRpc]
    private void SetRandomPlayerClientRpc(ulong _clientId)
    {
        for (int i = 0; i < 4; ++i)
        {
            if (alivePlayers[i] == null) continue;

            if (alivePlayers[i].GetComponent<NetworkObject>().OwnerClientId == _clientId)
            {
                randomPlayer = alivePlayers[i];
            }
        }
    }

    [ClientRpc]
    private void SetAlivePlayerClientRpc(int _index)
    {
        alivePlayers[_index] = null;
    }


    #endregion
}
