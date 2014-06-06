using UnityEngine;
using System.Collections;

public class Vertex  {
    public static Vector3 bNE (Vector3 center, Vector3 size) {
        size /= 2;
        center.x += size.x;
        center.y -= size.y;
        center.z += size.z;
        return center;
    }
    
    public static Vector3 bNW (Vector3 center, Vector3 size) {
        size /= 2;
        center.x -= size.x;
        center.y -= size.y;
        center.z += size.z;
        return center;
    }
    
    public static Vector3 bSW (Vector3 center, Vector3 size) {
        size /= 2;
        center.x -= size.x;
        center.y -= size.y;
        center.z -= size.z;
        return center;
    }
    
    public static Vector3 bSE (Vector3 center, Vector3 size) {
        size /= 2;
        center.x += size.x;
        center.y -= size.y;
        center.z -= size.z;
        return center;
    }
    
    public static Vector3 tNE (Vector3 center, Vector3 size) {
        size /= 2;
        center.x += size.x;
        center.y += size.y;
        center.z += size.z;
        return center;
    }
    
    public static Vector3 tNW (Vector3 center, Vector3 size) {
        size /= 2;
        center.x -= size.x;
        center.y += size.y;
        center.z += size.z;
        return center;
    }
    
    public static Vector3 tSW (Vector3 center, Vector3 size) {
        size /= 2;
        center.x -= size.x;
        center.y += size.y;
        center.z -= size.z;
        return center;
    }
    
    public static Vector3 tSE (Vector3 center, Vector3 size) {
        size /= 2;
        center.x += size.x;
        center.y += size.y;
        center.z -= size.z;
        return center;
    }
}