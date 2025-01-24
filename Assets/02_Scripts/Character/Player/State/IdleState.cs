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
        // 대기 상태일 때만 스킬이 사용 가능함. 스킬 사용 체크.
        SkillType skillType = playerMng.GetNextSkill();

        if(skillType != SkillType.None)
        {
            // 이따 바꿔야 함.
            playerMng.UseSkill(skillType);
        }
    }
}
