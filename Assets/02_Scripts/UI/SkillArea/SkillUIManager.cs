using System.Collections.Generic;
using UnityEngine;

using EnumTypes;
using StructTypes;

/// <summary>
/// ��ų ���� �� ���� ���� UI ���� �����ϱ� ���� �Ŵ��� Ŭ����.
/// </summary>
public class SkillUIManager : MonoBehaviour
{
    [SerializeField]
    private List<SkillUIData> skillUIList = null;

    private Dictionary<SkillSlot, SkillUI_Base> skillUIMap = null;

    public Dictionary<SkillSlot, SkillUI_Base> SkillUIMap
    {
        get { return skillUIMap; }
    }

    /// <summary>
    /// ��ų ���� Ȥ�� ��ų ��� ������ ��� ���θ� �����Ѵ�.
    /// </summary>
    /// <param name="_type">��ų Ÿ��</param>
    /// <param name="_enabled">��� ����</param>
    public void SetSkillUIEnabled(SkillSlot _type, bool _enabled)
    {
        if(skillUIMap.TryGetValue(_type, out var skillUI))
        {
            skillUI.SetEnabled(_enabled);
        }
        else
        {
            Debug.LogFormat("{0} type skill don't need to show skill area!", _type);
        }
    }

    /// <summary>
    /// ��ų ���� Ȥ�� ��ų ��� ������ �Է¹��� ����ŭ �����δ�.
    /// </summary>
    /// <param name="_type">��ų Ÿ��</param>
    /// <param name="_horizontal">���� �Է°�</param>
    /// <param name="_vertical">���� �Է°�</param>
    public void SetSkillAimPoint(SkillSlot _type, float _horizontal, float _vertical)
    {
        if (skillUIMap.TryGetValue(_type, out var skillUI))
        {
            skillUI.AimSkill(_horizontal,_vertical);
        }
    }

    /// <summary>
    /// ��ų�� ���� ���� Ȥ�� ��ų ��� ������ �����Ѵ�.
    /// </summary>
    /// <param name="_slot">��ų Ÿ��</param>
    /// <returns>��ų ���� or ��ų ����(Euler Angle)</returns>
    public SkillPointData GetSkillAimPoint(SkillSlot _slot)
    {
        if (skillUIMap.TryGetValue(_slot, out var skillUI))
        {
            return skillUI.GetSkillAimPoint();
        }
        else
        {
            Debug.LogFormat("{0} type skill don't need to show skill area!", _slot);

            return new SkillPointData();
        }
    }

    /// <summary>
    /// ������ ��ų UI �ܿ� �ٸ� ��ų UI�� Ȱ��ȭ�Ǿ��ִ��� Ȯ���Ѵ�.
    /// </summary>
    /// <param name="_slot">���� ��ų ����</param>
    /// <param name="_otherSkillSlot">�ٸ� Ȱ��ȭ���� ��ų UI�� ����</param>
    /// <returns>�ٸ� ��ų UI�� Ȱ��ȭ ����</returns>
    public bool IsOtherSkillUIEnabled(SkillSlot _slot, out SkillSlot _otherSkillSlot)
    {
        foreach(var skillUIPair in skillUIMap)
        {
            if (skillUIPair.Key != _slot && skillUIPair.Value.IsEnabled())
            {
                _otherSkillSlot = skillUIPair.Key;
                return true;
            }
        }

        _otherSkillSlot = SkillSlot.None;

        return false;
    }

    public void SetAllSkillUIEnabled(bool _enabled)
    {
        if(skillUIMap != null)
        {
            foreach (var skillUI in skillUIMap.Values)
            {
                if(skillUI != null)
                    skillUI.SetEnabled(_enabled);
            }
        }
    }

    private void Awake()
    {
        skillUIMap = new Dictionary<SkillSlot, SkillUI_Base>();

        foreach(var skillUIData in skillUIList)
        {
            skillUIMap.Add(skillUIData.skillType, skillUIData.skillUI);
        }
    }
}
