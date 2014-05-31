using UnityEngine;
using System.Collections;

public class WorldController : MonoBehaviour
{
    public World world;

    // editor params
    public string saveName = "World 1";
    public TerrainType worldType = TerrainType.MAINWORLD;
    public GameObject chunkPrefab;

    void Start ()
    {
        world = new World (saveName, worldType);
        world.ChunkLoadEvent += ChunkLoad;
        world.LoadInRange (new Vector2i (-3, -3), new Vector2i (3, 3));
    }

    void ChunkLoad (Vector2i pos)
    {
        float worldX = pos.x * World.CHUNK_SIZE - .5f;
        float worldZ = pos.z * World.CHUNK_SIZE - .5f;

        for (int y = 0; y < World.WORLD_HEIGHT; y++) {
            float worldY = y * World.CHUNK_SIZE - .5f;

            Vector3 position = new Vector3 (worldX, worldY, worldZ);
            Quaternion rotation = Quaternion.identity;
            GameObject obj = Instantiate (chunkPrefab, position, rotation) as GameObject;
            obj.transform.parent = transform;
            obj.name = System.String.Format("Chunk ({0}, {1}, {2})", pos.x, y, pos.z);
        }
    }
}
