using UnityEngine;
using System.Collections;
using M = System.Math;

public class TerrainGen
{
    const int WORLD_SCALE = 50; // 50
    const double WORLD_HEIGHT_SCALE = 1; // 1
    const int WORLD_SEA_LEVEL = 75;

    /** Generate a column at the specified position of a world of the specified type */
    public static Column Generate(TerrainType type, Vector2i colPos)
    {
        Column col = new Column();
        int xOffset = colPos.x * World.CHUNK_SIZE;
        int zOffset = colPos.z * World.CHUNK_SIZE;

        GenerateClimate(col, xOffset, zOffset);
        BuildTerrain(col, xOffset, zOffset);
        GenerateStructures(col, xOffset, zOffset);
        GenerateLight(col);

        // return the generated terrain column
        return col;
    }

    #region CLIMATE

    private static void GenerateClimate(Column col, int xOffset, int zOffset)
    {
        for (int x = 0; x < World.CHUNK_SIZE; x++)
        {
            for (int z = 0; z < World.CHUNK_SIZE; z++)
            {
                int worldX = x + xOffset;
                int worldZ = z + zOffset;

                col.humidity[x, z] = (float)Humidity(worldX, worldZ);
                col.temperature[x, z] = (float)Temperature(worldX, worldZ);
            }
        }
    }

    private static double Humidity(int x, int z)
    {
        return PerlinNoise(x, -2000, z, 15 * WORLD_SCALE, 1, 1);
    }

    private static double Temperature(int x, int z)
    {
        return PerlinNoise(x, -3000, z, 15 * WORLD_SCALE, 1, 1);
    }

    private static bool IsDesert(int x, int z, double humidity)
    {
        humidity = M.Pow(humidity, 1.25);
        double threshold = .1 + PerlinNoise(x, -3000, z, WORLD_SCALE, .2, 1);
        return humidity < threshold;
    }

    #endregion

    #region TERRAIN
    private static void BuildTerrain(Column col, int xOffset, int zOffset)
    {
        int worldHeight = World.CHUNK_SIZE * World.WORLD_HEIGHT;
        int numStoneLayers = Block.STONE.Length;

        for (int x = 0; x < World.CHUNK_SIZE; x++)
        {
            for (int z = 0; z < World.CHUNK_SIZE; z++)
            {
                int worldX = x + xOffset;
                int worldZ = z + zOffset;

                // terrain values
                double hills = Hills(worldX, worldZ) * HillScale(worldX, worldZ);
                int elevation = Elevation(worldX, worldZ);
                double[] layers = StoneLayers(worldX, worldZ, numStoneLayers);
                int dirt = Dirt(worldX, worldZ);
                double humidity = col.humidity[x, z];

                // terrain heights
                int stoneHeight = (int)(WORLD_HEIGHT_SCALE * (elevation + hills));
                int dirtHeight = (int)(WORLD_HEIGHT_SCALE * (elevation + dirt + hills * .9));
                int rockyDirtHeight = (int)(WORLD_HEIGHT_SCALE * (stoneHeight >= dirtHeight ? elevation + dirt + hills * .93 : 0));
                int seaLevel = (int)(WORLD_HEIGHT_SCALE * WORLD_SEA_LEVEL);

                // build the blocks
                int maxHeight = 0;
                int stoneType = 0;
                bool desert = IsDesert(worldX, worldZ, humidity);
                col.blockID[x, 0, z] = Block.BEDROCK;
                for (int y = 1; y < worldHeight; y++)
                {
                    ushort id = Block.AIR;

                    // pick block
                    if (y < stoneHeight)
                    {
                        if (y > stoneHeight * layers[stoneType])
                            stoneType++;
                        id = Block.STONE[numStoneLayers - 1 - stoneType];
                    }
                    else if (y <= dirtHeight)
                    {
                        if (dirtHeight < seaLevel - 1)
                        {
                            id = Block.STONE[0]; // ocean floor
                        }
                        else if (dirtHeight < seaLevel + 2)
                        {
                            id = Block.SAND; // beaches
                        }
                        else if (desert)
                        {
                            id = Block.SAND; // deserts
                        }
                        else if (y < dirtHeight)
                        {
                            id = Block.DIRT; // dirt under grass
                        }
                        else
                        {
                            id = Block.GRASS; // grass on top of dirt
                        }
                    }
                    else if (y < rockyDirtHeight)
                    {
                        id = Block.ROCKY_DIRT;
                    }
                    else if (y < seaLevel)
                    {
                        id = Block.AIR; // placeholder for water
                    }

                    // place block
                    if (id != Block.AIR)
                    {
                        col.blockID[x, y, z] = id;
                        maxHeight = y;
                    }
                }

                // cache the max height of this column
                if (maxHeight > col.maxHeight)
                    col.maxHeight = maxHeight;
            }
        }
    }

    private static double HillScale(int x, int z)
    {
        return M.Pow(M.Abs(4.8 * (PerlinNoise(x, -1000, z, 15 * WORLD_SCALE, 2, 1) - 1)), 3);
    }

    private static double Hills(int x, int z)
    {
        int scale = WORLD_SCALE * 2;
        double min = .1;
        double a = min + PerlinNoise(x, 0000, z, scale, 1 - min, 1);
        double b = min + PerlinNoise(x, 1000, z, scale, 1 - min, 1);
        double c = min + PerlinNoise(x, 2000, z, scale, 1 - min, 1);
        double d = min + PerlinNoise(x, 3000, z, scale, 1 - min, 1);
        return Max(Min(Combine(x, z, a, b), Combine(z, x, c, d)), Min(Combine(x, z, c, b), Combine(z, x, a, d)));
    }

    private static int Elevation(int x, int z)
    {
        int baseElevation = 90;
        double rollingHills = PerlinNoise(x, -2000, z, 5 * WORLD_SCALE, 20, 1);
        double continents = PerlinNoise(x, -3000, z, 25 * WORLD_SCALE, -75, 1);
        return baseElevation + (int)(rollingHills + continents);
    }

    public static double[] StoneLayers(int x, int z, int count)
    {
        double[] result = new double[count];

        // seed the array
        for (int i = 0; i < result.Length; i++)
        {
            double prev = i == 0 ? 0 : result[i - 1];
            result[i] = prev + 1.0 / count + PerlinNoise(x, -10000 * i, z, WORLD_SCALE, 1.0 / count, 1);
        }

        // normalize
        for (int i = 0; i < result.Length; i++)
        {
            result[i] /= result[count - 1];
        }

        return result;
    }

    public static int Dirt(int x, int z)
    {
        return 3 + (int)PerlinNoise(x, 4000, z, WORLD_SCALE, 3, 1);
    }

    #endregion

    #region STRUCTURES

    private static void GenerateStructures(Column col, int xOffset, int zOffset)
    {
        int worldHeight = World.CHUNK_SIZE * World.WORLD_HEIGHT;

        for (int x = 0; x < World.CHUNK_SIZE; x++)
        {
            for (int z = 0; z < World.CHUNK_SIZE; z++)
            {
                int worldX = x + xOffset;
                int worldZ = z + zOffset;

                for (int y = 0; y < worldHeight; y++)
                {

                }
            }
        }
    }

    #endregion

    #region LIGHT

    private static void GenerateLight(Column col)
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

    #endregion

    #region NOISE

    private static double Combine(int x, int z, double a, double b)
    {
        double weightA = PerlinNoise(x, -20000, z, .35 * WORLD_SCALE, 1, 1);
        double weightB = 1 - weightA;
        return a * weightA + b * weightB;
    }

    private static double Min(double a, double b)
    {
        return M.Min(a, b);
    }

    private static double Max(double a, double b)
    {
        return M.Max(a, b);
    }

    public static double PerlinNoise(int x, int y, int z, double scale, double height, double power)
    {
        double rValue = Noise.GetNoise(((double)x) / scale, ((double)y) / scale, ((double)z) / scale);
        rValue *= height;
        if (power != 0)
            rValue = System.Math.Pow(rValue, power);
        return rValue;
    }

    #endregion
}

public enum TerrainType
{
    MAINWORLD
}
