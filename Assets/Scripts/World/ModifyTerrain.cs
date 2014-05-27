using UnityEngine;
using System.Collections;

public class ModifyTerrain : MonoBehaviour
{
	GameWorld world;
	GameObject cameraObject;
	public float smallestBlockThickness = 0.2f;

	void Start ()
	{
		world = gameObject.GetComponent<GameWorld> ();
	}

	public byte GetBlockCenter (float range) {
		byte result = ListBlocks.AIR;

		if (!cameraObject) {
			cameraObject = GameObject.FindGameObjectWithTag ("MainCamera");
		}
		
		if (cameraObject) {
			//Returns the block directly in front of the player
			Ray ray = new Ray (cameraObject.transform.position, cameraObject.transform.forward);
			RaycastHit hit;
			
			if (Physics.Raycast (ray, out hit)) {
				if (hit.distance < range) {
					result = GetBlockAt (hit);
				}
				Debug.DrawLine (ray.origin, ray.origin + (ray.direction * hit.distance), Color.green, 2);
			}
		}

		return result;
	}

	public byte GetBlockAt (RaycastHit hit)
	{
		Vector3 position = hit.point;
		position += (hit.normal * -smallestBlockThickness);
		
		return GetBlockAt (position);
	}

	public byte GetBlockAt (Vector3 position)
	{
		int x = Mathf.RoundToInt (position.x);
		int y = Mathf.RoundToInt (position.y);
		int z = Mathf.RoundToInt (position.z);

		return world.Block (x, y, z, ListBlocks.AIR);
	}

	public void ReplaceBlockCenter (float range, byte block)
	{
		if (!cameraObject) {
			cameraObject = GameObject.FindGameObjectWithTag ("MainCamera");
		}

		if (cameraObject) {
			//Replaces the block directly in front of the player
			Ray ray = new Ray (cameraObject.transform.position, cameraObject.transform.forward);
			RaycastHit hit;
		
			if (Physics.Raycast (ray, out hit)) {
			
				if (hit.distance < range) {
					ReplaceBlockAt (hit, block);
				}
				Debug.DrawLine (ray.origin, ray.origin + (ray.direction * hit.distance), Color.green, 2);
			}
		}
	}
	
	public void AddBlockCenter (float range, byte block)
	{
		if (!cameraObject) {
			cameraObject = GameObject.FindGameObjectWithTag ("MainCamera");
		}

		if (cameraObject) {
			//Adds the block specified directly in front of the player
			Ray ray = new Ray (cameraObject.transform.position, cameraObject.transform.forward);
			RaycastHit hit;
		
			if (Physics.Raycast (ray, out hit)) {
			
				if (hit.distance < range) {
					AddBlockAt (hit, block);
				}
				Debug.DrawLine (ray.origin, ray.origin + (ray.direction * hit.distance), Color.green, 2);
			}
		}
	}
	
	public void ReplaceBlockCursor (byte block)
	{
		//Replaces the block specified where the mouse cursor is pointing
		Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
		RaycastHit hit;
		
		if (Physics.Raycast (ray, out hit)) {
			
			ReplaceBlockAt (hit, block);
			Debug.DrawLine (ray.origin, ray.origin + (ray.direction * hit.distance),
			               Color.green, 2);
			
		}
		
	}
	
	public void AddBlockCursor (byte block)
	{
		//Adds the block specified where the mouse cursor is pointing
		Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
		RaycastHit hit;

		if (Physics.Raycast (ray, out hit)) {
			
			AddBlockAt (hit, block);
			Debug.DrawLine (ray.origin, ray.origin + (ray.direction * hit.distance),
			               Color.green, 2);
		}
		
	}
	
	public void ReplaceBlockAt (RaycastHit hit, byte block)
	{
		//removes a block at these impact coordinates, you can raycast against the terrain and call this with the hit.point
		Vector3 position = hit.point;
		position += (hit.normal * -smallestBlockThickness);
		
		SetBlockAt (position, block);
	}
	
	public void AddBlockAt (RaycastHit hit, byte block)
	{
		//adds the specified block at these impact coordinates, you can raycast against the terrain and call this with the hit.point
		Vector3 position = hit.point;
		position += (hit.normal * (1-smallestBlockThickness));
		
		SetBlockAt (position, block);
		
	}
	
	public void SetBlockAt (Vector3 position, byte newBlock)
	{
		//sets the specified block at these coordinates
		
		int x = Mathf.RoundToInt (position.x);
		int y = Mathf.RoundToInt (position.y);
		int z = Mathf.RoundToInt (position.z);

		Block[] blocks = ListBlocks.instance.blocks;

		byte currentBlock = world.Block (x, y, z, ListBlocks.AIR);
		if (!ListBlocks.instance.blocks [currentBlock].indestructable) {
			blocks [currentBlock].OnBreak (world, x, y, z);
			world.SetBlockAt (x, y, z, newBlock);
			blocks [newBlock].OnBuild (world, x, y, z);
		}
	}
}
