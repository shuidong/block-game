﻿using UnityEngine;
using System.Collections;

[System.Serializable]
public struct Vector2i : System.IEquatable<Vector2i>
{
    public int x, z;
    
    public Vector2i (int x, int z)
    {
        this.x = x;
        this.z = z;
    }
    
    public float Distance (Vector2i other)
    {
        int xDiff = other.x - x;
        int zDiff = other.z - z;
        return Mathf.Sqrt (xDiff * xDiff + zDiff * zDiff);
    }
    
    public bool Equals (Vector2i other)
    {
        return other.x == x && other.z == z;
    }
}

[System.Serializable]
public struct Vector3i : System.IEquatable<Vector3i>
{
    public int x, y, z;
    
    public Vector3i (int x, int y, int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }
    
    public float Distance (Vector3i other)
    {
        int xDiff = other.x - x;
        int yDiff = other.z - z;
        int zDiff = other.z - z;
        return Mathf.Sqrt (xDiff * xDiff + yDiff * yDiff + zDiff * zDiff);
    }
    
    public bool Equals (Vector3i other)
    {
        return other.x == x && other.y == y && other.z == z;
    }
}
