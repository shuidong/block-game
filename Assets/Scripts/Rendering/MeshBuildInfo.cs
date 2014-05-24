using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeshBuildInfo {
	public List<Vector3> vertices;
	public List<int> triangles;
	public List<Vector2> uv;
	public List<Color> colors;
	public int faceCount;

	public MeshBuildInfo() {
		vertices = new List<Vector3>();
		triangles = new List<int> ();
		uv = new List<Vector2> ();
		colors = new List<Color>();
		faceCount = 0;
	}
	
	public void Clear() {
		vertices.Clear ();
		triangles.Clear ();
		uv.Clear ();
		colors.Clear ();
		faceCount = 0;
	}
}