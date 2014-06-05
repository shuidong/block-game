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

    void Start()
    {
        cameraObject = GameObject.FindGameObjectWithTag("MainCamera");
        world = GetComponent<CollideWithTerrain>().worldObject.world;
    }

    void FixedUpdate()
    {
        Ray ray = new Ray(cameraObject.transform.position, cameraObject.transform.forward);
        RaycastHit hit;
        bool didHit = Physics.Raycast(ray, out hit, reach);

        // update the block indicator
        if (blockIndicator)
        {
            if (didHit)
            {
                // try to hit the terrain
                ushort block = ModifyTerrain.GetBlock(world, hit);
                blockIndicator.text = Block.GetInstance(block).name;
            }
            else
            {
                // otherwise make it blank
                blockIndicator.text = "";
            }
        }

        // update the target cube
        if (targetCube)
        {
            if (didHit)
            {
                targetCube.transform.position = ModifyTerrain.GetHitPositionIn(hit);
                targetCube.SetActive(true);
            }
            else
            {
                targetCube.SetActive(false);
            }
        }
    }
}
