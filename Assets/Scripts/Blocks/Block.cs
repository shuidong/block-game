using UnityEngine;
using System.Collections;

public class Block
{
    // block ids
    public const short AIR = 0;
    public const short STONE = 1;

    // array of instances of all block types
    private static Block[] blocks;

    /** Instantiate all block types and store them in the array */
    static Block ()
    {
        blocks = new Block[short.MaxValue + 1];
        blocks [AIR] = new BlockAir ();
        blocks [STONE] = new BlockStone ();
    }

    // block parameters
    public virtual bool Opaque {
        get {
            return true;
        }
    }
}
