using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;

public class World
{
    // global world constants
    public const int CHUNK_SIZE = 15;
    public const int WORLD_HEIGHT = 10;

    // basic world info
    private string saveName;
    private string saveDir;
    private TerrainType worldType;

    // currently loaded world
    private Dictionary<Vector2i, Column> loadedData;
    private List<Vector2i> renderedData;

    // thread communication
    private List<ColumnLoadTask> loadQueue = new List<ColumnLoadTask>();
    private List<ColumnUnloadTask> unloadQueue = new List<ColumnUnloadTask>();
    private List<ChunkRenderTask> renderQueue = new List<ChunkRenderTask>();

    /** Initialize a world with a 'name' and a 'worldType'. The 'worldType' is used to determine the kind of terrain generation in the world */
    public World(string saveName, TerrainType worldType)
    {
        // init fields
        this.saveName = saveName;
        this.worldType = worldType;
        this.saveDir = Application.persistentDataPath + "/saves/" + this.saveName + "/" + this.worldType + "/";

        // create save dir
        Debug.Log("Creating new " + this.worldType + ":\n\t-> " + saveDir);
        Directory.CreateDirectory(saveDir);

        // create data structs
        loadedData = new Dictionary<Vector2i, Column>();
        renderedData = new List<Vector2i>();
    }

    /** Given a rectangle with corners 'min' and 'max', load all the chunks within the rectangle and unload all the chunks not within the rectangle */
    public void LoadInRange(Vector2i min, Vector2i max)
    {
        // helper variables
        int xMin = min.x;
        int zMin = min.z;
        int xMax = max.x;
        int zMax = max.z;
        List<Vector2i> removal = new List<Vector2i>();
        List<Vector2i> addition = new List<Vector2i>();
        List<Vector2i> render = new List<Vector2i>();

        // mark positions for removal
        foreach (Vector2i pos in loadedData.Keys)
        {
            if (pos.x < xMin - 1 || pos.z < zMin - 1 || pos.x > xMax + 1 || pos.z > zMax + 1)
            {
                removal.Add(pos);
            }
        }

        // remove columns outside of range
        foreach (Vector2i pos in removal)
        {
            lock (this)
            {
                loadedData.Remove(pos);
            }
        }

        // call removal events
        foreach (Vector2i pos in removal)
        {
            ColumnUnloadTask task = new ColumnUnloadTask(pos);
            bool contains;
            lock (this) contains = renderedData.Remove(pos);
            if (contains)
            {
                lock (unloadQueue) unloadQueue.Add(task);
            }
        }

        // mark positions for addition
        for (int x = xMin - 1; x <= xMax + 1; x++)
        {
            for (int z = zMin - 1; z <= zMax + 1; z++)
            {
                Vector2i pos = new Vector2i(x, z);
                lock (this)
                {
                    if (!loadedData.ContainsKey(pos))
                    {
                        addition.Add(pos);
                    }
                }
            }
        }

        // add columns within range
        foreach (Vector2i pos in addition)
        {
            lock (this)
            {
                    // TODO load from file if available

                    // generate terrain
                    Column col = TerrainGen.Generate(worldType, pos);

                    // add to loaded world
                    loadedData.Add(pos, col);
            }
        }

        // mark positions for render
        for (int x = xMin; x <= xMax; x++)
        {
            for (int z = zMin; z <= zMax; z++)
            {
                Vector2i pos = new Vector2i(x, z);
                lock (this)
                {
                    if (!renderedData.Contains(pos))
                    {
                        render.Add(pos);
                    }
                }
            }
        }

        foreach (Vector2i pos in render) {
            // notify main thread
            if (pos.x >= xMin && pos.z >= zMin && pos.x <= xMax && pos.z <= zMax)
            {
                ColumnLoadTask loadTask = new ColumnLoadTask(pos, RenderColumn(pos));
                lock (loadQueue) loadQueue.Add(loadTask);
                lock (this) renderedData.Add(pos);
            }
        }
    }

    /** Return the block ID at the position (worldX, worldY, worldZ). If the position is not currently loaded, return def */
    public ushort GetBlockAt(int worldX, int worldY, int worldZ, ushort def)
    {
        Vector2i colPos = MiscMath.WorldToColumnCoords(worldX, worldZ);
        int localX = MiscMath.Mod(worldX, CHUNK_SIZE);
        int localY = worldY;
        int localZ = MiscMath.Mod(worldZ, CHUNK_SIZE);
        return GetBlockAt(colPos, localX, localY, localZ, def);
    }

    /** Return the block ID at the position (localX, localY, localZ) in the chunk at chunkPos. If the position is not currently loaded, return def */
    public ushort GetBlockAt(Vector3i chunkPos, int localX, int localY, int localZ, ushort def)
    {
        Vector2i colPos = new Vector2i(chunkPos.x, chunkPos.z);
        localY += chunkPos.y * CHUNK_SIZE;
        return GetBlockAt(colPos, localX, localY, localZ, def);
    }

    /** Return the block ID at the position (localX, localY, localZ) in the column at colPos. If the position is not currently loaded, return def */
    public ushort GetBlockAt(Vector2i colPos, int localX, int localY, int localZ, ushort def)
    {
        // if Y is out of bounds return the default
        if (localY < 0 || localY >= WORLD_HEIGHT * CHUNK_SIZE)
            return def;

        // if other coords are out of bounds find another chunk
        if (localX < 0 || localZ < 0 || localX >= CHUNK_SIZE || localZ >= CHUNK_SIZE)
        {
            int worldX = colPos.x * CHUNK_SIZE + localX;
            int worldZ = colPos.z * CHUNK_SIZE + localZ;
            return GetBlockAt(worldX, localY, worldZ, def);
        }

        // return the block id at this column, or default
        lock (this)
        {
            Column col;
            if (loadedData.TryGetValue(colPos, out col))
            {
                // return the block
                return col.blockID[localX, localY, localZ];
            }
            else
            {
                // not found ):
                return def;
            }
        }
    }

    /** Return the light level at the position (worldX, worldY, worldZ). If the position is not currently loaded, return def */
    public byte GetLightAt(int worldX, int worldY, int worldZ, byte def)
    {
        Vector2i colPos = MiscMath.WorldToColumnCoords(worldX, worldZ);
        int localX = MiscMath.Mod(worldX, CHUNK_SIZE);
        int localY = worldY;
        int localZ = MiscMath.Mod(worldZ, CHUNK_SIZE);
        return GetLightAt(colPos, localX, localY, localZ, def);
    }

    /** Return the light level at the position (localX, localY, localZ) in the chunk at chunkPos. If the position is not currently loaded, return def */
    public byte GetLightAt(Vector3i chunkPos, int localX, int localY, int localZ, byte def)
    {
        Vector2i colPos = new Vector2i(chunkPos.x, chunkPos.z);
        localY += chunkPos.y * CHUNK_SIZE;
        return GetLightAt(colPos, localX, localY, localZ, def);
    }

    /** Return the light level at the position (localX, localY, localZ) in the column at colPos. If the position is not currently loaded, return def */
    public byte GetLightAt(Vector2i colPos, int localX, int localY, int localZ, byte def)
    {
        // if Y is out of bounds return the default
        if (localY < 0 || localY >= WORLD_HEIGHT * CHUNK_SIZE)
            return def;

        // if other coords are out of bounds find another chunk
        if (localX < 0 || localZ < 0 || localX >= CHUNK_SIZE || localZ >= CHUNK_SIZE)
        {
            int worldX = colPos.x * CHUNK_SIZE + localX;
            int worldZ = colPos.z * CHUNK_SIZE + localZ;
            return GetLightAt(worldX, localY, worldZ, def);
        }

        // return the light level at this column, or default
        lock (this)
        {
            Column col;
            if (loadedData.TryGetValue(colPos, out col))
            {
                // return the block
                return col.lightLevel[localX, localY, localZ];
            }
            else
            {
                // not found ):
                return def;
            }
        }
    }

    /** Set the block ID at the position (worldX, worldY, worldZ). If the position is not currently loaded, do nothing */
    public void SetBlockAt(int worldX, int worldY, int worldZ, ushort newBlock)
    {
        lock (this)
        {
            Vector2i colPos = MiscMath.WorldToColumnCoords(worldX, worldZ);
            Column col;
            if (loadedData.TryGetValue(colPos, out col))
            {
                int localX = MiscMath.Mod(worldX, CHUNK_SIZE);
                int localY = worldY;
                int localZ = MiscMath.Mod(worldZ, CHUNK_SIZE);
                ushort oldBlock = col.blockID[localX, localY, localZ];

                // let the old block do whatever it needs to
                Block.GetInstance(oldBlock).OnBreak(this, worldX, worldY, worldZ, newBlock);

                // set the block
                col.blockID[localX, localY, localZ] = newBlock;

                // TODO update lighting

                // let the new block do whatever it needs to
                Block.GetInstance(newBlock).OnPlace(this, worldX, worldY, worldZ, oldBlock);
            }
        }
    }

    /** Get the maximum height of the specified column */
    public int GetMaxHeightAt(Vector2i pos)
    {
        lock (this)
        {
            return loadedData[pos].maxHeight;
        }
    }

    /** Generate the mesh for a specified chunk */
    public MeshBuildInfo RenderChunk(Vector3i pos)
    {
        MeshBuildInfo mesh = new MeshBuildInfo();
        ushort block;
        IRenderBlock renderer;
        for (int x = 0; x < CHUNK_SIZE; x++)
        {
            for (int y = 0; y < CHUNK_SIZE; y++)
            {
                for (int z = 0; z < CHUNK_SIZE; z++)
                {
                    block = GetBlockAt(pos, x, y, z, 0);
                    renderer = Block.GetInstance(block).renderer;
                    if (renderer != null)
                        renderer.Render(mesh, this, pos, x, y, z);
                }
            }
        }
        mesh.Build();
        return mesh;
    }

    /** Generate the mesh for a specified column */
    public MeshBuildInfo[] RenderColumn(Vector2i pos)
    {
        MeshBuildInfo[] meshes = new MeshBuildInfo[WORLD_HEIGHT];
        for (int h = 0; h < WORLD_HEIGHT; h++)
        {
            if (h % WORLD_HEIGHT / 4 == 0)
                Thread.Sleep(1);
            Vector3i chunkPos = new Vector3i(pos.x, h, pos.z);

            if (h * CHUNK_SIZE > GetMaxHeightAt(pos))
            {
                // don't render if we're past the max height
                meshes[h] = new MeshBuildInfo();
                meshes[h].Build();
            }
            else
            {
                // render this chunk
                lock (this)
                    meshes[h] = RenderChunk(chunkPos);
            }
        }
        return meshes;
    }

    /** Return the next column that should be loaded */
    public ColumnLoadTask GetNextLoadColumn()
    {
        lock (loadQueue)
        {
            if (loadQueue.Count > 0)
            {
                ColumnLoadTask result = loadQueue[0];
                loadQueue.RemoveAt(0);
                return result;
            }
            else
            {
                return null;
            }
        }
    }

    /** Return the next column that should be unloaded */
    public ColumnUnloadTask GetNextUnloadColumn()
    {
        lock (unloadQueue)
        {
            if (unloadQueue.Count > 0)
            {
                ColumnUnloadTask result = unloadQueue[0];
                unloadQueue.RemoveAt(0);
                return result;
            }
            else
            {
                return null;
            }
        }
    }

    /** Return the next column that should be rendered */
    public ChunkRenderTask GetNextRenderChunk()
    {
        lock (renderQueue)
        {
            if (renderQueue.Count > 0)
            {
                ChunkRenderTask result = renderQueue[0];
                renderQueue.RemoveAt(0);
                return result;
            }
            else
            {
                return null;
            }
        }
    }
}

public class ColumnLoadTask
{
    public Vector2i pos;
    public MeshBuildInfo[] meshes;

    public ColumnLoadTask(Vector2i pos, MeshBuildInfo[] meshes)
    {
        this.pos = pos;
        this.meshes = meshes;
    }
}

public class ColumnUnloadTask
{
    public Vector2i pos;

    public ColumnUnloadTask(Vector2i pos)
    {
        this.pos = pos;
    }
}

public class ChunkRenderTask
{
    public Vector3i pos;
    public MeshBuildInfo mesh;

    public ChunkRenderTask(Vector3i pos, MeshBuildInfo mesh)
    {
        this.pos = pos;
        this.mesh = mesh;
    }
}