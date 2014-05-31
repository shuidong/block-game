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
        for (int x = 0; x < World.CHUNK_SIZE; x++) {
            for (int z = 0; z < World.CHUNK_SIZE; z++) {
                // generate height for this cell
                int stoneHeight = worldHeight / 2 - 3 + PerlinNoise (x + xOffset, 0, z + zOffset, 25, 6, 1f);
                int dirtHeight = stoneHeight + 4 + PerlinNoise (x + xOffset, 100, z + zOffset, 25, 3, 1f);

                // build the blocks
                for (int y = 0; y < worldHeight; y++) {
                    col.lightLevel [x, y, z] = CubeRenderHelper.MAX_LIGHT;
                    if (y < stoneHeight)
                        col.blockID [x, y, z] = Block.STONE;
                    else if (y < dirtHeight)
                        col.blockID [x, y, z] = Block.DIRT;
                    else 
                        col.blockID [x, y, z] = Block.AIR;
                }
            }
        }

        // return the generated terrain column
        return col;
    }

    public static int PerlinNoise (int x, int y, int z, float scale, float height, float power)
    {
        float rValue = Noise.GetNoise (((double)x) / scale, ((double)y) / scale, ((double)z) / scale);
        rValue *= height;
        if (power != 0)
            rValue = Mathf.Pow (rValue, power);
        return (int)rValue;
    }
}

public enum TerrainType
{
    MAINWORLD,
    CAVEWORLD
}
