using UnityEngine;

using EnumTypes;

public class StatusManager
{
    private StatusEffectType currentStatusEffect = StatusEffectType.Normal;

    private PlayerManager playerMng = null;

    private int maxHp = 0;
    private int currentHp = 0;

    public void Init(PlayerManager _playerMng)
    {
        playerMng = _playerMng;
        maxHp = _playerMng.PlayerData.maxHp;
        currentHp = maxHp;
    }

    public void SetCurrentHp(int _hp)
    {
        currentHp = Mathf.Clamp(_hp, 0, maxHp);

        if (currentHp <= 0)
            playerMng.OnPlayerDead?.Invoke();
    }

    public void SetMaxHp(int _maxHp)
    {
        maxHp = Mathf.Clamp(_maxHp, 1, maxHp);

        if (currentHp > maxHp)
            SetCurrentHp(maxHp);
    }

    public void OnDamaged(int _damage)
    {
        if(currentStatusEffect == StatusEffectType.Immune || currentStatusEffect == StatusEffectType.Evade)
        {
            return;
        }

        SetCurrentHp(currentHp - _damage);
    }


}
