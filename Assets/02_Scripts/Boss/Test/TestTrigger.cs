using UnityEngine;

public class TestTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "BossAttack")
        {
            float damage = other.GetComponent<BossAttackCollider>().Damage;
            Debug.Log("������ ����" + damage);
        }
    }
}
