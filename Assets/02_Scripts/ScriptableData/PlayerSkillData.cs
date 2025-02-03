using UnityEngine;

using EnumTypes;

[CreateAssetMenu(fileName = "PlayerSkillData", menuName = "Scriptable Objects/PlayerSkillData")]
public class PlayerSkillData : ScriptableObject
{
    public SkillType skillType;

    public float damage = 5f;

    public float aggro = 5f;

    public float coolTime = 3f;

    public PlayerSkillRangeType rangeType = PlayerSkillRangeType.None;

    public PlayerSkillActivatedType activatedType = PlayerSkillActivatedType.Casting;

    public ParticleSystem skillParticle = null;
}
