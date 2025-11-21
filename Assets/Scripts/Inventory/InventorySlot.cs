using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class InventorySlot : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        if(transform.childCount == 0)
        {
            // Obtengo el objeto drageado, q si o si es de tipo InventoryItem en este caso
            InventoryItem invItem = eventData.pointerDrag.GetComponent<InventoryItem>();
            // invItem.SetParent(transform);
            invItem.parentAfterDrag = transform;
        }
    }    
}