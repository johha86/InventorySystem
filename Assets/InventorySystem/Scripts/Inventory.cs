using System;

public class Inventory
{
    internal ItemHolder[] Items;
    internal int ItemCounter;
    internal ItemHolder m_selectedItem;
    internal ItemHolder m_draggingItem;
    internal readonly int m_maximumItems;

    #region Initialization
    public Inventory(ItemHolder[] itemsCollection)
    {
        Items = itemsCollection;
        ItemCounter = 0;
        m_maximumItems = itemsCollection.Length;
    }
    #endregion

    #region Public Methods
    public void AddItem(Item newItem)
    {
        if (newItem == null)
            throw new ArgumentNullException("newItem");
        if (IsFull) return;

        //Look for a new free space inside the collection
        for (int i = 0; i < Items.Length; i++)
        {
            if (Items[i].Item == null)
            {
                Items[i].AddItem(newItem);
                Items[i].m_index = i;
                ItemCounter++;
                break;
            }
        }
    }

    public void SwapItemHolders(int indexA, int indexB)
    {
        if (Items[indexA].Item == null && Items[indexB].Item == null) throw new InvalidOperationException();

        //Swaping items
        var tempA = Items[indexA];
        Items[indexA] = Items[indexB];
        Items[indexB] = tempA;

        //Updating the items index
        Items[indexA].m_index = indexA;
        Items[indexB].m_index = indexB;
    }

    public void RemoveItem(int index)
    {
        if (index < 0 || index >= MaxItems) return;

        if (ItemCounter > 0)
        {
            if (Items[index] == m_selectedItem)
                DeselectItem();

            Items[index].RemoveItem();
            ItemCounter--;
        }
    }

    public void SelectItem(int index)
    {
        //If the items count is 0 then wouldn't possible that an item is selected.
        if (Count == 0) throw new InvalidOperationException();

        if (index < 0 || index >= MaxItems) return;

        if (Items[index].Item != null)
            m_selectedItem = Items[index];
    }

    public void DeselectItem()
    {
        if (m_selectedItem == null) throw new InvalidOperationException();

        m_selectedItem = null;
    }
    #endregion

    #region Properties
    public int Count { get { return ItemCounter; } }

    public bool IsFull { get { return ItemCounter == Items.Length; } }

    public int MaxItems { get { return m_maximumItems; } }

    public ItemHolder SelectedItem { get { return m_selectedItem; } }
    #endregion
}
