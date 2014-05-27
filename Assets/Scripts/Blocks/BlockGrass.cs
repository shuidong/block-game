using UnityEngine;
using System.Collections;

public class BlockGrass : Block {
	public BlockGrass() {
		name = "Grass";
		textures = new TextureLayout (2, 1, 3);
	}

	public override void BlockTick (GameWorld world, int x, int y, int z)
	{
		// grass spread
		int rX = x + Random.Range(-1,2);
		int rY = y + Random.Range(-1,2);
		int rZ = z + Random.Range(-1,2);

		if (world.Block (rX, rY, rZ, 0) == ListBlocks.DIRT) {
			world.SetBlockAt(rX, rY, rZ, ListBlocks.GRASS);
		}
	}
}
