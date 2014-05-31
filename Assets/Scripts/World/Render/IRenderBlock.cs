using UnityEngine;
using System.Collections;

public interface IRenderBlock {
    void Render(MeshBuildInfo current, World world, Vector2i colPos, int x, int y, int z);
}
