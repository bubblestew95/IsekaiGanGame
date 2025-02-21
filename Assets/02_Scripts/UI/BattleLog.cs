using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BattleLog : MonoBehaviour
{
    /// <summary>
    /// 행동 이미지
    /// </summary>
    [SerializeField]
    private LogSlot image_How = null;
    /// <summary>
    /// 주체 이미지
    /// </summary>
    [SerializeField]
    private LogSlot image_Who = null;
    /// <summary>
    /// 대상 이미지
    /// </summary>
    [SerializeField]
    private LogSlot image_Whom = null;

    /// <summary>
    /// 로그 출력 지속시간
    /// </summary>
    [SerializeField]
    private float showLogDuration = 2f;



    /// <summary>
    /// 로그의 이미지를 설정하는 함수.
    /// </summary>
    public void SetLogImages()
    {

    }

    /// <summary>
    /// 로그를 출력하는 함수
    /// </summary>
    public void ShowLog()
    {
        // 로그를 출력한다.
        {
            image_How.gameObject.SetActive(true);
            image_Who.gameObject.SetActive(true);
            image_Whom.gameObject.SetActive(true);
        }

        // 일정 시간 후 로그를 다시 종료한다.
        StartCoroutine(HideLogCoroutine(showLogDuration));
    }

    //
    private IEnumerator HideLogCoroutine(float _duration)
    {
        yield return new WaitForSeconds(_duration);

        // 로그를 다시 숨긴다.
        {
            image_How.gameObject.SetActive(false);
            image_Who.gameObject.SetActive(false);
            image_Whom.gameObject.SetActive(false);
        }
    }
}
