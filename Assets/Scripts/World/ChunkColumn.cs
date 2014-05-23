﻿using UnityEngine;
using System.Collections;
using System.Threading;

public class ChunkColumn : MonoBehaviour {
	[HideInInspector]
	public GameWorld world;
	[HideInInspector]
	public byte[,,] data;
	[HideInInspector]
	public Chunk[] chunks;
	[HideInInspector]
	public int height;
	[HideInInspector]
	public Vector2 location;
	public GameObject chunkPrefab;
	public int chunkSize;

	void Start() {
		chunks = new Chunk[height];

		// instantiate chunks
		int x = (int)location.x;
		int y;
		int z = (int)location.y;
		for (int i = 0; i < height; i++) {
			y = i;
			GameObject newObject = Instantiate(chunkPrefab, new Vector3 (x * chunkSize - 0.5f, y * chunkSize - 0.5f, z * chunkSize - 0.5f), new Quaternion (0, 0, 0, 0)) as GameObject;
			newObject.transform.parent = transform;
			Chunk newChunk = newObject.GetComponent<Chunk>();
			newChunk.location = new Vector3(x, y, z);
			newChunk.column = this;
			chunks[i] = newChunk;
		}

		// build data
		new Thread (new ThreadStart (GenerateTerrain)).Start ();
	}

	public byte LocalBlock(int x, int y, int z, byte def) {
		if (y >= chunkSize * height || y < 0)
			return def;
		if (x < 0 || y < 0 || z < 0 || x >= chunkSize || z >= chunkSize) {
			return world.Block((int)location.x * chunkSize + x, y, (int)location.y * chunkSize + z, def);
		} else {
			return data[x,y,z];
        }
    }

	public byte LocalBlock(int x, int y, int z) {
		return data[x,y,z];
    }
    
    void GenerateTerrain() {
		// gen terrain
		int startX = (int)location.x * chunkSize;
		int startZ = (int)location.y * chunkSize;
		
		Block[] blocks = ListBlocks.instance.blocks;
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
						data [bX, y, bZ] = stoneID;
					} else if (y < dirt) {
                        data [bX, y, bZ] = dirtID;
                    } else if (y == dirt) {
                        data [bX, y, bZ] = grassID;
                    }

					blocks[data[bX, y, bZ]].OnLoad(world, x, y, z);
                }
            }
		}

		foreach (Chunk c in chunks) {
			c.modified = true;
		}
	}

	void OnDestroy() {
		Block[] blocks = ListBlocks.instance.blocks;
		int startX = (int)location.x * chunkSize;
		int startZ = (int)location.y * chunkSize;
		for (int x=startX; x<startX + chunkSize; x++) {
			for (int z=startZ; z<startZ + chunkSize; z++) {
				for (int y=0; y < height * chunkSize; y++) {
					int bX = x - startX;
					int bZ = z - startZ;
					blocks[data[bX, y, bZ]].OnUnload(world, x, y, z);
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
        return (int) rValue;
    }
}
