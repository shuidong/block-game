using UnityEngine;
using System.Collections;

public class ToggleBehaviours : MonoBehaviour {

	public KeyCode toggleKey;
	public Behaviour[] targets;
	
	void Update () {
		if (Input.GetKeyDown (toggleKey))
			foreach (Behaviour b in targets)
				b.enabled = !b.enabled;
	}
}
