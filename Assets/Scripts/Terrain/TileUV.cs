using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileUV
{
    private int x, y;
    private float atlasSize = 4f; // n of columns

    private Vector2[] uvs;

    // constructor 
    // recibe un par de enteros x,y y segun eso crea una lista uvs con las locaciones en el atlas
    public TileUV (int x, int y)
    {
        this.x = x;
        this.y = y;

        uvs = new Vector2[4]
        {
            new Vector2((x / atlasSize) + 0.001f, (y / atlasSize) + 0.001f),
            new Vector2(((x + 1) / atlasSize) - 0.001f, (y / atlasSize) + 0.001f), // lower right vertex
            new Vector2(((x + 1) / atlasSize) - 0.001f, ((y + 1) / atlasSize) - 0.001f), // upper right vertex
            new Vector2((x / atlasSize) + 0.001f, ((y + 1) / atlasSize) - 0.001f), // upper left vertex
        };

    }

    // esto para obtener la lista de uvs a la hora de renderizar
    public Vector2[] GetUV()
    {
        return uvs;
    }

    // dictionary que a cada TileType le asigna un new TileUV
    public static Dictionary<TileType, TileUV> uvLookup = new Dictionary<TileType, TileUV>()
    {
        {TileType.Dirt, new TileUV(0,2)},
        {TileType.GrassSide, new TileUV(0,3)},
        {TileType.Grass, new TileUV(1,2)},
        {TileType.Stone, new TileUV(1,3)},
    };
}