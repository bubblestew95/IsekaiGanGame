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

    /// <summary>
    /// ��ų ���� Ȥ�� ��ų ��� ������ ��� ���θ� �����Ѵ�.
    /// </summary>
    /// <param name="_type">��ų Ÿ��</param>
    /// <param name="_enabled">��� ����</param>
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

    /// <summary>
    /// ��ų ���� Ȥ�� ��ų ��� ������ �Է¹��� ���� ���� �����Ѵ�.
    /// </summary>
    /// <param name="_type">��ų Ÿ��</param>
    /// <param name="_horizontal">���� �Է°�</param>
    /// <param name="_vertical">���� �Է°�</param>
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

    /// <summary>
    /// ��ų�� ���� ���� Ȥ�� ��ų ��� ������ �����Ѵ�.
    /// </summary>
    /// <param name="_type">��ų Ÿ��</param>
    /// <returns>��ų ���� or ��ų ����(Euler Angle)</returns>
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
