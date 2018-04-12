using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[Serializable]
public class ItemEvent : UnityEvent<Item>
{
}

[Serializable]
public class DropEvent : UnityEvent<Item,int,bool>
{
}

public class InventoryPanel : MonoBehaviour
{
    //-----Items collection--------
    [Tooltip("Reference to the items holder.")]
    public ItemHolder[] Items;

    [SerializeField]
    private GameObject m_Selector;    

    //----Label------
    [Header("Label")]
    [SerializeField]
    private Text m_InfoLabel;
    [SerializeField]
    private float m_InfoLabelTimeInScreen = 2f;
    [Range(0.01f,1f)]
    [SerializeField]
    private float m_InfoLabelAnimationSpeed = 0.1f;
    private Coroutine m_InfoLabelCoroutine;
    
    private ItemHolder m_draggingItem;    
    internal Inventory m_inventory;    
    
    //Public events 
    public ItemEvent OnItemSelected;
    public DropEvent OnItemDropped;
    public ItemEvent OnItemAdded;

    #region Monobehaviour Methods
    // Use this for initialization
    void Start()
    {
        m_inventory = new Inventory(Items);
        m_singleton = this;

        //Hide the selector's gameobject
        m_Selector.SetActive(false);        

        if (OnItemSelected == null)
            OnItemSelected = new ItemEvent();
        if (OnItemDropped == null)
            OnItemDropped = new DropEvent();
        if (OnItemAdded == null)
            OnItemAdded = new ItemEvent();
    }
    #endregion

    /// <summary>
    /// Notify to the Inventory that a drag event has began.
    /// </summary>
    /// <param name="item"></param>
    public void NotifyDragBegan(ItemHolder item)
    {
        m_draggingItem = item;
    }
    /// <summary>
    ///  Notify to the Inventory that a drag event has ended.
    /// </summary>
    /// <param name="item"></param>
    public void NotifyDrop()
    {
        ItemHolder item = null;
        List<RaycastResult> hitObjects = new List<RaycastResult>();

        //Cast all elements in the current position under the cursor
        var pointer = new PointerEventData(EventSystem.current);
        pointer.position = Input.mousePosition;
        EventSystem.current.RaycastAll(pointer, hitObjects);

        if (hitObjects.Count >= 2)
        {
            item = hitObjects[1].gameObject.GetComponent<ItemHolder>();
        }
        //Check if the dropped item need to be swapped to a new position.
        if (null != item)
        {            
            Vector2 dropperPosition = item.transform.position;
            item.transform.position = m_draggingItem.m_initialPosition;
            m_draggingItem.transform.position = dropperPosition;
            //Swapping items positions
            m_inventory.SwapItemHolders(item.m_index, m_draggingItem.m_index);
            //Notify to all listeners
            OnItemDropped.Invoke(m_draggingItem.Item, m_draggingItem.m_index, true);
        }
        else
        {
            //just return the dropped element to its original position
            m_draggingItem.ResetToInitialPosition();
            //Notify to all listeners
            OnItemDropped.Invoke(m_draggingItem.Item, m_draggingItem.m_index, false);
        }

        //Update the selector position if its needed
        if (null != m_inventory.SelectedItem)
        {
            var itemTransform = m_inventory.SelectedItem.transform;
            m_Selector.transform.position = itemTransform.position;
            m_Selector.SetActive(true);
        }

        m_draggingItem = null;
    }
    /// <summary>
    /// Notify to the inventory about a click on an item holder.
    /// </summary>
    /// <param name="holder"></param>
    public void NotifyClick(ItemHolder holder)
    {
        if (m_inventory.SelectedItem == holder) return;
        //Retrieve a reference to the holder inside the inventory
        var itemInInventory = Items.First((e) => e == holder);
        if (itemInInventory == null) return;

        //Show the selector
        var itemTransform = itemInInventory.transform;
        m_Selector.transform.position = itemTransform.position;
        m_Selector.SetActive(true);

        //Update the internal state of the Inventory
        m_inventory.SelectItem(holder.m_index);
        if (m_InfoLabelCoroutine != null)
            StopCoroutine(m_InfoLabelCoroutine);
        m_InfoLabelCoroutine = StartCoroutine(InfoLabelAnimation(holder.Item.Name));

        //Notify to all listeners
        OnItemSelected.Invoke(m_inventory.SelectedItem.Item);
    }
    /// <summary>
    /// Add a new Item inside the inventory
    /// </summary>
    public void AddItem(Item newItem)
    {
        //Notify to all listeners
        m_inventory.AddItem(newItem);

        OnItemAdded.Invoke(newItem);
    }
    /// <summary>
    /// Remove a item inside the inventory
    /// </summary>
    /// <param name="index"></param>
    public void RemoveItem(int index)
    {
        m_inventory.RemoveItem(index);
    }    

    private static InventoryPanel m_singleton;
    /// <summary>
    /// Singleton instance to the Invetory UI Element.
    /// </summary>
    public static InventoryPanel Instance { get { return m_singleton; } }

    public bool IsFull { get { return m_inventory.IsFull; } }

    public int ItemCount { get { return m_inventory.Count; } }

    IEnumerator InfoLabelAnimation(string text)
    {
        m_InfoLabel.text = text;
        Color c = m_InfoLabel.color;
        c.a = 1;
        m_InfoLabel.color = c;

        //wait
        yield return new WaitForSeconds(m_InfoLabelTimeInScreen);

        while (m_InfoLabel.color.a > 0.01)
        {
            //Decrease the alfa value
            c = m_InfoLabel.color;
            c.a -= m_InfoLabelAnimationSpeed;
            m_InfoLabel.color = c;
            
            //continue
            yield return new WaitForSeconds(0.1f);
        }

        m_InfoLabel.text = "";
    }
}
