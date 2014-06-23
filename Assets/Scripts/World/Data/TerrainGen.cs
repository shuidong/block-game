using UnityEngine;
using System.Collections;
using M = System.Math;

public class TerrainGen
{
    const int WORLD_SCALE = 50;

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
                double hilliness = Hilliness(worldX, worldZ);
                double height = Height(worldX, worldZ);
                int stoneHeight = 100 + (int)(height * hilliness);
                double[] layers = StoneLayers(worldX, worldZ, numStoneLayers);
                int dirtHeight = 100 + Dirt(worldX, worldZ) + (int)(height * hilliness * .9);
                int rockyDirtHeight = stoneHeight >= dirtHeight ? 100 + Dirt(worldX, worldZ) + (int)(height * hilliness * .93) : 0;
                double humidity = col.humidity[x, z];

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
                    else if (y < dirtHeight)
                    {
                        id = !desert ? Block.DIRT : Block.SAND;
                    }
                    else if (y == dirtHeight)
                    {
                        id = !desert ? Block.GRASS : Block.SAND;
                    }
                    else if (y < rockyDirtHeight)
                    {
                        id = Block.ROCKY_DIRT;
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

    private static double Hilliness(int x, int z)
    {
        return M.Pow(M.Abs(4.8 * (PerlinNoise(x, -1000, z, 15 * WORLD_SCALE, 2, 1) - 1)), 3);
    }

    private static double Height(int x, int z)
    {
        int scale = WORLD_SCALE * 2;
        double a = PerlinNoise(x, 0000, z, scale, 1, 1);
        double b = PerlinNoise(x, 1000, z, scale, 1, 1);
        double c = PerlinNoise(x, 2000, z, scale, 1, 1);
        double d = PerlinNoise(x, 3000, z, scale, 1, 1);
        return Max(Min(Combine(x, z, a, b), Combine(z, x, c, d)), Min(Combine(x, z, c, b), Combine(z, x, a, d)));
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
        return 3 + (int)PerlinNoise(x, 4000, z, WORLD_SCALE / 2, 3, 1);
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
    MAINWORLD,
    CAVEWORLD
}
