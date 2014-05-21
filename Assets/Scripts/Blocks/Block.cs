using UnityEngine;
using System.Collections;

public abstract class Block {
	public byte id;
	public string name = "noname";
	public bool indestructable = false;
	public bool normalCube = true;
	public TextureLayout textures;

	public virtual void Render(MeshBuildInfo current, WorldChunk chunk, int x, int y, int z) {
		Block[] blocks = ListBlocks.instance.blocks;
		Vector3 center = Vector3.one/2f;
		Vector3 size = Vector3.one;
		byte def = ListBlocks.AIR;
		if (!blocks[chunk.LocalBlock (x, y + 1, z, def)].normalCube) {
			CubeRenderer.CubeTop (current, x, y, z, textures, center, size);
		}
		
		if (!blocks[chunk.LocalBlock (x, y - 1, z, def)].normalCube) {
			CubeRenderer.CubeBottom (current, x, y, z, textures, center, size);
		}
		
		if (!blocks[chunk.LocalBlock (x + 1, y, z, def)].normalCube) {
			CubeRenderer.CubeEast (current, x, y, z, textures, center, size);
		}
		
		if (!blocks[chunk.LocalBlock (x - 1, y, z, def)].normalCube) {
			CubeRenderer.CubeWest (current, x, y, z, textures, center, size);
		}
		
		if (!blocks[chunk.LocalBlock (x, y, z + 1, def)].normalCube) {
			CubeRenderer.CubeNorth (current, x, y, z, textures, center, size);
		}
		
		if (!blocks[chunk.LocalBlock (x, y, z - 1, def)].normalCube) {
			CubeRenderer.CubeSouth (current, x, y, z, textures, center, size);
		}
	}

	public virtual void OnCreate(GameWorld world, int x, int y, int z) {
		Debug.Log (name + " created");
	}

	public virtual void OnRemove(GameWorld world, int x, int y, int z) {
		Debug.Log (name + " removed");
	}
}