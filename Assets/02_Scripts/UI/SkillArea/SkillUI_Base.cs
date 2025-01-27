using EnumTypes;
using UnityEngine;

/// <summary>
/// 스킬 범위 및 방향 지정 UI들의 공통 부모.
/// </summary>
public abstract class SkillUI_Base : MonoBehaviour
{
    [SerializeField]
    protected SkillType skillType = SkillType.None;
}
