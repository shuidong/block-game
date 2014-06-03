using UnityEngine;
using System.Collections;

public class JumpSensor : MonoBehaviour {
	public bool triggered;

	void OnTriggerEnter(Collider other) {
		triggered = true;
	}

	void OnTriggerStay(Collider other) {
		triggered = true;
	}

	void OnTriggerExit(Collider other) {
		triggered = false;
	}
}
