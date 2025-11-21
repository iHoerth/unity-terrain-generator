using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    public GameObject invGroup;
    public Image crosshair;
    public Button invBtn;

    public bool inventoryOpened = false;

    // void Awake()
    // {

    // }

    void Update()
    {

    }

    public void ToggleInventory()
    {
        inventoryOpened = !inventoryOpened;
        // Show Inventory Modal and hide Crosshair and button
        invGroup.SetActive(inventoryOpened);
        crosshair.gameObject.SetActive(!inventoryOpened);
        invBtn.gameObject.SetActive(!inventoryOpened);
    }   

    
}