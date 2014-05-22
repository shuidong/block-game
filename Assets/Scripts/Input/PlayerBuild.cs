using UnityEngine;
using System.Collections;

public class PlayerBuild : MonoBehaviour {

	public ModifyTerrain world;
	public byte placeID = 1;
	public byte reach = 4;
	public GameObject collisionMakerPrefab;

	void Start() {
		if (collisionMakerPrefab) {
			GameObject newObject = Instantiate (collisionMakerPrefab) as GameObject;
			CollisionMaker col = newObject.GetComponent<CollisionMaker> ();
			col.targetObject = transform;
			col.xRadius = reach + 1;
			col.yRadius = reach + 1;
			col.zRadius = reach + 1;
			col.world = world.GetComponent<GameWorld>();
		}
	}

	void Update() {
		if (world) {
			if (InputProxy.GetButtonDown("Dig"))
				world.ReplaceBlockCenter(reach, 0);
			if (InputProxy.GetButtonDown("Use"))
				world.AddBlockCenter(reach,placeID);
		}
	}
}
