﻿using UnityEngine;
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

    /** Initialize a world with a 'name' and a 'worldType'. The 'worldType' is used to determine the kind of terrain generation in the world */
    public World (string name, TerrainType worldType)
    {
        // init fields
        this.saveName = name;
        this.saveDir = Application.persistentDataPath + "saves/" + saveName + "/" + worldType + "/";
        this.worldType = worldType;

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
                Column col = TerrainGen.Generate (worldType, pos);
                loadedData.Add (pos, col);
            }
        }
    }

    /** Return the block ID at the position (x, y, z). If the position is not currently loaded, return def */
    public short GetBlockAt (int x, int y, int z, short def)
    {
        lock (this) {
            Vector2i colPos = MiscMath.WorldToColumnCoords (x, z);
            Column col;
            if (loadedData.TryGetValue (colPos, out col)) {
                x = MiscMath.Mod (x, CHUNK_SIZE);
                z = MiscMath.Mod (z, CHUNK_SIZE);
                return col.blockID [x, y, z];
            } else {
                return def;
            }
        }
    }
}