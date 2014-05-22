using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ListBlocks : MonoBehaviour {
	public Block[] blocks;
	public static ListBlocks instance;

	// ids
	public const byte AIR = 0;
	public const byte STONE = 1;
	public const byte GRASS = 2;
	public const byte DIRT = 3;

	void Awake() {
		instance = this;

		// instantiate blocks
		blocks = new Block[256];
		blocks[AIR] = new BlockAir ();
		blocks[STONE] = new BlockStone ();
		blocks[GRASS] = new BlockGrass ();
		blocks[DIRT] = new BlockDirt ();
		blocks[4] = new BlockSlab ();

		// set IDs
		for (int i = 0; i < blocks.Length; i++) {
			if (blocks[i] != null)
				blocks[i].id = (byte)i;
		}
	}

	public Block FindByName(string name) {
		foreach (Block block in blocks) {
			if (name == block.name) return block;
		}
		return null;
	}
}
