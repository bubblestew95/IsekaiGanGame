using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BossBehaviorManager : NetworkBehaviour
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
        if (IsServer)
        {
            bossBT.behaviorEndCallback += () => StartCoroutine(BossPerformAction());
            bossStateManager.bossHp10Callback += SetHP10;
            bossStateManager.bossHpHalfCallback += SetHPHalf;
            bossStateManager.bossStunCallback += SetStun;
            bossStateManager.bossDieCallback += SetDie;
        }
    }

    // Ư�����ǿ��� ������ �ൿ�� �ϳ� �����ϴ� �Լ�
    // ��Ÿ��� ��Ÿ�� ����� ������ ���¸� enum������ ����
    private BossState GetRandomAction()
    {
        Debug.Log("GetRandomAction�����");

        tmpList.Clear();

        float dis = bossStateManager.GetDisWithoutY();

        tmpList = bossSkillManager.IsSkillInRange(dis, bossSkillManager.RandomSkills);
        tmpList = bossSkillManager.IsSkillCooldown(tmpList);
        tmpList = bossSkillManager.CheckBackAttack(tmpList, bossStateManager.Players, bossStateManager.Boss);

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
        Debug.Log("BossPerformAction�����");

        // ���� �� ������
        yield return delay1f;

        // �ǰ� 10�� �￴���� ���� ����
        if (hp10Trigger)
        {
            hp10Trigger = false;
            hp10Cnt++;
            SetBossBehavior(BossState.Attack6);
            yield break;
        }

        // �� 4�� ������ ���� ����
        if (hp10Cnt == 4)
        {
            hp10Cnt = 0;
            SetBossBehavior(BossState.Attack5);
            yield break;
        }

        // �ǰ� 50�� �￴���� ���� ����
        if (hpHalfTrigger)
        {
            hpHalfTrigger = false;
            SetBossBehavior(BossState.Phase2);
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

    // ������ �����϶�
    private void SetStun()
    {
        SetBossBehavior(BossState.Stun);
    }

    // ������ �׾�����
    private void SetDie()
    {
        SetBossBehavior(BossState.Die);
    }
}
