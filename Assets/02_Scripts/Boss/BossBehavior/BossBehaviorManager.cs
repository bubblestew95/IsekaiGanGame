using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossBehaviorManager : MonoBehaviour
{
    [SerializeField] private BossStateManager bossStateManager;
    [SerializeField] private BossSkillManager bossSkillManager;
    [SerializeField] private BossBT bossBT;

    private List<BossSkill> tmpList = new List<BossSkill>();
    private WaitForSeconds delay1f = new WaitForSeconds(1f);
    private bool hp10Trigger = false;
    private bool hpHalfTrigger = false;
    private int hp10Cnt = 0;

    private void Start()
    {
        bossBT.behaviorEndCallback += () => StartCoroutine(BossPerformAction());
        bossStateManager.bossHp10Callback += SetHP10;
        bossStateManager.bossHpHalfCallback += SetHPHalf;
    }

    // Ư�����ǿ��� ������ �ൿ�� �ϳ� �����ϴ� �Լ�
    // ��Ÿ��� ��Ÿ�� ����� ������ ���¸� enum������ ����
    private BossState GetRandomAction()
    {
        tmpList.Clear();

        float dis = bossStateManager.GetDisWithoutY();

        tmpList = bossSkillManager.IsSkillInRange(dis, bossSkillManager.Skills);
        tmpList = bossSkillManager.IsSkillCooldown(tmpList);

        int randomIndex = UnityEngine.Random.Range(0, tmpList.Count);

        if (tmpList.Count == 0)
        {
            return BossState.Chase;
        }

        return (BossState)Enum.Parse(typeof(BossState), tmpList[randomIndex].SkillData.SkillName);
    }

    // ������ Ư�� �ൿ�� �ϵ��� �����ϴ� �Լ�
    private void SetBossBehavior(BossState _state)
    {
        bossBT.curState = _state;
    }

    // ������ Ư���ൿ �ϵ��� ���������Ű�� �Լ�
    private IEnumerator BossPerformAction()
    {
        // ���� �� ������
        yield return delay1f;

        // �ǰ� 10�� �￴���� ���� ����
        if (hp10Trigger)
        {
            hp10Trigger = false;
            hp10Cnt++;
            SetBossBehavior(BossState.Chase);
            yield break;
        }

        // �� 4�� ������ ���� ����
        if (hp10Cnt == 4)
        {
            hp10Cnt = 0;
            SetBossBehavior(BossState.Chase);
            yield break;
        }

        // �ǰ� 50�� �￴���� ���� ����
        if (hpHalfTrigger)
        {
            hpHalfTrigger = false;
            SetBossBehavior(BossState.Chase);
            yield break;
        }

        // ���� ���ǵ鿡 ���� �ٸ��� ���� 
        SetBossBehavior(GetRandomAction());
    }

    // hp10�� ����� ����Ǵ� ���ϼ���
    private void SetHP10()
    {
        hp10Trigger = true;
    }

    // hp50�� ����� ����Ǵ� ���ϼ���
    private void SetHPHalf()
    {
        hpHalfTrigger = true;
    }
}
