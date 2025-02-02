using UnityEngine;

public class BossAttackCollider : MonoBehaviour
{
    private float damage = 0f;

    public float Damage 
    { 
        get { return damage; } 
        set { damage = value; } 
    }
}
