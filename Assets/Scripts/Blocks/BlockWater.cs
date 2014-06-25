using UnityEngine;
using System.Collections;

public class BlockWater : Block
{
    public readonly uint height;

    public BlockWater(uint height)
    {
        this.height = height;
        int max = Block.WATER.Length;
        name = System.String.Format("Water ({0}/{1})", height+1, max);
        renderer = new RenderFluid(new TextureLayout(0), Color.white, (height+1f)/max, Block.WATER[height]);
        opaque = false;
    }

    public override void Notify(World world, int x, int y, int z)
    {
        base.Notify(world, x, y, z);
        TryFlood(world, x-1, y, z);
        TryFlood(world, x+1, y, z);
        TryFlood(world, x, y-1, z);
        TryFlood(world, x, y, z-1);
        TryFlood(world, x, y, z+1);
    }

    private void TryFlood(World world, int x, int y, int z)
    {
        if (Block.GetInstance(world.GetBlockAt(x, y, z, Block.STONE[0])).floodable)
        {
            world.SetBlockAt(x, y, z, MyID);
        }
    }
}
