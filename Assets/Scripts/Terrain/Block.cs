using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block
{
    // propiedades/atributos
    public TileType top, side, bottom; // dirt grass air etc etc just an enum type
    public TileUV topUV, sideUV, bottomUV; // actual UV coords 
    
    public BlockType type;

    // Constructor with only 1 arg (for blocks with the same texture in all faces)
    public Block(TileType tileType) 
    {   
        // Type (e.g. "Grass" or "Dirt" etc)
        this.top = tileType;
        this.side = tileType;
        this.bottom = tileType;

        // UV Coords => TileUV is dictionary to look up that tileType (key) to retrieve its UV coord in the atlas (value)
        this.topUV = TileUV.uvLookup[top];
        this.sideUV = TileUV.uvLookup[side];
        this.bottomUV = TileUV.uvLookup[bottom];
    }

    // Constructor with 3 args, for blocks with different textures for each face (e.g. Grass)
    public Block(TileType tileTypeTop, TileType tileTypeSide, TileType tileTypeBottom)
    {   
        // Type (e.g. "Grass" or "Dirt" etc)
        this.top = tileTypeTop;
        this.side = tileTypeSide;
        this.bottom = tileTypeBottom;

        // UV Coords => TileUV is dictionary to look up that tileType (key) to retrieve its UV coord in the atlas (value)
        this.topUV = TileUV.uvLookup[top];
        this.sideUV = TileUV.uvLookup[side];
        this.bottomUV = TileUV.uvLookup[bottom];
    }

    //Dictionary<TKey, TValue> name = new Dictionary<Tkey, Tvalue>()  donde Tkey es el tipo de las claves y Tvalue el de los valores
    public static Dictionary<BlockType, Block> blockData = new Dictionary<BlockType, Block>()
    {
        {BlockType.Grass, new Block(TileType.Grass, TileType.GrassSide, TileType.Dirt)},
        {BlockType.Dirt, new Block(TileType.Dirt)},
        {BlockType.Stone, new Block(TileType.Stone)},
    };
}