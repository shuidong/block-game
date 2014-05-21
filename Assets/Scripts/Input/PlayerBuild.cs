using UnityEngine;
using System.Collections;

public class PlayerBuild : MonoBehaviour {

	public ModifyTerrain world;
	public byte placeID = 1;

	void Update() {
		if (world) {
			if (InputProxy.GetButtonDown("Dig"))
				world.ReplaceBlockCenter(4, 0);
			if (InputProxy.GetButtonDown("Use"))
				world.AddBlockCenter(4,placeID);
		}
	}
}
