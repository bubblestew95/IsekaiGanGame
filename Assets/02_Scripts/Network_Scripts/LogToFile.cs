using UnityEngine;
using System.IO;

public class LogToFile : MonoBehaviour
{
    private static string logFilePath;

    void Awake()
    {
        logFilePath = Application.persistentDataPath + "/GameLog.txt";
        Debug.Log("�α� ���� ���: " + logFilePath);

        // ���� �α� ���� ���� �� ���� ����
        if (File.Exists(logFilePath))
        {
            File.Delete(logFilePath);
        }

        Application.logMessageReceived += HandleLog;
    }

    void OnDestroy()
    {
        Application.logMessageReceived -= HandleLog;
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        File.AppendAllText(logFilePath, logString + "\n");
    }
}
