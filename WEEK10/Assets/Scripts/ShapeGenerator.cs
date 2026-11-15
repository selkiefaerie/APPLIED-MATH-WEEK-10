
using System.Collections.Generic;
using UnityEngine;

public class ShapeGenerator : MonoBehaviour
{
    [Header("Shape Settings")]
    [SerializeField] private Material lineMat;
    [SerializeField] private float scale = 1.5f;
    [SerializeField] private float focalLength = 20f;

    [Header("Rotation Settings")]
    [SerializeField] private float xRotationSpeed = 45f;
    [SerializeField] private float yRotationSpeed = 45f;

    private class Shape
    {
        public GameObject obj;
        public Mesh mesh;
        public Vector3[] originalVertices;
        public int[] edges;
        public Vector2 screenPosition;
        public float xRotation;
        public float yRotation;
    }

    private readonly List<Shape> shapes = new List<Shape>();

    void Start()
    {
        CreatePyramid();
        CreateSphere();
        CreateCube();
    }

    void Update()
    {
        foreach (Shape shape in shapes)
        {
            shape.xRotation += xRotationSpeed * Time.deltaTime;
            shape.yRotation += yRotationSpeed * Time.deltaTime;

            ProjectShape(shape);
        }
    }

    // PYRAMID
    void CreatePyramid()
    {
        Vector3[] vertices =
        {
            new Vector3(-1, -1, -1),
            new Vector3(1, -1, -1),
            new Vector3(1, -1, 1),
            new Vector3(-1, -1, 1),
            new Vector3(0, 1.5f, 0)
        };

        int[] edges =
        {
            0, 1, 1, 2, 2, 3, 3, 0,
            0, 4, 1, 4, 2, 4, 3, 4
        };

        CreateShape("Pyramid", vertices, edges,
            new Vector2(-5.5f, 0));
    }

    // SPHERE
    void CreateSphere()
    {
        int segments = 12;
        int rings = 8;

        List<Vector3> vertices = new List<Vector3>();
        List<int> edges = new List<int>();

        for (int i = 0; i <= rings; i++)
        {
            float latitude = -Mathf.PI / 2f +
                Mathf.PI * i / rings;

            for (int j = 0; j < segments; j++)
            {
                float longitude =
                    2f * Mathf.PI * j / segments;

                float x = Mathf.Cos(latitude) *
                          Mathf.Cos(longitude);

                float y = Mathf.Sin(latitude);

                float z = Mathf.Cos(latitude) *
                          Mathf.Sin(longitude);

                vertices.Add(new Vector3(x, y, z));
            }
        }

        for (int i = 0; i <= rings; i++)
        {
            for (int j = 0; j < segments; j++)
            {
                int current = i * segments + j;
                int next = i * segments +
                    (j + 1) % segments;

                edges.Add(current);
                edges.Add(next);
            }
        }

        for (int i = 0; i < rings; i++)
        {
            for (int j = 0; j < segments; j++)
            {
                int current = i * segments + j;
                int next = (i + 1) * segments + j;

                edges.Add(current);
                edges.Add(next);
            }
        }

        CreateShape("Sphere", vertices.ToArray(),
            edges.ToArray(), Vector2.zero);
    }

    // CUBE
    void CreateCube()
    {
        Vector3[] vertices =
        {
            new Vector3(-1, -1, -1),
            new Vector3(1, -1, -1),
            new Vector3(1, 1, -1),
            new Vector3(-1, 1, -1),

            new Vector3(-1, -1, 1),
            new Vector3(1, -1, 1),
            new Vector3(1, 1, 1),
            new Vector3(-1, 1, 1)
        };

        int[] edges =
        {
            0, 1, 1, 2, 2, 3, 3, 0,
            4, 5, 5, 6, 6, 7, 7, 4,
            0, 4, 1, 5, 2, 6, 3, 7
        };

        CreateShape("Cube", vertices, edges,
            new Vector2(5.5f, 0));
    }

    // CREATE SHAPE OBJECT
    void CreateShape(string shapeName,
        Vector3[] vertices, int[] edges,
        Vector2 screenPosition)
    {
        GameObject obj = new GameObject(shapeName);
        obj.transform.SetParent(transform);

        Mesh mesh = new Mesh();
        mesh.name = shapeName + " Mesh";

        mesh.vertices = vertices;
        mesh.SetIndices(edges, MeshTopology.Lines, 0);

        mesh.RecalculateBounds();

        MeshFilter filter = obj.AddComponent<MeshFilter>();
        filter.mesh = mesh;

        MeshRenderer renderer =
            obj.AddComponent<MeshRenderer>();

        if (lineMat != null)
            renderer.material = lineMat;

        Shape shape = new Shape();

        shape.obj = obj;
        shape.mesh = mesh;
        shape.originalVertices = vertices;
        shape.edges = edges;
        shape.screenPosition = screenPosition;

        shapes.Add(shape);
    }

    // ROTATION
    void ProjectShape(Shape shape)
    {
        Vector3[] projectedVertices =
            new Vector3[shape.originalVertices.Length];

        for (int i = 0; i < shape.originalVertices.Length; i++)
        {
            Vector3 point = shape.originalVertices[i];

            float x = point.x;

            float y = point.y *
                Mathf.Cos(shape.xRotation * Mathf.Deg2Rad)
                - point.z *
                Mathf.Sin(shape.xRotation * Mathf.Deg2Rad);

            float z = point.y *
                Mathf.Sin(shape.xRotation * Mathf.Deg2Rad)
                + point.z *
                Mathf.Cos(shape.xRotation * Mathf.Deg2Rad);

            point = new Vector3(x, y, z);

            float xRot = point.x *
                Mathf.Cos(shape.yRotation * Mathf.Deg2Rad)
                + point.z *
                Mathf.Sin(shape.yRotation * Mathf.Deg2Rad);

            float zRot = -point.x *
                Mathf.Sin(shape.yRotation * Mathf.Deg2Rad)
                + point.z *
                Mathf.Cos(shape.yRotation * Mathf.Deg2Rad);

            point = new Vector3(xRot, point.y, zRot);

            float p = focalLength /
                (focalLength + point.z);

            Vector2 projected = new Vector2(
                point.x,
                point.y
            ) * scale * p;

            projected += shape.screenPosition;

            projectedVertices[i] = new Vector3(
                projected.x,
                projected.y,
                0
            );
        }

        shape.mesh.vertices = projectedVertices;
        shape.mesh.RecalculateBounds();
    }
}
