using UnityEngine;
using System.Collections;

public class BlockBedrock : Block {
	public BlockBedrock() {
		name = "Bedrock";
		indestructable = true;
		textures = new TextureLayout (0);
	}
}
