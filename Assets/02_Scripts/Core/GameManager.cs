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
    private NetworkGameManager networkGameManager = null;

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
    /// �÷��̾ �������� �������� ��׷� ��ġ�� ����.
    /// </summary>
    /// <param name="_damageSource">�������� �������� �ִ� �÷��̾�</param>
    public void DamageToBoss(PlayerManager _damageGiver, int _damage, float _aggro)
    {
        if (_damageGiver.NetworkController.IsClient)
        {
            ulong clientId = _damageGiver.GetComponent<NetworkObject>().OwnerClientId;

            DamageToBoss_Multi(clientId, _damage, _aggro);
        }

        // UI ����ȭ
        // ���� ��Ƽ ������ �� �� �κ��� �Ƹ��� �����ؾ� �� ��?
        // UpdatePlayerHpUI(_damageGiver);
    }

    /// <summary>
    /// �÷��̾ �������� ����.
    /// </summary>
    /// <param name="_damageReceiver"></param>
    public void DamageToPlayer(PlayerManager _damageReceiver, int _damage, Vector3 _attackPos, float _knockBackDist)
    {
        if (isLocalGame)
            ApplyDamageToPlayer(_damageReceiver, _damage, _attackPos, _knockBackDist);
        else
        {
            networkGameManager.OnPlayerDamaged(_damageReceiver, _damage, _attackPos, _knockBackDist);
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
