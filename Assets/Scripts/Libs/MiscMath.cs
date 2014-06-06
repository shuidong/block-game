using UnityEngine;
using System.Collections;

public static class MiscMath
{
    public static int Mod (int k, int n)
    {
        return ((k %= n) < 0) ? k + n : k;
    }

    public static Vector2i WorldToColumnCoords(float x, float z)
    {
        int iX = Mathf.RoundToInt(x);
        int iZ = Mathf.RoundToInt(z);
        Vector2i loc = new Vector2i(iX / World.CHUNK_SIZE, iZ / World.CHUNK_SIZE);
        if (iX < 0 && iX % World.CHUNK_SIZE != 0)
            loc.x--;
        if (iZ < 0 && iZ % World.CHUNK_SIZE != 0)
            loc.z--;
        return loc;
    }

    public static Vector3i WorldToChunkCoords(float x, float y, float z)
    {
        int iX = Mathf.RoundToInt(x);
        int iY = Mathf.RoundToInt(y);
        int iZ = Mathf.RoundToInt(z);
        Vector3i loc = new Vector3i(iX / World.CHUNK_SIZE, iY / World.CHUNK_SIZE, iZ / World.CHUNK_SIZE);
        if (iX < 0 && iX % World.CHUNK_SIZE != 0)
            loc.x--;
        if (iY < 0 && iY % World.CHUNK_SIZE != 0)
            loc.y--;
        if (iZ < 0 && iZ % World.CHUNK_SIZE != 0)
            loc.z--;
        return loc;
    }
}
