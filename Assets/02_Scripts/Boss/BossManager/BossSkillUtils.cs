using System.Collections.Generic;
using System.Diagnostics;

public static class BossSkillUtils
{
    private static List<BossSkillCooldown> availList = new List<BossSkillCooldown>();

    /// <summary>
    /// ��Ÿ� ���� ��밡���� ��ų ����Ʈ�� ��ȯ
    /// </summary>
    /// <param name="_range"> ���� ������ ��׷� �÷��̾��� �Ÿ� </param>
    ///     /// <param name="_skills"> �����Ÿ� üũ�� ��ų����Ʈ </param>
    public static List<BossSkillCooldown> GetAvailableSkillsInRange(float _range, List<BossSkillCooldown> _skills)
    {
        availList.Clear();

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
        availList.Clear();

        foreach (BossSkillCooldown skill in _skills)
        {

                availList.Add(skill);
            
        }

        return availList;
    }
}
