using UnityEngine;
using System.Collections;

public class CollideWithTerrain : MonoBehaviour {

    public GameObject collisionMakerPrefab;
    public WorldController worldObject;

    void Awake() {
        GameObject obj = Instantiate(collisionMakerPrefab) as GameObject;
        CollisionMaker comp = obj.GetComponent<CollisionMaker>();
        comp.targetObject = transform;
        comp.world = worldObject.world;
        comp.xRadius = 5;
        comp.yRadius = 5;
        comp.zRadius = 5;
    }
}
