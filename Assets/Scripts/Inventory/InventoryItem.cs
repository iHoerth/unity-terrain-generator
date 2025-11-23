using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventoryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("UI")]
    public Image itemImage;
    public Item item;
    private CanvasGroup canvasGroup;
    [HideInInspector] public Transform parentAfterDrag;
    [HideInInspector] public int quantity;
    
    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    void Start()
    {
        if (item != null)
            UpdateItem(item, quantity);
    }

    public void UpdateItem(Item newItem, int quantity = 1)
    {   
        this.quantity = quantity;
        this.item = newItem;
        this.itemImage.sprite = newItem.sprite;
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
