using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

public class WorldChunk : MonoBehaviour {
	public struct ChunkLocation {
		public int x, y, z;
		public ChunkLocation(int x, int y, int z) {
			this.x = x;
			this.y = y;
			this.z = z;
		}

		public float Distance2(ChunkLocation other) {
			int a = x - other.x;
			//int b = y - other.y;
			int c = z - other.z;
			return Mathf.Sqrt (a*a + c*c);
		}
	}

	// mesh generation
	private MeshBuildInfo newMesh = new MeshBuildInfo();
	private Vector3[] newVerticesArr;
	private int[] newTrianglesArr;
	private Vector2[] newUVArr;
	
	// meshes
	private Mesh mesh;
	private MeshCollider col;

	// world
	public int chunkSize = 16;
	public GameWorld world;
	public ListBlocks blocks;

	// this chunk
	public ChunkLocation location;
	public byte[,,] data;
	public bool needsUpdate;
	public bool modified;
	private bool generatedOnce;
	
	void Awake() {
		mesh = GetComponent<MeshFilter> ().mesh;
		col = GetComponent<MeshCollider> ();
		data = new byte[chunkSize,chunkSize,chunkSize];
	}

	void Start() {
		generatedOnce = false;
		blocks = ListBlocks.instance;
		StartCoroutine(GenerateTerrain ());
	}

	void LateUpdate () {
		if(modified && generatedOnce) {
			StartCoroutine(GenerateMesh(true));
		}

		if(needsUpdate){
			UpdateMesh();
			needsUpdate=false;
		}

	}

	void OnDrawGizmosSelected() {
		Vector3 size = Vector3.one * chunkSize;
		Vector3 center = transform.position + size / 2;
		Gizmos.color = Color.gray;
		Gizmos.DrawWireCube (center, size);
	}

	IEnumerator GenerateTerrain() {
		// gen terrain
		int startX = location.x * chunkSize;
		int startY = location.y * chunkSize;
		int startZ = location.z * chunkSize;

		ListBlocks blocks = ListBlocks.instance;
		byte stoneID = blocks.FindByName ("Stone").id;
		byte dirtID = blocks.FindByName ("Dirt").id;
		byte grassID = blocks.FindByName ("Grass").id;

		for (int x=startX; x<startX + chunkSize; x++) {
			for (int z=startZ; z<startZ + chunkSize; z++) {
				int stone = 40 + PerlinNoise (x, 0, z, 25, 7, 1.5f);
				int dirt = stone + PerlinNoise (x, 0, z, 25, 2, 1.0f) + 1;

				for (int y=startY; y < startY + chunkSize; y++) {
					if (y <= stone) {
						data [x - startX, y - startY, z - startZ] = stoneID;
					} else if (y < dirt) {
						data [x - startX, y - startY, z - startZ] = dirtID;
					} else if (y == dirt) {
						data [x - startX, y - startY, z - startZ] = grassID;
					}
				}
			}
			yield return null;
		}

		StartCoroutine(GenerateMesh (false));
	}

	private int PerlinNoise (int x, int y, int z, float scale, float height, float power)
	{
		float rValue = Noise.GetNoise (((double)x) / scale, ((double)y) / scale, ((double)z) / scale);
		rValue *= height;
		if (power != 0)
			rValue = Mathf.Pow (rValue, power);
		return (int) rValue;
	}
	
	public byte LocalBlock (int x, int y, int z, byte def)
	{
		if (x < 0 || y < 0 || z < 0 || x >= chunkSize || y >= chunkSize || z >= chunkSize) {
			return world.Block(location.x*chunkSize + x, location.y*chunkSize + y, location.z*chunkSize + z, def);
		} else {
			return data[x,y,z];
		}
	}

	public byte LocalBlock (int x, int y, int z)
	{
		return data[x,y,z];
	}
	
	private IEnumerator GenerateMesh (bool allAtOnce)
	{
		Block[] blocks = ListBlocks.instance.blocks;

		for (int x=0; x<chunkSize; x++) {
			for (int y=0; y<chunkSize; y++) {
				for (int z=0; z<chunkSize; z++) {
					blocks[LocalBlock (x, y, z)].Render(newMesh, this, x, y, z);
				}
			}
			if (!allAtOnce) {
				yield return null;
			}
		}

		newVerticesArr = newMesh.vertices.ToArray ();
		newUVArr = newMesh.uv.ToArray ();
		newTrianglesArr = newMesh.triangles.ToArray ();
		
		needsUpdate = true;
		generatedOnce = true;
	}

	private void UpdateMesh ()
	{
		mesh.Clear ();
		mesh.vertices = newVerticesArr;
		mesh.uv = newUVArr;
		mesh.triangles = newTrianglesArr;
		mesh.Optimize ();
		mesh.RecalculateNormals ();
		
		col.sharedMesh = null;
		col.sharedMesh = mesh;
		
		newMesh.Clear ();
	}
}
