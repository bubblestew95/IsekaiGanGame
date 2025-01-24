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
            Debug.Log("A ����");
            agentGraph.BlackboardReference.SetVariableValue("BossState", GetRandomAction());
        }
    }

    // Ư�����ǿ��� ������ �ൿ�� �ϳ� �����ϴ� �Լ�
    // ��Ÿ�Ӱ� ��Ÿ� ����� ������ ���¸� enum������ ������.
    private BossState GetRandomAction()
    {
        tmpList.Clear();

        float dis = GetDisWithoutY();

        tmpList = BossSkillUtils.GetAvailableSkillsInRange(dis, skills);

        foreach (BossSkillCooldown skill in tmpList)
        {
            Debug.Log("��Ÿ� üũ�� ��ų :" +  skill);
        }

        tmpList = BossSkillUtils.GetSkillsCooldownOn(tmpList);

        foreach (BossSkillCooldown skill in tmpList)
        {
            Debug.Log("��Ÿ�� üũ�� ��ų :" + skill);
        }

        int randomIndex = UnityEngine.Random.Range(0, tmpList.Count);

        return (BossState)Enum.Parse(typeof(BossState), tmpList[randomIndex].bossSkillData.SkillName);
    }

    // ������ �÷��̾� ������ �Ÿ� ���
    private float GetDisWithoutY()
    {
        Vector2 bossPos2D = new Vector2(Boss.transform.position.x, Boss.transform.position.z);
        Vector2 playerPos2D = new Vector2(aggroPlayer.transform.position.x, aggroPlayer.transform.position.z);

        return Vector2.Distance(bossPos2D, playerPos2D);
    }
}
