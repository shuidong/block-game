using UnityEngine;

public class BlockGrass : Block
{
    private System.Random random;

    public BlockGrass()
    {
        name = "Grass";
        renderer = new RenderFullBlock(new TextureLayout(2, 1, 3), Color.white);
        random = new System.Random();
    }

    public override void Tick(World world, int x, int y, int z)
    {
        base.Tick(world, x, y, z);

        // grass growth and death
        if (!Block.GetInstance(world.GetBlockAt(x, y + 1, z, Block.AIR)).opaque)
        {
            // pick a direction
            int xOffset = x + random.Next(3) - 1;
            int zOffset = z + random.Next(3) - 1;

            for (int yOffset = y - 1; yOffset <= y + 1; yOffset++)
            {
                // decide if should grow
                if (world.GetBlockAt(xOffset, yOffset, zOffset, Block.AIR) == Block.DIRT && !Block.GetInstance(world.GetBlockAt(xOffset, yOffset + 1, zOffset, Block.AIR)).opaque)
                {
                    // grow
                    world.SetBlockAt(xOffset, yOffset, zOffset, Block.GRASS);
                    break;
                }
            }
        }
        else
        {
            // die
            world.SetBlockAt(x, y, z, Block.DIRT);
            return;
        }
    }
}
