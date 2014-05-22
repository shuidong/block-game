using UnityEngine;
using System.Collections;

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
		chunkSize = world.chunkSize;
		data = new byte[chunkSize,height*chunkSize,chunkSize];
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
		GenerateTerrain ();
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
		
		ListBlocks blocks = ListBlocks.instance;
		byte stoneID = blocks.FindByName ("Stone").id;
		byte dirtID = blocks.FindByName ("Dirt").id;
		byte grassID = blocks.FindByName ("Grass").id;
		
		for (int x=startX; x<startX + chunkSize; x++) {
			for (int z=startZ; z<startZ + chunkSize; z++) {
				int stone = 40 + PerlinNoise (x, 0, z, 25, 7, 1.5f);
				int dirt = stone + PerlinNoise (x, 0, z, 25, 2, 1.0f) + 1;
				
				for (int y=0; y < height * chunkSize; y++) {
					if (y <= stone) {
						data [x - startX, y, z - startZ] = stoneID;
					} else if (y < dirt) {
                        data [x - startX, y, z - startZ] = dirtID;
                    } else if (y == dirt) {
                        data [x - startX, y, z - startZ] = grassID;
                    }
                }
            }
		}

		foreach (Chunk c in chunks) {
			c.modified = true;
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
