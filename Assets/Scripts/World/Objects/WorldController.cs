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
    public GameObject chunkPrefab;

    // instantiated chunks
    Dictionary<Vector3i, ChunkRenderer> instances= new Dictionary<Vector3i, ChunkRenderer>();

    void Awake ()
    {
        world = new World (saveName, worldType);
        isPlaying = true;
    }

    void OnDestroy ()
    {
        isPlaying = false;
    }

    void Start ()
    {
        player = GameObject.FindGameObjectWithTag ("Player").GetComponent<Transform> ();
        lock (playerPosLock)
            playerPos = player.position;
        Thread t = new Thread (new ThreadStart (LoadChunksAroundPlayer));
        t.Priority = System.Threading.ThreadPriority.Lowest;
        t.Start();
    }

    void FixedUpdate ()
    {
        lock (playerPosLock)
            playerPos = player.position;

        ChunkUnloadTask unload = world.GetNextUnloadChunk ();
        if (unload != null)
            DestroyChunk (unload.pos);
        else {
            ChunkLoadTask load = world.GetNextLoadChunk ();
            if (load != null)
                InstantiateChunk (load.pos, load.meshes);
        }
    }

    void LoadChunksAroundPlayer ()
    {
        try {
            while (isPlaying) {
                Vector2i center;
                lock (playerPosLock)
                    center = MiscMath.WorldToColumnCoords (playerPos.x, playerPos.z);
                Vector2i offset = new Vector2i (2, 2);
                Vector2i min = center - offset;
                Vector2i max = center + offset;
                world.LoadInRange (min, max);
                Thread.Sleep (1000); 
            }
        } catch (System.Exception e) {
            Debug.LogError (e);
        }
    }

    void InstantiateChunk (Vector2i pos, MeshBuildInfo[] meshes)
    {
        float worldX = pos.x * World.CHUNK_SIZE - .5f;
        float worldZ = pos.z * World.CHUNK_SIZE - .5f;

        // for each chunk in the column
        for (int y = 0; y < World.WORLD_HEIGHT; y++) {
            float worldY = y * World.CHUNK_SIZE - .5f;

            // instantiate the chunk
            Vector3 position = new Vector3 (worldX, worldY, worldZ);
            Quaternion rotation = Quaternion.identity;
            GameObject obj = Instantiate (chunkPrefab, position, rotation) as GameObject;
            instances.Add(new Vector3i(pos.x, y, pos.z), obj.GetComponent<ChunkRenderer>());
            obj.transform.parent = transform;
            obj.name = System.String.Format ("Chunk ({0}, {1}, {2})", pos.x, y, pos.z);

            // set mesh for the chunk
            Mesh mesh = obj.GetComponent<MeshFilter> ().mesh;
            mesh.Clear ();
            mesh.vertices = meshes [y].verticesArray;
            mesh.uv = meshes [y].uvArray;
            mesh.triangles = meshes [y].trianglesArray;
            mesh.colors = meshes [y].colorsArray;
            mesh.Optimize ();
            mesh.RecalculateNormals ();
        }
    }

    void DestroyChunk(Vector2i pos)
    {
        for (int y = 0; y < World.WORLD_HEIGHT; y++) {
            Vector3i chunkPos = new Vector3i (pos.x, y, pos.z);
            Destroy(instances[chunkPos].gameObject);
        }
    }
}
