using UnityEngine;

public class UITo3DObjectScaler : MonoBehaviour
{
    public RectTransform uiElement;  // UI ��� (RectTransform)
    public Transform object3D;       // 3D ������Ʈ
    public Camera worldCamera;       // ���� ī�޶�
    public Canvas canvas;            // UI�� ���Ե� Canvas

    private Vector3 initial3DPos;    // �ʱ� 3D ������Ʈ ��ġ
    private Vector2 initialCanvasSize; // �ʱ� ĵ���� ũ��

    void Start()
    {
        if (object3D != null)
            initial3DPos = object3D.position; // �ʱ� 3D ������Ʈ ��ġ ����

        if (canvas != null)
            initialCanvasSize = canvas.pixelRect.size; // �ʱ� Canvas ũ�� ����
    }

    void Update()
    {
        if (uiElement == null || object3D == null || worldCamera == null || canvas == null) return;

        // ���� ȭ�� ũ�� ��������
        Vector2 currentCanvasSize = canvas.pixelRect.size;

        // UI ����� ȭ�� ��ǥ(Screen Position) ��������
        Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(null, uiElement.position);

        // ȭ�� ��ǥ�� 3D ������ Ray�� ��ȯ
        Ray ray = worldCamera.ScreenPointToRay(screenPos);

        // Ray�� �浹�ϴ� 3D ������Ʈ ��ġ�� ��ġ ã��
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            object3D.position = hit.point; // �浹�� �������� 3D ������Ʈ �̵�
        }

        // ȭ�� ũ�⿡ ���� ��ġ �ڵ� ���� (���� ����)
        float scaleFactorX = currentCanvasSize.x / initialCanvasSize.x;
        float scaleFactorY = currentCanvasSize.y / initialCanvasSize.y;

        // 3D ������Ʈ ��ġ�� ȭ�� ������ ���� ����
        object3D.position = new Vector3(
            initial3DPos.x * scaleFactorX,
            initial3DPos.y * scaleFactorY,
            initial3DPos.z
        );
    }
}
