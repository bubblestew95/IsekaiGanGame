using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class CameraSync : MonoBehaviour
{
    private Camera mainCamera;

    void OnEnable()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (mainCamera != null)
        {
            // �� ī�޶��� ��ġ�� ȸ���� ���� ī�޶� ����ȭ
            Vector3 sceneCameraPosition = SceneView.lastActiveSceneView.camera.transform.position;
            Quaternion sceneCameraRotation = SceneView.lastActiveSceneView.camera.transform.rotation;

            mainCamera.transform.position = sceneCameraPosition;
            mainCamera.transform.rotation = sceneCameraRotation;
        }
    }
}
