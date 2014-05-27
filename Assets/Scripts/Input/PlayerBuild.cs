using UnityEngine;
using System.Collections;

public class PlayerBuild : MonoBehaviour {

	public ModifyTerrain world;
	public byte placeID = 1;
	public byte reach = 4;
	public GameObject collisionMakerPrefab;
	private CollisionMaker collisionMaker;

	void Start() {
		Screen.lockCursor = true;
		Screen.showCursor = false;
		if (collisionMakerPrefab) {
			GameObject newObject = Instantiate (collisionMakerPrefab) as GameObject;
			collisionMaker = newObject.GetComponent<CollisionMaker> ();
			collisionMaker.targetObject = transform;
			collisionMaker.xRadius = reach + 1;
			collisionMaker.yRadius = reach + 1;
			collisionMaker.zRadius = reach + 1;
			collisionMaker.world = world.GetComponent<GameWorld>();
		}
	}

	void Update() {
		if (world) {
			if (InputProxy.GetButtonDown("Dig")) {
				world.ReplaceBlockCenter(reach, 0);
				collisionMaker.UpdateColliders();
			}
			if (InputProxy.GetButtonDown("Use")) {
				world.AddBlockCenter(reach,placeID);
				collisionMaker.UpdateColliders();
			}
			if (InputProxy.GetButtonDown("Equip")) {
				placeID = world.GetBlockCenter(reach);
			}
		}
	}
}
