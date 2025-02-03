using EnumTypes;
using UnityEngine;

/// <summary>
/// ��ų ���� �� ���� ���� UI���� ���� �θ�.
/// </summary>
public abstract class SkillUI_Base : MonoBehaviour
{
    /// <summary>
    /// ��ų�� Ư�� ��ġ Ȥ�� ������ ���� �����Ѵ�.
    /// </summary>
    public virtual void AimSkill(Vector3 _aim)
    {
    }

    public virtual void SetEnabled(bool _enabled)
    {
    }
}
