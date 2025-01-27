using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class FanVisible : MonoBehaviour
{
    public float angle = 360f; // 부채꼴의 각도
    public float radius = 10f; // 부채꼴의 반지름
    public int segmentCount = 50; // 세그먼트 수 (원형 디테일)

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            MeshFilter m = GetComponent<MeshFilter>();
            m.mesh = BuildMesh();
        }
    }

    private Mesh BuildMesh()
    {
        Mesh mesh = new Mesh();
        mesh.name = "CircleMesh";

        // 중심점 + 외곽 점들
        Vector3[] vertices = new Vector3[segmentCount + 2]; // 중심점 포함
        int vertIndex = 0;

        // 중심점
        vertices[vertIndex++] = Vector3.zero;

        // 외곽 점 계산
        float angleStep = angle / segmentCount * Mathf.Deg2Rad; // 각도 간격 (라디안 단위)
        for (int i = 0; i <= segmentCount; i++)
        {
            float currentAngle = i * angleStep;
            float x = Mathf.Cos(currentAngle) * radius;
            float y = Mathf.Sin(currentAngle) * radius;
            vertices[vertIndex++] = new Vector3(x, y, 0);
        }

        // 삼각형 인덱스 배열
        int[] triangles = new int[segmentCount * 3];
        int triIndex = 0;

        for (int i = 1; i <= segmentCount; i++)
        {
            triangles[triIndex++] = 0; // 중심점
            triangles[triIndex++] = i;
            triangles[triIndex++] = i + 1;
        }

        // UV 및 노멀 설정
        Vector2[] uvs = new Vector2[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            uvs[i] = new Vector2(vertices[i].x / radius + 0.5f, vertices[i].y / radius + 0.5f);
        }

        Vector3[] normals = new Vector3[vertices.Length];
        for (int i = 0; i < normals.Length; i++)
        {
            normals[i] = Vector3.back;
        }

        // Mesh에 데이터 할당
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.normals = normals;

        return mesh;
    }
}
