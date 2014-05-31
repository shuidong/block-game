using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class ChunkRenderer : MonoBehaviour
{
    [HideInInspector]
    public MeshFilter meshFilter;

    void Awake ()
    {
        meshFilter = GetComponent<MeshFilter>();
    }
}
