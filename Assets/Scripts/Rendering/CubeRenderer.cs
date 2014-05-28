using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CubeRenderer
{
	public const byte T_SIZE = 4;
	public const byte MAX_LIGHT = 15;
	public static Color[] lightColors;

	static CubeRenderer ()
	{
		lightColors = new Color[MAX_LIGHT + 1];
		Color light = new Color (1 / 2f, 1 / 2f, 1 / 2f);
		for (int i = MAX_LIGHT; i >= 0; i--) {
			lightColors [i] = light;
			light *= .85f;
		}
	}

	private static byte LightAverage (byte a, byte b, byte c, byte d)
	{
		return (byte)((a + b + c + d) / 4);
	}

	private static bool FlipTriangles (byte a, byte b, byte c, byte d)
	{
		return (a + c) < (b + d);
	}

	public static void CubeTop (MeshBuildInfo current, int x, int y, int z, TextureLayout tex, Vector3 center, Vector3 size, byte[] l, bool smooth, Color color)
	{
		Vector3 pos = new Vector3 (x, y, z);
		current.vertices.Add (pos + Vertex.tNW (center, size));
		current.vertices.Add (pos + Vertex.tNE (center, size));
		current.vertices.Add (pos + Vertex.tSE (center, size));
		current.vertices.Add (pos + Vertex.tSW (center, size));

		// light array format
		//     N
		//   0 1 2
		// W 3 4 5 E
		//   6 7 8
		//     S

		// get light at each corner
		byte[] corners = new byte[4];
		if (smooth) {
			corners [0] = LightAverage (l [0], l [1], l [3], l [4]);
			corners [1] = LightAverage (l [1], l [2], l [4], l [5]);
			corners [2] = LightAverage (l [4], l [5], l [7], l [8]);
			corners [3] = LightAverage (l [3], l [4], l [6], l [7]);
		} else {
			for (int i = 0; i < 4; i++) {
				corners [i] = l [4];
			}
		}

		// decide whether to flip triangles
		bool flip = !smooth ? false : FlipTriangles (corners [0], corners [1], corners [2], corners [3]);

		// finish up the face
		byte textureID = tex.texTop;
		PushFace (current, TextureLocation (textureID), corners, flip, color);

	}
	
	public static void CubeNorth (MeshBuildInfo current, int x, int y, int z, TextureLayout tex, Vector3 center, Vector3 size, byte[] l, bool smooth, Color color)
	{
		Vector3 pos = new Vector3 (x, y, z);
		current.vertices.Add (pos + Vertex.bNE (center, size));
		current.vertices.Add (pos + Vertex.tNE (center, size));
		current.vertices.Add (pos + Vertex.tNW (center, size));
		current.vertices.Add (pos + Vertex.bNW (center, size));

		// light array format
		//     T
		//   0 1 2
		// W 3 4 5 E
		//   6 7 8
		//     B

		// get light at each corner
		byte[] corners = new byte[4];
		if (smooth) {
			corners [0] = LightAverage (l [4], l [5], l [7], l [8]);
			corners [1] = LightAverage (l [1], l [2], l [4], l [5]);
			corners [2] = LightAverage (l [0], l [1], l [3], l [4]);
			corners [3] = LightAverage (l [3], l [4], l [6], l [7]);
		} else {
			for (int i = 0; i < 4; i++) {
				corners [i] = l [4];
			}
		}
		
		// decide whether to flip triangles
		bool flip = !smooth ? false : FlipTriangles (corners [2], corners [1], corners [0], corners [3]);

		byte textureID = tex.texNorth;
		PushFace (current, TextureLocation (textureID), corners, flip, color);
	}
	
	public static void CubeEast (MeshBuildInfo current, int x, int y, int z, TextureLayout tex, Vector3 center, Vector3 size, byte[] l, bool smooth, Color color)
	{
		Vector3 pos = new Vector3 (x, y, z);
		current.vertices.Add (pos + Vertex.bSE (center, size));
		current.vertices.Add (pos + Vertex.tSE (center, size));
		current.vertices.Add (pos + Vertex.tNE (center, size));
		current.vertices.Add (pos + Vertex.bNE (center, size));

		// light array format
		//     N
		//   0 1 2
		// B 3 4 5 T
		//   6 7 8
		//     S
		
		// get light at each corner
		byte[] corners = new byte[4];
		if (smooth) {
			corners [0] = LightAverage (l [3], l [4], l [6], l [7]);
			corners [1] = LightAverage (l [4], l [5], l [7], l [8]);
			corners [2] = LightAverage (l [1], l [2], l [4], l [5]);
			corners [3] = LightAverage (l [0], l [1], l [3], l [4]);
		} else {
			for (int i = 0; i < 4; i++) {
				corners [i] = l [4];
			}
		}

		// decide whether to flip triangles
		bool flip = !smooth ? false : FlipTriangles (corners [3], corners [2], corners [1], corners [0]);

		byte textureID = tex.texEast;
		PushFace (current, TextureLocation (textureID), corners, flip, color);
	}
	
	public static void CubeSouth (MeshBuildInfo current, int x, int y, int z, TextureLayout tex, Vector3 center, Vector3 size, byte[] l, bool smooth, Color color)
	{
		Vector3 pos = new Vector3 (x, y, z);
		current.vertices.Add (pos + Vertex.bSW (center, size));
		current.vertices.Add (pos + Vertex.tSW (center, size));
		current.vertices.Add (pos + Vertex.tSE (center, size));
		current.vertices.Add (pos + Vertex.bSE (center, size));

		// light array format
		//     T
		//   0 1 2
		// W 3 4 5 E
		//   6 7 8
		//     B
		
		// get light at each corner
		byte[] corners = new byte[4];
		if (smooth) {
			corners [0] = LightAverage (l [3], l [4], l [6], l [7]);
			corners [1] = LightAverage (l [0], l [1], l [3], l [4]);
			corners [2] = LightAverage (l [1], l [2], l [4], l [5]);
			corners [3] = LightAverage (l [4], l [5], l [7], l [8]);
		} else {
			for (int i = 0; i < 4; i++) {
				corners [i] = l [4];
			}
		}

		// decide whether to flip triangles
		bool flip = !smooth ? false : FlipTriangles (corners [1], corners [2], corners [3], corners [0]);

		byte textureID = tex.texSouth;
		PushFace (current, TextureLocation (textureID), corners, flip, color);
	}
	
	public static void CubeWest (MeshBuildInfo current, int x, int y, int z, TextureLayout tex, Vector3 center, Vector3 size, byte[] l, bool smooth, Color color)
	{
		Vector3 pos = new Vector3 (x, y, z);
		current.vertices.Add (pos + Vertex.bNW (center, size));
		current.vertices.Add (pos + Vertex.tNW (center, size));
		current.vertices.Add (pos + Vertex.tSW (center, size));
		current.vertices.Add (pos + Vertex.bSW (center, size));

		// light array format
		//     N
		//   0 1 2
		// B 3 4 5 T
		//   6 7 8
		//     S
		
		// get light at each corner
		byte[] corners = new byte[4];
		if (smooth) {
			corners [0] = LightAverage (l [0], l [1], l [3], l [4]);
			corners [1] = LightAverage (l [1], l [2], l [4], l [5]);
			corners [2] = LightAverage (l [4], l [5], l [7], l [8]);
			corners [3] = LightAverage (l [3], l [4], l [6], l [7]);
		} else {
			for (int i = 0; i < 4; i++) {
				corners [i] = l [4];
			}
		}

		// decide whether to flip triangles
		bool flip = !smooth ? false : FlipTriangles (corners [0], corners [1], corners [2], corners [3]);

		byte textureID = tex.texWest;
		PushFace (current, TextureLocation (textureID), corners, flip, color);
	}
	
	public static void CubeBottom (MeshBuildInfo current, int x, int y, int z, TextureLayout tex, Vector3 center, Vector3 size, byte[] l, bool smooth, Color color)
	{
		Vector3 pos = new Vector3 (x, y, z);
		current.vertices.Add (pos + Vertex.bSW (center, size));
		current.vertices.Add (pos + Vertex.bSE (center, size));
		current.vertices.Add (pos + Vertex.bNE (center, size));
		current.vertices.Add (pos + Vertex.bNW (center, size));
        
		// light array format
		//     N
		//   0 1 2
		// W 3 4 5 E
		//   6 7 8
		//     S

		// get light at each corner
		byte[] corners = new byte[4];
		if (smooth) {
			corners [0] = LightAverage (l [3], l [4], l [6], l [7]);
			corners [1] = LightAverage (l [4], l [5], l [7], l [8]);
			corners [2] = LightAverage (l [1], l [2], l [4], l [5]);
			corners [3] = LightAverage (l [0], l [1], l [3], l [4]);
		} else {
			for (int i = 0; i < 4; i++) {
				corners [i] = l [4];
			}
		}

		// decide whether to flip triangles
		bool flip = !smooth ? false : FlipTriangles (corners [3], corners [2], corners [1], corners [0]);

		byte textureID = tex.texBottom;
		PushFace (current, TextureLocation (textureID), corners, flip, color);
	}

	private static void PushFace (MeshBuildInfo current, Vector2 texturePos, byte[] light, bool flip, Color color)
	{
		// apply light to verts
		for (int i = 0; i < light.Length; i++) {
			current.colors.Add (color * lightColors [light [i]]);
		}

		// create triangles
		int f = flip ? 1 : 0;
		int faceCount = current.faceCount;
		current.triangles.Add (faceCount * 4 + ((0 + f) % 4));
		current.triangles.Add (faceCount * 4 + ((1 + f) % 4));
		current.triangles.Add (faceCount * 4 + ((2 + f) % 4));
		current.triangles.Add (faceCount * 4 + ((0 + f) % 4));
		current.triangles.Add (faceCount * 4 + ((2 + f) % 4));
		current.triangles.Add (faceCount * 4 + ((3 + f) % 4));

		// apply textures
		float tUnit = 1f / T_SIZE;
		current.uv.Add (new Vector2 (tUnit * texturePos.x + tUnit, tUnit * texturePos.y));
		current.uv.Add (new Vector2 (tUnit * texturePos.x + tUnit, tUnit * texturePos.y + tUnit));
		current.uv.Add (new Vector2 (tUnit * texturePos.x, tUnit * texturePos.y + tUnit));
		current.uv.Add (new Vector2 (tUnit * texturePos.x, tUnit * texturePos.y));
        
		// count number of faces in the mesh
		current.faceCount++;
	}

	public static Vector2 TextureLocation (byte tIndex)
	{
		return new Vector2 (tIndex % T_SIZE, tIndex / T_SIZE);
	}
}