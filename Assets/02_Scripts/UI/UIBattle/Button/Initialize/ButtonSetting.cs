using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using EnumTypes;

public class ButtonSetting : MonoBehaviour
{
    [SerializeField]
    private SkillType buttonSkillType = SkillType.None;

    public List<Image> images = new List<Image>();
    public List<TextMeshProUGUI> textMeshPros = null;
    public Button button = null;
    public CoolTime cooltime = null;

    private SkillButtonsManager skillButtonsManager = null;

    public SkillType ButtonSkillType
    {
        get { return buttonSkillType; }
    }

    private void Awake()
    {
        button = GetComponentInChildren<Button>();
        images = GetComponentsInChildren<Image>(true).ToList();
        textMeshPros = GetComponentsInChildren<TextMeshProUGUI>().ToList();
        cooltime = GetComponentInChildren<CoolTime>();

        skillButtonsManager = GetComponentInParent<SkillButtonsManager>();
    }
    private void Start()
    {
        foreach (TextMeshProUGUI TMPro in textMeshPros)
        {
            TMPro.raycastTarget = false;
        }
        foreach (Image img in images)
        {
            img.raycastTarget = false;
        }
        button.gameObject.GetComponent<Image>().raycastTarget = true;
        button.onClick.AddListener(ButtonPressed);

        SetCooltime(0.1f);
    }

    public void SetCooltime(float _cooltime)
    {
        cooltime.SetMaxCooltime(_cooltime);
    }

    private void ButtonPressed()
    {
        //if (!cooltime.isPressed)
        //{
        //    cooltime.CooltimeDown();
        //}
        //cooltime.isPressed = true;

        skillButtonsManager.OnSkillButtonClicked(ButtonSkillType);
    }
}
