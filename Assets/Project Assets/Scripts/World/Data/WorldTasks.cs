using UnityEngine;
using System.Collections;

public class ColumnInstantiateTask
{
    public Vector2i pos;
    public MeshBuildInfo[] meshes;

    public ColumnInstantiateTask(Vector2i pos, MeshBuildInfo[] meshes)
    {
        this.pos = pos;
        this.meshes = meshes;
    }
}

public class ColumnDestroyTask
{
    public Vector2i pos;

    public ColumnDestroyTask(Vector2i pos)
    {
        this.pos = pos;
    }
}

public class ChunkUpdateTask
{
    public Vector3i pos;
    public MeshBuildInfo mesh;

    public ChunkUpdateTask(Vector3i pos, MeshBuildInfo mesh)
    {
        this.pos = pos;
        this.mesh = mesh;
    }
}

public class ChunkRenderTask
{
    public Vector3i pos;

    public ChunkRenderTask(Vector3i pos)
    {
        this.pos = pos;
    }
}

public class ColumnSaveTask
{
    public Vector2i pos;

    public ColumnSaveTask(Vector2i pos)
    {
        this.pos = pos;
    }
}
