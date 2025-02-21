using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class FollowCamera : MonoBehaviour
{
    private PlayerManager playerManager = null;
    private BossStateManager bossStateManager = null;
    private MushStateManager mushStateManager = null;
    private int myIndex;
    private bool IsDie = false;
    private GameObject DieUi;

    // ī�޶� ���� ����
    public Transform Cam;
    public float shakeAmout = 0.2f;
    public float shakeTime = 0.2f;

    private GameObject[] alivePlayer;

    private void Awake()
    {
        FindAnyObjectByType<NetworkGameManager>().loadingFinishCallback += FindPlayerObjectForClient;
        FindAnyObjectByType<NetworkGameManager>().gameEndCallback += DieUiOff;
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
                break;
            }

            yield return checkTime;
        }

        yield return checkTime;

        IsDie = true;
        PlayerDieSetting();
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

    // �÷��̾� �׾����� UI Ű�� �Լ�
    private void PlayerDieSetting()
    {
        DieUi = GameObject.Find("ObserverMode");
        DieUi.transform.GetChild(0).gameObject.SetActive(true);
        DieUi.transform.GetChild(1).gameObject.SetActive(true);
        DieUi.transform.GetChild(2).gameObject.SetActive(true);
        DieUi.transform.GetChild(3).gameObject.SetActive(true);

        Button LeftBtn = DieUi.transform.Find("Left").gameObject.GetComponent<Button>();
        Button RightBtn = DieUi.transform.Find("Right").gameObject.GetComponent<Button>();
        LeftBtn.onClick.AddListener(ChangePlayerCamLeft);
        RightBtn.onClick.AddListener(ChangePlayerCamRight);
    }

    // ���� �������� ui����
    private void DieUiOff()
    {
        DieUi.SetActive(false);
    }
}
