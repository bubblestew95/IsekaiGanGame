using UnityEngine;

public class BossSkill
{
    private BossSkillData skillData;
    private float lastUsedTime;

    public BossSkillData SkillData {  get { return skillData; } }

    public BossSkill(BossSkillData _data)
    {
        skillData = _data;
        lastUsedTime = -skillData.CoolDown;
    }

    // ��ų�� ����.
    public void UseSkill()
    {
        if (CheckCooldown())
        {
            lastUsedTime = Time.time;
        }
    }

    // ��ų �����ִ��� check
    public bool CheckCooldown()
    {
        return Time.time >= lastUsedTime + skillData.CoolDown;
    }

    // ��Ÿ� üũ
    public bool CheckRange(float _range)
    {
        if (skillData.MinRange <= _range && skillData.MaxRange >= _range)
        {
            return true;
        }

        return false;
    }
}
