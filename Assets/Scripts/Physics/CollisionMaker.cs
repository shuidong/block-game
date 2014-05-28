using UnityEngine;
using System.Collections;

public class CollisionMaker : MonoBehaviour {
	public int xRadius = 3;
	public int yRadius = 3;
	public int zRadius = 3;
	public Transform targetObject;
	public GameWorld world;
	private Vector3 origin;
	private BoxCollider[,,] points;

	void Start() {
		points = new BoxCollider[xRadius*2-1, yRadius*2-1, zRadius*2-1];
		origin = new Vector3 (-xRadius+1, -yRadius+1, -zRadius+1);
		for (int x = 0; x < points.GetLength(0); x++) {
			for (int y = 0; y < points.GetLength(1); y++) {
				for (int z = 0; z < points.GetLength(2); z++) {
					BoxCollider b = gameObject.AddComponent<BoxCollider>();
					points[x,y,z] = b;
				}
			}
		}
		UpdateColliders ();
	}

	void LateUpdate() {
		Vector3 oldPos = transform.position;

		if (targetObject) {
			Vector3 pos = targetObject.position;
			pos.Set(Mathf.Round(pos.x), Mathf.Round(pos.y), Mathf.Round(pos.z));

			if (pos != oldPos) {
				transform.position = pos;
				UpdateColliders();
			}
		}
	}

	public void UpdateColliders() {
		Vector3 myPos = transform.position;
		for (int x = 0; x < points.GetLength(0); x++) {
			for (int y = 0; y < points.GetLength(1); y++) {
				for (int z = 0; z < points.GetLength(2); z++) {
					Vector3 boxOffset = origin + new Vector3(x, y, z);
					byte blockID = world.Block(boxOffset + myPos, ListBlocks.STONE).block;
					BoxCollider boxCollider = points[x,y,z];
					Block block = ListBlocks.instance.blocks[blockID];
					if(block.collide) {
						boxCollider.enabled = true;
						Bounds shape = block.GetBounds();
						boxCollider.center = boxOffset + shape.center;
						boxCollider.size = shape.size;
					} else if(boxCollider.enabled) {
						boxCollider.enabled = false;
						boxCollider.center = Vector3.zero;
						boxCollider.size = Vector3.one;
					}
				}
			}
		}
	}

	void OnDrawGizmosSelected() {
		if (points == null)
			return;

		Gizmos.color = Color.blue;
		Vector3 pos = transform.position;
		foreach (BoxCollider b in points) {
			if(b.enabled) Gizmos.DrawWireCube(b.center + pos, b.size);
		}
	}
}
