using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

public class GameWorld : MonoBehaviour
{
	// world settings
	public string worldName = "World 1";
	public Vector3 spawnPoint = new Vector3 (0, 30, 0);
	public int height = 2;
	public int loadRange = 5;
	public int unloadRange = 7;
	public bool loadDynamically = true;

	// instantiation info
	public int chunkSize = 15;
	public GameObject columnPrefab;
	public GameObject playerPrefab;
	public LoadingIndicator loadingScreen;

	// currently loaded state
	[HideInInspector]
	public GameObject player;
	[HideInInspector]
	public Dictionary<Vector2, ChunkColumn> loadedWorld;

	// chunk rendering thread stuff
	public List<Chunk> chunkUpdateQueue = new List<Chunk>();
	private bool[] chunkUpdaterIdle;
	private Chunk[] currentlyWorkingChunk;
	private int numCPU;

	//

	void Start ()
	{
		numCPU = SystemInfo.processorCount;
		chunkUpdaterIdle = new bool[numCPU];
		currentlyWorkingChunk = new Chunk[numCPU];
		for (int i = 0; i < numCPU; i++)
			chunkUpdaterIdle[i] = true;

		transform.position = Vector3.zero;
		loadedWorld = new Dictionary<Vector2, ChunkColumn> ();
		LoadChunks ();
		Thread t = new Thread (new ThreadStart (SaveColumns));
		t.Priority = System.Threading.ThreadPriority.Lowest;
		t.Start ();
	}

	void Update() {
		Vector3 playerPos = player == null ? Vector3.zero : player.transform.position;
		Vector3 playerChunk = GetChunkLocation ((int)playerPos.x, (int)playerPos.y, (int)playerPos.z);

		// render chunks
		for (int i = 0; i < numCPU; i++) {
			if (chunkUpdaterIdle [i] && chunkUpdateQueue.Count > 0) {
				// find closest chunk
				float bestDist = float.MaxValue;
				int bestIndex = -1;
				for (int j = 0; j < chunkUpdateQueue.Count; j++) {
					float dist = Vector3.Distance (playerChunk, chunkUpdateQueue [j].location);
					if (dist < bestDist) {
						bestIndex = j;
						bestDist = dist;
					}
				}

				// update the chunk
				Chunk c = chunkUpdateQueue [bestIndex];
				if (!c.needsUpdate) {
					currentlyWorkingChunk [i] = c;
					chunkUpdateQueue.RemoveAt (bestIndex);

					lock (chunkUpdaterIdle) chunkUpdaterIdle [i] = false;
					Thread t = new Thread (new ParameterizedThreadStart (RenderChunk));
					t.Priority = System.Threading.ThreadPriority.Highest;
					t.Start (i);
				}
			}
		}
	}

	void RenderChunk(System.Object o) {
		try {
			int i = (int)o;
			lock (currentlyWorkingChunk[i]) {
				try {
					currentlyWorkingChunk [i].GenerateMesh ();
				} catch(System.Exception e) {
					Debug.LogError(e);
				}
			}
			currentlyWorkingChunk [i] = null;
			lock(chunkUpdaterIdle) chunkUpdaterIdle [i] = true;
		} catch (System.Exception e) {
			Debug.LogError(e);
		}
	}

	public int saveInterval = 10000;
	public bool playing = true;

	void SaveColumns() {
		while (playing) {
			try {
				Thread.Sleep(saveInterval);

				// find columns that need saving
				List<ChunkColumn> cols = new List<ChunkColumn>();
				lock(loadedWorld) {
					foreach (ChunkColumn col in loadedWorld.Values) {
						if (col.needsSave) {
							cols.Add(col);
						}
					}
				}

				// save them after unlocking
				foreach (ChunkColumn col in cols) {
					col.Save();
					col.needsSave = false;
				}
			} catch (System.Exception e) {
				Debug.LogError (e);
			}
		}
	}

	void OnApplicationQuit() {
		playing = false;
	}

	public void LoadChunks ()
	{
		StartCoroutine (LoadChunksCoroutine ());
	}

	IEnumerator LoadChunksCoroutine ()
	{
		yield return null;
		loadingScreen.autoText = false;
		while (true) {
			Vector3 playerPos = spawnPoint;
			if (player)
				playerPos = player.transform.position;

			Vector2 playerLoc = GetColumnLocation ((int)playerPos.x, (int)playerPos.z);

			List<Vector2> removalList = new List<Vector2> ();

			if (!player || loadDynamically) {

				// mark chunks for unload
				foreach (ChunkColumn col in loadedWorld.Values) {
					float dist = Vector2.Distance (playerLoc, col.location);
					if (Mathf.Ceil (dist) >= unloadRange) {
						removalList.Add (col.location);
					}
				}

				// unload marked chunks
				foreach (Vector2 loc in removalList) {
					UnloadColumn ((int)loc.x, (int)loc.y);
				}

				// load needed chunks
				for (int x = (int)playerLoc.x-loadRange; x <= (int)playerLoc.x+loadRange; x++) {
					for (int z = (int)playerLoc.y-loadRange; z <= (int)playerLoc.y+loadRange; z++) {
						Vector2 loc = new Vector2 (x, z);

						float dist = Vector2.Distance (playerLoc, loc);
						if (Mathf.Ceil (dist) < loadRange && !loadedWorld.ContainsKey (loc)) {
							if(loadingScreen)
								loadingScreen.SetText("GENERATE: ("  + x + ", " + z + ")");
							LoadColumn (x, z);
							if (player) {
								yield return new WaitForSeconds (.08f);
							} else {
								yield return null;
							}
						}
					}
				}

			}

			yield return new WaitForSeconds (1);

			if (!player) {
				// if the player hasn't been spawned, spawn it after 20 frames
				loadingScreen.SetText("");
				for (int i = 0; i < 20; i++)
					yield return null;

				// wait for chunks to render
				while (chunkUpdateQueue.Count > 0) {
					loadingScreen.SetText("RENDER: (" + chunkUpdateQueue.Count + " left)");
					yield return new WaitForSeconds(0.1f);
				}

				player = Instantiate (playerPrefab, spawnPoint, Quaternion.identity) as GameObject;
				player.GetComponent<PlayerBuild> ().world = gameObject.GetComponent<ModifyTerrain> ();
				if(loadingScreen)
					loadingScreen.Done();
			}
		}
	}

	Vector3 GetChunkLocation (int x, int y, int z)
	{
		Vector3 loc = new Vector3 (x / chunkSize, y / chunkSize, z / chunkSize);
		if (x < 0 && x % chunkSize != 0)
			loc.x--;
		if (y < 0 && y % chunkSize != 0)
			loc.y--;
		if (z < 0 && z % chunkSize != 0)
			loc.z--;
		return loc;
	}

	Vector3 GetColumnLocation (int x, int z)
	{
		Vector2 loc = new Vector2 (x / chunkSize, z / chunkSize);
		if (x < 0 && x % chunkSize != 0)
			loc.x--;
		if (z < 0 && z % chunkSize != 0)
			loc.y--;
		return loc;
	}

	int mod (int k, int n)
	{
		return ((k %= n) < 0) ? k + n : k;
	}

	public BlockMeta Block (Vector3 pos, byte def)
	{
		int x = (int)pos.x;
		int y = (int)pos.y;
		int z = (int)pos.z;
		return Block (x, y, z, def);
	}

	public BlockMeta Block (int x, int y, int z, byte def)
	{
		Vector2 loc = GetColumnLocation (x, z);
		ChunkColumn column;
		
		// return the block if it's loaded
		if (y >= 0 && y < height * chunkSize && loadedWorld.TryGetValue (loc, out column)) {
			return column.LocalBlock (mod (x, chunkSize), y, mod (z, chunkSize));
		}
		
		// else return def
		return new BlockMeta (def, 0);
	}

	public byte Light (int x, int y, int z, byte def)
	{
		Vector2 loc = GetColumnLocation (x, z);
		ChunkColumn column;
		
		// return the block if it's loaded
		if (y >= 0 && y < height * chunkSize && loadedWorld.TryGetValue (loc, out column)) {
			return column.LocalLight (mod (x, chunkSize), y, mod (z, chunkSize));
		}
		
		// else return def
		return def;
	}

	void LoadColumn (int x, int z)
	{
		Vector2 loc = new Vector2 (x, z);
		if (!loadedWorld.ContainsKey (loc)) {
			GameObject newObject = Instantiate (columnPrefab, new Vector3 (x * chunkSize - 0.5f, 0, z * chunkSize - 0.5f), new Quaternion (0, 0, 0, 0)) as GameObject;
			newObject.transform.parent = transform;
			ChunkColumn column = newObject.GetComponent<ChunkColumn> ();
			column.world = this;
			column.height = height;
			column.location = loc;
			column.chunkSize = chunkSize;
			column.data = new ChunkColumn.PersistentData(chunkSize, height);
			lock(loadedWorld) {
				loadedWorld.Add (loc, column);
			}
		}
	}

	void MarkColumnModified (int x, int z)
	{
		Vector2 loc = new Vector2 (x, z);
		if (loadedWorld.ContainsKey (loc)) {
			ChunkColumn col = loadedWorld [loc];
			for (int y=0; y<height; y++) {
				col.chunks [y].modified = true;
			}
		}
	}
	
	public void UnloadColumn (int x, int z)
	{
		Vector2 loc = new Vector2 (x, z);
		if (loadedWorld.ContainsKey (loc)) {
			ChunkColumn col = loadedWorld [loc];
			Object.Destroy (col.gameObject);
			lock(loadedWorld) {
				loadedWorld.Remove (loc);
			}
		}
	}

	public void SetBlockAt (int x, int y, int z, byte block, byte meta)
	{
		Block[] blocks = ListBlocks.instance.blocks;

		//sets the specified block at these coordinates
		Vector2 loc = GetColumnLocation (x, z);
		ChunkColumn col;
		if (y >= 0 && y < height * chunkSize && loadedWorld.TryGetValue (loc, out col)) {

			int cX = mod (x, chunkSize);
			int cZ = mod (z, chunkSize);

			// set block
			col.data.blockArray [cX, y, cZ] = block;
			col.data.metaArray [cX, y, cZ] = meta;

			// update lighting
			if (col.CanSeeSky (cX, y, cZ)) {

				// if block is exposed to the sun
				int yy = y;
				if (blocks [block].opaque) {
					// remove light which passed through this point
					while (yy >= 0 && (y == yy || !blocks[col.data.blockArray[cX, yy, cZ]].opaque)) {
						col.FloodFillDarkness (cX, yy, cZ, CubeRenderer.MAX_LIGHT + 1);
						yy--;
					}
				} else {
					// flood fill sunlight
					while (yy >= 0 && (!blocks[col.data.blockArray[cX, yy, cZ]].opaque)) {
						col.FloodFillLight (cX, yy, cZ, CubeRenderer.MAX_LIGHT, true);
						yy--;
					}
				}
			} else {
				if (blocks [block].opaque) {
					// remove light which passed through this point
					col.FloodFillDarkness (cX, y, cZ, CubeRenderer.MAX_LIGHT + 1);
				}
			}

			// update nearby blocks and their light
			for (int xx = -1; xx <= 1; xx++) {
				for (int yy = -1; yy <= 1; yy++) {
					for (int zz = -1; zz <= 1; zz++) {
						RefillLightAtPosition (x + xx, y + yy, z + zz);
					}
				}
			}
		}

		// save the column
		col.needsSave = true;
	}

	void RefillLightAtPosition (int x, int y, int z)
	{
		if (y >= 0 && y < height * chunkSize) {
			Vector2 loc = GetColumnLocation (x, z);
			ChunkColumn col;
			if (loadedWorld.TryGetValue (loc, out col)) {
				x = mod (x, chunkSize);
				z = mod (z, chunkSize);
				col.FloodFillLight (x, y, z, col.data.lightArray [x, y, z], true);
			}
		}
	}
	
	public void UpdateChunkAt (int x, int y, int z)
	{ 
		//Updates the chunk containing this block
		if (y >= 0 && y < height * chunkSize) {
			Vector3 loc = GetChunkLocation (x, y, z);
			ChunkColumn col;
			if (loadedWorld.TryGetValue (new Vector2 (loc.x, loc.z), out col)) {
				col.chunks [(int)loc.y].modified = true;
			}
		}
	}
	
	Rect debugRect = new Rect(Screen.width-215, 50, 200, 250);
	void OnGUI()
	{
		GUI.color = Color.white;
		string s = "World Info\n";
		s += "Columns Loaded: " + loadedWorld.Count + "\n";
		s += "Load Distance: [" + loadRange + ", " + unloadRange + "]\n";
		s += "Chunk Render Queue: " + chunkUpdateQueue.Count + "\n";
		s += "Render Threads:\n";
		for (int i = 0; i < chunkUpdaterIdle.Length; i++) {
			s += "#" + i + ": " + (chunkUpdaterIdle[i] ? "Idle\n" : "Busy\n");
		}
		debugRect = GUI.Window(1, debugRect, DoMyWindow, s);
	}
	
	void DoMyWindow(int windowID)
	{
		GUI.DragWindow(new Rect(0, 0, Screen.width, Screen.height));
	}
}
