using UnityEngine;
using System.Collections;

public class ToggleShadows : MonoBehaviour {
	public KeyCode toggleKey;
	public Light target;

	void Update() {
		if (Input.GetKeyDown (toggleKey)) {
			if (target.shadows == LightShadows.None)
				target.shadows = LightShadows.Hard;
			else
				target.shadows = LightShadows.None;
		}
	}
}
