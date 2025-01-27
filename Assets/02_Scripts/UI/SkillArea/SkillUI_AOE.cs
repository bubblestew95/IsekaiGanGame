using UnityEngine;

using EnumTypes;

public class SkillUI_AOE : SkillUI_Base
{
    [SerializeField]
    private RectTransform targetRectTr = null;

    private float maxRangeRadius = 5f;

    /// <summary>
    /// 대상 지점으로 타겟 지정 스킬 UI를 옮긴다.
    /// </summary>
    /// <param name="_pos">옮기고자 하는 대상 지점</param>
    public void SetTargetPosition(Vector3 _pos)
    {
        targetRectTr.position = Vector3.ClampMagnitude(_pos, maxRangeRadius);
    }
}
