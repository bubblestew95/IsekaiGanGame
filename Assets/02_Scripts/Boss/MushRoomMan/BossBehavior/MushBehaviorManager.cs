using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

// ������ � �ൿ�� ���� ����
public class MushBehaviorManager : NetworkBehaviour
{
    // �����Ұ͵�
    private MushStateManager mushStateManager;
    private BossSkillManager mushSkillManager;
    private MushBT mushBT;

    // ����BehaviourManager ������
    private List<BossSkill> tmpList = new List<BossSkill>();
    private WaitForSeconds delay1f = new WaitForSeconds(1f);
    private bool attack3Trigger = false;

    // �ʱ�ȭ
    private void Awake()
    {
        mushStateManager = FindAnyObjectByType<MushStateManager>();
        mushSkillManager = FindAnyObjectByType<BossSkillManager>();
        mushBT = FindAnyObjectByType<MushBT>();
    }

    private void Start()
    {
        if (IsServer)
        {
            mushBT.behaviorEndCallback += () => StartCoroutine(BossPerformAction());
            mushStateManager.bossDieCallback += SetDie;
            mushStateManager.bossChangeStateCallback += SetChase;
            mushStateManager.bossHp25Callback += SetAttack3;
        }
    }

    #region [Behavior]

    // Ư�����ǿ��� ������ �ൿ�� �ϳ� �����ϴ� �Լ�
    // ��Ÿ��� ��Ÿ�� ����� ������ ���¸� enum������ ����
    private MushState GetRandomAction()
    {
        tmpList.Clear();

        float dis = GetDisWithoutY();

        tmpList = mushSkillManager.IsSkillInRange(dis, mushSkillManager.RandomSkills);
        tmpList = mushSkillManager.IsSkillCooldown(tmpList);

        int randomIndex = UnityEngine.Random.Range(0, tmpList.Count);

        if (tmpList.Count == 0)
        {
            return MushState.Chase;
        }

        return (MushState)Enum.Parse(typeof(MushState), tmpList[randomIndex].SkillData.SkillName);
    }

    // ������ Ư�� �ൿ�� �ϵ��� �����ϴ� �Լ�
    private void SetBossBehavior(MushState _state)
    {
        mushBT.CurState = _state;
    }

    // ������ Ư���ൿ �ϵ��� ���������Ű�� �Լ�
    private IEnumerator BossPerformAction()
    {
        // ���� �� ������
        yield return delay1f;

        if (attack3Trigger)
        {
            attack3Trigger = false;
            SetBossBehavior(MushState.Attack3);
            yield break;
        }

        // ���� ���ǵ鿡 ���� �ٸ��� ���� 
        SetBossBehavior(GetRandomAction());
    }

    // ������ �׾�����
    private void SetDie()
    {
        SetBossBehavior(MushState.Die);
    }

    private void SetChase()
    {
        SetBossBehavior(MushState.Chase);
    }

    private void SetAttack3()
    {
        attack3Trigger = true;
    }

    #endregion

    #region [Function]

    // �÷��̾�� ���� �Ÿ� ���
    private float GetDisWithoutY()
    {
        Vector2 bossPos2D = new Vector2(mushStateManager.Boss.transform.position.x, mushStateManager.Boss.transform.position.z);
        Vector2 playerPos2D = new Vector2(mushStateManager.AggroPlayer.transform.position.x, mushStateManager.AggroPlayer.transform.position.z);

        return Vector2.Distance(bossPos2D, playerPos2D);
    }

    #endregion
}
