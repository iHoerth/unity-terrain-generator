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

    public bool inventoryOpened = false;

    void Awake()
    {
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
}