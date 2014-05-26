using UnityEngine;
using System.Collections;

public abstract class Block {
	public byte id;
	public string name = "noname";
	public bool indestructable = false;
	public bool normalCube = true;
	public bool collide = true;
	public bool opaque = true;
	public TextureLayout textures;

	public virtual void Render(MeshBuildInfo current, Chunk chunk, int x, int y, int z) {
		Block[] blocks = ListBlocks.instance.blocks;
		Bounds bounds = GetBounds ();
		Vector3 center = Vector3.one/2f + bounds.center;
		Vector3 size = bounds.size;
		byte def = ListBlocks.AIR;

		if (!normalCube) {
			//CubeRenderer.Cube(current, x, y, z, textures, center, size);
		} else {
			if (!blocks [chunk.LocalBlock (x, y + 1, z, def)].normalCube) {
				byte[] l = new byte[9];
				for (int zz = -1; zz <= 1; zz++) {
					for (int xx = -1; xx <= 1; xx++) {
						l[3*(zz+1) + (xx+1)] = chunk.LocalLight (xx + x, y+1, -zz + z, 0);
					}
				}
				CubeRenderer.CubeTop (current, x, y, z, textures, center, size, l);
			}
		
			if (!blocks [chunk.LocalBlock (x, y - 1, z, def)].normalCube) {
				byte[] l = new byte[9];
				for (int zz = -1; zz <= 1; zz++) {
					for (int xx = -1; xx <= 1; xx++) {
						l[3*(zz+1) + (xx+1)] = chunk.LocalLight (xx + x, y-1, -zz + z, 0);
					}
				}
				CubeRenderer.CubeBottom (current, x, y, z, textures, center, size, l);
			}
		
			if (!blocks [chunk.LocalBlock (x + 1, y, z, def)].normalCube) {
				byte[] l = new byte[9];
				for (int zz = -1; zz <= 1; zz++) {
					for (int yy = -1; yy <= 1; yy++) {
						l[3*(zz+1) + (yy+1)] = chunk.LocalLight (x+1, yy + y, -zz + z, 0);
					}
				}
				CubeRenderer.CubeEast (current, x, y, z, textures, center, size, l);
			}
		
			if (!blocks [chunk.LocalBlock (x - 1, y, z, def)].normalCube) {
				byte[] l = new byte[9];
				for (int zz = -1; zz <= 1; zz++) {
					for (int yy = -1; yy <= 1; yy++) {
						l[3*(zz+1) + (yy+1)] = chunk.LocalLight (x-1, yy + y, -zz + z, 0);
					}
				}
				CubeRenderer.CubeWest (current, x, y, z, textures, center, size, l);
			}
		
			if (!blocks [chunk.LocalBlock (x, y, z + 1, def)].normalCube) {
				byte[] l = new byte[9];
				for (int yy = -1; yy <= 1; yy++) {
					for (int xx = -1; xx <= 1; xx++) {
						l[3*(yy+1) + (xx+1)] = chunk.LocalLight (xx + x, -yy + y, z+1, 0);
					}
				}
				CubeRenderer.CubeNorth (current, x, y, z, textures, center, size, l);
			}
		
			if (!blocks [chunk.LocalBlock (x, y, z - 1, def)].normalCube) {
				byte[] l = new byte[9];
				for (int yy = -1; yy <= 1; yy++) {
					for (int xx = -1; xx <= 1; xx++) {
						l[3*(yy+1) + (xx+1)] = chunk.LocalLight (xx + x, -yy + y, z-1, 0);
					}
				}
				CubeRenderer.CubeSouth (current, x, y, z, textures, center, size, l);
			}
		}
	}

	public virtual Bounds GetBounds() {
		return new Bounds (Vector3.zero, Vector3.one);
	}

	public virtual void OnLoad(GameWorld world, int x, int y, int z) {
    }

	public virtual void OnBuild(GameWorld world, int x, int y, int z) {
    }

	public virtual void OnBreak(GameWorld world, int x, int y, int z) {
	}

	public virtual void OnUnload(GameWorld world, int x, int y, int z) {
    }
}