using UnityEngine;

using EnumTypes;

public class SkillUI_AOE : SkillUI_Base
{
    [SerializeField]
    private RectTransform targetRectTr = null;

    private float maxRangeRadius = 5f;

    /// <summary>
    /// ��� �������� Ÿ�� ���� ��ų UI�� �ű��.
    /// </summary>
    /// <param name="_pos">�ű���� �ϴ� ��� ����</param>
    public void SetTargetPosition(Vector3 _pos)
    {
        targetRectTr.position = Vector3.ClampMagnitude(_pos, maxRangeRadius);
    }
}
