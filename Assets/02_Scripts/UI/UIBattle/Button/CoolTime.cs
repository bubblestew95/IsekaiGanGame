using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CoolTime : MonoBehaviour
{
    public float maxCooltime = 0;
    public float curCooltime = 0;
    private Image coolImage = null;
    private TextMeshProUGUI coolSecondUI = null;
    private void Awake()
    {
        coolSecondUI = GetComponentInChildren<TextMeshProUGUI>();
        coolImage = GetComponent<Image>();
    }

    public void SetMaxCooltime(float _cooltime)
    {
        maxCooltime = _cooltime;
    } // 초기 쿨타임 설정

    public void CooltimeDown()
    {
        curCooltime = maxCooltime;
        StartCoroutine(cooldown());
    } // 함수 실행하면 쿨이 돈다


    private IEnumerator cooldown() // UI 표시랑 쿨 계산 동시에 하는 코루틴
    {
        float coolratio = 0;
        while (curCooltime<=0)
        {
            curCooltime -= Time.deltaTime; //쿨타임 계산
            coolSecondUI.text = curCooltime.ToString(); //쿨타임UI Txt표시
            coolratio = curCooltime / maxCooltime; // 현재쿨 비율 계산
            coolImage.fillAmount = coolratio;//쿨타임UI 시계표시
            yield return null;
        }
    }
}
