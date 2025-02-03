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

    public Transform debugTr = null;

    private Dictionary<SkillType, SkillUI_Base> skillUIMap = null;

    public void SetSkillUIEnabled(SkillType _type, bool _enabled)
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

    public void SetSkillAimPoint(SkillType _type, float _horizontal, float _vertical)
    {
        if (skillUIMap.TryGetValue(_type, out var skillUI))
        {
            skillUI.AimSkill(_horizontal,_vertical);
        }
        else
        {
            Debug.LogFormat("{0} type skill don't need to show skill area!", _type);
        }
    }

    public Vector3 GetSkillAimPoint(SkillType _type)
    {
        if (skillUIMap.TryGetValue(_type, out var skillUI))
        {
            return skillUI.GetSkillAimPoint();
        }
        else
        {
            Debug.LogFormat("{0} type skill don't need to show skill area!", _type);

            return Vector3.zero;
        }
    }

    private void Awake()
    {
        skillUIMap = new Dictionary<SkillType, SkillUI_Base>();

        foreach(var skillUIData in skillUIList)
        {
            skillUIMap.Add(skillUIData.skillType, skillUIData.skillUI);
        }
    }
}
