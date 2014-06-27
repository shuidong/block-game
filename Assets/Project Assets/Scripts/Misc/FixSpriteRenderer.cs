using UnityEngine;
using System.Collections;

public class FixSpriteRenderer : MonoBehaviour {
	void Awake()
	{
		SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
		if (spriteRenderer != null)
			spriteRenderer.material.mainTexture = spriteRenderer.sprite.texture;
	}
}
