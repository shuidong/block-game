using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;

public class Chunk : MonoBehaviour {
	// mesh generation
	private MeshBuildInfo newMesh = new MeshBuildInfo();
	private Vector3[] newVerticesArr;
	private int[] newTrianglesArr;
	private Vector2[] newUVArr;
	private Color[] newColorsArr;
	
	// meshes
	private Mesh mesh;

	// world
	[HideInInspector]
	public int chunkSize;
	[HideInInspector]
	public ChunkColumn column;

	// this chunk
	[HideInInspector]
	public Vector3 location;
	public bool modified;
	public bool needsUpdate;
	[HideInInspector]
	public bool hold;

	void Awake() {
		mesh = GetComponent<MeshFilter> ().mesh;
	}

	void Start() {
		chunkSize = column.chunkSize;
	}

	void LateUpdate () {
		if (!hold) {
			if (modified) {
				//GenerateMesh ();
				lock(column.world.chunkUpdateQueue) {
					if(!column.world.chunkUpdateQueue.Contains(this))
						column.world.chunkUpdateQueue.Add(this);
					modified = false;
				}
			}

			if (needsUpdate) {
				UpdateMesh ();
				needsUpdate = false;
			}
		}
	}

	void OnDrawGizmosSelected() {
		Vector3 size = Vector3.one * chunkSize;
		Vector3 center = transform.position + size / 2;
		Gizmos.color = Color.black;
		Gizmos.DrawWireCube (center, size);
	}

	void OnDestroy() {
		column.world.chunkUpdateQueue.Remove (this);
	}

	public BlockMeta LocalBlock (int x, int y, int z, byte def)
	{
		return column.LocalBlock (x, (int)location.y*chunkSize + y, z, def);
	}

	public byte LocalLight (int x, int y, int z, byte def)
	{
		return column.LocalLight (x, (int)location.y*chunkSize + y, z, def);
	}
	
	public void GenerateMesh ()
	{
		Block[] blocks = ListBlocks.instance.blocks;

		for (int x=0; x<chunkSize; x++) {
			for (int y=0; y<chunkSize; y++) {
				for (int z=0; z<chunkSize; z++) {
					BlockMeta b = LocalBlock (x, y, z, 0);
					blocks[b.block].Render(newMesh, this, x, y, z, b.meta);
				}
			}
		}

		newVerticesArr = newMesh.vertices.ToArray ();
		newUVArr = newMesh.uv.ToArray ();
		newTrianglesArr = newMesh.triangles.ToArray ();
		newColorsArr = newMesh.colors.ToArray ();

		needsUpdate = true;
	}

	private void UpdateMesh ()
	{
		lock (this) {
			//int numVerts = mesh.vertices.Length;
			//int numUV = mesh.uv.Length;
			//int numColors = mesh.colors.Length;

			//if (numVerts == numUV && numVerts == numColors) {
				mesh.Clear ();
				mesh.vertices = newVerticesArr;
				mesh.uv = newUVArr;
				mesh.triangles = newTrianglesArr;
				mesh.colors = newColorsArr;
				mesh.Optimize ();
				mesh.RecalculateNormals ();
			//} else {
			//	print ("caught");
			//	modified = true;
			//}
			newMesh.Clear ();
		}
	}
}
