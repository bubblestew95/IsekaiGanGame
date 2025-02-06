using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "RangeHoldingSkill", menuName = "Scriptable Objects/Player Skill/RangeHolding")]
public class RangeHoldingSkill : AttackSkill
{
    [Header("Range Holding Skill")]
    public float duration = 2f;
    public float damageTickTime = 0.5f;
    public float maxDistance = 5f;

    public override void UseSkill(PlayerManager _player)
    {
        base.UseSkill(_player);

        _player.StartCoroutine(SkillHoldingCoroutine(_player));
    }

    public override void EndSkill(PlayerManager _player)
    {
        base.EndSkill(_player);
    }

    private IEnumerator SkillHoldingCoroutine(PlayerManager _player)
    {
        float currentTime = 0f;
        WaitForSeconds waitSec = new WaitForSeconds(damageTickTime);

        Vector3 direction = _player.LastSkillUsePoint;

        Ray ray = new Ray(_player.RangeAttackStartTr.position, direction);
        RaycastHit hit;

        while (currentTime <= duration)
        {
            if(Physics.Raycast(ray, out hit, maxDistance))
            {
                _player.AddDamageToBoss(damage, aggro);
            }

            yield return waitSec;

            currentTime += damageTickTime;
        }
    }
}
