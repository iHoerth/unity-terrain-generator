using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    public GameObject invGroup;
    public GameObject toolBar;
    public GameObject inventoryItemPrefab;

    [HideInInspector] 
    public InventorySlot[] inventorySlots;

    public Image crosshair;
    public Button darkBgToggle;
    public Button invBtn;


    public bool inventoryOpened = false;

    void Awake()
    {
        // InventoryManager ref
        Transform invManager = invGroup.transform.parent;

        // Busca TODOS los InventorySlot que cuelgan de InventoryManager
        inventorySlots = invManager.GetComponentsInChildren<InventorySlot>(true);

        invBtn.onClick.AddListener(ToggleInventory);
        darkBgToggle.onClick.AddListener(ToggleInventory);
    }

    public void ToggleInventory()
    {
        inventoryOpened = !inventoryOpened;
        // Show Inventory Modal and hide Crosshair and button
        invGroup.SetActive(inventoryOpened);
        crosshair.gameObject.SetActive(!inventoryOpened);
        invBtn.gameObject.SetActive(!inventoryOpened);
    }   

    public void AddItem(Item newItem, int quantity = 1)
    {
        InventorySlot firstEmptySlot = null;
        InventoryItem sameItem = null;

        foreach(InventorySlot slot in inventorySlots)
        {
            InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();

            // Find first empty slot and save reference
            if(firstEmptySlot == null && itemInSlot == null) 
            {
                firstEmptySlot = slot;
                continue;
            }

            if(itemInSlot != null && itemInSlot.item == newItem)
            {
                sameItem = itemInSlot;
            }

        }

        if(newItem.stackable && sameItem != null)
        {
            // sameItem.quantity += quantity;
            sameItem.UpdateItem(newItem, sameItem.quantity + quantity);
            // Debug.Log(sameItem.quantity);
        }

        else
        {
            if(firstEmptySlot != null)
            {
                CreateItem(newItem, firstEmptySlot, quantity);
            }

            else Debug.Log("Inventory is Full");
        }
        // if i am here means it could not be stacked
    }

    public void CreateItem(Item item, InventorySlot slot, int quantity = 1)
    {
        Debug.Log($"CreateItem: {item?.name} en slot {slot.name}");

        GameObject newItemGameObject = Instantiate(inventoryItemPrefab, slot.transform);
        InventoryItem inventoryItem = newItemGameObject.GetComponent<InventoryItem>();
        inventoryItem.UpdateItem(item, quantity);
    }

    public void RemoveItem(Item item, InventorySlot slot)
    {
        
    }
}
