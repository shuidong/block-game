using UnityEngine;
using System.Collections;

public class BlockGrass : Block {
	public BlockGrass() {
		name = "Grass";
		textures = new TextureLayout (2, 1, 3);
	}
}
