using UnityEngine;
using System.Collections;

public class CollisionMaker : MonoBehaviour {
	public int xRadius = 3;
	public int yRadius = 3;
	public int zRadius = 3;
	public Transform targetObject;
	public World world;
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
        StartCoroutine (UpdatePeriodically ());
	}

	void FixedUpdate() {
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

    IEnumerator UpdatePeriodically() {
        while (true) {
            yield return new WaitForSeconds(1);
            UpdateColliders();
        }
    }

	public void UpdateColliders() {
		Vector3 myPos = transform.position;
		for (int x = 0; x < points.GetLength(0); x++) {
			for (int y = 0; y < points.GetLength(1); y++) {
				for (int z = 0; z < points.GetLength(2); z++) {
					Vector3 boxOffset = origin + new Vector3(x, y, z);
                    Vector3 boxPosition = boxOffset + myPos;

                    int worldX = (int) boxPosition.x;
                    int worldY = (int) boxPosition.y;
                    int worldZ = (int) boxPosition.z;

                    ushort blockID = world.GetBlockAt(worldX, worldY, worldZ, Block.DIRT);
					BoxCollider boxCollider = points[x,y,z];
					Block block = Block.GetInstance(blockID);
					if(block.opaque) {
						boxCollider.enabled = true;
						Bounds shape = block.collisionBounds;
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
}
