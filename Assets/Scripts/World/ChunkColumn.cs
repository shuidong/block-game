using UnityEngine;
using System.Collections;
using System.Threading;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Reflection;

public class ChunkColumn : MonoBehaviour
{
	[HideInInspector]
	public GameWorld world;

	[System.Serializable]
	public class PersistentData : ISerializable {
		private const string KEY_BLOCK = "Blocks array";
		private const string KEY_LIGHT = "Lighting array";
		private const string KEY_META  = "Metadata array";

		public byte[,,] blockArray;
		public byte[,,] lightArray;
		public byte[,,] metaArray;

		public PersistentData(int chunkSize, int height) {
			blockArray = new byte[chunkSize,chunkSize*height,chunkSize];
			lightArray = new byte[chunkSize,chunkSize*height,chunkSize];
			metaArray = new byte[chunkSize,chunkSize*height,chunkSize];
		}

		public PersistentData (SerializationInfo info, StreamingContext ctxt) {
			string version = (string)info.GetValue(Constants.KEY_SAVE_VERSION, typeof(string));

			if (version == Constants.SAVE_VERSION_1) {
				blockArray = (byte[,,])info.GetValue(KEY_BLOCK, typeof(byte[,,]));
				lightArray = (byte[,,])info.GetValue(KEY_LIGHT, typeof(byte[,,]));
				metaArray  = (byte[,,])info.GetValue(KEY_META , typeof(byte[,,]));
			} else {
				print ("ERROR LOADING CHUNK, VERSION IS: " + version);
			}
		}

		public void GetObjectData (SerializationInfo info, StreamingContext ctxt) {
			info.AddValue (Constants.KEY_SAVE_VERSION, Constants.SAVE_VERSION_1);
			info.AddValue (KEY_BLOCK, blockArray);
			info.AddValue (KEY_LIGHT, lightArray);
			info.AddValue (KEY_META , metaArray );
		}
	}
	public PersistentData data;
	public string savePath;
	public bool needsSave;

	[HideInInspector]
	public Chunk[] chunks;

	[HideInInspector]
	public int height;

	[HideInInspector]
	public Vector2 location;

	public GameObject chunkPrefab;
	public int chunkSize;
	private Block[] blocks;
	public int tickRate = 4;

	void Start ()
	{
		chunks = new Chunk[height];
		blocks = ListBlocks.instance.blocks;
		savePath = Application.persistentDataPath + "/" + world.worldName + "/" + ((int)location.x) + "x" + ((int)location.y) + ".blockdata";

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

	void FixedUpdate ()
	{
		int xOffset = (int)(location.x * chunkSize);
		int zOffset = (int)(location.y * chunkSize);
		int wHeight = chunkSize * height;

		for (int i = 0; i < tickRate; i++) {
			int x = Random.Range (0, chunkSize);
			int y = Random.Range (0, wHeight);
			int z = Random.Range (0, chunkSize);
			byte blockID = data.blockArray [x, y, z];
			blocks [blockID].BlockTick (world, xOffset + x, y, zOffset + z, data.metaArray[x, y, z]);
		}
	}

	public BlockMeta LocalBlock (int x, int y, int z, byte def)
	{
		if (y >= chunkSize * height || y < 0)
			return new BlockMeta (def, 0);
		if (x < 0 || z < 0 || x >= chunkSize || z >= chunkSize) {
			return world.Block ((int)location.x * chunkSize + x, y, (int)location.y * chunkSize + z, def);
		} else {
			return new BlockMeta (data.blockArray [x, y, z], data.metaArray [x, y, z]);
		}
	}

	public byte LocalLight (int x, int y, int z, byte def)
	{
		if (y >= chunkSize * height || y < 0)
			return def;
		if (x < 0 || z < 0 || x >= chunkSize || z >= chunkSize) {
			return world.Light ((int)location.x * chunkSize + x, y, (int)location.y * chunkSize + z, def);
		} else {
			return data.lightArray [x, y, z];
		}
	}

	public BlockMeta LocalBlock (int x, int y, int z)
	{
		return new BlockMeta (data.blockArray [x, y, z], data.metaArray [x, y, z]);
	}

	public byte LocalLight (int x, int y, int z)
	{
		return data.lightArray [x, y, z];
	}
    
	public void GenerateTerrain ()
	{
		try {
			if(!Load()) {
				// gen terrain
				int startX = (int)location.x * chunkSize;
				int startZ = (int)location.y * chunkSize;
				
				byte stoneID = ListBlocks.STONE;
				byte dirtID = ListBlocks.DIRT;
				byte grassID = ListBlocks.GRASS;
				byte bedrockID = ListBlocks.BEDROCK;
				
				for (int x=startX; x<startX + chunkSize; x++) {
					for (int z=startZ; z<startZ + chunkSize; z++) {
						int stone0 = 02 + PerlinNoise (x, 02, z, 25, 7, 1.5f);
						int stone1 = 07 + PerlinNoise (x, 07, z, 25, 7, 1.5f);
						int stone2 = 15 + PerlinNoise (x, 15, z, 25, 7, 1.5f);
						int stone3 = 25 + PerlinNoise (x, 25, z, 25, 7, 1.5f);
						int stone4 = 35 + PerlinNoise (x, 35, z, 25, 7, 1.5f);
						int dirt   = 06 + PerlinNoise (x, 06 + stone4, z, 25, 2, 1.0f) + stone4;

						int bX = x - startX;
						int bZ = z - startZ;

						for (int y=0; y < height * chunkSize; y++) {
							if (y == 0) {
								data.blockArray [bX, y, bZ] = bedrockID;
							} else if (y <= stone0) {
								data.blockArray [bX, y, bZ] = stoneID;
								data.metaArray [bX, y, bZ] = 4;
							} else if (y <= stone1) {
								data.blockArray [bX, y, bZ] = stoneID;
								data.metaArray [bX, y, bZ] = 3;
							} else if (y <= stone2) {
								data.blockArray [bX, y, bZ] = stoneID;
								data.metaArray [bX, y, bZ] = 2;
							} else if (y <= stone3) {
								data.blockArray [bX, y, bZ] = stoneID;
								data.metaArray [bX, y, bZ] = 1;
							} else if (y <= stone4) {
								data.blockArray [bX, y, bZ] = stoneID;
								data.metaArray [bX, y, bZ] = 0;
							} else if (y < dirt) {
								data.blockArray [bX, y, bZ] = dirtID;
							} else if (y == dirt) {
								data.blockArray [bX, y, bZ] = grassID;
							}

							blocks [data.blockArray [bX, y, bZ]].OnLoad (world, x, y, z, data.metaArray [bX, y, bZ]);
						}
					}
				}

				GenerateSunlight ();
				needsSave = true;
			}

			foreach (Chunk c in chunks) {
				c.modified = true;
			}
		} catch (System.Exception e) {
			Debug.LogError (e);
		}
	}

	public void GenerateSunlight ()
	{
		byte maxLight = CubeRenderer.MAX_LIGHT;
		int yMax = height * chunkSize - 1;

		// beam sunlight downwards
		for (int bX=0; bX<chunkSize; bX++) {
			for (int bZ=0; bZ<chunkSize; bZ++) {
				for (int y = yMax; y >= 0; y--) {
					if (blocks [data.blockArray [bX, y, bZ]].opaque) {
						break;
					} else {
						data.lightArray [bX, y, bZ] = maxLight;
					}
				}
			}
		}

		// flood fill
		for (int bX=0; bX<chunkSize; bX++) {
			for (int bZ=0; bZ<chunkSize; bZ++) {
				for (int y = yMax; y >= 0; y--) {
					if (data.lightArray [bX, y, bZ] > 0) {
						FloodFillLight (bX, y, bZ, data.lightArray [bX, y, bZ], true);
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
			if (blocks [data.blockArray [x, i, z]].opaque)
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
		byte light = data.lightArray [x, y, z];
		if (light > 0 && light < prevLight) {
			// spread
			FloodFillDarkness (x - 1, y, z, light);
			FloodFillDarkness (x + 1, y, z, light);
			FloodFillDarkness (x, y - 1, z, light);
			FloodFillDarkness (x, y + 1, z, light);
			FloodFillDarkness (x, y, z - 1, light);
			FloodFillDarkness (x, y, z + 1, light);

			// darken
			data.lightArray [x, y, z] = 0;
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
		while (chunks.Length < height || ReferenceEquals( chunks [y / chunkSize], null ))
			Thread.Sleep (10);
		chunks [y / chunkSize].modified = true;
		if (blocks [data.blockArray [x, y, z]].opaque) {
			data.lightArray [x, y, z] = 0;
			return;
		}

		//spread here if needed
		if (remainingLight > 0 && (source || data.lightArray [x, y, z] < remainingLight)) {
			data.lightArray [x, y, z] = remainingLight;
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
					blocks [data.blockArray [bX, y, bZ]].OnUnload (world, x, y, z, data.metaArray [bX, y, bZ]);
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

	public void Save () {
		string fileName = savePath;

		// create folder
		string folder = System.IO.Path.GetDirectoryName(fileName);
		if (!System.IO.Directory.Exists(folder)) {
			System.IO.Directory.CreateDirectory(folder);
		}

		// save to file
		Stream stream = File.Open(fileName, FileMode.OpenOrCreate);
		BinaryFormatter bformatter = new BinaryFormatter();
		bformatter.Binder = new VersionDeserializationBinder();
		bformatter.Serialize(stream, data);
        stream.Close();
    }

	public bool Load () {
		string fileName = savePath;

		// make sure the file exists
		if (!File.Exists (fileName))
			return false;

		// load the file
		Stream stream = File.Open(fileName, FileMode.Open);
		BinaryFormatter bformatter = new BinaryFormatter();
		bformatter.Binder = new VersionDeserializationBinder(); 
		data = (PersistentData)bformatter.Deserialize(stream);
        stream.Close();
		return true;
    }
}

public sealed class VersionDeserializationBinder : SerializationBinder 
{ 
	public override System.Type BindToType( string assemblyName, string typeName )
	{ 
		if ( !string.IsNullOrEmpty( assemblyName ) && !string.IsNullOrEmpty( typeName ) ) 
		{ 
			System.Type typeToDeserialize = null; 
			
			assemblyName = Assembly.GetExecutingAssembly().FullName; 
			
			// The following line of code returns the type. 
			typeToDeserialize = System.Type.GetType( System.String.Format( "{0}, {1}", typeName, assemblyName ) ); 
			
			return typeToDeserialize; 
		} 
        
        return null; 
    } 
}
