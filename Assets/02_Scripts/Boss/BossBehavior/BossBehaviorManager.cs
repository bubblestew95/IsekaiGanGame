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

    // 특정조건에서 랜덤한 행동을 하나 선택하는 함수
    // 사거리와 쿨타임 계산후 랜덤한 상태를 enum값으로 리턴
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

    // 보스가 특정 행동을 하도록 설정하는 함수
    private void SetBossBehavior(BossState _state)
    {
        bossBT.curState = _state;
    }

    // 보스가 특정행동 하도록 최종실행시키는 함수
    private IEnumerator BossPerformAction()
    {
        // 패턴 후 딜레이
        yield return delay1f;

        // 피가 10퍼 깍였을때 쓰는 패턴
        if (hp10Trigger)
        {
            hp10Trigger = false;
            hp10Cnt++;
            SetBossBehavior(BossState.Chase);
            yield break;
        }

        // 돌 4번 던진후 쓰는 패턴
        if (hp10Cnt == 4)
        {
            hp10Cnt = 0;
            SetBossBehavior(BossState.Chase);
            yield break;
        }

        // 피가 50퍼 깍였을때 쓰는 패턴
        if (hpHalfTrigger)
        {
            hpHalfTrigger = false;
            SetBossBehavior(BossState.Chase);
            yield break;
        }

        // 각종 조건들에 따라 다르게 실행 
        SetBossBehavior(GetRandomAction());
    }

    // hp10퍼 까였을때 실행되는 패턴설정
    private void SetHP10()
    {
        hp10Trigger = true;
    }

    // hp50퍼 까였을때 실행되는 패턴설정
    private void SetHPHalf()
    {
        hpHalfTrigger = true;
    }
}
