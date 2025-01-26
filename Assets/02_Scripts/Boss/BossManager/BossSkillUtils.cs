using System.Collections.Generic;
using UnityEngine;

public static class BossSkillUtils
{
    /// <summary>
    /// ��Ÿ� ���� ��밡���� ��ų ����Ʈ�� ��ȯ
    /// </summary>
    /// <param name="_range"> ���� ������ ��׷� �÷��̾��� �Ÿ� </param>
    ///     /// <param name="_skills"> �����Ÿ� üũ�� ��ų����Ʈ </param>
    public static List<BossSkillCooldown> GetAvailableSkillsInRange(float _range, List<BossSkillCooldown> _skills)
    {
        List<BossSkillCooldown> availList = new List<BossSkillCooldown>();

        foreach (BossSkillCooldown skill in _skills)
        {
            if (_range >= skill.bossSkillData.MinRange && _range <= skill.bossSkillData.MaxRange)
            {
                availList.Add(skill);
            }
        }

        return availList;
    }

    /// <summary>
    /// ��Ÿ�� ���Ƽ� ��밡���� ��ų���� ��ȯ
    /// </summary>
    /// <param name="_skills"> ��Ÿ�� üũ�� ��ų����Ʈ </param>
    public static List<BossSkillCooldown> GetSkillsCooldownOn(List<BossSkillCooldown> _skills)
    {
        List<BossSkillCooldown> availList = new List<BossSkillCooldown>();

        foreach (BossSkillCooldown skill in _skills)
        {
            if (skill.IsCooldownOver())
            {
                availList.Add(skill);
            }
        }

        return availList;
    }
}
