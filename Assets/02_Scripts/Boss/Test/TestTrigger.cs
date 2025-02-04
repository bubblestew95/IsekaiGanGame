using System.Collections;
using UnityEngine;

public class TestTrigger : MonoBehaviour
{
    private bool isFire = false;

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

        if (other.gameObject.tag == "Fire")
        {
            Debug.Log("�Ѷ߰�");
            if (!isFire)
            {
                StartCoroutine(FireDamage());
            }
        }
    }

    private IEnumerator FireDamage()
    {
        isFire = true;

        float elapseTime = 0f;
        int tickCnt = 0;

        // 0.5�ʴ� 1ƽ, 6ƽ�� �Ҳ���
        while (true)
        {
            elapseTime += Time.deltaTime;

            if (elapseTime >= 0.5f)
            {
                elapseTime = 0f;
                tickCnt++;
                Debug.Log("ƽ�� " + tickCnt + "�� ����");
            }

            if (tickCnt == 6)
            {
                break;
            }

            yield return null;
        }

        Debug.Log("ȭ�� ����");

        isFire = false;
    }
}
