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

	// GET BLOCKS

	public BlockMeta GetBlockCenter (float range) {
		BlockMeta result = new BlockMeta (ListBlocks.AIR, 0);

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
			}
		}

		return result;
	}

	public BlockMeta GetBlockAt (RaycastHit hit)
	{
		Vector3 position = hit.point;
		position += (hit.normal * -smallestBlockThickness);
		
		return GetBlockAt (position);
	}

	public BlockMeta GetBlockAt (Vector3 position)
	{
		int x = Mathf.RoundToInt (position.x);
		int y = Mathf.RoundToInt (position.y);
		int z = Mathf.RoundToInt (position.z);

		return world.Block (x, y, z, ListBlocks.AIR);
	}

	// SET BLOCKS

	public void ReplaceBlockCenter (float range, byte block, byte meta)
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
					ReplaceBlockAt (hit, block, meta);
				}
			}
		}
	}
	
	public void AddBlockCenter (float range, byte block, byte meta)
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
					AddBlockAt (hit, block, meta);
				}
			}
		}
	}
	
	public void ReplaceBlockCursor (byte block, byte meta)
	{
		//Replaces the block specified where the mouse cursor is pointing
		Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
		RaycastHit hit;
		
		if (Physics.Raycast (ray, out hit)) {
			
			ReplaceBlockAt (hit, block, meta);
			
		}
		
	}
	
	public void AddBlockCursor (byte block, byte meta)
	{
		//Adds the block specified where the mouse cursor is pointing
		Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
		RaycastHit hit;

		if (Physics.Raycast (ray, out hit)) {
			
			AddBlockAt (hit, block, meta);
		}
		
	}
	
	public void ReplaceBlockAt (RaycastHit hit, byte block, byte meta)
	{
		//removes a block at these impact coordinates, you can raycast against the terrain and call this with the hit.point
		Vector3 position = hit.point;
		position += (hit.normal * -smallestBlockThickness);
		
		SetBlockAt (position, block, meta);
	}
	
	public void AddBlockAt (RaycastHit hit, byte block, byte meta)
	{
		//adds the specified block at these impact coordinates, you can raycast against the terrain and call this with the hit.point
		Vector3 position = hit.point;
		position += (hit.normal * (1-smallestBlockThickness));
		
		SetBlockAt (position, block, meta);
		
	}
	
	public void SetBlockAt (Vector3 position, byte newBlock, byte meta)
	{
		//sets the specified block at these coordinates
		
		int x = Mathf.RoundToInt (position.x);
		int y = Mathf.RoundToInt (position.y);
		int z = Mathf.RoundToInt (position.z);

		Block[] blocks = ListBlocks.instance.blocks;

		BlockMeta currentBlock = world.Block (x, y, z, ListBlocks.AIR);
		if (!ListBlocks.instance.blocks [currentBlock.block].indestructable) {
			blocks [currentBlock.block].OnBreak (world, x, y, z, currentBlock.meta);
			world.SetBlockAt (x, y, z, newBlock, meta);
			blocks [newBlock].OnBuild (world, x, y, z, meta);
		}
	}
}
