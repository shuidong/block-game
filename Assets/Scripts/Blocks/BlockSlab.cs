using UnityEngine;
using System.Collections;

public class BlockSlab : Block {
	public BlockSlab() {
		name = "Slab";
		textures = new TextureLayout (0);
		normalCube = false;
	}

	public override Bounds GetBounds() {
		Vector3 center = new Vector3 (0, -1/4f, 0);
		Vector3 size = new Vector3 (1, 1/2f, 1);
		return new Bounds (center, size);
	}
}
