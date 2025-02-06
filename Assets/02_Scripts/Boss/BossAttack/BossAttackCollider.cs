using UnityEngine;

public class BossAttackCollider : MonoBehaviour
{
    public delegate void AttackColliderDelegate();
    public AttackColliderDelegate rockCollisionCallback;

    private int damage = 0;
    private string skillName = string.Empty;
    private float knockBackDistance = 0f;

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

    public float KnockBackDistance
    {
        get { return knockBackDistance; }
        set { knockBackDistance = value; }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player" && gameObject.tag == "BossAttack")
        {
            // �÷��̾� ������ �Ե��� ����
            GameManager.Instance.DamageToPlayer(other.gameObject.GetComponent<PlayerManager>(), damage, transform.position, KnockBackDistance);
        }

        if (skillName == "Attack8" && other.tag == "Rock" && gameObject.tag == "BossAttack")
        {
            rockCollisionCallback?.Invoke();
        }
    }
}
