using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class InventoryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("UI")]
    public Image itemImage;
    public Item item;
    public TMP_Text quantityText;
    private CanvasGroup canvasGroup;
    [HideInInspector] public Transform parentAfterDrag;
    [HideInInspector] public int quantity;
    
    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        itemImage = GetComponent<Image>();
        quantityText = GetComponentInChildren<TMP_Text>();
    }

    public void UpdateItem(Item newItem, int quantity = 1)
    {   
        this.quantity = quantity;
        this.item = newItem;

        if (newItem != null)
            itemImage.sprite = newItem.sprite;

        if(this.quantity == 1)
            this.quantityText.text = "".ToString();
        else 
            this.quantityText.text = quantity.ToString();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        parentAfterDrag = transform.parent;
        transform.SetParent(transform.root);

        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = false;

    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;        
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        transform.SetParent(parentAfterDrag);
        // itemImage.raycastTarget = true;
        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = true;
    }
}
