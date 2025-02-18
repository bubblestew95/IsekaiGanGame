using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class MushAttackJump : MonoBehaviour
{
    [SerializeField] private int damage = 0;
    [SerializeField] private float dis = 0f;
    [SerializeField] private float TickTime = 1f;

    private float stayTime = 0f;
    private GameObject child;

    private void Start()
    {
        child = transform.GetChild(0).gameObject;

        StartCoroutine(ChangeColliderCoroutine());
    }

    private void Update()
    {
        if (child == null) Destroy(gameObject);
    }

    // �ݶ��̴� ���� �ٲٴ� �ڷ�ƾ
    private IEnumerator ChangeColliderCoroutine()
    {
        yield return new WaitForSeconds(0.3f);

        ChangeColliderInfo();
    }

    // �������� �ݶ��̴� ���� �ٲ�
    private void ChangeColliderInfo()
    {
        MushAttackCollider col = GetComponent<MushAttackCollider>();

        col.Damage = 0;
        col.KnockBackDistance = 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player" && gameObject.tag == "BossAttack" && other.gameObject.GetComponent<NetworkObject>().OwnerClientId == NetworkManager.Singleton.LocalClientId)
        {
            // �÷��̾� ������ �Ե��� ����
            GameManager.Instance.DamageToPlayer(other.gameObject.GetComponent<PlayerManager>(), damage);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Player" && gameObject.tag == "BossAttack" && other.gameObject.GetComponent<NetworkObject>().OwnerClientId == NetworkManager.Singleton.LocalClientId)
        {
            stayTime += Time.deltaTime;

            if (stayTime > TickTime)
            {
                // �÷��̾� ������ �Ե��� ����
                GameManager.Instance.DamageToPlayer(other.gameObject.GetComponent<PlayerManager>(), damage);
                stayTime = 0f;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        stayTime = 0f;
    }
}

