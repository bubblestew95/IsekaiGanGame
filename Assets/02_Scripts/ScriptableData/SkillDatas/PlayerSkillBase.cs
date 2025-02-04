using UnityEngine;

using EnumTypes;

public abstract class PlayerSkillBase : ScriptableObject
{
    public SkillSlot skillSlot;

    public float coolTime = 3f;

    public ParticleSystem skillParticle = null;

    /// <summary>
    /// ��ų �ִϸ��̼� ���� �� �� �Լ��� ȣ���ϸ� ��.
    /// </summary>
    /// <param name="_player"></param>
    public virtual void StartSkill(PlayerManager _player)
    {
    }
    /// <summary>
    /// �ִϸ��̼ǿ��� �� �Լ��� �̺�Ʈ�� ȣ���ؼ� ��ų ����� ó���ϸ� ��.
    /// </summary>
    /// <param name="_player">��ų�� ����ϴ� �÷��̾�</param>
    /// <param name="_order">�ִϸ��̼� �� �� ��ų�� ������ ����� �ٸ��� ó���ϰ� ������ �̰� ���� ��.</param>
    public virtual void UseSkill(PlayerManager _player)
    {
    }

    /// <summary>
    /// ��ų �ִϸ��̼��� ������ �� ���� Ư���� ���� �ϰ� ������ �̰� ���� ��.
    /// </summary>
    /// <param name="_player"></param>
    public virtual void EndSkill(PlayerManager _player)
    {

    }
}
