using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    public GameObject invGroup;
    public Image crosshair;
    public Button darkBgToggle;
    public Button invBtn;

    [HideInInspector] 
    public InventorySlot[] inventorySlots;

    public bool inventoryOpened = false;

    void Awake()
    {
        inventorySlots = invGroup.GetComponentsInChildren<InventorySlot>(true);

        Debug.Log(inventorySlots[0]);

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

    public void AddItem(Item item, int quantity = 1)
    {
        foreach(InventorySlot slot in inventorySlots)
        {
            InventoryItem itemInSlot = slot.GetComponentsInChildren<InventoryItem>()[0];
            // Si no tiene hijo, esto va a devolver null, entonces
            if(itemInSlot == null)
            {
                itemInSlot.UpdateItem(item, quantity);
                return;
            }
            else if(itemInSlot.item.type == item.type && item.stackable)
            {
                // esto esta mas o menos XD
                itemInSlot.UpdateItem(item, quantity);
            }

            itemInSlot.transform.SetParent(slot.transform); 
        }
    }

}