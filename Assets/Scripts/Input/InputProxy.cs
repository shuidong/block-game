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

	public static float GetAxis(string name) {
		if (instance.leftStick && instance.leftStick.gameObject.activeInHierarchy) {
			if (name == "Horizontal")
				return instance.leftStickPos.x;
			else if (name == "Vertical")
				return instance.leftStickPos.y;
        }

		if (instance.rightStick && instance.rightStick.gameObject.activeInHierarchy) {
			if (name == "Mouse X")
				return instance.rightStickPos.x;
			else if (name == "Mouse Y")
				return instance.rightStickPos.y;
        }

		return Input.GetAxis(name);
	}

	public static bool GetButtonDown(string name) {
		// temporarily cancel these input in mobile mode
		if (instance.leftStick && instance.leftStick.gameObject.activeInHierarchy) {
			if (name == "Dig" || name == "Use" || name == "Equip")
				return false;
		}
		
		return Input.GetButtonDown(name);
	}

	public static bool GetButton(string name) {
		if (instance.solidJumpSensor && instance.notSolidJumpSensor && name == "Jump" && GetAxis("Vertical") > 0) {
			if (instance.solidJumpSensor.triggered && !instance.notSolidJumpSensor.triggered)
				return true;
		}

		return Input.GetButton(name);
	}
}