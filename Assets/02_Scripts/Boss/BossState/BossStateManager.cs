using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Mono.Cecil.Cil;

public class BossStateManager : MonoBehaviour
{
    public delegate void BossStateDelegate();
    public delegate void BossStateDelegate2(GameObject _target);
    public BossStateDelegate bossDieCallback;
    public BossStateDelegate bossHp10Callback;
    public BossStateDelegate bossHpHalfCallback;
    public BossStateDelegate bossStunCallback;
    public BossStateDelegate2 bossRandomTargetCallback;

    [SerializeField] public GameObject aggroPlayer;
    [SerializeField] private GameObject[] players;
    [SerializeField] private GameObject boss;
    [SerializeField] public float chainTime;
    [SerializeField] private GameObject bossSkin;
    [SerializeField] private int maxHp;
    [SerializeField] private int curHp;

    public GameObject Boss {get {return boss;}}
    public GameObject BossSkin { get { return bossSkin; } }
    public GameObject[] Players {get {return players;}}
    public int MaxHp { get { return maxHp; } }
    public int CurHp { get { return curHp; } }

    private List<BossChain> activeChain = new List<BossChain>();
    private bool[] hpCheck = new bool[9];
    private GameObject randomTarget;
    private float[] playerDamage = new float[4];
    private float[] playerAggro = new float[4];
    private float bestAggro;

    // ã�Ƽ� �ִ°�
    private DamageParticle damageParticle;
    private UIBossHpsManager bossHpUI;
    private BgmController bgmController;
    private BossAttackCollider attackCollider;
    private BossBT bossBT;

    private void Awake()
    {
        for (int i = 0; i < hpCheck.Length; i++)
        {
            hpCheck[i] = false;
        }

        for (int i = 0; i < 4; i++)
        {
            playerDamage[i] = 0;
            playerAggro[i] = 0;
        }

        bestAggro = 0f;

        attackCollider = GetComponent<BossAttackCollider>();

        curHp = maxHp;

        // 
        damageParticle = FindFirstObjectByType<DamageParticle>();
        bossHpUI = FindFirstObjectByType<UIBossHpsManager>();
        bgmController = FindFirstObjectByType<BgmController>();
        bossBT = FindAnyObjectByType<BossBT>();
    }

    private void Start()
    {
        // ����8 ���� �ݹ�
        attackCollider.rockCollisionCallback += BossStun;
        bossBT.phase2BehaviorEndCallback += ChangePhase2BGM;

        // ui�ʱ� ����
        bossHpUI.SetMaxHp(maxHp);
        bossHpUI.HpBarUIUpdate();

        // �ʹ� aggro 0�̿��� �����ϴ� �Լ�
        GetHighestAggroTarget();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            BossStun();
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            TakeDamage(null, 10, 10);
        }
    }

    // ��������ŭ hp���� �϶���Ŵ.
    public void TakeDamage(PlayerManager _player, int _damage, float _aggro)
    {
        if (curHp <= 0) return;

        // �÷��̾� ��׷�, ������ ��ġ ���
        RegisterDamageAndAggro(_player, _damage, _aggro);

        // �����̻� �߰�
        // AddChainList();

        // �������� ����
        curHp -= _damage;

        if (curHp <= 0)
        {
            curHp = 0;
            bossDieCallback?.Invoke();
        }

        // ���� hp�ݹ�(���� �ǿ� ���� ���� ����)
        CheckHpCallback();

        // ��׷� �÷��̾� �����ϴ� �Լ�
        GetHighestAggroTarget();

        // ���� �ǰ� ��ƼŬ ����
        damageParticle.SetupAndPlayParticles(_damage);

        // ���� UI����
        bossHpUI.BossDamage(_damage);
        bossHpUI.HpBarUIUpdate();

        // ���� ��� ����
        bgmController.ExcitedLevel(ChangeHpToExciteLevel());
    }

    // Ư�� hp�����϶� ���� �ݹ��� ����
    private void CheckHpCallback()
    {
        float hp = ((float)curHp / (float)maxHp) * 100f;

        Debug.Log("���� hp" + hp);

        if (hp <= 90f && !hpCheck[0])
        {
            hpCheck[0] = true;
            bossHp10Callback?.Invoke();

            randomTarget = RandomPlayer();
            bossRandomTargetCallback?.Invoke(randomTarget);
        }
        else if (hp <= 80f && !hpCheck[1])
        {
            hpCheck[1] = true;
            bossHp10Callback?.Invoke();

            randomTarget = RandomPlayer();
            bossRandomTargetCallback?.Invoke(randomTarget);
        }
        else if (hp <= 70f && !hpCheck[2])
        {
            hpCheck[2] = true;
            bossHp10Callback?.Invoke();

            randomTarget = RandomPlayer();
            bossRandomTargetCallback?.Invoke(randomTarget);
        }
        else if (hp <= 60f && !hpCheck[3])
        {
            hpCheck[3] = true;
            bossHp10Callback?.Invoke();

            randomTarget = RandomPlayer();
            bossRandomTargetCallback?.Invoke(randomTarget);
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

            randomTarget = RandomPlayer();
            bossRandomTargetCallback?.Invoke(randomTarget);
        }
        else if (hp <= 30f && !hpCheck[6])
        {
            hpCheck[6] = true;
            bossHp10Callback?.Invoke();

            randomTarget = RandomPlayer();
            bossRandomTargetCallback?.Invoke(randomTarget);
        }
        else if (hp <= 20f && !hpCheck[7])
        {
            hpCheck[7] = true;
            bossHp10Callback?.Invoke();

            randomTarget = RandomPlayer();
            bossRandomTargetCallback?.Invoke(randomTarget);
        }
        else if (hp <= 10f && !hpCheck[8])
        {
            hpCheck[8] = true;
            bossHp10Callback?.Invoke();

            randomTarget = RandomPlayer();
            bossRandomTargetCallback?.Invoke(randomTarget);
        }
    }

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

    // ������ ��׷� �÷��̾� ������ �Ÿ� ���
    public float GetDisWithoutY()
    {
        Vector2 bossPos2D = new Vector2(boss.transform.position.x, boss.transform.position.z);
        Vector2 playerPos2D = new Vector2(aggroPlayer.transform.position.x, aggroPlayer.transform.position.z);

        return Vector2.Distance(bossPos2D, playerPos2D);
    }

    // ������ �÷��̾ ȣ���ϴ� �Լ�
    private GameObject RandomPlayer()
    {
        int randomIndex = Random.Range(0, players.Length);

        return players[randomIndex];
    }

    //������ ���ϰɷ����� ȣ��Ǵ� �Լ�
    public void BossStun()
    {
        bossStunCallback?.Invoke();
    }

    // ��׷� ��ġ �ʱ�ȭ �ϴ� �Լ�
    private void ResetAggro()
    {
        for (int i = 0; i < playerAggro.Length; i++)
        {
            playerAggro[i] = 0f;
        }
    }


    // �÷��̾��� ������, ��׷� ��ġ �����ϴ� �Լ�
    private void RegisterDamageAndAggro(PlayerManager _player, int _damage, float _aggro)
    {
        if (_player == null) return;

        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] == _player.gameObject)
            {
                playerDamage[i] += _damage;
                playerAggro[i] += _aggro;
            }
        }
    }

    // ���� ��׷� ��ġ �������� ��׷� ��� �Ǻ��ϴ� �Լ�
    private void GetHighestAggroTarget()
    {
        // ���� ��׷� 0�϶� -> ���� 1�� �ƹ��� aggro 10���� ����� aggroPlayer ���·�
        foreach (float aggro in playerAggro)
        {
            if (aggro == 0f)
            {
                continue;
            }
            else
            {
                int num = Random.Range(0, players.Length);
                playerAggro[num] = 10f;
                aggroPlayer = players[num];
                return;
            }
        }

        int aggroPlayerIndex = 0;

        // ������ ��׷� �պ��� ��׷ΰ� 1.2�� ũ�� ��׷� �ٲ�
        for (int i = 0; i < players.Length;i++)
        {
            if (bestAggro * 1.2f <= playerAggro[i])
            {
                bestAggro = playerAggro[i];
                aggroPlayerIndex = i;
            }
        }

        // ��׷� �÷��̾� ����
        aggroPlayer = players[aggroPlayerIndex];
    }

    // hp 1�������϶� 0~1, 2������ �϶� 0~1
    private float ChangeHpToExciteLevel()
    {
        float hp = ((float)curHp / (float)maxHp);

        if (hp >= 0.5f)
        {
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
    }
}
