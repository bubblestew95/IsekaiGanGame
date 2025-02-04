using EnumTypes;
using StructTypes;
using UnityEngine;

/// <summary>
/// ��ų ���� �� ���� ���� UI���� ���� �θ�.
/// </summary>
public abstract class SkillUI_Base : MonoBehaviour
{
    /// <summary>
    /// ��ų�� Ư�� ��ġ Ȥ�� ������ ���� �����Ѵ�.
    /// </summary>
    public abstract void AimSkill(float _horizontal, float _vertical);
    /// <summary>
    /// ������ ��ų ���� ��ġ Ȥ�� ��ų ������ �����Ѵ�.
    /// </summary>
    public abstract SkillPointData GetSkillAimPoint();

    public abstract void SetEnabled(bool _enabled);
}
