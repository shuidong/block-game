using UnityEngine;
using System.Collections;

public class Attach : MonoBehaviour {

	Vector3 offset;
	public Transform target;
	Transform t;

	void Awake() {
		t = transform;
		offset = t.position - target.position;
	}

	void Update () {
		t.position = target.position + offset;
		this.enabled = false;
	}
}
