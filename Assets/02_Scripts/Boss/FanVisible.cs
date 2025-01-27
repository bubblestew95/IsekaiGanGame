using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class FanVisible : MonoBehaviour
{
    public float angle = 360f; // ��ä���� ����
    public float radius = 10f; // ��ä���� ������
    public int segmentCount = 50; // ���׸�Ʈ �� (���� ������)

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

        // �߽��� + �ܰ� ����
        Vector3[] vertices = new Vector3[segmentCount + 2]; // �߽��� ����
        int vertIndex = 0;

        // �߽���
        vertices[vertIndex++] = Vector3.zero;

        // �ܰ� �� ���
        float angleStep = angle / segmentCount * Mathf.Deg2Rad; // ���� ���� (���� ����)
        for (int i = 0; i <= segmentCount; i++)
        {
            float currentAngle = i * angleStep;
            float x = Mathf.Cos(currentAngle) * radius;
            float y = Mathf.Sin(currentAngle) * radius;
            vertices[vertIndex++] = new Vector3(x, y, 0);
        }

        // �ﰢ�� �ε��� �迭
        int[] triangles = new int[segmentCount * 3];
        int triIndex = 0;

        for (int i = 1; i <= segmentCount; i++)
        {
            triangles[triIndex++] = 0; // �߽���
            triangles[triIndex++] = i;
            triangles[triIndex++] = i + 1;
        }

        // UV �� ��� ����
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

        // Mesh�� ������ �Ҵ�
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.normals = normals;

        return mesh;
    }
}
