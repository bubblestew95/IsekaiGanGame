using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CopyCode : MonoBehaviour
{
    public Button copyButton;  // ���� ��ư
    public TMP_Text textToCopy;    // ������ �ؽ�Ʈ�� ����ִ� Text ������Ʈ

    void Start()
    {
        // ��ư Ŭ�� �� �ؽ�Ʈ ����
        copyButton.onClick.AddListener(CopyTextToClipboard);
    }

    void CopyTextToClipboard()
    {
        string textToCopyString = textToCopy.text;

#if UNITY_ANDROID
        // �ȵ���̵忡�� Ŭ������ ����
        CopyTextToClipboardAndroid(textToCopyString);
#else
        // PC������ �⺻ Unity ��� ���
        GUIUtility.systemCopyBuffer = textToCopyString;
#endif

        Debug.Log("����� �ؽ�Ʈ: " + textToCopyString);
    }

    void CopyTextToClipboardAndroid(string text)
    {
        try
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (AndroidJavaObject context = currentActivity.Call<AndroidJavaObject>("getApplicationContext"))
            using (AndroidJavaObject clipboardManager = context.Call<AndroidJavaObject>("getSystemService", "clipboard"))
            using (AndroidJavaClass clipDataClass = new AndroidJavaClass("android.content.ClipData"))
            {
                using (AndroidJavaObject clipData = clipDataClass.CallStatic<AndroidJavaObject>("newPlainText", "label", text))
                {
                    clipboardManager.Call("setPrimaryClip", clipData);
                }
            }

            Debug.Log("�ؽ�Ʈ�� Ŭ�����忡 ����Ǿ����ϴ�: " + text);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Ŭ������ ���� �� ���� �߻�: " + e.Message);
        }
    }




    AndroidJavaObject GetCurrentActivity()
    {
        using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            return unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        }
    }
}
