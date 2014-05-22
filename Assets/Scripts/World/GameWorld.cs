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
	public int chunkSize = 15;
	public GameObject columnPrefab;
	public GameObject playerPrefab;

	// currently loaded state
	private GameObject player;
	private Dictionary<Vector2, ChunkColumn> loadedWorld;

	void Start ()
	{
		transform.position = Vector3.zero;
		loadedWorld = new Dictionary<Vector2, ChunkColumn> ();
		LoadChunks ();
	}

	public void LoadChunks() {
		StartCoroutine (LoadChunksCoroutine());
	}

	IEnumerator LoadChunksCoroutine() {
		while (true) {
			Vector3 playerPos = spawnPoint;
			if (player) playerPos = player.transform.position;

			Vector2 playerLoc = GetColumnLocation ((int)playerPos.x, (int)playerPos.z);

			List<Vector2> removalList = new List<Vector2>();

			// mark chunks for unload
			foreach (ChunkColumn col in loadedWorld.Values) {
				float dist = Vector2.Distance(playerLoc, col.location);
				if (Mathf.Ceil(dist) >= unloadRange) {
					removalList.Add(col.location);
				}
			}

			// unload marked chunks
			foreach (Vector2 loc in removalList) {
				UnloadColumn((int)loc.x, (int)loc.y);
			}

			// load needed chunks
			for (int x = (int)playerLoc.x-loadRange; x <= (int)playerLoc.x+loadRange; x++) {
				for (int z = (int)playerLoc.y-loadRange; z <= (int)playerLoc.y+loadRange; z++) {
					Vector2 loc = new Vector2(x, z);

					float dist = Vector2.Distance(playerLoc, loc);
					if (Mathf.Ceil(dist) < loadRange && !loadedWorld.ContainsKey(loc)) {
						LoadColumn(x, z);
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

	Vector3 GetChunkLocation(int x, int y, int z) {
		Vector3 loc = new Vector3 (x / chunkSize, y / chunkSize, z / chunkSize);
		if (x < 0 && x % chunkSize != 0)
			loc.x--;
		if (y < 0 && y % chunkSize != 0)
			loc.y--;
		if (z < 0 && z % chunkSize != 0)
			loc.z--;
		return loc;
	}

	Vector3 GetColumnLocation(int x, int z) {
		Vector2 loc = new Vector2 (x / chunkSize, z / chunkSize);
		if (x < 0 && x % chunkSize != 0)
			loc.x--;
		if (z < 0 && z % chunkSize != 0)
			loc.y--;
		return loc;
	}

	int mod(int k, int n) { return ((k %= n) < 0) ? k+n : k;  }

	/**
	 * Get the block at the specified position in world coords. 
	 * If the block is not loaded, def is returned
	 */
	public byte Block (int x, int y, int z, byte def)
	{
		Vector2 loc = GetColumnLocation (x, z);
		ChunkColumn column;

		// return the block if it's loaded
		if (loadedWorld.TryGetValue (loc, out column)) {
			return column.LocalBlock(mod(x, chunkSize), y, mod(z, chunkSize));
        }
        
        // else return def
        return def;
	}

	void LoadColumn(int x, int z) {
		Vector2 loc = new Vector2 (x, z);
		if (!loadedWorld.ContainsKey (loc)) {
			GameObject newObject = Instantiate (columnPrefab, new Vector3 (x * chunkSize - 0.5f, 0, z * chunkSize - 0.5f), new Quaternion (0, 0, 0, 0)) as GameObject;
			newObject.transform.parent = transform;
			ChunkColumn column = newObject.GetComponent<ChunkColumn> ();
			column.world = this;
			column.height = height;
			column.location = loc;
			loadedWorld.Add(loc, column);
		}
	}

	void MarkColumnModified(int x, int z) {
		Vector2 loc = new Vector2(x, z);
		if (loadedWorld.ContainsKey (loc)) {
			ChunkColumn col = loadedWorld[loc];
			for (int y=0; y<height; y++) {
				col.chunks[y].modified = true;
			}
		}
	}
	
	public void UnloadColumn (int x, int z)
	{
		Vector2 loc = new Vector2(x, z);
		if (loadedWorld.ContainsKey (loc)) {
			ChunkColumn col = loadedWorld[loc];
			Object.Destroy(col.gameObject);
			loadedWorld.Remove(loc);
		}
	}

	public void SetBlockAt (int x, int y, int z, byte block)
	{
		//sets the specified block at these coordinates
		Vector2 loc = GetColumnLocation (x, z);
		ChunkColumn col;
		if (loadedWorld.TryGetValue (loc, out col)) {
			int cX = mod (x, chunkSize);
			int cZ = mod (z, chunkSize);
			col.data[cX, y, cZ] = block;
			for (int xx = -1; xx <= 1; xx++)
				for (int yy = -1; yy <= 1; yy++)
					for(int zz = -1; zz <= 1; zz++)
						UpdateChunkAt (x+xx, y+yy, z+zz);
		}
	}
	
	public void UpdateChunkAt (int x, int y, int z)
	{ 
		//Updates the chunk containing this block
		Vector3 loc = GetChunkLocation (x, y, z);
		ChunkColumn col;
		if (loadedWorld.TryGetValue (new Vector2(loc.x, loc.z), out col)) {
			col.chunks[(int)loc.y].modified = true;
        }
    }
}
