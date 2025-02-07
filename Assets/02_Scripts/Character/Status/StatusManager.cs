using System.Collections.Generic;
using UnityEngine;

using EnumTypes;

public class StatusManager
{
    private List<StatusEffectType> currentStatusEffects = null;

    private PlayerManager playerMng = null;

    private int maxHp = 0;
    private int currentHp = 0;

    public int MaxHp
    {
        get { return maxHp; }
    }

    public int CurrentHp
    {
        get { return currentHp; }
    }

    public void Init(PlayerManager _playerMng)
    {
        playerMng = _playerMng;
        maxHp = _playerMng.PlayerData.maxHp;
        currentHp = maxHp;
        currentStatusEffects = new List<StatusEffectType>();

    }

    public void SetCurrentHp(int _hp)
    {
        currentHp = Mathf.Clamp(_hp, 0, maxHp);

        if (currentHp <= 0)
            playerMng.ChangeState(PlayerStateType.Death);
    }

    public void SetMaxHp(int _maxHp)
    {
        maxHp = Mathf.Clamp(_maxHp, 1, maxHp);
        
        if (currentHp > maxHp)
            SetCurrentHp(maxHp);
    }

    /// <summary>
    /// 오너에게 데미지를 가하는 함수. 무적 상태와 회피 상태일때는 데미지를 안 준다.
    /// </summary>
    /// <param name="_damage"></param>
    public void OnDamaged(int _damage)
    {
        if(IsStatusActivated(StatusEffectType.Immune) || IsStatusActivated(StatusEffectType.Evade))
        {
            Debug.Log("On Damaged Character is now immune or evade status!");
            return;
        }

        SetCurrentHp(currentHp - _damage);
    }

    /// <summary>
    /// 오너에게 현재 Status Effect와 관계 없이 무조건 데미지를 준다.
    /// </summary>
    /// <param name="_damage"></param>
    public void OnDamagedAbsolute(int _damage)
    {
        SetCurrentHp(currentHp - _damage);
    }

    /// <summary>
    /// 현재 인자로 들어온 이펙트 타입이 적용중인지 여부를 리턴하는 함수.
    /// </summary>
    /// <param name="_effectType">검색하고자 하는 이펙트 타입</param>

    public bool IsStatusActivated(StatusEffectType _effectType)
    {
        if(currentStatusEffects == null)
        {
            Debug.LogWarning("Current Status Effects List is not valid!");
            return false;
        }

        return currentStatusEffects.Contains(_effectType);
    }
}
