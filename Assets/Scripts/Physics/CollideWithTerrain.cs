using UnityEngine;
using System.Collections;

public class CollideWithTerrain : MonoBehaviour {

    public GameObject collisionMakerPrefab;
    public WorldController worldObject;
    public int radius = 2;

    void Awake() {
        GameObject obj = Instantiate(collisionMakerPrefab) as GameObject;
        CollisionMaker comp = obj.GetComponent<CollisionMaker>();
        comp.targetObject = transform;
        comp.world = worldObject.world;
        comp.xRadius = radius;
        comp.yRadius = radius;
        comp.zRadius = radius;
    }
}
