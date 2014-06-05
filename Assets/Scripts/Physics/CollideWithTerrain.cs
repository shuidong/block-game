using UnityEngine;
using System.Collections;

public class CollideWithTerrain : MonoBehaviour {

    public GameObject collisionMakerPrefab;
    public WorldController worldObject;
    public int radius = 2;

    [HideInInspector]
    public CollisionMaker collisionMaker;

    void Awake() {
        GameObject obj = Instantiate(collisionMakerPrefab) as GameObject;
        collisionMaker = obj.GetComponent<CollisionMaker>();
        collisionMaker.targetObject = transform;
        collisionMaker.world = worldObject.world;
        collisionMaker.xRadius = radius;
        collisionMaker.yRadius = radius;
        collisionMaker.zRadius = radius;
    }
}
