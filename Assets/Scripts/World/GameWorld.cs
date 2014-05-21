using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameWorld : MonoBehaviour {
	// world settings
	public string worldName = "World 1";
	public Vector3 spawnPoint = new Vector3(0, 30, 0);
	public int height = 2;
	public int loadRange = 5;
	public int unloadRange = 7;

	// instantiation info
	public int chunkSize = 16;
	public GameObject chunkPrefab;
	public GameObject playerPrefab;

	// currently loaded state
	private GameObject player;
	private Dictionary<WorldChunk.ChunkLocation, WorldChunk> loadedWorld;

	void Start ()
	{
		transform.position = Vector3.zero;
		loadedWorld = new Dictionary<WorldChunk.ChunkLocation, WorldChunk> ();
		LoadChunks ();
	}

	public void LoadChunks() {
		StartCoroutine (LoadChunksCoroutine());
	}

	IEnumerator LoadChunksCoroutine() {
		while (true) {
			Vector3 playerPos = spawnPoint;
			if (player) playerPos = player.transform.position;

			WorldChunk.ChunkLocation playerLoc = GetChunkLocation ((int)playerPos.x, (int)playerPos.y, (int)playerPos.z);

			List<WorldChunk.ChunkLocation> removalList = new List<WorldChunk.ChunkLocation>();

			// mark chunks for unload
			foreach (WorldChunk chunk in loadedWorld.Values) {
				float dist = playerLoc.Distance2(chunk.location);
				if (Mathf.Ceil(dist) >= unloadRange) {
					removalList.Add(chunk.location);
				}
			}

			// unload marked chunks
			foreach (WorldChunk.ChunkLocation loc in removalList) {
				UnloadColumn(loc.x, loc.z);
			}

			// load needed chunks
			for (int x = playerLoc.x-loadRange; x <= playerLoc.x+loadRange; x++) {
				for (int z = playerLoc.z-loadRange; z <= playerLoc.z+loadRange; z++) {
					WorldChunk.ChunkLocation loc = new WorldChunk.ChunkLocation(x, 0, z);
					float dist = playerLoc.Distance2(loc);
					if (Mathf.Ceil(dist) < loadRange && !loadedWorld.ContainsKey(loc)) {
						StartCoroutine(LoadColumn(x, z));
						if (player)
							yield return new WaitForSeconds(.1f);
					}
	            }
	        }
			yield return new WaitForSeconds(1);

			if (!player) {
				// if the player hasn't been spawned, spawn it after 20 frames
				for (int i = 0; i < 20; i++)
					yield return null;
				player = Instantiate(playerPrefab, spawnPoint, Quaternion.identity) as GameObject;
				player.GetComponent<PlayerBuild>().world = gameObject.GetComponent<ModifyTerrain>();
			}
		}
    }

	WorldChunk.ChunkLocation GetChunkLocation(int x, int y, int z) {
		WorldChunk.ChunkLocation loc = new WorldChunk.ChunkLocation (x / chunkSize, y / chunkSize, z / chunkSize);
		if (x < 0 && x % chunkSize != 0)
			loc.x--;
		if (y < 0 && y % chunkSize != 0)
			loc.y--;
		if (z < 0 && z % chunkSize != 0)
            loc.z--;
		return loc;
	}

	int mod(int k, int n) {  return ((k %= n) < 0) ? k+n : k;  }

	/**
	 * Get the block at the specified position in world coords. 
	 * If the block is not loaded, def is returned
	 */
	public byte Block (int x, int y, int z, byte def)
	{
		WorldChunk.ChunkLocation loc = GetChunkLocation (x, y, z);
		WorldChunk chunk;
		
		// return the block if it's loaded
		if (loadedWorld.TryGetValue (loc, out chunk)) {
			return chunk.LocalBlock(mod(x, chunkSize), mod(y, chunkSize), mod(z, chunkSize));
        }
        
        // else return def
        return def;
	}

	IEnumerator LoadColumn(int x, int z) {
		for (int y=height-1; y>=0; y--) {
			WorldChunk.ChunkLocation loc = new WorldChunk.ChunkLocation(x, y, z);
			if (!loadedWorld.ContainsKey(loc)) {
				GameObject newObject = Instantiate (chunkPrefab, new Vector3 (x * chunkSize - 0.5f, y * chunkSize - 0.5f, z * chunkSize - 0.5f), new Quaternion (0, 0, 0, 0)) as GameObject;
				newObject.transform.parent = transform;
				WorldChunk newChunk = newObject.GetComponent<WorldChunk>();
				newChunk.chunkSize = chunkSize;
				newChunk.location = loc;
                newChunk.world = this;
                loadedWorld.Add(loc, newChunk);
            }
			yield return null;
        }
	}

	void MarkColumnModified(int x, int z) {
		if (loadedWorld.ContainsKey (new WorldChunk.ChunkLocation (x, 0, z))) {
			for (int y=0; y<height; y++) {
				WorldChunk.ChunkLocation loc = new WorldChunk.ChunkLocation(x, y, z);
				loadedWorld[loc].modified = true;
			}
		}
	}
	
	public void UnloadColumn (int x, int z)
	{
		for (int y=height-1; y>=0; y--) {
			WorldChunk.ChunkLocation loc = new WorldChunk.ChunkLocation(x, y, z);
			WorldChunk chunk = null;
			loadedWorld.TryGetValue(loc, out chunk);
			if(chunk) {
				Object.Destroy (chunk.gameObject);
				loadedWorld.Remove(loc);
			}
		}
	}

	public void SetBlockAt (int x, int y, int z, byte block)
	{
		//adds the specified block at these coordinates
		WorldChunk.ChunkLocation loc = GetChunkLocation (x, y, z);
		WorldChunk chunk;
		if (loadedWorld.TryGetValue (loc, out chunk)) {
			int cX = mod (x, chunkSize);
			int cY = mod (y, chunkSize);
			int cZ = mod (z, chunkSize);
			chunk.data[cX, cY, cZ] = block;
			for (int xx = -1; xx <= 1; xx++)
				for (int yy = -1; yy <= 1; yy++)
					for(int zz = -1; zz <= 1; zz++)
						UpdateChunkAt (x+xx, y+yy, z+zz);
		}
	}
	
	public void UpdateChunkAt (int x, int y, int z)
	{ 
		//Updates the chunk containing this block
		WorldChunk.ChunkLocation loc = GetChunkLocation (x, y, z);
		WorldChunk chunk;
		if (loadedWorld.TryGetValue (loc, out chunk)) {
			chunk.modified = true;
        }
    }
}
