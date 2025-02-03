using UnityEngine;

using EnumTypes;

public abstract class PlayerSkillBase : ScriptableObject
{
    public SkillType skillType;

    // public float damage = 5f;

    // public float aggro = 5f;

    public float coolTime = 3f;

    public ParticleSystem skillParticle = null;

    public abstract void UseSkill(PlayerManager _player, int _order);

    public virtual void EndSkill(PlayerManager _player)
    {

    }
}
