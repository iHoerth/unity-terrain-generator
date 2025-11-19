using System.Collections.Generic;
using UnityEngine;

public struct MeshData
{
    public List<int> triangles;
    public List<Vector2> uvs;
    public List<Vector3> vertices;

    public MeshData(     
        List<int> tris,
        List<Vector2> uv,
        List<Vector3> verts
    )
    {
        triangles = tris;
        uvs = uv;
        vertices = verts;
    }
}