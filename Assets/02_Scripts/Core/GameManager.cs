using Unity.Netcode;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region Singleton Variable, Properties
    private static GameManager instance = null;

    public static GameManager Instance
    {
        get { return instance; }
    }
    #endregion

    #region Inspector Variables

    [SerializeField]
    private bool isLocalGame = false;

    [SerializeField]
    private bool isPCMode = false;

    #endregion

    #region Private Variables

    private BossStateManager bossStateManager = null;
    private NetworkGameManager networkGameManager = null;

    #endregion

    #region Properties

    public bool IsLocalGame
    {
        get { return isLocalGame; }
    }

    public bool IsPCMode
    {
        get { return isPCMode; }
    }

    #endregion

    #region Public Functions

    public Transform GetBossTransform()
    {
        return bossStateManager.transform;
    }

    public int GetBossHp()
    {
        return -1;
    }

    public void DamageToBoss(PlayerManager _damageGiver, int _damage, float _aggro)
    {
        ulong clientId = _damageGiver.GetComponent<NetworkObject>().OwnerClientId;

        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            DamageToBoss_Multi(clientId, _damage, _aggro);
        }
    }


    public void DamageToPlayer(PlayerManager _damageReceiver, int _damage)
    {
        if (isLocalGame)
            ApplyDamageToPlayer(_damageReceiver, _damage);
        else
        {
            networkGameManager.OnPlayerDamaged(_damageReceiver, _damage);
        }
    }


    public void DamageToPlayer(PlayerManager _damageReceiver, int _damage, Vector3 _attackPos, float _knockBackDist)
    {
        DamageToPlayer(_damageReceiver, _damage);

        if (isLocalGame)
        {
            ApplyKnockbackToPlayer(_damageReceiver, _attackPos, _knockBackDist);
        }
        else
        {
            networkGameManager.OnPlayerKnockback(_damageReceiver, _attackPos, _knockBackDist);
        }
    }

    public void DamageToBoss_Multi(ulong _clientId, int _damage, float _aggro)
    {
        bossStateManager.BossDamageReceiveServerRpc(_clientId, _damage, _aggro);
    }


    public void ApplyDamageToPlayer
        (PlayerManager _damageReceiver, int _damage)
    {
        _damageReceiver.AttackManager.TakeDamage(_damage);

        UpdatePlayerHpUI(_damageReceiver);
    }

    public void ApplyKnockbackToPlayer(PlayerManager _target, Vector3 _attackPos, float _knockbackDist)
    {
        if(_target.StateMachine.CurrentState.StateType == EnumTypes.PlayerStateType.Damaged
            || _target.StateMachine.CurrentState.StateType == EnumTypes.PlayerStateType.Death
            || _target.StateMachine.CurrentState.StateType == EnumTypes.PlayerStateType.Dash)
        {
            return;
        }

        _target.AttackManager.KnockbackPlayer(_attackPos, _knockbackDist);
    }

    #endregion

    #region Private Functions

    private void UpdatePlayerHpUI(PlayerManager _player)
    {
        _player.BattleUIManager.UpdatePlayerHp();
    }

    private void UpdateBossHpUI(PlayerManager _player)
    {
        _player.BattleUIManager.UpdateBossHp();
    }

    #endregion

    #region Unity Callbacks

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        bossStateManager = FindAnyObjectByType<BossStateManager>();
        networkGameManager = FindAnyObjectByType<NetworkGameManager>();
    }

    #endregion

}
