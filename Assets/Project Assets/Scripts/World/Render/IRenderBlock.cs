using UnityEngine;
using System.Collections;

public interface IRenderBlock
{
    void Render(MeshBuildInfo current, World world, Vector3i chunkPos, int x, int y, int z);
}
