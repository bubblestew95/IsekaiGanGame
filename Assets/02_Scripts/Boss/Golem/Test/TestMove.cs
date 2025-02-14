using UnityEngine;

public class TestMove : MonoBehaviour
{
    public float moveSpeed = 5f;

    void Update()
    {
        // ����Ű �Է� �ޱ�
        float horizontal = Input.GetAxis("Horizontal"); // A, D �Ǵ� �¿� ȭ��ǥ
        float vertical = Input.GetAxis("Vertical"); // W, S �Ǵ� ���� ȭ��ǥ

        // �̵� ���� ����
        Vector3 movement = new Vector3(horizontal, 0, vertical) * moveSpeed * Time.deltaTime;

        // Transform�� �̿��� �÷��̾� �̵�
        transform.Translate(movement);
    }
}
