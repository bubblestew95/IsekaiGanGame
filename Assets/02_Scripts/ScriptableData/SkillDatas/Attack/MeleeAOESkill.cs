using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "MeleeAOESkill", menuName = "Scriptable Objects/Player Skill/MeleeAOESkill")]
public class MeleeAOESkill : AttackSkill
{
    public float attackAreaRadius = 1f;
    public float duration = 2f;
    public float damageTickTime = 0.5f;

    public override void UseSkill(PlayerManager _player)
    {
        base.UseSkill(_player);
        _player.StartCoroutine(TickDamageCoroutine(_player.GetMeleeWeaponPostion()));
    }

    private IEnumerator TickDamageCoroutine(Vector3 _damagePos)
    {
        float currentTime = 0f;
        WaitForSeconds waitSec = new WaitForSeconds(damageTickTime);

        Ray ray = new Ray(_damagePos, Vector3.up);

        while (currentTime <= duration)
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
