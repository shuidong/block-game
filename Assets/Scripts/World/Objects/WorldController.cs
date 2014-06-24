using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

public class WorldController : MonoBehaviour
{
    public World world;
    Transform player;
    Vector3 playerPos;
    Object playerPosLock = new Object();
    bool isPlaying;

    // editor params
    public string saveName = "World 1";
    public TerrainType worldType = TerrainType.MAINWORLD;
    public int loadDistance = 8;
    public GameObject chunkPrefab;
    public bool saveChanges = true;
    public Material opaqueMaterial;
    public Material transparentMaterial;

    // instantiated chunks
    Dictionary<Vector3i, ChunkRenderer> opaqueInstances = new Dictionary<Vector3i, ChunkRenderer>();
    Dictionary<Vector3i, ChunkRenderer> transparentInstances = new Dictionary<Vector3i, ChunkRenderer>();
    List<ChunkRenderer> pooledInstances = new List<ChunkRenderer>();

    void Awake()
    {
        world = new World(saveName, worldType);
        isPlaying = true;
    }

    void OnDestroy()
    {
        isPlaying = false;
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        lock (playerPosLock)
            playerPos = player.position;

        // thread to load chunks as the player travels
        new Thread(new ThreadStart(ThreadLoadChunksAroundPlayer)).Start();

        // thread to render chunks
        new Thread(new ThreadStart(ThreadRender)).Start();

        // thread to save chunks
        if(saveChanges) new Thread(new ThreadStart(ThreadSave)).Start();

        // thread for block ticks
        new Thread(new ThreadStart(ThreadBlockTick)).Start();
    }

    void FixedUpdate()
    {
        // update cached player position
        lock (playerPosLock)
            playerPos = player.position;

        // unload far chunks
        ColumnDestroyTask unload;
        do
        {
            unload = world.GetNextDestroyColumn();
            if (unload != null)
                DestroyChunk(unload.pos);
        } while (unload != null);

        // load near chunks
        ColumnInstantiateTask load;
        do
        {
            load = world.GetNextInstantiateColumn();
            if (load != null)
                InstantiateChunk(load.pos, load.meshes);
        } while (load != null);

        // update rendered chunks
        ChunkUpdateTask update;
        do
        {
            update = world.GetNextUpdateChunk();

            if (update != null)
            {
                ChunkRenderer obj;
                if (opaqueInstances.TryGetValue(update.pos, out obj))
                {
                    update.mesh.opaque.ApplyToMesh(obj.GetComponent<MeshFilter>().mesh);
                }
                if (transparentInstances.TryGetValue(update.pos, out obj))
                {
                    update.mesh.transparent.ApplyToMesh(obj.GetComponent<MeshFilter>().mesh);
                }
            }

            if (update != null && opaqueInstances.ContainsKey(update.pos))
            {
                update.mesh.opaque.ApplyToMesh(opaqueInstances[update.pos].GetComponent<MeshFilter>().mesh);
            }
        } while (update != null);
    }

    void ThreadBlockTick()
    {
        try
        {
            while (isPlaying)
            {
                world.TickBlocks();
                Thread.Sleep(9);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }
    }

    void ThreadRender()
    {
        try
        {
            while (isPlaying)
            {
                // render modified chunks
                ChunkRenderTask render;
                do
                {
                    render = world.GetNextRenderChunk();
                    if (render != null)
                        world.PerformRenderTask(render);
                } while (render != null);

                // sleep for a bit
                Thread.Sleep(1);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }
    }

    void ThreadSave()
    {
        try
        {
            while (isPlaying)
            {
                // save modified columns
                ColumnSaveTask save;
                do
                {
                    save = world.GetNextSaveColumn();
                    if (save != null)
                        world.Save(save.pos);
                } while (save != null);

                // sleep for a bit
                Thread.Sleep(1);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }
    }

    private const float UNLOAD_INTERVAL_SECONDS = 1;
    void ThreadLoadChunksAroundPlayer()
    {
        try
        {
            long lastTime = System.DateTime.Now.Ticks;
            while (isPlaying)
            {
                // get range
                Vector2i center;
                lock (playerPosLock)
                    center = MiscMath.WorldToColumnCoords(playerPos.x, playerPos.z);
                Vector2i offset = new Vector2i(loadDistance, loadDistance);
                Vector2i min = center - offset;
                Vector2i max = center + offset;

                // load a chunk
                world.LoadNextColsInRange(min, max, 5);

                // unload chunks every interval
                long currentTime = System.DateTime.Now.Ticks;
                if (currentTime - lastTime >= UNLOAD_INTERVAL_SECONDS * 10000000)
                {
                    world.UnloadInRange(min, max);
                    lastTime = currentTime;
                }

                Thread.Sleep(1);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }
    }

    void InstantiateChunk(Vector2i pos, MeshBuildInfo[] meshes)
    {
        float worldX = pos.x * World.CHUNK_SIZE - .5f;
        float worldZ = pos.z * World.CHUNK_SIZE - .5f;

        // for each chunk in the column
        for (int y = 0; y < World.WORLD_HEIGHT; y++)
        {
            float worldY = y * World.CHUNK_SIZE - .5f;

            // instantiate the chunk
            Vector3 position = new Vector3(worldX, worldY, worldZ);
            Quaternion rotation = Quaternion.identity;

            // instantiate
            string name = System.String.Format(" Chunk at ({0}, {1}, {2})", pos.x, y, pos.z);
            ChunkRenderer opaque = MakeChunk(position, "Opaque" + name, meshes[y].opaque, opaqueMaterial);
            ChunkRenderer transparent = MakeChunk(position, "Transparent" + name, meshes[y].transparent, transparentMaterial);
            Vector3i chunkPos = new Vector3i(pos.x, y, pos.z);
            opaqueInstances.Add(chunkPos, opaque);
            transparentInstances.Add(chunkPos, transparent);
        }
    }

    ChunkRenderer MakeChunk(Vector3 scenePosition, string name, SingleMeshBuildInfo meshInfo, Material material)
    {
        GameObject obj;
        ChunkRenderer comp;
        MeshFilter filter;
        
        // create or reuse gameobject
        if (pooledInstances.Count > 0)
        {
            comp = pooledInstances[0];
            obj = comp.gameObject;
            obj.transform.position = scenePosition;
            pooledInstances.RemoveAt(0);
            obj.SetActive(true);
        }
        else
        {
            obj = Instantiate(chunkPrefab, scenePosition, Quaternion.identity) as GameObject;
            obj.transform.parent = transform;
            comp = obj.GetComponent<ChunkRenderer>();
        }
        filter = obj.GetComponent<MeshFilter>();

        // apply new settings
        obj.name = name;
        meshInfo.ApplyToMesh(filter.mesh);
        obj.renderer.material = material;

        return comp;
    }

    void DestroyChunk(Vector2i pos)
    {
        for (int y = 0; y < World.WORLD_HEIGHT; y++)
        {
            Vector3i chunkPos = new Vector3i(pos.x, y, pos.z);

            opaqueInstances[chunkPos].gameObject.SetActive(false);
            pooledInstances.Add(opaqueInstances[chunkPos]);
            opaqueInstances.Remove(chunkPos);
            
            transparentInstances[chunkPos].gameObject.SetActive(false);
            pooledInstances.Add(transparentInstances[chunkPos]);
            transparentInstances.Remove(chunkPos);
        }
    }
}
