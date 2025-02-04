using UnityEngine;

public class TestTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "BossAttack")
        {
            if (transform.tag == "BehindRock")
            {
                Debug.Log("돌뒤에 숨어서 안맞음");
                return;
            }
                float damage = other.GetComponent<BossAttackCollider>().Damage;
            Debug.Log("데미지 받음" + damage);
        }
    }
}
