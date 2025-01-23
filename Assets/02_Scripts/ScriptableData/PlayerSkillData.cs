using UnityEngine;

using EnumTypes;

[CreateAssetMenu(fileName = "PlayerSkillData", menuName = "Scriptable Objects/PlayerSkillData")]
public class PlayerSkillData : ScriptableObject
{
    public int skillIdx = 0;

    public float damage = 0f;

    public float coolTime = 0f;

    public SkillRangeType rangeType = SkillRangeType.None;

    public SkillActivatedType activatedType = SkillActivatedType.Casting;
}
