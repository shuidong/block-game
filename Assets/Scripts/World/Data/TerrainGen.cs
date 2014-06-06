using UnityEngine;
using System.Collections;

public class TerrainGen
{
    /** Generate a column at the specified position of a world of the specified type */
    public static Column Generate(TerrainType type, Vector2i colPos)
    {
        Column col = new Column();
        int xOffset = colPos.x * World.CHUNK_SIZE;
        int zOffset = colPos.z * World.CHUNK_SIZE;
        int worldHeight = World.CHUNK_SIZE * World.WORLD_HEIGHT;

        // basic stone world for now
        for (int x = 0; x < World.CHUNK_SIZE; x++)
        {
            for (int z = 0; z < World.CHUNK_SIZE; z++)
            {
                // generate height values for this cell
                int stoneLayers = Block.STONE.Length;
                int[] stoneHeights = new int[stoneLayers];
                for (int i = 0; i < stoneLayers; i++)
                {
                    int height = worldHeight / (stoneLayers + 1) / 2;
                    stoneHeights[i] = (height * (i + 1)) + PerlinNoise(x + xOffset, height, z + zOffset, 25, 6, 1f);
                }
                int dirtHeight = stoneHeights[stoneLayers - 1] + 3 + PerlinNoise(x + xOffset, 100, z + zOffset, 25, 3, 1f);

                // build the blocks
                for (int y = 0; y < worldHeight; y++)
                {
                    ushort id = Block.AIR;
                    bool found = false;

                    // choose bedrock
                    if (!found && y == 0)
                    {
                        id = Block.BEDROCK;
                        found = true;
                    }

                    // choose stone
                    if (!found)
                    {
                        for (int s = 0; s < stoneLayers; s++)
                        {
                            if (y < stoneHeights[s])
                            {
                                id = Block.STONE[stoneLayers - s - 1];
                                found = true;
                                break;
                            }
                        }
                    }

                    // choose dirt
                    if (!found && y < dirtHeight)
                    {
                        id = Block.DIRT;
                        found = true;
                    }

                    // choose grass
                    if (!found && y == dirtHeight)
                    {
                        id = Block.GRASS;
                        found = true;
                    }

                    col.blockID[x, y, z] = id;
                }

                // cache the max height of this column
                if (dirtHeight > col.maxHeight)
                    col.maxHeight = dirtHeight;
            }
        }

        Sunlight(col);

        // return the generated terrain column
        return col;
    }

    private static void Sunlight(Column col)
    {
        byte maxLight = CubeRenderHelper.MAX_LIGHT;
        int yMax = World.WORLD_HEIGHT * World.CHUNK_SIZE - 1;

        // beam sunlight downwards
        for (int bX = 0; bX < World.CHUNK_SIZE; bX++)
        {
            for (int bZ = 0; bZ < World.CHUNK_SIZE; bZ++)
            {
                for (int y = yMax; y >= 0; y--)
                {
                    if (Block.GetInstance(col.blockID[bX, y, bZ]).opaque)
                    {
                        break;
                    }
                    else
                    {
                        col.lightLevel[bX, y, bZ] = maxLight;
                    }
                }
            }
        }
    }

    public static int PerlinNoise(int x, int y, int z, float scale, float height, float power)
    {
        float rValue = Noise.GetNoise(((double)x) / scale, ((double)y) / scale, ((double)z) / scale);
        rValue *= height;
        if (power != 0)
            rValue = Mathf.Pow(rValue, power);
        return (int)rValue;
    }
}

public enum TerrainType
{
    MAINWORLD,
    CAVEWORLD
}
