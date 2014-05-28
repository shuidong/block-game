using UnityEngine;
using System.Collections;

public class InputProxy : MonoBehaviour {
	private static InputProxy instance;
	
	public CNJoystick leftStick;
	private Vector3 leftStickPos;
	private bool leftTweakedLastFrame;

	public CNTouchpad rightStick;
	private Vector3 rightStickPos;
	private bool rightTweakedLastFrame;
	public float rightScale = 0.5f;

	public JumpSensor solidJumpSensor;
	public JumpSensor notSolidJumpSensor;

	public NamedButton[] buttons;

	void Awake() {
		instance = this;
	}

	void Start() {
		if (leftStick) {
			leftStick.JoystickMovedEvent += LeftStickMoved;
			leftStick.FingerLiftedEvent += LeftStickReleased;
        }

		if (rightStick) {
			rightStick.JoystickMovedEvent += RightStickMoved;
			rightStick.FingerLiftedEvent += RightStickReleased;
		}
	}

	void Update() {
		if (leftStick)
			if (!leftTweakedLastFrame)
				LeftStickReleased ();
		leftTweakedLastFrame = false;

		if (rightStick)
			if (!rightTweakedLastFrame)
				RightStickReleased ();
		rightTweakedLastFrame = false;
	}

	void LeftStickReleased ()
	{
		leftStickPos = Vector3.zero;
	}
	
	void LeftStickMoved (Vector3 relativeVector)
	{
		leftStickPos = relativeVector;
		leftTweakedLastFrame = true;
	}

	void RightStickReleased ()
	{
		rightStickPos = Vector3.zero;
	}
	
	void RightStickMoved (Vector3 relativeVector)
	{
		rightStickPos = relativeVector * rightScale;
		rightTweakedLastFrame = true;
    }

	// handles horizontal, vertical, and mouse x/y
	public static float GetAxis(string name) {
		// on screen joystick
		if (instance.leftStick && instance.leftStick.gameObject.activeInHierarchy) {
			if (name == "Horizontal")
				return instance.leftStickPos.x;
			else if (name == "Vertical")
				return instance.leftStickPos.y;
        }

		// on screen touchpad
		if (instance.rightStick && instance.rightStick.gameObject.activeInHierarchy) {
			if (name == "Mouse X")
				return instance.rightStickPos.x;
			else if (name == "Mouse Y")
				return instance.rightStickPos.y;
        }

		// normal input
		return Input.GetAxis(name);
	}

	public static bool GetButtonDown(string name) {
		// on screen buttons
		bool found = false;
		foreach (NamedButton b in instance.buttons) {
			if (b.name == name && b.button) {
				found = true;
				if (b.button.justPressed)
					return true;
			}
		}
		
		// normal input
		return !found && Input.GetButtonDown(name);
	}

	public static bool GetButtonUp(string name) {
		// on screen buttons
		bool found = false;
		foreach (NamedButton b in instance.buttons) {
			if (b.name == name && b.button) {
				found = true;
				if (b.button.justReleased)
					return true;
			}
		}
		
		// normal input
		return !found && Input.GetButtonDown(name);
	}

	public static bool GetButton(string name) {
		// autojump
		if (instance.solidJumpSensor && instance.notSolidJumpSensor && name == "Jump" && GetAxis("Vertical") > 0) {
			if (instance.solidJumpSensor.triggered && !instance.notSolidJumpSensor.triggered)
				return true;
		}

		// on screen buttons
		bool found = false;
		foreach (NamedButton b in instance.buttons) {
			if (b.name == name && b.button) {
				found = true;
				if (b.button.pressed)
					return true;
			}
		}

		// normal input
		return !found && Input.GetButton(name);
	}
}

[System.Serializable]
public class NamedButton {
	public OnScreenButton button;
	public string name;
}