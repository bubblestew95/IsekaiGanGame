using UnityEngine;

public class BossAttackCollider : MonoBehaviour
{
    public delegate void AttackColliderDelegate();
    public AttackColliderDelegate rockCollisionCallback;

    private int damage = 0;
    private string skillName = string.Empty;

    public int Damage 
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
        if (other.gameObject.tag == "Player")
        {
            // 플레이어 데미지 입도록 설정
            GameManager.Instance.DamageToPlayer(other.gameObject.GetComponent<PlayerManager>(), damage);
        }

        if (skillName == "Attack8" && other.tag == "Rock" && gameObject.tag == "BossAttack")
        {
            rockCollisionCallback?.Invoke();
        }
    }
}
