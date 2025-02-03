using UnityEngine;

public class TestBackRock : MonoBehaviour
{
    public Transform boss; // ���� ��ġ
    public GameObject player;

    void Update()
    {
        Vector3 bossDir = (boss.position - transform.position).normalized; // �� �� ���� ����
        Vector3 playerDir = (player.transform.position - transform.position).normalized;

        // ���� �յ� �Ǻ� (���� ����� ���� �������� Ȯ��)
        float dot = Vector3.Dot(bossDir, playerDir);

        if (dot >= 0)
        {
            Debug.Log(player.name + "�÷��̾�� ������");
        }
        else
        {
            Debug.Log(player.name + "�÷��̾�� ������");
        }


    }
}
