using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Reflection;

public class World
{
    // global world constants
    public const int CHUNK_SIZE = 16;
    public const int WORLD_HEIGHT = 16;

    // basic world info
    private string saveName;
    private string saveDir;
    private TerrainType worldType;

    // currently loaded world
    private Dictionary<Vector2i, Column> loadedData;
    private List<Vector2i> renderedData;

    // thread communication
    private List<ColumnInstantiateTask> instantiateQueue = new List<ColumnInstantiateTask>();
    private List<ColumnDestroyTask> unloadQueue = new List<ColumnDestroyTask>();
    private List<ChunkUpdateTask> updateQueue = new List<ChunkUpdateTask>();
    private List<ChunkRenderTask> renderQueue = new List<ChunkRenderTask>();
    private List<ColumnSaveTask> saveQueue = new List<ColumnSaveTask>();

    // cache recently accessed column
    Column cachedColumn;
    Vector2i cachedPosition;

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

    /** Given a rectangle with corners 'min' and 'max', load 'count' columns within the rectangle, prioritizing the ones closer to the center */
    public void LoadNextColsInRange(Vector2i min, Vector2i max, int count)
    {
        // helper variables
        int xMin = min.x;
        int zMin = min.z;
        int xMax = max.x;
        int zMax = max.z;
        Vector2i center = new Vector2i((xMin + xMax) / 2, (zMin + zMax) / 2);
        List<Vector2i> candidates = new List<Vector2i>();
        List<Vector2i> addition = new List<Vector2i>();
        List<Vector2i> render = new List<Vector2i>();

        // build an ordered list of all the col positions
        for (int x = xMin; x <= xMax; x++)
        {
            for (int z = zMin; z <= zMax; z++)
            {
                candidates.Add(new Vector2i(x, z));
            }
        }
        candidates.Sort(new Vector2i.DistanceComparer(center));

        // pick the next ones to render
        foreach (Vector2i pos in candidates)
        {
            lock (this)
            {
                if (!renderedData.Contains(pos))
                {
                    render.Add(pos);
                    if (render.Count >= count)
                        break;
                }
            }
        }

        // pick the columns we need to load to support the render picks
        foreach (Vector2i pos in render)
        {
            for (int x = pos.x - 1; x <= pos.x + 1; x++)
            {
                for (int z = pos.z - 1; z <= pos.z + 1; z++)
                {
                    Vector2i loadPos = new Vector2i(x, z);
                    lock (this)
                    {
                        if (!loadedData.ContainsKey(loadPos) && !addition.Contains(loadPos))
                        {
                            addition.Add(loadPos);
                        }
                    }
                }
            }
        }

        // load columns
        foreach (Vector2i pos in addition)
        {
            // load from file if available
            Column col = Load(pos);

            // generate terrain
            if (col == null)
                col = TerrainGen.Generate(worldType, pos);

            // add to loaded world
            lock (this)
            {
                loadedData.Add(pos, col);
            }
        }

        // render columns and queue for mesh update
        foreach (Vector2i pos in render)
        {
            // notify main thread
            if (pos.x >= xMin && pos.z >= zMin && pos.x <= xMax && pos.z <= zMax)
            {
                ColumnInstantiateTask loadTask = new ColumnInstantiateTask(pos, RenderColumn(pos));
                lock (instantiateQueue) instantiateQueue.Add(loadTask);
                lock (this) renderedData.Add(pos);
            }
        }
    }

    /** Given a rectangle with corners 'min' and 'max', unload all the chunks not within the rectangle */
    public void UnloadInRange(Vector2i min, Vector2i max)
    {
        // helper variables
        int xMin = min.x;
        int zMin = min.z;
        int xMax = max.x;
        int zMax = max.z;
        List<Vector2i> removal = new List<Vector2i>();

        // mark positions for removal
        foreach (Vector2i pos in loadedData.Keys)
        {
            if (pos.x < xMin || pos.z < zMin || pos.x > xMax || pos.z > zMax)
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
            ColumnDestroyTask task = new ColumnDestroyTask(pos);
            bool contains;
            lock (this) contains = renderedData.Remove(pos);
            if (contains)
            {
                lock (unloadQueue) unloadQueue.Add(task);
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
        // if other coords are out of bounds find another chunk
        if (localX < 0 || localZ < 0 || localX >= CHUNK_SIZE || localZ >= CHUNK_SIZE)
        {
            int worldX = colPos.x * CHUNK_SIZE + localX;
            int worldZ = colPos.z * CHUNK_SIZE + localZ;
            return GetBlockAt(worldX, localY, worldZ, def);
        }

        // if Y is out of bounds return something sensible
        if (localY >= WORLD_HEIGHT * CHUNK_SIZE)
            return Block.AIR;
        if (localY < 0)
            return Block.BEDROCK;

        // return the block id at this column, or default
        lock (this)
        {
            // try to use the cached col
            if (cachedPosition.Equals(colPos) && cachedColumn != null)
            {
                return cachedColumn.blockID[localX, localY, localZ];
            }

            // find the column and cache before returning
            Column col;
            if (loadedData.TryGetValue(colPos, out col))
            {
                // return the block
                cachedPosition = colPos;
                cachedColumn = col;
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
            // try to use the cached col
            if (cachedColumn != null && cachedPosition.Equals(colPos))
            {
                return cachedColumn.lightLevel[localX, localY, localZ];
            }

            // find the column and cache before returning
            Column col;
            if (loadedData.TryGetValue(colPos, out col))
            {
                // return the light
                cachedPosition = colPos;
                cachedColumn = col;
                return col.lightLevel[localX, localY, localZ];
            }
            else
            {
                // not found ):
                return def;
            }
        }
    }

    /** Can this block see the sky? */
    public bool CanSeeSky(int x, int y, int z)
    {
        for (int h = y + 1; h < WORLD_HEIGHT * CHUNK_SIZE; h++)
        {
            if (Block.GetInstance(GetBlockAt(x, h, z, Block.AIR)).opaque)
                return false;
        }
        return true;
    }

    /** Set the block ID at the position (worldX, worldY, worldZ). If the position is not currently loaded, do nothing */
    public void SetBlockAt(int worldX, int worldY, int worldZ, ushort newBlockID)
    {
        Vector2i colPos = MiscMath.WorldToColumnCoords(worldX, worldZ);

        // get info
        ushort oldBlockID = GetBlockAt(worldX, worldY, worldZ, Block.AIR);
        Block oldBlock = Block.GetInstance(oldBlockID);
        Block newBlock = Block.GetInstance(newBlockID);

        // let the old block do whatever it needs to
        oldBlock.OnBreak(this, worldX, worldY, worldZ, newBlockID);

        // set the block
        lock (this)
        {
            Column col;
            if (loadedData.TryGetValue(colPos, out col))
            {
                int localX = MiscMath.Mod(worldX, CHUNK_SIZE);
                int localY = worldY;
                int localZ = MiscMath.Mod(worldZ, CHUNK_SIZE);
                col.blockID[localX, localY, localZ] = newBlockID;
            }
        }

        // let the new block do whatever it needs to
        newBlock.OnPlace(this, worldX, worldY, worldZ, oldBlockID);

        // update lighting if opacity changed
        if (newBlock.opaque != oldBlock.opaque)
        {
            if (newBlock.opaque)
            {
                // remove light spread
                FloodFillDark(worldX, worldY, worldZ);

                // anti sunbeam down if exposed to the sky
                if (CanSeeSky(worldX, worldY, worldZ))
                {
                    // flood dark
                    int h = worldY - 1;
                    do
                    {
                        FloodFillDark(worldX, h, worldZ);
                        h--;
                    } while (h > 0 && !Block.GetInstance(GetBlockAt(worldX, h, worldZ, Block.DIRT)).opaque);
                }
            }
            else
            {
                // sunbeam down if exposed to the sky
                if (CanSeeSky(worldX, worldY, worldZ))
                {
                    int h;

                    // sunbeam
                    h = worldY;
                    do
                    {
                        SetLightAt(worldX, h, worldZ, CubeRenderHelper.MAX_LIGHT);
                        h--;
                    } while (h > 0 && !Block.GetInstance(GetBlockAt(worldX, h, worldZ, Block.DIRT)).opaque);

                    // flood
                    h = worldY;
                    do
                    {
                        FloodFillLight(worldX, h, worldZ, CubeRenderHelper.MAX_LIGHT, true);
                        h--;
                    } while (h > 0 && !Block.GetInstance(GetBlockAt(worldX, h, worldZ, Block.DIRT)).opaque);
                }

                // let light from nearby blocks
                for (int x = worldX - 1; x <= worldX + 1; x++)
                {
                    for (int y = worldY - 1; y <= worldY + 1; y++)
                    {
                        for (int z = worldZ - 1; z <= worldZ + 1; z++)
                        {
                            byte remainingLight = GetLightAt(x, y, z, 0);
                            FloodFillLight(x, y, z, remainingLight, true);
                        }
                    }
                }
            }
        }

        // mark the chunks as modified
        for (int x = worldX - 1; x <= worldX + 1; x++)
        {
            for (int y = worldY - 1; y <= worldY + 1; y++)
            {
                for (int z = worldZ - 1; z <= worldZ + 1; z++)
                {
                    MarkModified(MiscMath.WorldToChunkCoords(x, y, z));
                }
            }
        }
    }

    /** Set the light level at the position (worldX, worldY, worldZ). If the position is not currently loaded, do nothing */
    public void SetLightAt(int worldX, int worldY, int worldZ, byte newLight)
    {
        Vector2i colPos = MiscMath.WorldToColumnCoords(worldX, worldZ);

        // set the light
        lock (this)
        {
            Column col;
            if (loadedData.TryGetValue(colPos, out col))
            {
                int localX = MiscMath.Mod(worldX, CHUNK_SIZE);
                int localY = worldY;
                int localZ = MiscMath.Mod(worldZ, CHUNK_SIZE);
                col.lightLevel[localX, localY, localZ] = newLight;
            }
        }

        // mark the chunks as modified
        for (int x = worldX - 1; x <= worldX + 1; x++)
        {
            for (int y = worldY - 1; y <= worldY + 1; y++)
            {
                for (int z = worldZ - 1; z <= worldZ + 1; z++)
                {
                    MarkModified(MiscMath.WorldToChunkCoords(x, y, z));
                }
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

    /** Mark this chunk as modified and in need of rerender */
    public void MarkModified(Vector3i chunkPos)
    {
        // mark for rerender
        lock (renderQueue)
        {
            foreach (ChunkRenderTask task in renderQueue)
            {
                if (task.pos.Equals(chunkPos))
                    return;
            }
            renderQueue.Add(new ChunkRenderTask(chunkPos));
        }

        // mark for save
        Vector2i colPos = new Vector2i(chunkPos.x, chunkPos.z);
        lock (saveQueue)
        {
            foreach (ColumnSaveTask task in saveQueue)
            {
                if (task.pos.Equals(colPos))
                    return;
            }
            saveQueue.Add(new ColumnSaveTask(colPos));
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

    /** Render a chunk and queue it for update */
    public void PerformRenderTask(ChunkRenderTask task)
    {
        MeshBuildInfo info = RenderChunk(task.pos);
        lock (updateQueue) updateQueue.Add(new ChunkUpdateTask(task.pos, info));
    }

    /** Generate the mesh for a specified column */
    public MeshBuildInfo[] RenderColumn(Vector2i pos)
    {
        MeshBuildInfo[] meshes = new MeshBuildInfo[WORLD_HEIGHT];
        for (int h = 0; h < WORLD_HEIGHT; h++)
        {
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
                meshes[h] = RenderChunk(chunkPos);
            }
        }
        return meshes;
    }

    /** Flood fill light from this block into the world */
    public void FloodFillLight(int x, int y, int z, byte remainingLight, bool source)
    {
        // stop if spread to limit
        if (remainingLight <= 0)
            return;

        // get current status
        ushort block = GetBlockAt(x, y, z, Block.DIRT);
        byte light = GetLightAt(x, y, z, 0);

        // if opaque, stop spreading
        if (Block.GetInstance(block).opaque)
        {
            SetLightAt(x, y, z, 0);
            return;
        }

        // spread here
        if (source || light < remainingLight)
        {
            SetLightAt(x, y, z, remainingLight);
            remainingLight--;

            if (remainingLight > 0)
            {
                FloodFillLight(x + 1, y, z, remainingLight, false);
                FloodFillLight(x - 1, y, z, remainingLight, false);
                FloodFillLight(x, y + 1, z, remainingLight, false);
                FloodFillLight(x, y - 1, z, remainingLight, false);
                FloodFillLight(x, y, z + 1, remainingLight, false);
                FloodFillLight(x, y, z - 1, remainingLight, false);
            }
        }
    }

    /** Undo flood fill light from this block into the world */
    public void FloodFillDark(int x, int y, int z)
    {
        List<Vector3i> endpoints = new List<Vector3i>();
        FloodFillDarkInternal(x, y, z, GetLightAt(x, y, z, 0), true, endpoints);

        foreach (Vector3i pos in endpoints)
        {
            byte light = GetLightAt(pos.x, pos.y, pos.z, 0);
            FloodFillLight(pos.x, pos.y, pos.z, light, true);
        }
    }

    /** Helper function for FloodFillDark */
    private void FloodFillDarkInternal(int x, int y, int z, byte previousLight, bool source, List<Vector3i> endpoints)
    {
        // get current status
        byte light = GetLightAt(x, y, z, CubeRenderHelper.MAX_LIGHT);

        // fill
        if (light > 0)
        {
            if (source || light < previousLight)
            {
                // darken this block
                SetLightAt(x, y, z, 0);

                // spread
                FloodFillDarkInternal(x + 1, y, z, light, false, endpoints);
                FloodFillDarkInternal(x - 1, y, z, light, false, endpoints);
                FloodFillDarkInternal(x, y + 1, z, light, false, endpoints);
                FloodFillDarkInternal(x, y - 1, z, light, false, endpoints);
                FloodFillDarkInternal(x, y, z + 1, light, false, endpoints);
                FloodFillDarkInternal(x, y, z - 1, light, false, endpoints);
            }
            else
            {
                // save endpoint for filling light later
                endpoints.Add(new Vector3i(x, y, z));
            }
        }
    }

    /** Save a column to disk */
    public void Save(Vector2i pos)
    {
        string fileName = GetColumnFile(pos);
        try
        {
            // save to file
            Stream stream = File.Open(fileName, FileMode.OpenOrCreate);
            BinaryFormatter bformatter = new BinaryFormatter();
            bformatter.Binder = new VersionDeserializationBinder();
            bformatter.Serialize(stream, loadedData[pos]);
            stream.Close();
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
            if (File.Exists(fileName))
                File.Delete(fileName);
        }
    }

    /** Load a column from disk */
    public Column Load(Vector2i pos)
    {
        string fileName = GetColumnFile(pos);
        try
        {
            // make sure the file exists
            if (!File.Exists(fileName))
                return null;

            // load the file
            Stream stream = File.Open(fileName, FileMode.Open);
            BinaryFormatter bformatter = new BinaryFormatter();
            bformatter.Binder = new VersionDeserializationBinder();
            Column col = (Column)bformatter.Deserialize(stream);
            stream.Close();
            return col;
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
            return null;
        }
    }

    /** Randomly tick one block in each column */
    public void TickBlocks()
    {
        lock (this)
        {
            foreach (Vector2i pos in renderedData)
            {
                Column col;
                if(loadedData.TryGetValue(pos, out col))
                    col.TickBlock(this, pos);
            }
        }
    }

    /** Get the file name of a position */
    public string GetColumnFile(Vector2i pos)
    {
        return saveDir + "" + pos.x + "_" + pos.z + ".column";
    }

    /** Return the next column that should be added to the scene */
    public ColumnInstantiateTask GetNextInstantiateColumn()
    {
        lock (instantiateQueue)
        {
            if (instantiateQueue.Count > 0)
            {
                ColumnInstantiateTask result = instantiateQueue[0];
                instantiateQueue.RemoveAt(0);
                return result;
            }
            else
            {
                return null;
            }
        }
    }

    /** Return the next column that should be removed from the scene */
    public ColumnDestroyTask GetNextDestroyColumn()
    {
        lock (unloadQueue)
        {
            if (unloadQueue.Count > 0)
            {
                ColumnDestroyTask result = unloadQueue[0];
                unloadQueue.RemoveAt(0);
                return result;
            }
            else
            {
                return null;
            }
        }
    }

    /** Return the next chunk that needs to apply its new mesh*/
    public ChunkUpdateTask GetNextUpdateChunk()
    {
        lock (updateQueue)
        {
            if (updateQueue.Count > 0)
            {
                ChunkUpdateTask result = updateQueue[0];
                updateQueue.RemoveAt(0);
                return result;
            }
            else
            {
                return null;
            }
        }
    }

    /** Return the next chunk that should have a new mesh generated */
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

    /** Return the next chunk that should be saved to disk */
    public ColumnSaveTask GetNextSaveColumn()
    {
        lock (saveQueue)
        {
            if (saveQueue.Count > 0)
            {
                ColumnSaveTask result = saveQueue[0];
                saveQueue.RemoveAt(0);
                return result;
            }
            else
            {
                return null;
            }
        }
    }
}

public sealed class VersionDeserializationBinder : SerializationBinder
{
    public override System.Type BindToType(string assemblyName, string typeName)
    {
        if (!string.IsNullOrEmpty(assemblyName) && !string.IsNullOrEmpty(typeName))
        {
            System.Type typeToDeserialize = null;

            assemblyName = Assembly.GetExecutingAssembly().FullName;

            // The following line of code returns the type. 
            typeToDeserialize = System.Type.GetType(System.String.Format("{0}, {1}", typeName, assemblyName));

            return typeToDeserialize;
        }

        return null;
    }
}
