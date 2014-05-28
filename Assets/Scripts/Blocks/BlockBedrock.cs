using UnityEngine;
using System.Collections;

public class BlockBedrock : Block {
	Color color = new Color (.35f, .35f, .35f, 1f);

	public BlockBedrock() {
		name = "Bedrock";
		indestructable = true;
		textures = new TextureLayout (0);
	}

	public override Color GetColor (GameWorld world, int x, int y, int z, byte meta)
	{
		return color;
	}
}
