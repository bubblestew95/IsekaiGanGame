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

    private void Awake()
    {
        skillUIMap = new Dictionary<SkillType, SkillUI_Base>();

        foreach(var skillUIData in skillUIList)
        {
            skillUIMap.Add(skillUIData.skillType, skillUIData.skillUI);
        }
    }
}
