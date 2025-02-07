using System.Collections;
using UnityEngine;
using EnumTypes;

public class PlayerAttackManager
{
    private PlayerManager playerManager = null;
    private MeleeWeapon meleeWeapon = null;

    public void Init(PlayerManager _playerManager)
    {
        playerManager = _playerManager;
        meleeWeapon = playerManager.PlayerMeleeWeapon;
    }

    public void AddDamageToBoss(int _damage, float _aggro)
    {
        GameManager.Instance.DamageToBoss(playerManager, _damage, _aggro);
    }

    public void EnableMeleeAttack(int _damage, float _aggro)
    {
        meleeWeapon.Init(_damage, _aggro);
        meleeWeapon.SetTriggerEnabled(true);
    }

    public void DisableMeleeAttack()
    {
        meleeWeapon.SetTriggerEnabled(false);
    }

    public Vector3 GetMeleeWeaponPostion()
    {
        return meleeWeapon.transform.position;
    }

    /// <summary>
    /// 플레이어가 데미지를 받음.
    /// </summary>
    /// <param name="_damage"></param>
    public void TakeDamage(int _damage, Vector3 _attackOriginPos, float _distance)
    {
        if (playerManager.StateMachine.CurrentState.StateType == PlayerStateType.Damaged
            ||
            playerManager.StateMachine.CurrentState.StateType == PlayerStateType.Death
            ||
            playerManager.StateMachine.CurrentState.StateType == PlayerStateType.Dash)
        {
            Debug.Log("Player is not damageable!");
            return;
        }

        playerManager.StatusManager.OnDamaged(_damage);
        KnockbackPlayer(_attackOriginPos, _distance);
        playerManager.ChangeState(PlayerStateType.Damaged);
    }

    public void KnockbackPlayer(Vector3 _attackOriginPos, float _distance)
    {
        playerManager.StartCoroutine(KnockBackCoroutine(_attackOriginPos, _distance));
    }

    public bool IsPlayerBehindBoss()
    {
        if (Vector3.Angle(
            playerManager.transform.forward, 
            GameManager.Instance.GetBossTransform().forward) < 80f)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 플레이어를 넉백시키는 코루틴
    /// </summary>
    /// <param name="_attackOriginPos"></param>
    /// <param name="_distance"></param>
    /// <returns></returns>
    private IEnumerator KnockBackCoroutine(Vector3 _attackOriginPos, float _distance)
    {
        float knockbackTime = 0.5f;
        float currentTime = 0f;
        float speed = _distance / knockbackTime;

        Vector3 direction = playerManager.transform.position - _attackOriginPos;
        direction.y = 0f;
        direction.Normalize();

        Debug.LogFormat("Direction : {0}, speed : {1}", direction, speed);

        while (currentTime <= knockbackTime)
        {
            playerManager.GetComponent<CharacterController>().Move(direction * speed * Time.deltaTime);
            currentTime += Time.deltaTime;
            yield return null;
        }
    }
}
