using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    private PlayerManager playerManager = null;
    private BossStateManager bossStateManager = null;
    private MushStateManager mushStateManager = null;
    private int myIndex;
    private bool IsDie = false;

    // ī�޶� ���� ����
    public Transform Cam;
    private float shakeAmout = 0.2f;
    private float shakeTime = 0.2f;

    private GameObject[] alivePlayer;

    private void Awake()
    {
        FindAnyObjectByType<NetworkGameManager>().loadingFinishCallback += FindPlayerObjectForClient;
        bossStateManager = FindAnyObjectByType<BossStateManager>();
        mushStateManager = FindAnyObjectByType<MushStateManager>();
    }

    private void Update()
    {
        if(playerManager != null)
            transform.position = playerManager.transform.position;
    }


    private void FindPlayerObjectForClient()
    {
        // �� id ��������
        ulong myClientId = NetworkManager.Singleton.LocalClientId;

        // ���� ã��
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            Debug.Log("�������� Ŭ�� ���̵� : " + client.ClientId);

            if (client.ClientId == myClientId)
            {
                playerManager = client.PlayerObject.GetComponent<PlayerManager>();
            }
        }

        Invoke("SetPlayerInfo", 1f);
    }

    // �÷��̾� ���� �����ϴ� �Լ�
    private void SetPlayerInfo()
    {
        if (bossStateManager != null)
        {
            alivePlayer = bossStateManager.AlivePlayers;
        }
        else if (mushStateManager != null)
        {
            alivePlayer = mushStateManager.AlivePlayers;
        }

        for (int i = 0; i < 4; ++i)
        {
            if (alivePlayer[i] == null) continue;

            if (alivePlayer[i].GetComponent<NetworkObject>().OwnerClientId == NetworkManager.Singleton.LocalClientId)
            {
                myIndex = i;
            }
        }

        StartCoroutine(DieCheckCoroutine());
    }

    // ī�޶� �ٲٴ� �Լ�
    private void ChangePlayerCamRight()
    {
        if (IsDie)
        {
            int startIndex = myIndex; // ���� ���� ������ (��ü ��ȸ �Ŀ��� �� ã���� ����)

            do
            {
                // ���� �ε����� �̵� (���� ��ȯ)
                myIndex = (myIndex + 1) % alivePlayer.Length;

                // ���� ��ȿ�� ������Ʈ�� Ż��
                if (alivePlayer[myIndex] != null)
                {
                    playerManager = alivePlayer[myIndex].GetComponent<PlayerManager>();
                    break;
                }

            } while (myIndex != startIndex); // ��� ��Ҹ� �������� ���� 

        }
    }

    // ī�޶� �ٲٴ� �Լ�2
    private void ChangePlayerCamLeft()
    {
        if (IsDie)
        {
            int startIndex = myIndex; // ���� ���� ������ (��ü ��ȸ �Ŀ��� �� ã���� ����)

            do
            {
                // ���� �ε����� �̵� (���� ��ȯ)
                myIndex = (myIndex - 1 + alivePlayer.Length) % alivePlayer.Length;

                // ���� ��ȿ�� ������Ʈ�� Ż��
                if (alivePlayer[myIndex] != null)
                {
                    playerManager = alivePlayer[myIndex].GetComponent<PlayerManager>();
                    break;
                }

            } while (myIndex != startIndex); // ��� ��Ҹ� �������� ���� 

        }
    }

    // ���� �׾����� check�ϴ� �Լ�
    private IEnumerator DieCheckCoroutine()
    {
        WaitForSeconds checkTime = new WaitForSeconds(1f);
        while (true)
        {
            if (alivePlayer[myIndex] == null)
            {
                IsDie = true;
            }

            yield return checkTime;
        }
    }

    // ī�޶� ���� �Լ�
    public IEnumerator ShakeCam()
    {
        float elapseTime = 0f;

        Vector3 originPos = Cam.localPosition;

        while (true)
        {
            elapseTime += Time.deltaTime;

            Cam.localPosition = Random.insideUnitSphere * shakeAmout + originPos;

            if (elapseTime >= shakeTime)
            {
                break;
            }

            yield return null;
        }

        Cam.localPosition = originPos;
    }
}
