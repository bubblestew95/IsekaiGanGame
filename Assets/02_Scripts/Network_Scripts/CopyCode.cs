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
        // �ȵ���̵忡�� Ŭ������ �����ϴ� �ڵ�
        using (AndroidJavaClass clipboardClass = new AndroidJavaClass("android.content.ClipboardManager"))
        using (AndroidJavaObject currentActivity = GetCurrentActivity())
        using (AndroidJavaObject clipboard = currentActivity.Call<AndroidJavaObject>("getSystemService", "clipboard"))
        {
            AndroidJavaObject clip = new AndroidJavaObject("android.content.ClipData", "label", text);
            clipboard.Call("setPrimaryClip", clip);
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
