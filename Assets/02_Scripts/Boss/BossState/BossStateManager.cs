using System.Collections.Generic;
using System.Collections;
using UnityEngine;

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

    public GameObject Boss {get {return boss;}}
    public GameObject BossSkin { get { return bossSkin; } }
    public GameObject[] Players {get {return players;}}

    private List<BossChain> activeChain = new List<BossChain>();
    private float maxHp;
    private float curHp;
    private bool[] hpCheck = new bool[5];
    private GameObject randomTarget;

    private void Awake()
    {
        hpCheck[0] = false;
        hpCheck[1] = false;
        hpCheck[2] = false;
        hpCheck[3] = false;
        hpCheck[4] = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            BossStun();
        }
    }

    // ������ ��������
    private void OnTriggerEnter(Collider _playerAttack)
    {
        // �߰��� ����ؾ� �ϴ� ��Ȳ
        // 1. ��Ʈ��ũ ����ȭ
        // 2. �÷��̾ ������ check�� ���� ����
        // 3. �÷��̾ ��׷� check

        // �÷��̾� ������ ���ݷ��� ������.

        // �ش� ���ݷ¸�ŭ �����Ǹ� �϶�

        // ���� ü�ξ��� �����̶�� �����̻� ����Ʈ�� �߰�

    }

    // ��������ŭ hp���� �϶���Ŵ.
    private void TakeDamage(float _damage)
    {
        if (curHp <= 0) return;

        curHp -= _damage;

        if (curHp <= 0)
        {
            curHp = 0;
            bossDieCallback?.Invoke();
        }

        CheckHpCallback();

        // UI�� ����ü�� ����ȭ ��Ű�� �ڵ� �ʿ�
    }

    // Ư�� hp�����϶� ���� �ݹ��� ����
    private void CheckHpCallback()
    {
        float hp = (curHp / maxHp) * 100f;

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
}
