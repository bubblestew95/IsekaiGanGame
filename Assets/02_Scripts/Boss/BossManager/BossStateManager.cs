using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class BossStateManager : MonoBehaviour
{
    public delegate void BossStateDelegate();
    public BossStateDelegate bossDieCallback;
    public BossStateDelegate bossHp10Callback;
    public BossStateDelegate bossHpHalfCallback;

    [SerializeField] public GameObject aggroPlayer;
    [SerializeField] private GameObject Boss;
    [SerializeField] public float damage;
    [SerializeField] public float chainTime;

    private List<BossChain> activeChain = new List<BossChain>();
    private float maxHp;
    private float curHp;
    private bool[] hpCheck = new bool[4];

    private void Awake()
    {
        hpCheck[0] = false;
        hpCheck[1] = false;
        hpCheck[2] = false;
        hpCheck[3] = false;
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
        }
        else if (hp <= 80f && !hpCheck[1])
        {
            hpCheck[1] = true;
            bossHp10Callback?.Invoke();
        }
        else if (hp <= 70f && !hpCheck[2])
        {
            hpCheck[2] = true;
            bossHp10Callback?.Invoke();
        }
        else if (hp <= 60f && !hpCheck[3])
        {
            hpCheck[3] = true;
            bossHp10Callback?.Invoke();
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
        Vector2 bossPos2D = new Vector2(Boss.transform.position.x, Boss.transform.position.z);
        Vector2 playerPos2D = new Vector2(aggroPlayer.transform.position.x, aggroPlayer.transform.position.z);

        return Vector2.Distance(bossPos2D, playerPos2D);
    }
}
