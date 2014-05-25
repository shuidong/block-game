using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

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
	public bool needsUpdate;
	public bool modified;
	
	void Awake() {
		mesh = GetComponent<MeshFilter> ().mesh;
	}

	void Start() {
		chunkSize = column.chunkSize;
	}

	void LateUpdate () {
		if(modified) {
			GenerateMesh();
			modified = false;
			needsUpdate = true;
		}

		if(needsUpdate){
			UpdateMesh();
			needsUpdate=false;
		}

	}

	void OnDrawGizmosSelected() {
		Vector3 size = Vector3.one * chunkSize;
		Vector3 center = transform.position + size / 2;
		Gizmos.color = Color.black;
		Gizmos.DrawWireCube (center, size);
	}

	public byte LocalBlock (int x, int y, int z, byte def)
	{
		return column.LocalBlock (x, (int)location.y*chunkSize + y, z, def);
	}

	public byte LocalLight (int x, int y, int z, byte def)
	{
		return column.LocalLight (x, (int)location.y*chunkSize + y, z, def);
	}
	
	private void GenerateMesh ()
	{
		Block[] blocks = ListBlocks.instance.blocks;

		for (int x=0; x<chunkSize; x++) {
			for (int y=0; y<chunkSize; y++) {
				for (int z=0; z<chunkSize; z++) {
					blocks[LocalBlock (x, y, z, 0)].Render(newMesh, this, x, y, z);
				}
			}
		}

		newVerticesArr = newMesh.vertices.ToArray ();
		newUVArr = newMesh.uv.ToArray ();
		newTrianglesArr = newMesh.triangles.ToArray ();
		newColorsArr = newMesh.colors.ToArray ();
	}

	private void UpdateMesh ()
	{
		mesh.Clear ();
		mesh.vertices = newVerticesArr;
		mesh.uv = newUVArr;
		mesh.triangles = newTrianglesArr;
		mesh.colors = newColorsArr;
		mesh.Optimize ();
		mesh.RecalculateNormals ();
		newMesh.Clear ();
	}
}
