using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSkillData", menuName = "Scriptable Objects/PlayerSkillData")]
public class PlayerSkillData : ScriptableObject
{
    [System.Serializable]
    public enum SkillRangeType
    {
        None
    }

    [System.Serializable]
    public enum SkillActivatedType
    {
        Casting,
        Holding,
        Immediately
    }

    public AnimationClip clip = null;

    public float damage = 0f;

    public float coolTime = 0f;

    public SkillRangeType rangeType = SkillRangeType.None;

    public SkillActivatedType activatedType = SkillActivatedType.Casting;
}
