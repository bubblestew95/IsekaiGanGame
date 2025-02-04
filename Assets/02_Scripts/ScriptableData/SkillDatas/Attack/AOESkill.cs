using System.Collections;
using UnityEngine;

/// <summary>
/// 일정 범위 내에 일정 시간동안 공격을 가하는 스킬.
/// </summary>
[CreateAssetMenu(fileName = "AOESkill", menuName = "Scriptable Objects/Player Skill/AOE")]
public class AOESkill : AttackSkill
{
    public float maxRange = 5f;
    public float attackAreaRadius = 1f;
    public float duration = 2f;
    public float damageTickTime = 0.5f;

    public override void UseSkill(PlayerManager _player, float multiply)
    {
        base.UseSkill(_player, multiply);
        _player.StartCoroutine(TickDamageCoroutine(_player.LastSkillUsePoint));
    }

    private IEnumerator TickDamageCoroutine(Vector3 _damagePos)
    {
        float currentTime = 0f;
        WaitForSeconds waitSec = new WaitForSeconds(damageTickTime);

        Ray ray = new Ray(_damagePos, Vector3.up);

        while(currentTime <= duration)
        {
            Collider[] hits = Physics.OverlapSphere(_damagePos, attackAreaRadius);
            // 충돌이 검출됐을 경우
            foreach (Collider hitCollider in hits)
            {
                Debug.Log(hitCollider.gameObject.name);
            }

            yield return waitSec;

            currentTime += damageTickTime;
        }
    }
}
