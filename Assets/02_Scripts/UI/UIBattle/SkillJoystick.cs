using UnityEngine;
using UnityEngine.EventSystems;

public class SkillJoystick : FixedJoystick
{
    // �̷��� �������� �Ƹ��� ��Ƽ ����ȭ �κп��� ������ ���� ���� ����.
    // �켱�� ��� ������ ������ �ϴ� �Ϳ� ������ �α�� �ϰ� �����ϰ���.
    private PlayerManager playerManager = null;

    private ButtonSetting buttonSetting = null;

    public override void OnPointerDown(PointerEventData eventData)
    {
        if(playerManager.IsSkillUsable(buttonSetting.ButtonSkillType))
        {
            Debug.Log("TestJoystick Downd!");

            base.OnPointerDown(eventData);
        }
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
    }

    private void Awake()
    {
        playerManager = FindAnyObjectByType<PlayerManager>();
        buttonSetting = GetComponent<ButtonSetting>();
    }
}
