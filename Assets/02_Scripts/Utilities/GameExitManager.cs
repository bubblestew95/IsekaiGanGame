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
            DontDestroyOnLoad(gameObject); // 씬 전환해도 유지
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 백그라운드 작업 시작
        backgroundThread = new Thread(SomeBackgroundTask);
        backgroundThread.Start();
    }

    private void SomeBackgroundTask()
    {
        while (isRunning)
        {
            Thread.Sleep(1000); // CPU 과부하 방지
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) // ESC 키로 종료
        {
            QuitGame();
        }
    }

    public void QuitGame()
    {
        //Debug.Log("게임 종료!");
        isRunning = false;

        if (backgroundThread != null && backgroundThread.IsAlive)
        {
            backgroundThread.Join(); // 안전하게 종료 대기
            backgroundThread = null;
        }

        Application.Quit();
        Process.GetCurrentProcess().Kill(); // 완전 종료
    }

    void OnApplicationQuit()
    {
        //Debug.Log("게임 종료 감지, 백그라운드 정리 중...");
        isRunning = false;

        if (backgroundThread != null && backgroundThread.IsAlive)
        {
            backgroundThread.Join();
            backgroundThread = null;
        }
    }
}
