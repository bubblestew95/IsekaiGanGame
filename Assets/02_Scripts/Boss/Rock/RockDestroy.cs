using UnityEngine;

public class RockDestroy : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "BossAttack")
        {
            string skill = other.GetComponent<BossAttackCollider>().SkillName;

            if (skill == "Attack5")
            {
                Destroy(gameObject);
            }
        }
    }
}
