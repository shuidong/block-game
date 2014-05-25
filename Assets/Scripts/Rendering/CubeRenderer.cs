using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CubeRenderer {
	public const byte T_SIZE = 4;
	public const byte MAX_LIGHT = 15;
	public static Color[] lightColors;

	static CubeRenderer() {
		lightColors = new Color[MAX_LIGHT+1];
		Color light = new Color(1/2f, 1/2f, 1/2f);
		for (int i = MAX_LIGHT; i >= 0; i--) {
			lightColors[i] = light;
			light *= .9f;
		}
	}

	public static void CubeTop (MeshBuildInfo current, int x, int y, int z, TextureLayout tex, Vector3 center, Vector3 size, byte lightLevel)
	{
		Vector3 pos = new Vector3 (x, y, z);
		current.vertices.Add (pos + Vertex.tNW(center, size));
		current.vertices.Add (pos + Vertex.tNE(center, size));
		current.vertices.Add (pos + Vertex.tSE(center, size));
		current.vertices.Add (pos + Vertex.tSW(center, size));

		current.colors.Add (lightColors[lightLevel]);
		current.colors.Add (lightColors[lightLevel]);
		current.colors.Add (lightColors[lightLevel]);
		current.colors.Add (lightColors[lightLevel]);
		
		byte textureID = tex.texTop;
		PushFace (current, TextureLocation(textureID));
		
	}
	
	public static void CubeNorth (MeshBuildInfo current, int x, int y, int z, TextureLayout tex, Vector3 center, Vector3 size, byte lightLevel)
	{
		Vector3 pos = new Vector3 (x, y, z);
		current.vertices.Add (pos + Vertex.bNE(center, size));
		current.vertices.Add (pos + Vertex.tNE(center, size));
		current.vertices.Add (pos + Vertex.tNW(center, size));
		current.vertices.Add (pos + Vertex.bNW(center, size));

		current.colors.Add (lightColors[lightLevel]);
		current.colors.Add (lightColors[lightLevel]);
		current.colors.Add (lightColors[lightLevel]);
		current.colors.Add (lightColors[lightLevel]);

		byte textureID = tex.texNorth;
		PushFace (current, TextureLocation(textureID));
	}
	
	public static void CubeEast (MeshBuildInfo current, int x, int y, int z, TextureLayout tex, Vector3 center, Vector3 size, byte lightLevel)
	{
		Vector3 pos = new Vector3 (x, y, z);
		current.vertices.Add (pos + Vertex.bSE(center, size));
		current.vertices.Add (pos + Vertex.tSE(center, size));
		current.vertices.Add (pos + Vertex.tNE(center, size));
		current.vertices.Add (pos + Vertex.bNE(center, size));

		current.colors.Add (lightColors[lightLevel]);
		current.colors.Add (lightColors[lightLevel]);
		current.colors.Add (lightColors[lightLevel]);
		current.colors.Add (lightColors[lightLevel]);

		byte textureID = tex.texEast;
		PushFace (current, TextureLocation(textureID));
	}
	
	public static void CubeSouth (MeshBuildInfo current, int x, int y, int z, TextureLayout tex, Vector3 center, Vector3 size, byte lightLevel)
	{
		Vector3 pos = new Vector3 (x, y, z);
		current.vertices.Add (pos + Vertex.bSW(center, size));
		current.vertices.Add (pos + Vertex.tSW(center, size));
		current.vertices.Add (pos + Vertex.tSE(center, size));
		current.vertices.Add (pos + Vertex.bSE(center, size));

		current.colors.Add (lightColors[lightLevel]);
		current.colors.Add (lightColors[lightLevel]);
		current.colors.Add (lightColors[lightLevel]);
		current.colors.Add (lightColors[lightLevel]);

		byte textureID = tex.texSouth;
		PushFace (current, TextureLocation(textureID));
	}
	
	public static void CubeWest (MeshBuildInfo current, int x, int y, int z, TextureLayout tex, Vector3 center, Vector3 size, byte lightLevel)
	{
		Vector3 pos = new Vector3 (x, y, z);
		current.vertices.Add (pos + Vertex.bNW(center, size));
		current.vertices.Add (pos + Vertex.tNW(center, size));
		current.vertices.Add (pos + Vertex.tSW(center, size));
		current.vertices.Add (pos + Vertex.bSW(center, size));

		current.colors.Add (lightColors[lightLevel]);
		current.colors.Add (lightColors[lightLevel]);
		current.colors.Add (lightColors[lightLevel]);
		current.colors.Add (lightColors[lightLevel]);

		byte textureID = tex.texWest;
		PushFace (current, TextureLocation(textureID));
	}
	
	public static void CubeBottom (MeshBuildInfo current, int x, int y, int z, TextureLayout tex, Vector3 center, Vector3 size, byte lightLevel)
	{
		Vector3 pos = new Vector3 (x, y, z);
		current.vertices.Add (pos + Vertex.bSW(center, size));
		current.vertices.Add (pos + Vertex.bSE(center, size));
		current.vertices.Add (pos + Vertex.bNE(center, size));
		current.vertices.Add (pos + Vertex.bNW(center, size));
        
		current.colors.Add (lightColors[lightLevel]);
		current.colors.Add (lightColors[lightLevel]);
		current.colors.Add (lightColors[lightLevel]);
		current.colors.Add (lightColors[lightLevel]);

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

		float tUnit = 1f / T_SIZE;
		
		current.uv.Add (new Vector2 (tUnit * texturePos.x + tUnit, tUnit * texturePos.y));
		current.uv.Add (new Vector2 (tUnit * texturePos.x + tUnit, tUnit * texturePos.y + tUnit));
		current.uv.Add (new Vector2 (tUnit * texturePos.x, tUnit * texturePos.y + tUnit));
		current.uv.Add (new Vector2 (tUnit * texturePos.x, tUnit * texturePos.y));
        
		current.faceCount++;
	}

	public static Vector2 TextureLocation (byte tIndex) {
		return new Vector2 (tIndex % T_SIZE, tIndex / T_SIZE);
    }
}