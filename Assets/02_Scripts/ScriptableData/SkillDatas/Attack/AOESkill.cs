using System.Collections;
using UnityEngine;

/// <summary>
/// ���� ���� ���� ���� �ð����� ������ ���ϴ� ��ų.
/// </summary>
[CreateAssetMenu(fileName = "AOESkill", menuName = "Scriptable Objects/Player Skill/AOE")]
public class AOESkill : AttackSkill
{
    [Header("AOE Skill")]
    public float maxRange = 5f;
    public float attackAreaRadius = 1f;
    public float duration = 2f;
    public float damageTickTime = 0.5f;

    public override void UseSkill(PlayerManager _player)
    {
        base.UseSkill(_player);

        _player.StartCoroutine(TickDamageCoroutine(_player));
    }

    private IEnumerator TickDamageCoroutine(PlayerManager _player)
    {
        float currentTime = 0f;
        WaitForSeconds waitSec = new WaitForSeconds(damageTickTime);
        Vector3 damagePos = _player.LastSkillUsePoint;

        while (currentTime <= duration)
        {
            Collider[] hits = Physics.OverlapSphere(damagePos, attackAreaRadius, bossLayerMask);
            // �浹�� ������� ���
            foreach (Collider hitCollider in hits)
            {
                _player.AddDamageToBoss(DamageCalculate(_player), aggro);
            }

            yield return waitSec;

            currentTime += damageTickTime;
        }
    }
}
