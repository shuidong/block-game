using UnityEngine;
using System.Collections;

public class Loader : MonoBehaviour
{
	void Start()
	{
		if(!Debug.isDebugBuild) Next();
	}

	void Update()
	{
		if(Input.anyKeyDown) Next();
	}

	void OnGUI ()
	{
		GUI.Label(new Rect(0, 0, Screen.width, Screen.height), "Game is running in debug mode. Press any key to continue.");
	}

	void Next()
	{
		Application.LoadLevel(Application.loadedLevel+1);
	}
}
