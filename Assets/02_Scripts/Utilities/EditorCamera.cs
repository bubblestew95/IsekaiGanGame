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
            // 씬 카메라의 위치와 회전을 메인 카메라에 동기화
            Vector3 sceneCameraPosition = SceneView.lastActiveSceneView.camera.transform.position;
            Quaternion sceneCameraRotation = SceneView.lastActiveSceneView.camera.transform.rotation;

            mainCamera.transform.position = sceneCameraPosition;
            mainCamera.transform.rotation = sceneCameraRotation;
        }
    }
}
