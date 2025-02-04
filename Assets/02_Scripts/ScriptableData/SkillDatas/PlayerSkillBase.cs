using UnityEngine;

using EnumTypes;

public abstract class PlayerSkillBase : ScriptableObject
{
    public SkillType skillType;

    public float coolTime = 3f;

    public ParticleSystem skillParticle = null;

    /// <summary>
    /// 애니메이션에서 이 함수를 이벤트로 호출해서 스킬 사용을 처리하면 됨.
    /// </summary>
    /// <param name="_player">스킬을 사용하는 플레이어</param>
    /// <param name="_order">애니메이션 중 각 스킬의 데미지 배수를 다르게 처리하고 싶으면 이거 쓰면 됨.</param>
    public abstract void UseSkill(PlayerManager _player, float multiply);

    /// <summary>
    /// 스킬 애니메이션이 끝났을 때 뭔가 특정한 일을 하고 싶으면 이걸 쓰면 됨.
    /// </summary>
    /// <param name="_player"></param>
    public virtual void EndSkill(PlayerManager _player)
    {

    }
}
