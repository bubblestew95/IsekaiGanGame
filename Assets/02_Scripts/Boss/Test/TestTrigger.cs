using UnityEngine;

public class TestTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "BossAttack")
        {
            if (transform.tag == "BehindRock")
            {
                Debug.Log("���ڿ� ��� �ȸ���");
                return;
            }
                float damage = other.GetComponent<BossAttackCollider>().Damage;
            Debug.Log("������ ����" + damage);
        }
    }
}
