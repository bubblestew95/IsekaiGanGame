using System.Collections;
using UnityEngine;
using EnumTypes;
using Unity.VisualScripting;

public class PlayerAttackManager
{
    private PlayerManager playerManager = null;
    private CharacterController characterController = null;
    private MeleeWeapon meleeWeapon = null;
    private Transform rangeAttackTransform = null;

    public Transform RangeAttackTransform
    {
        get { return rangeAttackTransform; }
    }

    public void Init(PlayerManager _playerManager)
    {
        playerManager = _playerManager;
        characterController = playerManager.GetComponent<CharacterController>();
        meleeWeapon = playerManager.PlayerMeleeWeapon;
        rangeAttackTransform = playerManager.RangeAttackStartTr;
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

    public bool IsMeleeWeaponSet()
    {
        return meleeWeapon != null;
    }

    /// <summary>
    /// �÷��̾ �������� ����.
    /// </summary>
    /// <param name="_damage"></param>
    public void TakeDamage(int _damage)
    {
        //if (playerManager.StateMachine.CurrentState.StateType == PlayerStateType.Damaged
        //    ||
        //    playerManager.StateMachine.CurrentState.StateType == PlayerStateType.Death
        //    ||
        //    playerManager.StateMachine.CurrentState.StateType == PlayerStateType.Dash)
        //{
        //    Debug.Log("Player is not damageable!");
        //    return;
        //}

        playerManager.StatusManager.OnDamaged(_damage);
    }

    /// <summary>
    /// �÷��̾�� �˹� ȿ���� �ο���.
    /// </summary>
    /// <param name="_attackOriginPos"></param>
    /// <param name="_distance"></param>
    public void KnockbackPlayer(Vector3 _attackOriginPos, float _distance)
    {
        // playerManager.ChangeState(PlayerStateType.Damaged);

        playerManager.AnimationManager.PlayDamagedAnimation();

        if (characterController.enabled)
        {
            playerManager.StartCoroutine(KnockBackCoroutine(_attackOriginPos, _distance));
        }
    }

    /// <summary>
    /// �÷��̾ ���� ������ �ڿ� ��ġ�ϰ� �ִ��� ���θ� ������.
    /// </summary>
    /// <returns></returns>
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
    /// �÷��̾� �˹� �� �з����� ȿ���� �ִ� �ڷ�ƾ.
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
