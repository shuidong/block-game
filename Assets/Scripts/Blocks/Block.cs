using UnityEngine;
using System.Collections;

public abstract class Block
{
    // block ids
    public static readonly ushort AIR = 0;
    public static readonly ushort BEDROCK = 1;
    public static readonly ushort DIRT = 2;
    public static readonly ushort GRASS = 3;
    public static readonly ushort[] STONE = { 4, 5, 6, 7, 8 };

    // array of instances of all block types
    private static Block[] blocks;

    /** Instantiate all block types and store them in the array */
    static Block()
    {
        blocks = new Block[ushort.MaxValue + 1];
        blocks[AIR] = new BlockAir();
        blocks[BEDROCK] = new BlockBedrock();
        blocks[DIRT] = new BlockDirt();
        blocks[GRASS] = new BlockGrass();
        for (uint i = 0; i < STONE.Length; i++)
            blocks[STONE[i]] = new BlockStone(i);
    }

    /** Get an instance of Block corresponding to the ID given */
    public static Block GetInstance(ushort id)
    {
        return blocks[id];
    }

    /*
     * Block Parameters
     */

    /** Does this block stop light? */
    public bool opaque = true;

    /** How to render this block */
    public IRenderBlock renderer = null;

    /** The collision bounding box for this block */
    public Bounds collisionBounds = new Bounds(Vector3.zero, Vector3.one);

    /** The human readable display name for this block */
    public string name = "No Name";

    /** Is this block impossible to break by a player? */
    public bool indestructable = false;

    /*
     * Block Events
     */

    /** Called when the block is replaced by another block or air */
    public virtual void OnBreak(World world, int x, int y, int z, ushort newBlock)
    {
    }

    /** Called when the block replaces another block or air */
    public virtual void OnPlace(World world, int x, int y, int z, ushort oldBlock)
    {
    }
}
