using UnityEngine;

public class SkillAreaUI_Test : MonoBehaviour
{
    public SkillAreaUI_Circle testSkillCircleUI = null;
    public Transform targetTr = null;

    private void Update()
    {
        ShowCircleAOEUI(targetTr.position);
    }

    public void ShowCircleAOEUI(Vector3 _targetPos)
    {
        testSkillCircleUI.SetTargetPosition(_targetPos);
    }
}
