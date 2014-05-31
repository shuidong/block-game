using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class World
{
    // global world constants
    public const int CHUNK_SIZE = 12;
    public const int WORLD_HEIGHT = 12;

    // basic world info
    private string saveName;
    private string saveDir;
    private TerrainType worldType;

    // currently loaded world
    private Dictionary<Vector2i, Column> loadedData;

    // events
    public delegate void ChunkLoadHandler(Vector2i pos);
    public delegate void ChunkUnloadHandler(Vector2i pos);
    public event ChunkLoadHandler ChunkLoadEvent;
    public event ChunkLoadHandler ChunkUnloadEvent;

    /** Initialize a world with a 'name' and a 'worldType'. The 'worldType' is used to determine the kind of terrain generation in the world */
    public World (string saveName, TerrainType worldType)
    {
        // init fields
        this.saveName = saveName;
        this.worldType = worldType;
        this.saveDir = Application.persistentDataPath + "/saves/" + this.saveName + "/" + this.worldType + "/";

        // create save dir
        Debug.Log ("Creating new " + this.worldType + ":\n\t-> " + saveDir);
        Directory.CreateDirectory (saveDir);

        // create data struct
        loadedData = new Dictionary<Vector2i, Column> ();
    }

    /** Given a rectangle with corners 'min' and 'max', load all the chunks within the rectangle and unload all the chunks not within the rectangle */
    public void LoadInRange (Vector2i min, Vector2i max)
    {
        lock (this) {
            // helper variables
            int xMin = min.x;
            int zMin = min.z;
            int xMax = max.x;
            int zMax = max.z;
            List<Vector2i> removal = new List<Vector2i> ();
            List<Vector2i> addition = new List<Vector2i> ();
            
            // mark positions for removal
            foreach (Vector2i pos in loadedData.Keys) {
                if (pos.x < xMin || pos.z < zMin || pos.x > xMax || pos.z > zMax) {
                    removal.Add (pos);
                }
            }
            
            // remove columns outside of range
            foreach (Vector2i pos in removal) {
                loadedData.Remove (pos);
                ChunkUnloadEvent(pos);
            }
            
            // mark positions for addition
            for (int x = xMin; x <= xMax; x++) {
                for (int z = zMin; z <= zMax; z++) {
                    Vector2i pos = new Vector2i (x, z);
                    if (!loadedData.ContainsKey (pos)) {
                        addition.Add (pos);
                    }
                }
            }
            
            // add columns within range
            foreach (Vector2i pos in addition) {
                // TODO load from file if available
                Column col = TerrainGen.Generate (worldType, pos);
                // TODO update lighting
                loadedData.Add (pos, col);
                ChunkLoadEvent(pos);
            }
        }
    }

    /** Return the block ID at the position (worldX, worldY, worldZ). If the position is not currently loaded, return def */
    public ushort GetBlockAt (int worldX, int worldY, int worldZ, ushort def)
    {
        Vector2i colPos = MiscMath.WorldToColumnCoords (worldX, worldZ);
        int localX = MiscMath.Mod (worldX, CHUNK_SIZE);
        int localY = worldY;
        int localZ = MiscMath.Mod (worldZ, CHUNK_SIZE);
        return GetBlockAt(colPos, localX, localY, localZ, def);
    }

    /** Return the block ID at the position (localX, localY, localZ) in the column at colPos. If the position is not currently loaded, return def */
    public ushort GetBlockAt (Vector2i colPos, int localX, int localY, int localZ, ushort def) {
        lock (this) {
            Column col;
            if (loadedData.TryGetValue (colPos, out col)) {
                // return the block
                return col.blockID [localX, localY, localZ];
            } else {
                // not found ):
                return def;
            }
        }
    }

    /** Return the block ID at the position (worldX, worldY, worldZ). If the position is not currently loaded, return def */
    public byte GetBlockAt (int worldX, int worldY, int worldZ, byte def)
    {
        Vector2i colPos = MiscMath.WorldToColumnCoords (worldX, worldZ);
        int localX = MiscMath.Mod (worldX, CHUNK_SIZE);
        int localY = worldY;
        int localZ = MiscMath.Mod (worldZ, CHUNK_SIZE);
        return GetLightAt(colPos, localX, localY, localZ, def);
    }

    /** Return the light level at the position (localX, localY, localZ) in the column at colPos. If the position is not currently loaded, return def */
    public byte GetLightAt (Vector2i colPos, int localX, int localY, int localZ, byte def) {
        lock (this) {
            Column col;
            if (loadedData.TryGetValue (colPos, out col)) {
                // return the block
                return col.lightLevel [localX, localY, localZ];
            } else {
                // not found ):
                return def;
            }
        }
    }

    /** Set the block ID at the position (worldX, worldY, worldZ). If the position is not currently loaded, do nothing */
    public void SetBlockAt (int worldX, int worldY, int worldZ, ushort newBlock)
    {
        lock (this) {
            Vector2i colPos = MiscMath.WorldToColumnCoords (worldX, worldZ);
            Column col;
            if (loadedData.TryGetValue (colPos, out col)) {
                int localX = MiscMath.Mod (worldX, CHUNK_SIZE);
                int localY = worldY;
                int localZ = MiscMath.Mod (worldZ, CHUNK_SIZE);
                ushort oldBlock = col.blockID [localX, localY, localZ];

                // let the old block do whatever it needs to
                Block.GetInstance(oldBlock).OnBreak(this, worldX, worldY, worldZ, newBlock);

                // set the block
                col.blockID [localX, localY, localZ] = newBlock;

                // TODO update lighting

                // let the new block do whatever it needs to
                Block.GetInstance(newBlock).OnPlace(this, worldX, worldY, worldZ, oldBlock);
            }
        }
    }
}