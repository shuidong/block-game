using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeshBuildInfo
{
    public SingleMeshBuildInfo opaque;
    public SingleMeshBuildInfo transparent;

    public MeshBuildInfo()
    {
        opaque = new SingleMeshBuildInfo();
        transparent = new SingleMeshBuildInfo();
    }

    public void Build()
    {
        opaque.Build();
        transparent.Build();
    }

    public void Clear()
    {
        opaque.Clear();
        transparent.Clear();
    }
}

public class SingleMeshBuildInfo
{
    public List<Vector3> vertices;
    public List<int> triangles;
    public List<Vector2> uv;
    public List<Color> colors;
    public int faceCount;

    // final arrays
    public Vector3[] verticesArray;
    public int[] trianglesArray;
    public Vector2[] uvArray;
    public Color[] colorsArray;

    public SingleMeshBuildInfo()
    {
        vertices = new List<Vector3>();
        triangles = new List<int>();
        uv = new List<Vector2>();
        colors = new List<Color>();
        faceCount = 0;
    }

    public void Clear()
    {
        vertices.Clear();
        triangles.Clear();
        uv.Clear();
        colors.Clear();

        verticesArray = null;
        trianglesArray = null;
        uvArray = null;
        colorsArray = null;
        faceCount = 0;
    }

    public void Build()
    {
        verticesArray = vertices.ToArray();
        vertices.Clear();

        trianglesArray = triangles.ToArray();
        triangles.Clear();

        uvArray = uv.ToArray();
        uv.Clear();

        colorsArray = colors.ToArray();
        colors.Clear();
    }

    public void ApplyToMesh(Mesh mesh)
    {
        mesh.Clear();
        mesh.vertices = verticesArray;
        mesh.uv = uvArray;
        mesh.triangles = trianglesArray;
        mesh.colors = colorsArray;
        mesh.Optimize();
        mesh.RecalculateNormals();
    }
}
