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
    private SkillType buttonSkillType = SkillType.None;

    public List<Image> images = new List<Image>();
    public List<TextMeshProUGUI> textMeshPros = null;
    //public Button button = null;
    public CoolTime cooltime = null;

    private SkillButtonsManager skillButtonsManager = null;
    private FixedJoystick joystick = null;

    private Coroutine currentSkillCoroutine = null;

    public SkillType ButtonSkillType
    {
        get { return buttonSkillType; }
    }

    private void Awake()
    {
        // button = GetComponentInChildren<Button>();
        images = GetComponentsInChildren<Image>(true).ToList();
        textMeshPros = GetComponentsInChildren<TextMeshProUGUI>().ToList();
        cooltime = GetComponentInChildren<CoolTime>();
        joystick = GetComponent<FixedJoystick>();
        skillButtonsManager = GetComponentInParent<SkillButtonsManager>();
    }

    private void Start()
    {
        /*
        //foreach (TextMeshProUGUI TMPro in textMeshPros)
        //{
        //    TMPro.raycastTarget = false;
        //}
        //foreach (Image img in images)
        //{
        //    img.raycastTarget = false;
        //}

        ////button.gameObject.GetComponent<Image>().raycastTarget = true;
        ////button.onClick.AddListener(ButtonPressed);
        */

        SetCooltime(0.1f);
    }

    public void SetCooltime(float _cooltime)
    {
        cooltime.SetMaxCooltime(_cooltime);
    }
    public void ButtonDown(BaseEventData _eventData)
    {
        Debug.Log("Button Down!");
        currentSkillCoroutine = StartCoroutine(SkillHoldingCoroutine(ButtonSkillType));
    }

    public void ButtonUp(BaseEventData _eventData)
    {
        Debug.Log("Button Up!");
        StopCoroutine(currentSkillCoroutine);
        skillButtonsManager.OnSkillButtonClickUp(ButtonSkillType);
    }

    private IEnumerator SkillHoldingCoroutine(SkillType _type)
    {
        skillButtonsManager.OnSkillButtonClickDown(_type);

        while (true)
        {
            // Debug.LogFormat("{0}, {1}", joystick.Horizontal, joystick.Vertical);
            skillButtonsManager.SendSkillDirection(_type, joystick.Horizontal, joystick.Vertical);

            yield return null;
        }
    }
}
