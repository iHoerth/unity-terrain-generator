using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class DemoScript : MonoBehaviour
{
    [HideInInspector]
    public Item[] itemsToPickup;

    // Individual refs to avoid array inspector bug
    public Item stoneBlock;
    public Item dirtBlock;
    public Item pickaxe;

    public InventoryManager inventoryManager;

    void Awake()
    {
        itemsToPickup = new Item[]
        {
            stoneBlock,
            dirtBlock,
            pickaxe
        };
    }

    public void PickUpItem(int id)
    {
        inventoryManager.AddItem(itemsToPickup[id]);
    }
}