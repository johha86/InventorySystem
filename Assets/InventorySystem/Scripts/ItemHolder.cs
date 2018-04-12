using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class ItemHolder : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    public GameObject ImageHolder;
    /// <summary>
    /// Item contained inside the item wrapper.
    /// </summary>
    public Item Item;
    /// <summary>
    /// Index inside the inventory.
    /// </summary>
    [HideInInspector]
    public int m_index;
    [HideInInspector]
    public Vector2 m_initialPosition;    

    private Image m_iconComponent;
    private bool m_isDragging;

    #region EventSystems
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (IsDraggable)
        {
            //Update the gameobject state
            m_initialPosition = this.transform.position;

            //Notify that a drag has began!
            InventoryPanel.Instance.NotifyDragBegan(this);
            
            m_isDragging = true;

            this.transform.SetAsLastSibling();
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (IsDraggable)
        {
            this.transform.position = Input.mousePosition;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (IsDraggable)
        {
            if (m_isDragging)
            {
                m_isDragging = false;
                //Notify that the current element has been dropped.
                InventoryPanel.Instance.NotifyDrop();
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (Item != null && !m_isDragging)
        {
            InventoryPanel.Instance.NotifyClick(this);
        }
    }
    #endregion

    public void AddItem(Item item)
    {
        if (item == null) return;

        Item = item;
        m_iconComponent.sprite = Item.SpriteImage;
    }

    public void RemoveItem()
    {
        Item = null;
        m_iconComponent.sprite = GetComponent<Image>().sprite;
    }

    public void ResetToInitialPosition()
    {
        this.transform.position = m_initialPosition;
    }
    /// <summary>
    /// Whether this Item Holder is on dragging by the user.
    /// </summary>
    public bool IsDragging { get { return m_isDragging; } }
    /// <summary>
    /// Whether this item holder can be draggable.
    /// </summary>
    public bool IsDraggable { get { return Item != null; } }

    #region Monobehaviour Methods
    
    void Start()
    {
        Assert.IsNotNull(ImageHolder);

        m_iconComponent = ImageHolder.GetComponent<Image>();
        Assert.IsNotNull(m_iconComponent, "Missing Image Component in Image Holder gameobject!");

        //Use the invisible default sprite of this item holder as the default sprite for the Icon. 
        m_iconComponent.sprite = GetComponent<Image>().sprite;
    }

    #endregion
}
