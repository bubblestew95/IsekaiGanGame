using UnityEngine;

public class BossAttackCollider : MonoBehaviour
{
    public delegate void AttackColliderDelegate();
    public AttackColliderDelegate rockCollisionCallback;

    private float damage = 0f;
    private string skillName = string.Empty;

    public float Damage 
    { 
        get { return damage; } 
        set { damage = value; } 
    }

    public string SkillName
    {
        get { return skillName; }
        set { skillName = value; }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.GetMask("Player"))
        {
            // 플레이어 데미지 입도록 설정
        }

        if (skillName == "Attack8" && other.tag == "Rock")
        {
            rockCollisionCallback?.Invoke();
        }
    }
}
