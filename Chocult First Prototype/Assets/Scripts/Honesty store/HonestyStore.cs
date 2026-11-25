using System;
using System.Collections.Generic;
using UnityEngine;

public class HonestyStore : MonoBehaviour, IInteractable
{
    [Header("References")]
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private PlayerCurrency playerCurrency;
    [SerializeField] private StoreUpgradeManager upgrades;
    [SerializeField] private HonestyStoreUI storeUI;

    [Header("Product Definitions")]
    [SerializeField] private StoreItemData[] itemDefinitions;

    [Header("Shelf Settings")]
    [SerializeField, Min(1)] private int baseStackCapacity = 10;
    [SerializeField] private List<StoreSlot> slots = new List<StoreSlot>();

    [Header("Stored Sales")]
    [SerializeField, Min(0)] private int storedEarnings;

    public int StoredEarnings => storedEarnings;
    public IReadOnlyList<StoreSlot> Slots => slots;

    public event Action OnStoreChanged;
    public event Action<int> OnSaleCompleted;

    private void Awake()
    {
        if (playerInventory == null) playerInventory = FindObjectOfType<PlayerInventory>();
        if (playerCurrency == null) playerCurrency = FindObjectOfType<PlayerCurrency>();
        if (upgrades == null) upgrades = GetComponent<StoreUpgradeManager>();
        EnsureSlotCount();
    }

    // Interacting automatically stocks every sellable item that can fit,
    // then opens the store panel so the player can review stock and collect earnings.
    public void Interact()
    {
        int stockedCount = StockAllSellableItems();

        if (storeUI != null)
            storeUI.Open(this, stockedCount);
        else
            Debug.LogWarning("HonestyStoreUI is not assigned to the Honesty Store.");
    }

    public int StockAllSellableItems()
    {
        if (playerInventory == null)
            return 0;

        int stockedCount = 0;

        // Work from a snapshot because StockItem removes entries from the inventory.
        List<ItemType> inventorySnapshot = new List<ItemType>(playerInventory.inventory);

        foreach (ItemType item in inventorySnapshot)
        {
            // Items without a StoreItemData definition are not sellable and stay in inventory.
            if (GetDefinition(item) == null)
                continue;

            if (StockItem(item, 1))
                stockedCount++;
        }

        return stockedCount;
    }

    public bool StockItem(ItemType item, int amount)
    {
        if (playerInventory == null || amount <= 0) return false;

        StoreItemData definition = GetDefinition(item);
        if (definition == null)
        {
            Debug.LogWarning("No StoreItemData configured for " + item);
            return false;
        }

        if (CountInventoryItem(item) < amount) return false;

        StoreSlot slot = FindSlotForItem(item);
        bool isNewSlot = slot == null;
        if (isNewSlot)
        {
            slot = FindEmptySlot();
            if (slot == null) return false;
        }

        int amountToStock = Mathf.Min(amount, baseStackCapacity - slot.Quantity);
        if (amountToStock <= 0) return false;

        for (int i = 0; i < amountToStock; i++) playerInventory.RemoveItem(item);

        if (isNewSlot) slot.Set(item, amountToStock, definition.DefaultPrice);
        else slot.Add(amountToStock);

        OnStoreChanged?.Invoke();
        return true;
    }

    public bool SetItemPrice(int slotIndex, int newPrice)
    {
        if (!IsValidOccupiedSlot(slotIndex) || newPrice < 1) return false;
        slots[slotIndex].SetPrice(newPrice);
        OnStoreChanged?.Invoke();
        return true;
    }

    public bool RemoveStock(int slotIndex, int amount)
    {
        if (!IsValidOccupiedSlot(slotIndex) || playerInventory == null || amount <= 0)
            return false;

        StoreSlot slot = slots[slotIndex];
        int amountToRemove = Mathf.Min(amount, slot.Quantity);
        int added = 0;

        // PlayerInventory has a four-item limit. Add what fits; leave the rest on the shelf.
        for (int i = 0; i < amountToRemove; i++)
        {
            if (!playerInventory.AddItem(slot.ItemType)) break;
            added++;
        }

        if (added == 0) return false;

        slot.Remove(added);
        if (!slot.IsOccupied) slot.Clear();
        OnStoreChanged?.Invoke();
        return true;
    }

    // Called by StoreSaleSimulator once every configured interval (10 seconds by default).
    public void ProcessSalesTick()
    {
        if (upgrades == null) return;

        bool changed = false;

        for (int i = 0; i < slots.Count; i++)
        {
            StoreSlot slot = slots[i];
            if (slot == null || !slot.IsOccupied) continue;

            StoreItemData definition = GetDefinition(slot.ItemType);
            if (definition == null) continue;

            // At default price, chance equals base chance.
            // Higher prices lower the chance; lower prices raise it.
            float priceRatio = (float)definition.DefaultPrice / Mathf.Max(1, slot.Price);
            float finalChance = Mathf.Clamp01(
                definition.BaseSaleChance * priceRatio + upgrades.GetSaleChanceBonus());

            if (UnityEngine.Random.value < finalChance)
            {
                int quantitySold = Mathf.Min(slot.Quantity, upgrades.GetSaleQuantity());
                int earnings = quantitySold * slot.Price;

                slot.Remove(quantitySold);
                storedEarnings += earnings;

                if (!slot.IsOccupied) slot.Clear();

                changed = true;
                OnSaleCompleted?.Invoke(earnings);
                Debug.Log("Honesty Store sold " + quantitySold + " " +
                          slot.ItemType + " for ₱" + earnings + ".");
            }
        }

        if (changed) OnStoreChanged?.Invoke();
    }

    public int CollectEarnings()
    {
        if (storedEarnings <= 0 || playerCurrency == null) return 0;

        int amount = storedEarnings;
        playerCurrency.AddMoney(amount);
        storedEarnings = 0;
        OnStoreChanged?.Invoke();
        return amount;
    }

    public float GetSaleChance(int slotIndex)
    {
        if (!IsValidOccupiedSlot(slotIndex) || upgrades == null) return 0f;

        StoreItemData definition = GetDefinition(slots[slotIndex].ItemType);
        if (definition == null) return 0f;

        float priceRatio = (float)definition.DefaultPrice / Mathf.Max(1, slots[slotIndex].Price);
        return Mathf.Clamp01(definition.BaseSaleChance * priceRatio +
                             upgrades.GetSaleChanceBonus());
    }

    private void EnsureSlotCount()
    {
        int targetCount = upgrades != null ? upgrades.GetSlotCapacity() : 2;
        while (slots.Count < targetCount) slots.Add(new StoreSlot());
    }

    private StoreItemData GetDefinition(ItemType item)
    {
        foreach (StoreItemData definition in itemDefinitions)
            if (definition != null && definition.ItemType == item) return definition;
        return null;
    }

    private StoreSlot FindSlotForItem(ItemType item)
    {
        foreach (StoreSlot slot in slots)
            if (slot != null && slot.IsOccupied && slot.ItemType == item &&
                slot.Quantity < baseStackCapacity) return slot;
        return null;
    }

    private StoreSlot FindEmptySlot()
    {
        EnsureSlotCount();
        foreach (StoreSlot slot in slots)
            if (slot != null && !slot.IsOccupied) return slot;
        return null;
    }

    private int CountInventoryItem(ItemType item)
    {
        int count = 0;
        foreach (ItemType inventoryItem in playerInventory.inventory)
            if (inventoryItem == item) count++;
        return count;
    }

    private bool IsValidOccupiedSlot(int index)
    {
        return index >= 0 && index < slots.Count &&
               slots[index] != null && slots[index].IsOccupied;
    }
}
