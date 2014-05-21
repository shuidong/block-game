using UnityEngine;
using System.Collections;

public class BlockAir : Block {
	public BlockAir() {
		name = "Air";
		normalCube = false;
	}

	public override void Render(MeshBuildInfo current, WorldChunk chunk, int x, int y, int z) {
	}
}

