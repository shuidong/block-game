using UnityEngine;
using System.Collections;

public class TerrainGen
{
    /** Generate a column at the specified position of a world of the specified type */
    public static Column Generate (TerrainType type, Vector2i colPos)
    {
        Column col = new Column ();
        int xOffset = colPos.x * World.CHUNK_SIZE;
        int zOffset = colPos.z * World.CHUNK_SIZE;
        int worldHeight = World.CHUNK_SIZE * World.WORLD_HEIGHT;

        // basic stone world for now
        NoiseGen gen = new NoiseGen (30, 1);
        for (int x = 0; x < World.CHUNK_SIZE; x++) {
            for (int z = 0; z < World.CHUNK_SIZE; z++) {
                // generate height for this cell
                int height = (int) gen.GetNoise (xOffset + x, 0, zOffset + z, worldHeight);
                if (height < 10)
                    height = 10;
                if (height > worldHeight - 10)
                    height = worldHeight - 10;

                // build the blocks
                for (int y = 0; y < worldHeight; y++) {
                    if (y < height)
                        col.blockID [x, y, z] = Block.STONE;
                    else 
                        col.blockID [x, y, z] = Block.AIR;
                }
            }
        }

        // return the generated terrain column
        return col;
    }
}

public enum TerrainType
{
    MAINWORLD,
    CAVEWORLD
}
