using UnityEngine;

using EnumTypes;

public abstract class PlayerSkillBase : ScriptableObject
{
    public SkillType skillType;

    public float coolTime = 3f;

    public ParticleSystem skillParticle = null;

    /// <summary>
    /// �ִϸ��̼ǿ��� �� �Լ��� �̺�Ʈ�� ȣ���ؼ� ��ų ����� ó���ϸ� ��.
    /// </summary>
    /// <param name="_player">��ų�� ����ϴ� �÷��̾�</param>
    /// <param name="_order">�ִϸ��̼� �� �� ��ų�� ������ ����� �ٸ��� ó���ϰ� ������ �̰� ���� ��.</param>
    public abstract void UseSkill(PlayerManager _player, float multiply);

    /// <summary>
    /// ��ų �ִϸ��̼��� ������ �� ���� Ư���� ���� �ϰ� ������ �̰� ���� ��.
    /// </summary>
    /// <param name="_player"></param>
    public virtual void EndSkill(PlayerManager _player)
    {

    }
}
