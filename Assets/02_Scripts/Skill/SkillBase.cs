using UnityEngine;

public abstract class SkillBase
{
    protected PlayerManager playerManager = null;

    public SkillBase(PlayerManager _playerManager)
    {
        playerManager = _playerManager;
    }

    public abstract void UseSkill();

    public virtual void EndSkill()
    {

    }
}
