using EnumTypes;
using UnityEngine;

/// <summary>
/// ��ų ���� �� ���� ���� UI���� ���� �θ�.
/// </summary>
public abstract class SkillUI_Base : MonoBehaviour
{
    [SerializeField]
    protected SkillType skillType = SkillType.None;
}
