using UnityEngine;
using System.Collections;

[RequireComponent(typeof(TextMesh))]
[RequireComponent(typeof(Camera))]
public class LoadingIndicator : MonoBehaviour {
	public string[] strings;
	public float time = 1f;
	public bool random = false;

	int current;

	void Awake() {
		if (random)
			current = Random.Range (0, strings.Length);
		else
			current = 0;
		StartCoroutine (SwitchIndicator());
	}

	IEnumerator SwitchIndicator() {
		while (true) {
			gameObject.GetComponent<TextMesh>().text = strings[current];
			camera.Render();
			yield return new WaitForSeconds(time);
			if (random)
				current = Random.Range (0, strings.Length);
			else
				current++;
			current %= strings.Length;
		}
	}

	public void Done() {
		StopCoroutine (SwitchIndicator());
		Destroy (gameObject);
	}
}
