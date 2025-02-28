using System.Diagnostics;
using System.Threading;
using UnityEngine;

public class GameExitManager : MonoBehaviour
{
    private static GameExitManager instance;
    private Thread backgroundThread;
    private bool isRunning = true;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // �� ��ȯ�ص� ����
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // ��׶��� �۾� ����
        backgroundThread = new Thread(SomeBackgroundTask);
        backgroundThread.Start();
    }

    private void SomeBackgroundTask()
    {
        while (isRunning)
        {
            Thread.Sleep(1000); // CPU ������ ����
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) // ESC Ű�� ����
        {
            QuitGame();
        }
    }

    public void QuitGame()
    {
        //Debug.Log("���� ����!");
        isRunning = false;

        if (backgroundThread != null && backgroundThread.IsAlive)
        {
            backgroundThread.Join(); // �����ϰ� ���� ���
            backgroundThread = null;
        }

        Application.Quit();
        Process.GetCurrentProcess().Kill(); // ���� ����
    }

    void OnApplicationQuit()
    {
        //Debug.Log("���� ���� ����, ��׶��� ���� ��...");
        isRunning = false;

        if (backgroundThread != null && backgroundThread.IsAlive)
        {
            backgroundThread.Join();
            backgroundThread = null;
        }
    }
}
