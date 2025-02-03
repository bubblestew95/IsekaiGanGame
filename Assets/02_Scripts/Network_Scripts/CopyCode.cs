using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CopyCode : MonoBehaviour
{
    public Button copyButton;  // 복사 버튼
    public TMP_Text textToCopy;    // 복사할 텍스트가 들어있는 Text 컴포넌트

    void Start()
    {
        // 버튼 클릭 시 텍스트 복사
        copyButton.onClick.AddListener(CopyTextToClipboard);
    }

    void CopyTextToClipboard()
    {
        string textToCopyString = textToCopy.text;

#if UNITY_ANDROID
        // 안드로이드에서 클립보드 복사
        CopyTextToClipboardAndroid(textToCopyString);
#else
        // PC에서는 기본 Unity 기능 사용
        GUIUtility.systemCopyBuffer = textToCopyString;
#endif

        Debug.Log("복사된 텍스트: " + textToCopyString);
    }

    void CopyTextToClipboardAndroid(string text)
    {
        // 안드로이드에서 클립보드 복사하는 코드
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
