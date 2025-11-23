using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Scriptable object/Item")]
public class Item : ScriptableObject
{
    [Header("Gameplay")]
    public TileBase tile;
    public ItemType type;
    public ActionType actionType;
    public Vector2Int range = new Vector2Int(5, 4);

    [Header("UI")]
    public bool stackable = true;
    public Sprite sprite;
    
    public enum ItemType
    {
        BuildingBlock,
        Tool
    }

    public enum ActionType
    {
        Dig,
        Mine
    }
}
