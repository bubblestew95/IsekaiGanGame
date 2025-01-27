using UnityEngine;
using System.Collections;

public class BossAttackCollider : MonoBehaviour
{
    [SerializeField] private GameObject[] Players;

    [SerializeField] private Material attackMat;

    private Vector3 attack1Rot;

    // Attack1 시작시 보스의 앞방향
    private void Attack1Rot()
    {
        attack1Rot = transform.forward;
    }

    // 보스 공격 콜라이더 생성 및 파괴
    private void Attack1()
    {
        GameObject AttackCollider = GameObject.CreatePrimitive(PrimitiveType.Cylinder);

        AttackCollider.tag = "BossAttack";
        AttackCollider.transform.position = transform.position + attack1Rot * 1f;
        AttackCollider.transform.localScale = new Vector3 (3f, 0.5f, 3f);
        AttackCollider.GetComponent<Collider>().isTrigger = true;
        AttackCollider.GetComponent<Renderer>().material = attackMat;
    }

    private void Attack2()
    {
        foreach(GameObject player in Players)
        {
            GameObject AttackCollider = GameObject.CreatePrimitive(PrimitiveType.Cylinder);

            AttackCollider.tag = "BossAttack";
            AttackCollider.transform.position = player.transform.position;
            AttackCollider.transform.localScale = new Vector3(3f, 0.5f, 3f);
            AttackCollider.GetComponent<Collider>().isTrigger = true;
            AttackCollider.GetComponent<Renderer>().material = attackMat;
        }
    }
}
