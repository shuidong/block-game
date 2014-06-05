using UnityEngine;
using System.Collections;

class ModifyTerrain
{
    const float smallestBlockThickness = 0.2f;

    /*
     * Helper
     */

    public static Vector3 RoundPosition(Vector3 position)
    {
        int x = Mathf.RoundToInt(position.x);
        int y = Mathf.RoundToInt(position.y);
        int z = Mathf.RoundToInt(position.z);
        return new Vector3(x, y, z);
    }

    public static Vector3 GetHitPositionIn(RaycastHit hit)
    {
        Vector3 position = hit.point;
        position += (hit.normal * -smallestBlockThickness);
        return RoundPosition(position);
    }

    public static Vector3 GetHitPositionOn(RaycastHit hit)
    {
        Vector3 position = hit.point;
        position += (hit.normal * (1 - smallestBlockThickness));
        return RoundPosition(position);
    }

    /*
     * Get blocks
     */

    public static ushort GetBlock(World world, RaycastHit hit)
    {
        return GetBlock(world, GetHitPositionIn(hit));
    }

    public static ushort GetBlock(World world, Vector3 position)
    {
        int x = Mathf.RoundToInt(position.x);
        int y = Mathf.RoundToInt(position.y);
        int z = Mathf.RoundToInt(position.z);
        return world.GetBlockAt(x, y, z, Block.AIR);
    }

    /*
     * Set blocks
     */

    public static void ReplaceBlock(World world, RaycastHit hit, ushort block)
    {
        SetBlock(world, GetHitPositionIn(hit), block);
    }

    public static void AddBlock(World world, RaycastHit hit, ushort block)
    {
        SetBlock(world, GetHitPositionOn(hit), block);
    }

    public static void SetBlock(World world, Vector3 position, ushort newBlock)
    {
        int x = Mathf.RoundToInt(position.x);
        int y = Mathf.RoundToInt(position.y);
        int z = Mathf.RoundToInt(position.z);
        world.SetBlockAt(x, y, z, newBlock);
    }
}