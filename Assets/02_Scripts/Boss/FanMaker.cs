using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class FanMaker : MonoBehaviour
{
    public float angle = 360f; // 부채꼴의 각도
    public float radius = 10f; // 부채꼴의 반지름
    public float thickness = 0.5f; // 부채꼴의 두께
    public int segmentCount = 50; // 세그먼트 수 (부채꼴의 디테일)

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
        mesh.name = "ThickFanMesh";

        int vertCount = (segmentCount + 2) * 2; // 상단 + 하단 점
        Vector3[] vertices = new Vector3[vertCount];
        int vertIndex = 0;

        // 상단(Top) 점 계산
        float angleStep = angle / segmentCount * Mathf.Deg2Rad;
        vertices[vertIndex++] = Vector3.zero; // 상단 중심점
        for (int i = 0; i <= segmentCount; i++)
        {
            float currentAngle = i * angleStep;
            float x = Mathf.Cos(currentAngle) * radius;
            float y = Mathf.Sin(currentAngle) * radius;
            vertices[vertIndex++] = new Vector3(x, y, thickness / 2); // 상단 외곽
        }

        // 하단(Bottom) 점 계산
        vertices[vertIndex++] = new Vector3(0, 0, -thickness / 2); // 하단 중심점
        for (int i = 0; i <= segmentCount; i++)
        {
            float currentAngle = i * angleStep;
            float x = Mathf.Cos(currentAngle) * radius;
            float y = Mathf.Sin(currentAngle) * radius;
            vertices[vertIndex++] = new Vector3(x, y, -thickness / 2); // 하단 외곽
        }

        // 삼각형 인덱스 배열 크기 계산 (상단 + 하단 + 측면)
        int[] triangles = new int[segmentCount * 6 + segmentCount * 6 + segmentCount * 6];
        int triIndex = 0;

        // 상단 삼각형
        for (int i = 1; i <= segmentCount; i++)
        {
            triangles[triIndex++] = 0; // 중심점
            triangles[triIndex++] = i;
            triangles[triIndex++] = i + 1;
        }

        // 하단 삼각형
        int bottomStart = segmentCount + 2;
        for (int i = 1; i <= segmentCount; i++)
        {
            triangles[triIndex++] = bottomStart; // 중심점
            triangles[triIndex++] = bottomStart + i + 1;
            triangles[triIndex++] = bottomStart + i;
        }

        // 측면 삼각형
        for (int i = 1; i <= segmentCount; i++)
        {
            // 외곽 상단-하단 연결 (앞면)
            int topOuter = i;
            int bottomOuter = bottomStart + i;
            triangles[triIndex++] = topOuter;
            triangles[triIndex++] = bottomOuter;
            triangles[triIndex++] = topOuter + 1;

            triangles[triIndex++] = topOuter + 1;
            triangles[triIndex++] = bottomOuter;
            triangles[triIndex++] = bottomOuter + 1;
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
            normals[i] = vertices[i].z > 0 ? Vector3.forward : Vector3.back;
        }

        // Mesh 데이터 설정
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.normals = normals;

        return mesh;
    }
}
