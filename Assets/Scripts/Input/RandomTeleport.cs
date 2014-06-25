using UnityEngine;
using System.Collections;

public class RandomTeleport : MonoBehaviour
{
    public KeyCode key = KeyCode.T;
    public float range = 100000f;

    void Start()
    {
        if (!Debug.isDebugBuild) Destroy(this);
    }

    void Update()
    {
        if (Input.GetKeyDown(key))
        {
            float x = transform.position.x;
            float y = World.WORLD_HEIGHT * World.CHUNK_SIZE - 5;
            float z = transform.position.z;
            float spd = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ? 5000 : 500;
            bool random = true;

            if (Input.GetKey(KeyCode.UpArrow))
            {
                random = false;
                z += spd;
            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                random = false;
                z -= spd;
            }
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                random = false;
                x -= spd;
            }
            if (Input.GetKey(KeyCode.RightArrow))
            {
                random = false;
                x += spd;
            }

            if (random)
            {
                x = Random.Range(-range, range);
                z = Random.Range(-range, range);
            }

            transform.position = new Vector3(x, y, z);
        }
    }

    private GUIStyle style;
    public Rect startRect = new Rect(150, 50, 125, 100);
    public bool allowDrag = true;
    private int windowID;

    void OnGUI()
    {
        // Copy the default label skin, change the color and the alignement
        if (style == null)
        {
            style = new GUIStyle(GUI.skin.label);
            style.normal.textColor = Color.white;
            style.padding = new RectOffset(8, 8, 20, 8);
            style.alignment = TextAnchor.MiddleCenter;
            windowID = System.Guid.NewGuid().GetHashCode();
        }

        GUI.color = Color.white;
        startRect = GUI.Window(windowID, startRect, DoMyWindow, "Player Position");
    }

    void DoMyWindow(int windowID)
    {
        string str = System.String.Format("x={0:F1}\ny={1:F1}\nz={2:F1}", transform.position.x, transform.position.y, transform.position.z);
        GUI.Label(new Rect(0, 0, startRect.width, startRect.height), str, style);
        if (allowDrag) GUI.DragWindow(new Rect(0, 0, Screen.width, Screen.height));
    }
}
