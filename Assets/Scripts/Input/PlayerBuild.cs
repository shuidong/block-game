using UnityEngine;
using System.Collections;

public class PlayerBuild : MonoBehaviour {

	public ModifyTerrain world;
	public BlockMeta placeBlock = new BlockMeta(1, 0);
	public byte reach = 4;
	public GameObject collisionMakerPrefab;
	private CollisionMaker collisionMaker;
	public TextMesh pointText;

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
			if(pointText) {
				pointText.text = ListBlocks.instance.blocks[world.GetBlockCenter(reach).block].name;
			}

			if (InputProxy.GetButtonDown("Dig")) {
				world.ReplaceBlockCenter(reach, 0, 0);
				collisionMaker.UpdateColliders();
			}
			if (InputProxy.GetButtonDown("Use")) {
				world.AddBlockCenter(reach, placeBlock.block, placeBlock.meta);
				collisionMaker.UpdateColliders();
			}
			if (InputProxy.GetButtonDown("Equip")) {
				BlockMeta id = world.GetBlockCenter(reach);
				if (id.block != ListBlocks.AIR) {
					placeBlock = id;
				}
			}
		}
	}
}
