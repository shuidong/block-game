using UnityEngine;
using System.Collections;

public struct TextureLayout {
	public byte texTop;
	public byte texBottom;
	public byte texEast;
	public byte texNorth;
	public byte texWest;
	public byte texSouth;
	
	public TextureLayout(byte allTex) {
		texTop = texBottom = texEast = texNorth = texWest = texSouth = allTex;
	}
	
	public TextureLayout(byte sideTex, byte botTex, byte topTex) {
		texEast = texNorth = texWest = texSouth = sideTex;
		texBottom = botTex;
		texTop = topTex;
	}
}