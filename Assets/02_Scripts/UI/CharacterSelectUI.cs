using UnityEngine;

public class CharacterSelectUI : MonoBehaviour
{
    public RectTransform[] characterSlots;  // 4���� ĳ���� ���� UI ����
    public Transform[] characterModels;    // 4���� 3D ĳ����
    public Camera worldCamera;             // 3D ���� �������ϴ� ī�޶�
    public Canvas canvas;

    private Vector2 initialCanvasSize;
    private Vector3[] initialCharacterPositions;

    void Start()
    {
        if (canvas != null)
            initialCanvasSize = canvas.pixelRect.size;

        initialCharacterPositions = new Vector3[characterModels.Length];

        for (int i = 0; i < characterModels.Length; i++)
        {
            initialCharacterPositions[i] = characterModels[i].position;
        }
    }

    void Update()
    {
        Vector2 currentCanvasSize = canvas.pixelRect.size;
        float scaleFactorX = currentCanvasSize.x / initialCanvasSize.x;
        float scaleFactorY = currentCanvasSize.y / initialCanvasSize.y;

        for (int i = 0; i < characterSlots.Length; i++)
        {
            Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(null, characterSlots[i].position);
            Ray ray = worldCamera.ScreenPointToRay(screenPos);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                characterModels[i].position = hit.point;
            }

            // UI ũ�⿡ �°� 3D ĳ���� ��ġ �ڵ� ����
            characterModels[i].position = new Vector3(
                initialCharacterPositions[i].x * scaleFactorX,
                initialCharacterPositions[i].y * scaleFactorY,
                initialCharacterPositions[i].z
            );
        }
    }
}
