using System;
using UnityEngine;

[Serializable]
public class StoreSlot
{
    [SerializeField] private ItemType itemType;
    [SerializeField, Min(0)] private int quantity;
    [SerializeField, Min(1)] private int price;

    public ItemType ItemType => itemType;
    public int Quantity => quantity;
    public int Price => price;
    public bool IsOccupied => quantity > 0;

    public void Set(ItemType type, int amount, int itemPrice)
    {
        itemType = type;
        quantity = Mathf.Max(0, amount);
        price = Mathf.Max(1, itemPrice);
    }

    public void Add(int amount) => quantity += Mathf.Max(0, amount);

    public int Remove(int amount)
    {
        int removed = Mathf.Min(quantity, Mathf.Max(0, amount));
        quantity -= removed;
        return removed;
    }

    public void SetPrice(int newPrice) => price = Mathf.Max(1, newPrice);
    public void Clear() { quantity = 0; price = 1; }
}
