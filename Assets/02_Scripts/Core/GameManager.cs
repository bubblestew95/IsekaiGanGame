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
    private bool isLocalGame = false;
    #endregion

    #region Private Variables

    private BossStateManager bossStateManager = null;

    #endregion

    #region Properties

    public bool IsLocalGame
    {
        get { return isLocalGame; }
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

    /// <summary>
    /// 플레이어가 보스에게 데미지와 어그로 수치를 더함.
    /// </summary>
    /// <param name="_damageSource">보스에게 데미지를 주는 플레이어</param>
    public void DamageToBoss(PlayerManager _damageGiver, int _damage, float _aggro)
    {
        Debug.LogFormat("Player deal to boss! damage : {0}, aggro : {1}", _damage, _aggro);

        ulong clientId = _damageGiver.GetComponent<NetworkObject>().OwnerClientId;

        if (_damageGiver.GetComponent<PlayerNetworkManager>().IsClient)
        {
            DamageToBoss_Multi(clientId, _damage, _aggro);
        }

        // UI 동기화
        // 추후 멀티 연동할 때 이 부분은 아마도 수정해야 할 듯?
        // UpdatePlayerHpUI(_damageGiver);
    }

    /// <summary>
    /// 플레이어가 데미지를 입음.
    /// </summary>
    /// <param name="_damageReceiver"></param>
    public void DamageToPlayer(PlayerManager _damageReceiver, int _damage, Vector3 _attackPos, float _knockBackDist)
    {
        if (isLocalGame)
            ApplyDamageToPlayer(_damageReceiver, _damage, _attackPos, _knockBackDist);
        else
        {

        }
    }

    public void DamageToBoss_Multi(ulong _clientId, int _damage, float _aggro)
    {
        bossStateManager.BossDamageReceiveServerRpc(_clientId, _damage, _aggro);
    }

    public void ApplyDamageToPlayer
        (PlayerManager _damageReceiver, int _damage, Vector3 _attackPos, float _knockbackDist)
    {
        _damageReceiver.AttackManager.TakeDamage(_damage, _attackPos, _knockbackDist);

        UpdatePlayerHpUI(_damageReceiver);
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
        if(instance != null)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            instance = this;
        }
    }

    private void Start()
    {
        bossStateManager = FindAnyObjectByType<BossStateManager>();
    }

    #endregion

}
