using UnityEngine;
using System.Collections;

public abstract class Block
{
    // block ids
    public const ushort AIR = 0;
    public const ushort DIRT = 1;
    public const ushort GRASS = 2;
    public const ushort STONE = 3;
    public const ushort BEDROCK = 4;

    // array of instances of all block types
    private static Block[] blocks;

    /** Instantiate all block types and store them in the array */
    static Block()
    {
        blocks = new Block[ushort.MaxValue + 1];
        blocks[AIR] = new BlockAir();
        blocks[DIRT] = new BlockDirt();
        blocks[GRASS] = new BlockGrass();
        blocks[STONE] = new BlockStone();
        blocks[BEDROCK] = new BlockBedrock();
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
