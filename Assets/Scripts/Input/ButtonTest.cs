using UnityEngine;
using System.Collections;

public class ButtonTest : MonoBehaviour {
	void Update () {
		CheckButton ("Dig");
		CheckButton ("Use");
		CheckButton ("Equip");
	}

	void CheckButton(string name) {
		if (InputProxy.GetButtonDown (name)) {
			print(name);
		}
	}
}
