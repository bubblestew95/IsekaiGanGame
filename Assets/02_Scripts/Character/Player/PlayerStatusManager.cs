using UnityEngine;

using EnumTypes;

public class PlayerStatusManager
{
    private int maxHp = 0;
    private int currentHp = 0;

    public void Init(int _maxHp)
    {
        maxHp = _maxHp;
        currentHp = maxHp;
    }

    public void SetCurrentHp(int _hp)
    {
        currentHp = Mathf.Clamp(_hp, 0, maxHp);
    }

    public void SetMaxHp(int _maxHp)
    {
        maxHp = Mathf.Clamp(_maxHp, 1, maxHp);
    }

    public void OnDamaged(int _hp)
    {

    }
}
