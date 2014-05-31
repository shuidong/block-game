using UnityEngine;
using System.Collections;

public class Column
{
    // column data
    public ushort[,,] blockID = new ushort[World.CHUNK_SIZE, World.CHUNK_SIZE * World.WORLD_HEIGHT, World.CHUNK_SIZE];
    public byte[,,] lightLevel = new byte[World.CHUNK_SIZE, World.CHUNK_SIZE * World.WORLD_HEIGHT, World.CHUNK_SIZE];
}
