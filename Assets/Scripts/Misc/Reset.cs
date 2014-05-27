using UnityEngine;
using System.Collections;

public class Reset : MonoBehaviour {
	void Start() {
		transform.parent = null;
		transform.position = Vector3.zero;
		transform.rotation = Quaternion.identity;
	}
}
