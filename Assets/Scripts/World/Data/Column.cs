using UnityEngine;
using System.Collections;
using System.Runtime.Serialization;

[System.Serializable]
public class Column : ISerializable
{
    private System.Random random;

    // column data
    public ushort[, ,] blockID = new ushort[World.CHUNK_SIZE, World.CHUNK_SIZE * World.WORLD_HEIGHT, World.CHUNK_SIZE];
    public byte[, ,] lightLevel = new byte[World.CHUNK_SIZE, World.CHUNK_SIZE * World.WORLD_HEIGHT, World.CHUNK_SIZE];
    public float[,] humidity = new float[World.CHUNK_SIZE, World.CHUNK_SIZE];
    public float[,] temperature = new float[World.CHUNK_SIZE, World.CHUNK_SIZE];

    // cached stuff for optimization
    public int maxHeight;

    // serialization keys
    private const string KEY_BLOCK_IDS = "BlockIDArray";
    private const string KEY_LIGHT_LEVELS = "LightLevelArray";
    private const string KEY_CACHE_MAX_HEIGHT = "CachedMaximumHeight";
    private const string KEY_HUMIDITY = "HumidityArray";
    private const string KEY_TEMPERATURE = "TemperatureArray";

    /** Crate an empty column */
    public Column()
    {
        random = new System.Random();
    }

    /** Deserialize this column */
    public Column(SerializationInfo info, StreamingContext ctxt)
    {
        random = new System.Random();
        string version = (string)info.GetValue(Constants.KEY_SAVE_VERSION, typeof(string));

        if (version == Constants.SAVE_VERSION_ORIG)
        {
            blockID = (ushort[, ,])info.GetValue(KEY_BLOCK_IDS, typeof(ushort[, ,]));
            lightLevel = (byte[, ,])info.GetValue(KEY_LIGHT_LEVELS, typeof(byte[, ,]));
            maxHeight = (int)info.GetValue(KEY_CACHE_MAX_HEIGHT, typeof(int));
            humidity = (float[,])info.GetValue(KEY_HUMIDITY, typeof(float[,]));
            temperature = (float[,])info.GetValue(KEY_TEMPERATURE, typeof(float[,]));
        }
        else
        {
            Debug.LogError("ERROR LOADING CHUNK FROM DISK\nSAVED VERSION IS: " + version + "\nEXPECTED VERSION IS: " + Constants.SAVE_VERSION_ORIG);
        }
    }

    /** Serialize this column */
    public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
    {
        info.AddValue(Constants.KEY_SAVE_VERSION, Constants.SAVE_VERSION_ORIG);
        info.AddValue(KEY_BLOCK_IDS, blockID);
        info.AddValue(KEY_LIGHT_LEVELS, lightLevel);
        info.AddValue(KEY_CACHE_MAX_HEIGHT, maxHeight);
        info.AddValue(KEY_HUMIDITY, humidity);
        info.AddValue(KEY_TEMPERATURE, temperature);
    }

    /** Randomly tick one block in this column */
    public void TickBlock(World world, Vector2i pos)
    {
        int x = random.Next(World.CHUNK_SIZE);
        int y = random.Next(World.CHUNK_SIZE * World.WORLD_HEIGHT);
        int z = random.Next(World.CHUNK_SIZE);
        ushort block;
        lock (this) block = blockID[x, y, z];
        Block.GetInstance(block).Tick(world, pos.x * World.CHUNK_SIZE + x, y, pos.z * World.CHUNK_SIZE + z);
    }
}
