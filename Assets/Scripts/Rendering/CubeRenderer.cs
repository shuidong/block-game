using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CubeRenderer {
	public const byte tSize = 4;

    public static void Cube(MeshBuildInfo current, int x, int y, int z, TextureLayout tex, Vector3 center, Vector3 size) {
		CubeTop (current, x, y, z, tex, center, size);
		CubeNorth (current, x, y, z, tex, center, size);
		CubeEast (current, x, y, z, tex, center, size);
		CubeWest (current, x, y, z, tex, center, size);
		CubeSouth (current, x, y, z, tex, center, size);
		CubeBottom (current, x, y, z, tex, center, size);
	}

	public static void CubeTop (MeshBuildInfo current, int x, int y, int z, TextureLayout tex, Vector3 center, Vector3 size)
	{
		Vector3 pos = new Vector3 (x, y, z);
		current.vertices.Add (pos + Vertex.tNW(center, size));
		current.vertices.Add (pos + Vertex.tNE(center, size));
		current.vertices.Add (pos + Vertex.tSE(center, size));
		current.vertices.Add (pos + Vertex.tSW(center, size));
		
		byte textureID = tex.texTop;
		PushFace (current, TextureLocation(textureID));
		
	}
	
	public static void CubeNorth (MeshBuildInfo current, int x, int y, int z, TextureLayout tex, Vector3 center, Vector3 size)
	{
		Vector3 pos = new Vector3 (x, y, z);
		current.vertices.Add (pos + Vertex.bNE(center, size));
		current.vertices.Add (pos + Vertex.tNE(center, size));
		current.vertices.Add (pos + Vertex.tNW(center, size));
		current.vertices.Add (pos + Vertex.bNW(center, size));
		
		byte textureID = tex.texNorth;
		PushFace (current, TextureLocation(textureID));
	}
	
	public static void CubeEast (MeshBuildInfo current, int x, int y, int z, TextureLayout tex, Vector3 center, Vector3 size)
	{
		Vector3 pos = new Vector3 (x, y, z);
		current.vertices.Add (pos + Vertex.bSE(center, size));
		current.vertices.Add (pos + Vertex.tSE(center, size));
		current.vertices.Add (pos + Vertex.tNE(center, size));
		current.vertices.Add (pos + Vertex.bNE(center, size));
		
		byte textureID = tex.texEast;
		PushFace (current, TextureLocation(textureID));
	}
	
	public static void CubeSouth (MeshBuildInfo current, int x, int y, int z, TextureLayout tex, Vector3 center, Vector3 size)
	{
		Vector3 pos = new Vector3 (x, y, z);
		current.vertices.Add (pos + Vertex.bSW(center, size));
		current.vertices.Add (pos + Vertex.tSW(center, size));
		current.vertices.Add (pos + Vertex.tSE(center, size));
		current.vertices.Add (pos + Vertex.bSE(center, size));
		
		byte textureID = tex.texSouth;
		PushFace (current, TextureLocation(textureID));
	}
	
	public static void CubeWest (MeshBuildInfo current, int x, int y, int z, TextureLayout tex, Vector3 center, Vector3 size)
	{
		Vector3 pos = new Vector3 (x, y, z);
		current.vertices.Add (pos + Vertex.bNW(center, size));
		current.vertices.Add (pos + Vertex.tNW(center, size));
		current.vertices.Add (pos + Vertex.tSW(center, size));
		current.vertices.Add (pos + Vertex.bSW(center, size));
		
		byte textureID = tex.texWest;
		PushFace (current, TextureLocation(textureID));
	}
	
	public static void CubeBottom (MeshBuildInfo current, int x, int y, int z, TextureLayout tex, Vector3 center, Vector3 size)
	{
		Vector3 pos = new Vector3 (x, y, z);
		current.vertices.Add (pos + Vertex.bSW(center, size));
		current.vertices.Add (pos + Vertex.bSE(center, size));
		current.vertices.Add (pos + Vertex.bNE(center, size));
		current.vertices.Add (pos + Vertex.bNW(center, size));
        
        byte textureID = tex.texBottom;
		PushFace (current, TextureLocation(textureID));
    }

	private static void PushFace (MeshBuildInfo current, Vector2 texturePos)
	{
		int faceCount = current.faceCount;
		current.triangles.Add (faceCount * 4); //1
		current.triangles.Add (faceCount * 4 + 1); //2
		current.triangles.Add (faceCount * 4 + 2); //3
		current.triangles.Add (faceCount * 4); //1
		current.triangles.Add (faceCount * 4 + 2); //3
		current.triangles.Add (faceCount * 4 + 3); //4
		
		float tUnit = 1f / tSize;
		
		current.uv.Add (new Vector2 (tUnit * texturePos.x + tUnit, tUnit * texturePos.y));
		current.uv.Add (new Vector2 (tUnit * texturePos.x + tUnit, tUnit * texturePos.y + tUnit));
		current.uv.Add (new Vector2 (tUnit * texturePos.x, tUnit * texturePos.y + tUnit));
		current.uv.Add (new Vector2 (tUnit * texturePos.x, tUnit * texturePos.y));
        
		current.faceCount++;
	}

	public static Vector2 TextureLocation (byte tIndex) {
		return new Vector2 (tIndex % tSize, tIndex / tSize);
    }
}