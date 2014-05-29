using UnityEngine;
using System.Collections;

[RequireComponent(typeof(TextMesh))]
[RequireComponent(typeof(Camera))]
public class LoadingIndicator : MonoBehaviour {
	public string[] strings;
	public float time = 1f;
	public bool random = false;
	public bool autoText = true;

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
			if(!autoText) {
				yield return null;
				continue;
			}

			SetText(strings[current]);
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

	public void SetText(string t) {
		gameObject.GetComponent<TextMesh>().text = t;
		camera.Render();
	}
}
