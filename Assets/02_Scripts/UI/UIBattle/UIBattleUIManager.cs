using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class UIBattleUIManager : MonoBehaviour
{
    public List<UIHpsManager> hpList = new List<UIHpsManager>();
    public List<UIWarningManager> warningList = new List<UIWarningManager>();
    public List <ButtonSetting> buttonList = new List<ButtonSetting>();
    public FloatingJoystick joyStick = null;

    private void Awake()
    {
        hpList = GetComponentsInChildren<UIHpsManager>().ToList();
        warningList = GetComponentsInChildren<UIWarningManager>().ToList();
        buttonList = GetComponentsInChildren<ButtonSetting>().ToList();
        joyStick = GetComponentInChildren<FloatingJoystick>();
    }
}
