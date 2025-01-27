using UnityEngine;
using UnityEngine.UI;

public class SkillAreaUI_Circle : MonoBehaviour
{
    public RectTransform targetRectTr = null;

    public void SetTargetPosition(Vector3 _pos)
    {
        targetRectTr.position = _pos;
    }
}
