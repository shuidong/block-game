using UnityEngine;
using System.Collections;

public abstract class Block
{

    #region BLOCK TYPES

    // block ids
    public static readonly ushort AIR = 0;
    public static readonly ushort BEDROCK = 1;
    public static readonly ushort[] STONE = { 2, 3, 4, 5, 6 };
    public static readonly ushort DIRT = 7;
    public static readonly ushort GRASS = 8;
    public static readonly ushort ROCKY_DIRT = 9;
    public static readonly ushort SAND = 10;
    public static readonly ushort[] WATER = { 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 };

    // array of instances of all block types
    private static Block[] blocks;

    /** Instantiate all block types and store them in the array */
    static Block()
    {
        blocks = new Block[ushort.MaxValue + 1];
        blocks[AIR] = new BlockAir();
        blocks[BEDROCK] = new BlockBedrock();
        for (uint i = 0; i < STONE.Length; i++)
            blocks[STONE[i]] = new BlockStone(i);
        blocks[DIRT] = new BlockDirt();
        blocks[GRASS] = new BlockGrass();
        blocks[ROCKY_DIRT] = new BlockDirtStone();
        blocks[SAND] = new BlockSand();
        for (uint i = 0; i < WATER.Length; i++)
            blocks[WATER[i]] = new BlockWater(i);

        for (int i = 0; i < blocks.Length; i++)
        {
            if (blocks[i] != null)
                blocks[i].id = (ushort) i;
        }
    }

    /** Get an instance of Block corresponding to the ID given */
    public static Block GetInstance(ushort id)
    {
        return blocks[id];
    }

    /** This block's unique id */
    private ushort id;
    public ushort MyID
    {
        get
        {
            return id;
        }
    }

    #endregion

    #region BLOCK PARAMETERS

    /** Does this block stop light? */
    public bool opaque = true;

    /** Is this block totally clear? */
    public bool clear = false;

    /** How to render this block */
    public IRenderBlock renderer = null;

    /** The collision bounding box for this block */
    public Bounds collisionBounds = new Bounds(Vector3.zero, Vector3.one);

    /** The human readable display name for this block */
    public string name = "No Name";

    /** Is this block impossible to break by a player? */
    public bool indestructable = false;

    /** Can this block be overridded by a liquid? */
    public bool floodable = false;

    #endregion

    #region BLOCK EVENTS

    /** Called when the block is replaced by another block or air */
    public virtual void OnBreak(World world, int x, int y, int z, ushort newBlock) { }

    /** Called when the block replaces another block or air */
    public virtual void OnPlace(World world, int x, int y, int z, ushort oldBlock) { }

    /** Called at random intervals, roughly once every 5 mins per block */
    public virtual void Tick(World world, int x, int y, int z) { }

    /** Called whenever a local block is changed (including itself) */
    public virtual void Notify(World world, int x, int y, int z) { }

    #endregion
}
