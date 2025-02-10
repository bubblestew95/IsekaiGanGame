using System;
using Unity.Multiplayer.Center.NetcodeForGameObjectsExample;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    #region Singleton Variable, Properties
    private static GameManager instance = null;

    public static GameManager Instance
    {
        get { return instance; }
    }
    #endregion

    #region Inspector Variables
    #endregion

    #region Private Variables

    private BossStateManager bossStateManager = null;

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
        Debug.LogFormat("Player deal to boss! damage : {0}, aggro : {1}", _damage, _aggro);

        ulong clientId = _damageGiver.GetComponent<NetworkObject>().OwnerClientId;

        if (true)//(_damageGiver.IsClient)
        {
            DamageToBoss_Multi(clientId, _damage, _aggro);
        }

        // UI ����ȭ
        // ���� ��Ƽ ������ �� �� �κ��� �Ƹ��� �����ؾ� �� ��?
        // UpdatePlayerHpUI(_damageGiver);
    }

    /// <summary>
    /// ������ �÷��̾�� �������� ����.
    /// </summary>
    /// <param name="_damageReceiver"></param>
    public void DamageToPlayer(PlayerManager _damageReceiver, int _damage, Vector3 _attackPos, float _knockBackDis)
    {
        _damageReceiver.AttackManager.TakeDamage(_damage, _attackPos, _knockBackDis);

        UpdatePlayerHpUI(_damageReceiver);
    }

    public void DamageToBoss_Multi(ulong _clientId, int _damage, float _aggro)
    {
        bossStateManager.BossDamageReceiveServerRpc(_clientId, _damage, _aggro);
    }

    public void DamageToPlayer_Multi(PlayerManager _damageReceiver, int _damage, Vector3 _attackPos, float _knockBackDis)
    {
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
