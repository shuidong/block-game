using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CNTouchpad : MonoBehaviour
{
	// Private delegate to automatically switch between Mouse input and Touch input
	private delegate void InputHandler ();

	public bool remoteTesting = false;
	// Editor variables
	//public float pixelToUnits = 1000f;
	public PlacementSnap placementSnap = PlacementSnap.leftBottom;
	public Rect tapZone;
	public float snappiness = 0.1f;

	// Script-only public variables
	public Camera CurrentCamera { get; set; }

	public event JoystickMoveEventHandler JoystickMovedEvent;
	public event FingerLiftedEventHandler FingerLiftedEvent;
	public event FingerTouchedEventHandler FingerTouchedEvent;
	/**
     * Private instance variables
     */
	// Joystick base, large circle
	private GameObject joystickBase;
	// Radius of the large circle in world units
	//private float joystickBaseRadius;
	// Camera frustum height
	private float frustumHeight;
	// Camera frustum width
	private float frustumWidth;
	// Finger ID to track
	private int myFingerId = -1;
	// Where did we touch initially
	private Vector3 invokeTouchPosition;
	// Relative position of the small joystick circle
	private Vector3 joystickRelativePosition;
	// Screen point in units cacher variable
	private Vector3 screenPointInUnits;
	// Magic Vector3, needed for different snap placements
	private Vector3 relativeExtentSummand;
	// This joystick is currently being tweaked
	private bool isTweaking = false;
	// Touch or Click
	private InputHandler CurrentInputHandler;
	// Distance to camera
	//private float distanceToCamera = 0.5f;
	// Half of screen sizes
	private float halfScreenHeight;
	private float halfScreenWidth;
	// Full screen sizes
	private float screenHeight;
	private float screenWidth;
	// Snap position, relative joystick position
	private Vector3 snapPosition;
	// (-halfScreenWidth, -halfScreenHeight, 0f)
	// Visually, it's the bottom left point in local units
	private Vector3 cleanPosition;
	// Some transform cache variables
	private Transform joystickBaseTransform;
	private Transform transformCache;

	// Use this for initialization
	void Start ()
	{
		CurrentCamera = transform.parent.camera;

		transformCache = transform;
		joystickBase = transformCache.FindChild ("Base").gameObject;
		joystickBaseTransform = joystickBase.transform;

		InitialCalculations ();

#if UNITY_IPHONE || UNITY_ANDROID || UNITY_WP8 || UNITY_BLACKBERRY
        CurrentInputHandler = TouchInputHandler;
#endif
#if UNITY_EDITOR || UNITY_WEBPLAYER || UNITY_STANDALONE
        // gameObject.SetActive(false);
        CurrentInputHandler = MouseInputHandler;
#endif
		if (remoteTesting)
			CurrentInputHandler = TouchInputHandler;
	}

	void Update ()
	{
		// Automatically call proper input handler
		CurrentInputHandler ();
	}

	/** Our touch Input handler
     * Most of the work is done in this function
     */
	void TouchInputHandler ()
	{
		// Current touch count
		int touchCount = Input.touchCount;
		// If we're not yet tweaking, we should check
		// whether any touch lands on our BoxCollider or not
		if (!isTweaking) {
			for (int i = 0; i < touchCount; i++) {
				// Get current touch
				Touch touch = Input.GetTouch (i);
				// We check it's phase.
				// If it's not a Begin phase, finger didn't tap the screen
				// it's probably just slided to our TapRect
				// So for the sake of optimization we won't do anything with this touch
				// But if it's a tap, we check if it lands on our TapRect 
				// See TouchOccured function
				if (touch.phase == TouchPhase.Began && TouchOccured (touch.position)) {
					// We should store our finger ID 
					myFingerId = touch.fingerId;
					// If it's a valid touch, we dispatch our FingerTouchEvent
					if (FingerTouchedEvent != null)
						FingerTouchedEvent ();
				}
			}
		}
        // We take Touch screen position and convert it to local joystick - relative coordinates
        else {
			// This boolean represents if current touch has a Ended phase.
			// It's here for more code readability
			bool isGoingToEnd = false;
			for (int i = 0; i < touchCount; i++) {
				Touch touch = Input.GetTouch (i);
				// For every finger out there, we check if OUR finger has just lifted from the screen
				if (myFingerId == touch.fingerId && touch.phase == TouchPhase.Ended) {
					// And if it does, we reset our Joystick with this function
					ResetJoystickPosition ();
					// We store our boolean here
					isGoingToEnd = true;
					// And dispatch our FingerLiftedEvent
					if (FingerLiftedEvent != null)
						FingerLiftedEvent ();
				}
			}
			// If user didn't lift his finger this frame
			if (!isGoingToEnd) {
				// We find our current Touch index (it's not always equal to Finger index)
				int currentTouchIndex = FindMyFingerId ();
				if (currentTouchIndex != -1) {
					// And call our TweakJoystick function with this finger
					TweakJoystick (Input.GetTouch (currentTouchIndex).position);
				}
			}
		}
	}
#if UNITY_EDITOR || UNITY_WEBPLAYER || UNITY_STANDALONE
    // Mouse input handler, nothing really interesting
    // It's pretty straightforward
    void MouseInputHandler()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TouchOccured(Input.mousePosition);
        }
        if (Input.GetMouseButton(0))
        {
            if (isTweaking)
            {
                TweakJoystick(Input.mousePosition);
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            ResetJoystickPosition();
        }
    }
#endif
	/**
     * Snappings calculation
     * Joystick radius calculation based on specified pixel to units value
     * Initial on-screen placement, relative to the specified camera
     */
	void InitialCalculations ()
	{
		halfScreenHeight = CurrentCamera.orthographicSize;
		halfScreenWidth = halfScreenHeight * CurrentCamera.aspect;

		screenHeight = halfScreenHeight * 2f;
		screenWidth = halfScreenWidth * 2f;

		snapPosition = new Vector3 ();
		snapPosition.z = transformCache.localPosition.z;
		cleanPosition = new Vector3 (-halfScreenWidth, -halfScreenHeight);

		switch (placementSnap) {
		// We do nothing, it's a default position
		case PlacementSnap.leftBottom:
			snapPosition.x = -halfScreenWidth + tapZone.width / 2f - tapZone.x;
			snapPosition.y = -halfScreenHeight + tapZone.height / 2f - tapZone.y;

                // Tap zone change so we can utilize Rect's .Contains() method
			tapZone.x = 0f;
			tapZone.y = 0f;
			break;
		// We swap Y component
		case PlacementSnap.leftTop:
			snapPosition.x = -halfScreenWidth + tapZone.width / 2f - tapZone.x;
			snapPosition.y = halfScreenHeight - tapZone.height / 2f - tapZone.y;

                // Tap zone change so we can utilize Rect's .Contains() method
			tapZone.x = 0f;
			tapZone.y = screenHeight - tapZone.height;
			break;
		// We swap X component
		case PlacementSnap.rightBottom:
			snapPosition.x = halfScreenWidth - tapZone.width / 2f - tapZone.x;
			snapPosition.y = -halfScreenHeight + tapZone.height / 2f - tapZone.y;

                // Tap zone change so we can utilize Rect's .Contains() method
			tapZone.x = screenWidth - tapZone.width;
			tapZone.y = 0f;
			break;
		// We swap both X and Y component
		case PlacementSnap.rightTop:
			snapPosition.x = halfScreenWidth - tapZone.width / 2f - tapZone.x;
			snapPosition.y = halfScreenHeight - tapZone.height / 2f - tapZone.y;

                // Tap zone change so we can utilize Rect's .Contains() method
			tapZone.x = screenWidth - tapZone.width;
			tapZone.y = screenHeight - tapZone.height;
			break;
		// We reset the position to where it started
		case PlacementSnap.none:
			snapPosition.x = transform.position.x;
			snapPosition.y = transform.position.y;
			
			// Tap zone change so we can utilize Rect's .Contains() method
			tapZone.x = screenWidth - tapZone.width;
			tapZone.y = screenHeight - tapZone.height;
			break;
		}
		transformCache.localPosition = snapPosition;
	}

	Vector3 origin;

	/**
     * Touch or mouse click occured
     * Store initial local position
     * Vector3 touchPosition is in Screen coordinates
     * 
     * Returns true if finger found
     * Returns fals if not
     */
	bool TouchOccured (Vector3 touchPosition)
	{
		ScreenPointToRelativeFrustumPoint (touchPosition);
		if (tapZone.Contains (screenPointInUnits)) {
			isTweaking = true;
			invokeTouchPosition = screenPointInUnits;
			transformCache.localPosition = cleanPosition;
			joystickBaseTransform.localPosition = invokeTouchPosition;
			return true;
		}
		return false;
	}

	/**
     * Try to drag small joystick knob to it's desired position (in Screen coordinates)
     */
	void TweakJoystick (Vector3 desiredPosition)
	{
		// We convert our screen coordinates of the touch to local frustum coordinates
		ScreenPointToRelativeFrustumPoint (desiredPosition);
		// And then we find our joystick relative position
		Vector3 dragDirection = screenPointInUnits - invokeTouchPosition;

		Vector3 moved = dragDirection - origin;
		origin = Vector3.Lerp(origin, dragDirection, snappiness);

		// If we're tweaking, we should dispatch our event
		if (JoystickMovedEvent != null) {
			JoystickMovedEvent (moved);
		}
	}

	/**
     * Resetting joystick sprite to its initial position
     */
	void ResetJoystickPosition ()
	{
		isTweaking = false;
		transformCache.localPosition = snapPosition;
		joystickBaseTransform.localPosition = Vector3.zero;
		origin = Vector3.zero;
		myFingerId = -1;
	}

	/**
     * We need to convert our touch or mouse position to our local joystick position
     */
	void ScreenPointToRelativeFrustumPoint (Vector3 point)
	{
		// Percentage
		float screenPointXPercent = point.x / Screen.width;
		float screenPointYPercent = point.y / Screen.height;

		screenPointInUnits.x = screenPointXPercent * screenWidth;
		screenPointInUnits.y = screenPointYPercent * screenHeight;
		screenPointInUnits.z = 0f;
	}

	// Sometimes when user lifts his finger, current touch index changes.
	// To keep track of our finger, we need to know which finger has the user lifted
	int FindMyFingerId ()
	{
		int touchCount = Input.touchCount;
		for (int i = 0; i < touchCount; i++) {
			if (Input.GetTouch (i).fingerId == myFingerId) {
				// We return current Touch index if it's our finger
				return i;
			}
		}
		// And we return -1 if there's no such finger
		// Usually this happend after user lifts the finger which he touched first
		return -1;
	}

	void OnDrawGizmos ()
	{
		Gizmos.color = Color.red;
		Vector3 gizmoPosition = new Vector3 (transform.position.x + tapZone.x, transform.position.y + tapZone.y, transform.position.z);
		Gizmos.DrawWireCube (gizmoPosition, new Vector3 (tapZone.width, tapZone.height));
	}
}
