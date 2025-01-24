using EnumTypes;
using UnityEngine;

public class IdleState : BasePlayerState
{
    public IdleState(PlayerManager playerMng) : base(playerMng)
    {
    }

    public override void OnEnterState()
    {
        
    }

    public override void OnExitState()
    {
        
    }

    public override void OnUpdateState()
    {
        // ��� ������ ���� ��ų�� ��� ������. ��ų ��� üũ.
        SkillType skillType = playerMng.GetNextSkill();

        if(skillType != SkillType.None)
        {
            // �̵� �ٲ�� ��.
            playerMng.UseSkill(skillType);
        }
    }
}
