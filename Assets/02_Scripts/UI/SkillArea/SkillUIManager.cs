using System.Collections.Generic;
using UnityEngine;

using EnumTypes;
using StructTypes;

/// <summary>
/// 스킬 범위 및 방향 지정 UI 들을 관리하기 위한 매니저 클래스.
/// </summary>
public class SkillUIManager : MonoBehaviour
{
    [SerializeField]
    private List<SkillUIData> skillUIList = null;

    private Dictionary<SkillSlot, SkillUI_Base> skillUIMap = null;

    /// <summary>
    /// 스킬 범위 혹은 스킬 사용 방향의 출력 여부를 설정한다.
    /// </summary>
    /// <param name="_type">스킬 타입</param>
    /// <param name="_enabled">출력 여부</param>
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
    /// 스킬 범위 혹은 스킬 사용 방향을 입력받은 값을 통해 설정한다.
    /// </summary>
    /// <param name="_type">스킬 타입</param>
    /// <param name="_horizontal">가로 입력값</param>
    /// <param name="_vertical">세로 입력값</param>
    public void SetSkillAimPoint(SkillSlot _type, float _horizontal, float _vertical)
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
    /// 스킬이 사용될 범위 혹은 스킬 사용 방향을 리턴한다.
    /// </summary>
    /// <param name="_type">스킬 타입</param>
    /// <returns>스킬 범위 or 스킬 방향(Euler Angle)</returns>
    public SkillPointData GetSkillAimPoint(SkillSlot _type)
    {
        if (skillUIMap.TryGetValue(_type, out var skillUI))
        {
            return skillUI.GetSkillAimPoint();
        }
        else
        {
            Debug.LogFormat("{0} type skill don't need to show skill area!", _type);

            return new SkillPointData();
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
