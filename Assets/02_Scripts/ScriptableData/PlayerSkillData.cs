using UnityEngine;

using EnumTypes;

[CreateAssetMenu(fileName = "PlayerSkillData", menuName = "Scriptable Objects/PlayerSkillData")]
public class PlayerSkillData : ScriptableObject
{
    public SkillType skillType;

    public float damage = 0f;

    public float coolTime = 0f;

    public PlayerSkillRangeType rangeType = PlayerSkillRangeType.None;

    public PlayerSkillActivatedType activatedType = PlayerSkillActivatedType.Casting;
}
