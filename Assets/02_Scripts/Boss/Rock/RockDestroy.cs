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
                Debug.Log("락 부서짐 호출");

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
