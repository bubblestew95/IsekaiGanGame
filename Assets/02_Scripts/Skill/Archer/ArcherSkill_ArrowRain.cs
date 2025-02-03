using System.Collections;
using UnityEngine;

public class ArcherSkill_ArrowRain : SkillBase
{
    public float duration = 5f;

    public ArcherSkill_ArrowRain(PlayerManager _playerManager) : base(_playerManager)
    {
    }

    public override void UseSkill()
    {
        
    }

    private IEnumerator SkillCoroutine()
    {


        yield return new WaitForSeconds(duration);
    }
}
