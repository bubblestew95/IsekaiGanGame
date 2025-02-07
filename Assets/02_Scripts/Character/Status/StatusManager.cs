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
    /// ���ʿ��� �������� ���ϴ� �Լ�. ���� ���¿� ȸ�� �����϶��� �������� �� �ش�.
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
    /// ���ʿ��� ���� Status Effect�� ���� ���� ������ �������� �ش�.
    /// </summary>
    /// <param name="_damage"></param>
    public void OnDamagedAbsolute(int _damage)
    {
        SetCurrentHp(currentHp - _damage);
    }

    /// <summary>
    /// ���� ���ڷ� ���� ����Ʈ Ÿ���� ���������� ���θ� �����ϴ� �Լ�.
    /// </summary>
    /// <param name="_effectType">�˻��ϰ��� �ϴ� ����Ʈ Ÿ��</param>

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
