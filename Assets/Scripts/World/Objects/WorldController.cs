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

    // instantiated chunks
    Dictionary<Vector3i, ChunkRenderer> instances = new Dictionary<Vector3i, ChunkRenderer>();

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
        Thread t = new Thread(new ThreadStart(LoadChunksAroundPlayer));
        t.Priority = System.Threading.ThreadPriority.Lowest;
        t.Start();
    }

    void FixedUpdate()
    {
        // update cached player position
        lock (playerPosLock)
            playerPos = player.position;

        // unload far chunks
        ColumnUnloadTask unload;
        do
        {
            unload = world.GetNextUnloadColumn();
            if (unload != null)
                DestroyChunk(unload.pos);
        } while (unload != null);

        // load near chunks
        ColumnLoadTask load;
        do
        {
            load = world.GetNextLoadColumn();
            if (load != null)
                InstantiateChunk(load.pos, load.meshes);
        } while (load != null);

        // render modified chunks
        ChunkRenderTask render;
        do
        {
            render = world.GetNextRenderChunk();
            if (render != null)
                world.PerformRenderTask(render);
        } while (render != null);

        // update rendered chunks
        ChunkUpdateTask update;
        do
        {
            update = world.GetNextUpdateChunk();
            if (update != null && instances.ContainsKey(update.pos))
                UpdateMesh(instances[update.pos], update.mesh);
        } while (update != null);

        // save modified columns
        ColumnSaveTask save;
        do
        {
            save = world.GetNextSaveColumn();
            if (save != null)
                world.Save(save.pos);
        } while (save != null);
    }

    void LoadChunksAroundPlayer()
    {
        try
        {
            while (isPlaying)
            {
                Vector2i center;
                lock (playerPosLock)
                    center = MiscMath.WorldToColumnCoords(playerPos.x, playerPos.z);
                Vector2i offset = new Vector2i(loadDistance, loadDistance);
                Vector2i min = center - offset;
                Vector2i max = center + offset;
                world.LoadInRange(min, max);
                Thread.Sleep(1000);
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
            GameObject obj = Instantiate(chunkPrefab, position, rotation) as GameObject;
            instances.Add(new Vector3i(pos.x, y, pos.z), obj.GetComponent<ChunkRenderer>());
            obj.transform.parent = transform;
            obj.name = System.String.Format("Chunk ({0}, {1}, {2})", pos.x, y, pos.z);
            UpdateMesh(obj.GetComponent<ChunkRenderer>(), meshes[y]);
        }
    }

    void UpdateMesh(ChunkRenderer obj, MeshBuildInfo build)
    {
        Mesh mesh = obj.GetComponent<MeshFilter>().mesh;
        mesh.Clear();
        mesh.vertices = build.verticesArray;
        mesh.uv = build.uvArray;
        mesh.triangles = build.trianglesArray;
        mesh.colors = build.colorsArray;
        mesh.Optimize();
        mesh.RecalculateNormals();
    }

    void DestroyChunk(Vector2i pos)
    {
        for (int y = 0; y < World.WORLD_HEIGHT; y++)
        {
            Vector3i chunkPos = new Vector3i(pos.x, y, pos.z);
            Destroy(instances[chunkPos].gameObject);
            instances.Remove(chunkPos);
        }
    }
}
