using UnityEngine;
using System.Collections;

public class BlockAir : Block {
	public BlockAir() {
		name = "Air";
		normalCube = false;
		collide = false;
		opaque = false;
	}

	public override void Render(MeshBuildInfo current, Chunk chunk, int x, int y, int z) {
	}
}

