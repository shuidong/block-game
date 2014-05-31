using UnityEngine;
using System.Collections;

public static class MiscMath
{
    public static int Mod (int k, int n)
    {
        return ((k %= n) < 0) ? k + n : k;
    }

    public static Vector2i WorldToColumnCoords (int x, int z)
    {
        Vector2i loc = new Vector2i (x / World.CHUNK_SIZE, z / World.CHUNK_SIZE);
        if (x < 0 && x % World.CHUNK_SIZE != 0)
            loc.x--;
        if (z < 0 && z % World.CHUNK_SIZE != 0)
            loc.z--;
        return loc;
    }
}
