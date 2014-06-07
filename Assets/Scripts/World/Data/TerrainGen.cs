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

        //BasicTerrain(col, xOffset, zOffset);
        DensityTerrain(col, xOffset, zOffset);

        Sunlight(col);

        // return the generated terrain column
        return col;
    }

    private static void DensityTerrain(Column col, int xOffset, int zOffset)
    {
        int worldHeight = World.CHUNK_SIZE * World.WORLD_HEIGHT;

        col.maxHeight = worldHeight;

        for (int x = 0; x < World.CHUNK_SIZE; x++)
        {
            for (int z = 0; z < World.CHUNK_SIZE; z++)
            {
                col.blockID[x, 0, z] = Block.BEDROCK;
                for (int y = 1; y < worldHeight; y++)
                {
                    //double density = PerlinNoise(x + xOffset, y, z + zOffset, 50, .4, 1) + .4;
                    double density = PerlinNoise(x + xOffset, y, z + zOffset, 50, 1, 1);
                    double threshold = ((double)y) / worldHeight;
                    if (density > threshold) col.blockID[x, y, z] = Block.STONE[0];
                }
            }
        }
    }

    private static void BasicTerrain(Column col, int xOffset, int zOffset)
    {
        int worldHeight = World.CHUNK_SIZE * World.WORLD_HEIGHT;

        for (int x = 0; x < World.CHUNK_SIZE; x++)
        {
            for (int z = 0; z < World.CHUNK_SIZE; z++)
            {
                // generate height values for this cell
                int stoneLayers = Block.STONE.Length;
                int[] stoneHeights = new int[stoneLayers];
                double scale = 50;
                double mountains = PerlinNoise(x + xOffset, -1000, z + zOffset, 1000, 3, 3) + 1;
                for (int i = 0; i < stoneLayers; i++)
                {
                    int height = worldHeight / (stoneLayers + 1) / 2;
                    stoneHeights[i] = (height * (i + 1)) + PerlinNoiseI(x + xOffset, height, z + zOffset, scale, 6, 1);
                }
                int dirtHeight = stoneHeights[stoneLayers - 1] + 3 + PerlinNoiseI(x + xOffset, 100, z + zOffset, scale, 3 * mountains, 1);

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

    public static int PerlinNoiseI(int x, int y, int z, double scale, double height, double power)
    {
        return (int)PerlinNoise(x, y, z, scale, height, power);
    }

    public static double PerlinNoise(int x, int y, int z, double scale, double height, double power)
    {
        double rValue = Noise.GetNoise(((double)x) / scale, ((double)y) / scale, ((double)z) / scale);
        rValue *= height;
        if (power != 0)
            rValue = System.Math.Pow(rValue, power);
        return rValue;
    }
}

public enum TerrainType
{
    MAINWORLD,
    CAVEWORLD
}
