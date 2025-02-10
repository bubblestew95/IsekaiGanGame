using System.Collections.Generic;
using System.Collections;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

using EnumTypes;

public class ButtonSetting : MonoBehaviour
{
    [SerializeField]
    private SkillSlot buttonSkillType = SkillSlot.None;

    public List<Image> images = new List<Image>();
    public List<TextMeshProUGUI> textMeshPros = null;
    public CoolTime cooltime = null;

    private SkillButtonsManager skillButtonsManager = null;
    private FixedJoystick joystick = null;

    private Coroutine currentSkillCoroutine = null;

    public SkillSlot ButtonSkillType
    {
        get { return buttonSkillType; }
    }

    private void Awake()
    {
        images = GetComponentsInChildren<Image>(true).ToList();
        textMeshPros = GetComponentsInChildren<TextMeshProUGUI>().ToList();
        cooltime = GetComponentInChildren<CoolTime>();
        joystick = GetComponent<FixedJoystick>();
        skillButtonsManager = GetComponentInParent<SkillButtonsManager>();
    }

    private void Start()
    {
        SetCooltime(0.2f);
    }

    public void SetCooltime(float _cooltime)
    {
        cooltime.SetMaxCooltime(_cooltime);
    }
    public void JoystickDown(BaseEventData _eventData)
    {
        skillButtonsManager.OnSkillJoystickDown(ButtonSkillType);
        currentSkillCoroutine = StartCoroutine(DirectionSkillHoldingCoroutine(ButtonSkillType));
    }

    public void JoystickUp(BaseEventData _eventData)
    {
        if(currentSkillCoroutine != null)
            StopCoroutine(currentSkillCoroutine);
        skillButtonsManager.OnSkillJoystickUp(ButtonSkillType);
    }

    /// <summary>
    /// ��ư Ÿ���� ��ų�� �������ٰ� ���� �� ȣ���.
    /// </summary>
    /// <param name="_eventData"></param>
    public void ButtonUp(BaseEventData _eventData)
    {
        skillButtonsManager.OnSkillButtonUp(ButtonSkillType);
    }

    /// <summary>
    /// ������ ��ų�� ���̽�ƽ ��ǥ ���� ���� �Ŵ������� ����ؼ� ������ �ڷ�ƾ.
    /// </summary>
    /// <param name="_type">�����ϰ��� �ϴ� ��ų.</param>
    /// <returns></returns>
    private IEnumerator DirectionSkillHoldingCoroutine(SkillSlot _type)
    {
        while (true)
        {
            skillButtonsManager.SendSkillDirection(_type, joystick.Horizontal, joystick.Vertical);

            yield return null;
        }
    }
}
