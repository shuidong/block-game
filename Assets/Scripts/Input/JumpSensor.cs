using UnityEngine;
using System.Collections;

public class JumpSensor : MonoBehaviour {
	public bool triggered;
    private bool enteredThisFrame;

    void LateUpdate()
    {
        enteredThisFrame = false;
    }

	void OnTriggerEnter(Collider other) {
		triggered = true;
        enteredThisFrame = true;
	}

	void OnTriggerStay(Collider other) {
		triggered = true;
	}

	void OnTriggerExit(Collider other) {
		if(!enteredThisFrame) triggered = false;
	}
}
