using UnityEngine;

using EnumTypes;

public class SkillUI_AOE : SkillUI_Base
{
    [SerializeField]
    private RectTransform targetRectTr = null;
    [SerializeField]
    private GameObject targetImageObj = null;
    [SerializeField]
    private GameObject rangeImageObj = null;

    private float maxRangeRadius = 5f;

    public override void AimSkill(Vector3 _aim)
    {
        base.AimSkill(_aim);
        SetTargetPosition(_aim);
    }
    public override void SetEnabled(bool _enabled)
    {
        base.SetEnabled(_enabled);
        targetImageObj.SetActive(enabled);
        rangeImageObj.SetActive(enabled);
    }

    /// <summary>
    /// 대상 지점으로 타겟 지정 스킬 UI를 옮긴다.
    /// </summary>
    /// <param name="_pos">옮기고자 하는 대상 지점</param>
    private void SetTargetPosition(Vector3 _pos)
    {
        targetRectTr.position = Vector3.ClampMagnitude(_pos, maxRangeRadius);
    }
}
