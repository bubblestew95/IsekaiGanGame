using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class FanMaker : MonoBehaviour
{
    public float angle = 360f; // ��ä���� ����
    public float radius = 10f; // ��ä���� ������
    public float thickness = 0.5f; // ��ä���� �β�
    public int segmentCount = 50; // ���׸�Ʈ �� (��ä���� ������)

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

        int vertCount = (segmentCount + 2) * 2; // ��� + �ϴ� ��
        Vector3[] vertices = new Vector3[vertCount];
        int vertIndex = 0;

        // ���(Top) �� ���
        float angleStep = angle / segmentCount * Mathf.Deg2Rad;
        vertices[vertIndex++] = Vector3.zero; // ��� �߽���
        for (int i = 0; i <= segmentCount; i++)
        {
            float currentAngle = i * angleStep;
            float x = Mathf.Cos(currentAngle) * radius;
            float y = Mathf.Sin(currentAngle) * radius;
            vertices[vertIndex++] = new Vector3(x, y, thickness / 2); // ��� �ܰ�
        }

        // �ϴ�(Bottom) �� ���
        vertices[vertIndex++] = new Vector3(0, 0, -thickness / 2); // �ϴ� �߽���
        for (int i = 0; i <= segmentCount; i++)
        {
            float currentAngle = i * angleStep;
            float x = Mathf.Cos(currentAngle) * radius;
            float y = Mathf.Sin(currentAngle) * radius;
            vertices[vertIndex++] = new Vector3(x, y, -thickness / 2); // �ϴ� �ܰ�
        }

        // �ﰢ�� �ε��� �迭 ũ�� ��� (��� + �ϴ� + ����)
        int[] triangles = new int[segmentCount * 6 + segmentCount * 6 + segmentCount * 6];
        int triIndex = 0;

        // ��� �ﰢ��
        for (int i = 1; i <= segmentCount; i++)
        {
            triangles[triIndex++] = 0; // �߽���
            triangles[triIndex++] = i;
            triangles[triIndex++] = i + 1;
        }

        // �ϴ� �ﰢ��
        int bottomStart = segmentCount + 2;
        for (int i = 1; i <= segmentCount; i++)
        {
            triangles[triIndex++] = bottomStart; // �߽���
            triangles[triIndex++] = bottomStart + i + 1;
            triangles[triIndex++] = bottomStart + i;
        }

        // ���� �ﰢ��
        for (int i = 1; i <= segmentCount; i++)
        {
            // �ܰ� ���-�ϴ� ���� (�ո�)
            int topOuter = i;
            int bottomOuter = bottomStart + i;
            triangles[triIndex++] = topOuter;
            triangles[triIndex++] = bottomOuter;
            triangles[triIndex++] = topOuter + 1;

            triangles[triIndex++] = topOuter + 1;
            triangles[triIndex++] = bottomOuter;
            triangles[triIndex++] = bottomOuter + 1;
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
            normals[i] = vertices[i].z > 0 ? Vector3.forward : Vector3.back;
        }

        // Mesh ������ ����
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.normals = normals;

        return mesh;
    }
}
