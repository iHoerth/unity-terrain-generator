using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class InventorySlot : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        // Obtengo el objeto drageado, q si o si es de tipo InventoryItem en este caso
        InventoryItem draggedItem = eventData.pointerDrag.GetComponent<InventoryItem>();

        if(transform.childCount == 0)
        {
            draggedItem.parentAfterDrag = transform;
        }

        else
        {
            InventoryItem itemInSlot = GetComponentInChildren<InventoryItem>();
            itemInSlot.transform.SetParent(draggedItem.originalParent);

            draggedItem.parentAfterDrag = transform;
        }
    }
}