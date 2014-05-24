using UnityEngine;
using System.Collections;

public class BlockLamp : Block {
	public BlockLamp() {
		name = "Lamp";
		textures = new TextureLayout (4);
		normalCube = false;
	}
	
	public override Bounds GetBounds() {
		Vector3 center = new Vector3 (0, -1/8f, 0);
		Vector3 size = new Vector3 (.25f, .75f, .25f);
		return new Bounds (center, size);
	}
}
