using UnityEngine;
using System.Collections;

public class OnScreenButton : MonoBehaviour {

	Camera parentCamera;
	Transform t;

	Vector2 screenSize;
	Vector2 cameraView;

	int myTouch = -1;

	public Rect tapZone;
	public bool mouse;

	[HideInInspector]
	public bool pressed, justPressed, justReleased;

	void Start ()
	{
		t = transform;
		parentCamera = t.parent.camera;
		screenSize = new Vector2 (Screen.width, Screen.height);
		float cHeight = parentCamera.orthographicSize * 2;
		cameraView = new Vector2 (cHeight * screenSize.x/screenSize.y, cHeight);
	}

	void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Vector3 gizmoPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);
		Gizmos.DrawWireCube(gizmoPosition, new Vector3(tapZone.width, tapZone.height));
	}

	void Update() {
		justPressed = false;
		justReleased = false;

		if (mouse) {
			if (Input.GetMouseButtonDown (0) && IsWithinBounds (TransformScreenPos (Input.mousePosition))) {
				pressed = true;
				justPressed = true;
			}
			
			if (Input.GetMouseButtonUp (0) && pressed) {
				pressed = false;
				justReleased = true;
			}
		} else {
			foreach (Touch t in Input.touches) {
				if (myTouch < 0) {
					// look for any touch down
					if (t.phase == TouchPhase.Began && IsWithinBounds (TransformScreenPos (t.position))) {
						myTouch = t.fingerId;
						pressed = true;
						justPressed = true;
					}
				} else if (myTouch == t.fingerId) {
					// look for my touch up
					if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled) {
						myTouch = -1;
						pressed = false;
						justReleased = true;
					}
				}
			}
		}
	}

	Vector2 TransformScreenPos(Vector3 input) {
		float x = cameraView.x*(input.x/screenSize.x - .5f);
		float y = cameraView.y*(input.y/screenSize.y - .5f);
		return new Vector2 (x, y);
	}

	bool IsWithinBounds(Vector2 pos) {
		Rect transformed = tapZone;
		transformed.center = new Vector2 (t.localPosition.x, t.localPosition.y);
		return transformed.Contains(pos);
	}
}
