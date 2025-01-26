using System.Collections.Generic;
using UnityEngine;

public static class BossSkillUtils
{
    /// <summary>
    /// 사거리 내에 사용가능한 스킬 리스트를 반환
    /// </summary>
    /// <param name="_range"> 현재 보스와 어그로 플레이어의 거리 </param>
    ///     /// <param name="_skills"> 사정거리 체크할 스킬리스트 </param>
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
    /// 쿨타임 돌아서 사용가능한 스킬들을 반환
    /// </summary>
    /// <param name="_skills"> 쿨타임 체크할 스킬리스트 </param>
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
