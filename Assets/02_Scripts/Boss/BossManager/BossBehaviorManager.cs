using System;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;

public class BossBehaviorManager : MonoBehaviour
{
    [SerializeField] private List<BossSkillCooldown> skills;
    [SerializeField] private GameObject aggroPlayer;
    [SerializeField] private GameObject Boss;
    [SerializeField] private BehaviorGraphAgent agentGraph;

    private List<BossSkillCooldown> tmpList = new List<BossSkillCooldown>();

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            Debug.Log("A 누름");
            agentGraph.BlackboardReference.SetVariableValue("BossState", GetRandomAction());
        }
    }

    // 특정조건에서 랜덤한 행동을 하나 선택하는 함수
    // 쿨타임과 사거리 계산후 랜덤한 상태를 enum값으로 가져옴.
    private BossState GetRandomAction()
    {
        tmpList.Clear();

        float dis = GetDisWithoutY();

        tmpList = BossSkillUtils.GetAvailableSkillsInRange(dis, skills);

        foreach (BossSkillCooldown skill in tmpList)
        {
            Debug.Log("사거리 체크후 스킬 :" +  skill);
        }

        tmpList = BossSkillUtils.GetSkillsCooldownOn(tmpList);

        foreach (BossSkillCooldown skill in tmpList)
        {
            Debug.Log("쿨타임 체크후 스킬 :" + skill);
        }

        int randomIndex = UnityEngine.Random.Range(0, tmpList.Count);

        return (BossState)Enum.Parse(typeof(BossState), tmpList[randomIndex].bossSkillData.SkillName);
    }

    // 보스와 플레이어 사이의 거리 계산
    private float GetDisWithoutY()
    {
        Vector2 bossPos2D = new Vector2(Boss.transform.position.x, Boss.transform.position.z);
        Vector2 playerPos2D = new Vector2(aggroPlayer.transform.position.x, aggroPlayer.transform.position.z);

        return Vector2.Distance(bossPos2D, playerPos2D);
    }
}
