using System.Collections.Generic;
using UnityEngine;

public class BossSkillManager : MonoBehaviour
{
    [SerializeField] private List<BossSkillData> skillDatas;
    private List<BossSkill> skills = new List<BossSkill>();

    public List<BossSkill> Skills { get { return skills; } }

    private void Start()
    {
        Init();
    }

    private void Init()
    {
        foreach (BossSkillData skillData in skillDatas)
        {
            skills.Add(new BossSkill(skillData));
        }
    }

    // ��ų ��ٿ����� Ȯ���ϴ� �Լ�
    public List<BossSkill> IsSkillCooldown(List<BossSkill> _skills)
    {
        List<BossSkill> availList = new List<BossSkill>();

        foreach (BossSkill skill in _skills)
        {
            if (skill.CheckCooldown())
            {
                availList.Add(skill);
            }
        }

        return availList;
    }

    // ��ų ��Ÿ� �ȿ� �ִ��� Ȯ���ϴ� �Լ�
    public List<BossSkill> IsSkillInRange(float _range, List<BossSkill> _skills)
    {
        List<BossSkill> availList = new List<BossSkill>();

        foreach (BossSkill skill in _skills)
        {
            if (skill.CheckRange(_range))
            {
                availList.Add(skill);
            }
        }
        return availList;
    }
}
