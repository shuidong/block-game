using UnityEngine;
using System.Collections;

public abstract class Block
{
    // block ids
    public const ushort AIR = 0;
    public const ushort STONE = 1;
    public const ushort DIRT = 3;
    public const ushort GRASS = 4;

    // array of instances of all block types
    private static Block[] blocks;

    /** Instantiate all block types and store them in the array */
    static Block ()
    {
        blocks = new Block[ushort.MaxValue + 1];
        blocks [AIR] = new BlockAir ();
        blocks [STONE] = new BlockStone ();
        blocks [DIRT] = new BlockDirt ();
        blocks [GRASS] = new BlockGrass ();
    }

    /** Get an instance of Block corresponding to the ID given */
    public static Block GetInstance (ushort id)
    {
        return blocks [id];
    }

    /*
     * Block Parameters
     */

    /** Does this block stop light? */
    public virtual bool Opaque {
        get {
            return true;
        }
    }

    /** How to render this block */
    public abstract IRenderBlock Renderer {
        get;
    }

    /*
     * Block Events
     */

    /** Called when the block is replaced by another block or air */
    public virtual void OnBreak (World world, int x, int y, int z, ushort newBlock)
    {
    }

    /** Called when the block replaces another block or air */
    public virtual void OnPlace (World world, int x, int y, int z, ushort oldBlock)
    {
    }
}
