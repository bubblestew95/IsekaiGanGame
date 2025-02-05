using UnityEngine;

public class RockDestroy : MonoBehaviour
{
    private bool donDestory = true;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "BossAttack")
        {
            string skill = other.GetComponent<BossAttackCollider>().SkillName;

            if (skill == "Attack5" || skill == "SpecialAttack" || skill == "Attack6" || skill == "Attack7" || skill == "Attack8")
            {
                Debug.Log("�� �μ��� ȣ��");

                if (skill == "Attack6" && donDestory)
                {
                    donDestory = false;
                    return;
                }

                Destroy(gameObject);
            }
        }
    }
}
