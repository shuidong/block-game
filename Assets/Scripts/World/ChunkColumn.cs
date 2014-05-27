using UnityEngine;
using System.Collections;
using System.Threading;

public class ChunkColumn : MonoBehaviour
{
	[HideInInspector]
	public GameWorld
		world;
	[HideInInspector]
	public byte[,,]
		blockData;
	[HideInInspector]
	public byte[,,]
		lightData;
	[HideInInspector]
	public Chunk[]
		chunks;
	[HideInInspector]
	public int
		height;
	[HideInInspector]
	public Vector2
		location;
	public GameObject chunkPrefab;
	public int chunkSize;
	private Block[] blocks;

	void Start ()
	{
		chunks = new Chunk[height];
		blocks = ListBlocks.instance.blocks;

		// instantiate chunks
		int x = (int)location.x;
		int y;
		int z = (int)location.y;
		for (int i = 0; i < height; i++) {
			y = i;
			GameObject newObject = Instantiate (chunkPrefab, new Vector3 (x * chunkSize - 0.5f, y * chunkSize - 0.5f, z * chunkSize - 0.5f), new Quaternion (0, 0, 0, 0)) as GameObject;
			newObject.transform.parent = transform;
			Chunk newChunk = newObject.GetComponent<Chunk> ();
			newChunk.location = new Vector3 (x, y, z);
			newChunk.column = this;
			chunks [i] = newChunk;
		}

		// build data
		new Thread (new ThreadStart (GenerateTerrain)).Start ();
	}

	public byte LocalBlock (int x, int y, int z, byte def)
	{
		if (y >= chunkSize * height || y < 0)
			return def;
		if (x < 0 || z < 0 || x >= chunkSize || z >= chunkSize) {
			return world.Block ((int)location.x * chunkSize + x, y, (int)location.y * chunkSize + z, def);
		} else {
			return blockData [x, y, z];
		}
	}

	public byte LocalLight (int x, int y, int z, byte def)
	{
		if (y >= chunkSize * height || y < 0)
			return def;
		if (x < 0 || z < 0 || x >= chunkSize || z >= chunkSize) {
			return world.Light ((int)location.x * chunkSize + x, y, (int)location.y * chunkSize + z, def);
		} else {
			return lightData [x, y, z];
		}
	}

	public byte LocalBlock (int x, int y, int z)
	{
		return blockData [x, y, z];
	}

	public byte LocalLight (int x, int y, int z)
	{
		return lightData [x, y, z];
	}
    
	public void GenerateTerrain ()
	{
		// gen terrain
		int startX = (int)location.x * chunkSize;
		int startZ = (int)location.y * chunkSize;
		
		byte stoneID = ListBlocks.STONE;
		byte dirtID = ListBlocks.DIRT;
		byte grassID = ListBlocks.GRASS;
		
		for (int x=startX; x<startX + chunkSize; x++) {
			for (int z=startZ; z<startZ + chunkSize; z++) {
				int stone = 40 + PerlinNoise (x, 0, z, 25, 7, 1.5f);
				int dirt = stone + PerlinNoise (x, 0, z, 25, 2, 1.0f) + 1;

				int bX = x - startX;
				int bZ = z - startZ;

				for (int y=0; y < height * chunkSize; y++) {
					if (y <= stone) {
						blockData [bX, y, bZ] = stoneID;
					} else if (y < dirt) {
						blockData [bX, y, bZ] = dirtID;
					} else if (y == dirt) {
						blockData [bX, y, bZ] = grassID;
					}

					blocks [blockData [bX, y, bZ]].OnLoad (world, x, y, z);
				}
			}
		}

		GenerateSunlight ();

		foreach (Chunk c in chunks) {
			c.modified = true;
		}
	}

	public void GenerateSunlight ()
	{
		lightData = new byte[chunkSize, chunkSize * height, chunkSize];
		byte maxLight = CubeRenderer.MAX_LIGHT;
		int yMax = height * chunkSize - 1;

		// beam sunlight downwards
		for (int bX=0; bX<chunkSize; bX++) {
			for (int bZ=0; bZ<chunkSize; bZ++) {
				for (int y = yMax; y >= 0; y--) {
					if (blocks [blockData [bX, y, bZ]].opaque) {
						break;
					} else {
						lightData [bX, y, bZ] = maxLight;
					}
				}
			}
		}

		// flood fill
		for (int bX=0; bX<chunkSize; bX++) {
			for (int bZ=0; bZ<chunkSize; bZ++) {
				for (int y = yMax; y >= 0; y--) {
					if (lightData [bX, y, bZ] > 0) {
						FloodFillLight (bX, y, bZ, lightData [bX, y, bZ], true);
					} else {
						break;
					}
				}
			}
		}
	}

	public bool CanSeeSky (int x, int y, int z)
	{
		for (int i = y+1; i < height * chunkSize; i++) {
			if (blocks [blockData [x, i, z]].opaque)
				return false;
		}
		return true;
	}

	public void FloodFillDarkness (int x, int y, int z, byte prevLight)
	{
		// check if flood needs to go to another column
		ChunkColumn nextCol;
		if (x < 0) {
			world.loadedWorld.TryGetValue (location + new Vector2 (-1, 0), out nextCol);
			nextCol.FloodFillDarkness (x + chunkSize, y, z, prevLight);
			return;
		} else if (x >= chunkSize) {
			world.loadedWorld.TryGetValue (location + new Vector2 (1, 0), out nextCol);
			nextCol.FloodFillDarkness (x - chunkSize, y, z, prevLight);
			return;
		} else if (z < 0) {
			world.loadedWorld.TryGetValue (location + new Vector2 (0, -1), out nextCol);
			nextCol.FloodFillDarkness (x, y, z + chunkSize, prevLight);
			return;
		} else if (z >= chunkSize) {
			world.loadedWorld.TryGetValue (location + new Vector2 (0, 1), out nextCol);
			nextCol.FloodFillDarkness (x, y, z - chunkSize, prevLight);
			return;
		}
		
		// check if flood is out of bounds
		if (y < 0 || y >= height * chunkSize) {
			return;
		}

		// darken and spread
		byte light = lightData [x, y, z];
		if (light > 0 && light < prevLight) {
			// spread
			FloodFillDarkness (x - 1, y, z, light);
			FloodFillDarkness (x + 1, y, z, light);
			FloodFillDarkness (x, y - 1, z, light);
			FloodFillDarkness (x, y + 1, z, light);
			FloodFillDarkness (x, y, z - 1, light);
			FloodFillDarkness (x, y, z + 1, light);

			// darken
			lightData [x, y, z] = 0;
			chunks [y / chunkSize].modified = true;
		}
	}
	
	public void FloodFillLight (int x, int y, int z, byte remainingLight, bool source)
	{
		// check if flood needs to go to another column
		ChunkColumn nextCol;
		if (x < 0) {
			if (world.loadedWorld.TryGetValue (location + new Vector2 (-1, 0), out nextCol)) {
				nextCol.FloodFillLight (x + chunkSize, y, z, remainingLight, source);
			}
			return;
		} else if (x >= chunkSize) {
			if (world.loadedWorld.TryGetValue (location + new Vector2 (1, 0), out nextCol)) {
				nextCol.FloodFillLight (x - chunkSize, y, z, remainingLight, source);
			}
			return;
		} else if (z < 0) {
			if (world.loadedWorld.TryGetValue (location + new Vector2 (0, -1), out nextCol)) {
				nextCol.FloodFillLight (x, y, z + chunkSize, remainingLight, source);
			}
			return;
		} else if (z >= chunkSize) {
			if (world.loadedWorld.TryGetValue (location + new Vector2 (0, 1), out nextCol)) {
				nextCol.FloodFillLight (x, y, z - chunkSize, remainingLight, source);
			}
			return;
		}

		// check if flood is out of bounds
		if (y < 0 || y >= height * chunkSize) {
			return;
		}
        
		// block at walls
		chunks [y / chunkSize].modified = true;
		if (blocks [blockData [x, y, z]].opaque) {
			lightData [x, y, z] = 0;
			return;
		}

		//spread here if needed
		if (remainingLight > 0 && (source || lightData [x, y, z] < remainingLight)) {
			lightData [x, y, z] = remainingLight;
			remainingLight--;

			// spread to neighboring blocks
			if (remainingLight > 0) {
				FloodFillLight (x - 1, y, z, remainingLight, false);
				FloodFillLight (x + 1, y, z, remainingLight, false);
				FloodFillLight (x, y - 1, z, remainingLight, false);
				FloodFillLight (x, y + 1, z, remainingLight, false);
				FloodFillLight (x, y, z - 1, remainingLight, false);
				FloodFillLight (x, y, z + 1, remainingLight, false);
			}
			return;
		}
	}

	void OnDestroy ()
	{
		int startX = (int)location.x * chunkSize;
		int startZ = (int)location.y * chunkSize;
		for (int x=startX; x<startX + chunkSize; x++) {
			for (int z=startZ; z<startZ + chunkSize; z++) {
				for (int y=0; y < height * chunkSize; y++) {
					int bX = x - startX;
					int bZ = z - startZ;
					blocks [blockData [bX, y, bZ]].OnUnload (world, x, y, z);
				}
			}
		}
	}
	
	private int PerlinNoise (int x, int y, int z, float scale, float height, float power)
	{
		float rValue = Noise.GetNoise (((double)x) / scale, ((double)y) / scale, ((double)z) / scale);
		rValue *= height;
		if (power != 0)
			rValue = Mathf.Pow (rValue, power);
		return (int)rValue;
	}
}
