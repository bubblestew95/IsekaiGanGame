using UnityEngine;
using System.Collections;
using Unity.Netcode;

public class CreateFire : MonoBehaviour
{
    public GameObject firePrefab; // fire ������
    public int fireCount = 10; // ������ ����
    public float spawnRangeX = 17.5f; // ���� ����
    public float spawnRangeZ = 15f;
    public float spawnTime = 30f; // 30�ʸ��� ����

    private void Start()
    {
        StartCoroutine(SpawnFiresRoutine()); // �ڷ�ƾ ����
    }

    private IEnumerator SpawnFiresRoutine()
    {
        while (true) // ���� ����
        {
            SpawnFires();
            yield return new WaitForSeconds(spawnTime); // 30�� ���
        }
    }

    private void SpawnFires()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            for (int i = 0; i < fireCount; i++)
            {
                // XZ ��鿡�� ������ ��ġ ����
                float randomX = Random.Range(-spawnRangeX, spawnRangeX);
                float randomZ = Random.Range(-spawnRangeZ, spawnRangeZ);
                Vector3 spawnPosition = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

                // fire ������ ����
                GameObject fire = Instantiate(firePrefab, spawnPosition, Quaternion.Euler(270, 0, 0));

                fire.GetComponent<NetworkObject>().Spawn(true);
            }
        }
    }
}
