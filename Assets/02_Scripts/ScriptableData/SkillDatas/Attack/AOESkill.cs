using System.Collections;
using UnityEngine;

/// <summary>
/// ���� ���� ���� ���� �ð����� ������ ���ϴ� ��ų.
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
            // �浹�� ������� ���
            foreach (Collider hitCollider in hits)
            {
                Debug.Log(hitCollider.gameObject.name);
            }

            yield return waitSec;

            currentTime += damageTickTime;
        }
    }
}
