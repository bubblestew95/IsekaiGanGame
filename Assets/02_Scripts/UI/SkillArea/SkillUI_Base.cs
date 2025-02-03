using EnumTypes;
using UnityEngine;

/// <summary>
/// 스킬 범위 및 방향 지정 UI들의 공통 부모.
/// </summary>
public abstract class SkillUI_Base : MonoBehaviour
{
    /// <summary>
    /// 스킬을 특정 위치 혹은 방향을 향해 조준한다.
    /// </summary>
    public abstract void AimSkill(float _horizontal, float _vertical);
    /// <summary>
    /// 지정된 스킬 범위 위치 혹은 스킬 방향을 리턴한다.
    /// </summary>
    public abstract Vector3 GetSkillAimPoint();

    public abstract void SetEnabled(bool _enabled);
}
