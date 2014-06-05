using UnityEngine;
using System.Collections;

public class PlayerLogic : MonoBehaviour
{
    private GameObject cameraObject;
    private World world;

    // editor params
    public TextMesh blockIndicator;
    public GameObject targetCube;
    public float reach = 4;
    public ushort held = 1;

    // capture cursor
    public static bool lockCursor = true;

    void Start()
    {
        cameraObject = GameObject.FindGameObjectWithTag("MainCamera");
        world = GetComponent<CollideWithTerrain>().worldObject.world;
    }

    void Update()
    {
        // capture cursor
        if (Screen.lockCursor != lockCursor)
        {
            if (lockCursor && Input.GetMouseButton(0))
                Screen.lockCursor = true;
            else if (!lockCursor)
                Screen.lockCursor = false;
        }

        // interact with the world
        Ray ray = new Ray(cameraObject.transform.position, cameraObject.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, reach))
        {
            // did hit
            targetCube.transform.position = WorldInterface.GetHitPositionIn(hit);
            targetCube.SetActive(true);

            ushort block = WorldInterface.GetBlock(world, hit);
            blockIndicator.text = Block.GetInstance(block).name;

            if (InputProxy.GetButtonDown("Dig"))
            {
                WorldInterface.ReplaceBlock(world, hit, Block.AIR);
                GetComponent<CollideWithTerrain>().collisionMaker.UpdateColliders();
            }

            if (InputProxy.GetButtonDown("Use"))
            {
                WorldInterface.AddBlock(world, hit, held);
                GetComponent<CollideWithTerrain>().collisionMaker.UpdateColliders();
            }

            if (InputProxy.GetButtonDown("Equip"))
            {
                held = WorldInterface.GetBlock(world, hit);
            }
        }
        else
        {
            // did not hit
            blockIndicator.text = "";
            targetCube.SetActive(false);
        }
    }
}
