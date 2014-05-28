using UnityEngine;
using System.Collections;

public class BlockStone : Block {
	public BlockStone() {
		name = "Stone";
		textures = new TextureLayout (0);
	}

	public override Color GetColor (GameWorld world, int x, int y, int z, byte meta)
	{
		float val = 1f - meta * .125f;
		return new Color (val, val, val, 1f);
	}
}
