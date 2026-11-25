using UnityEngine;
using UnityEngine.UI;

public class HonestyStoreUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject storePanel;

    [Header("Other Controls")]
    [SerializeField] private Button collectButton;
    [SerializeField] private Button closeButton;

    [Header("Display")]
    [SerializeField] private Text statusText;
    [SerializeField] private Text shelfContentsText;
    [SerializeField] private Text earningsText;

    private HonestyStore currentStore;

    private void Awake()
    {
        if (storePanel != null)
            storePanel.SetActive(false);

        if (collectButton != null)
            collectButton.onClick.AddListener(CollectEarnings);

        if (closeButton != null)
            closeButton.onClick.AddListener(Close);
    }

    public void Open(HonestyStore store, int newlyStockedCount)
    {
        currentStore = store;

        if (storePanel != null)
            storePanel.SetActive(true);

        if (statusText != null)
        {
            statusText.text = newlyStockedCount > 0
                ? "Automatically stocked " + newlyStockedCount + " item(s) from your inventory."
                : "No sellable items could be added. Your shelf may be full or your inventory has no configured products.";
        }

        RefreshDisplays();
    }

    public void Close()
    {
        if (storePanel != null)
            storePanel.SetActive(false);

        currentStore = null;
    }

    public void RefreshDisplays()
    {
        if (currentStore == null)
            return;

        if (earningsText != null)
            earningsText.text = "Uncollected earnings: ₱" + currentStore.StoredEarnings;

        if (shelfContentsText != null)
        {
            System.Text.StringBuilder display = new System.Text.StringBuilder();
            var slots = currentStore.Slots;

            for (int i = 0; i < slots.Count; i++)
            {
                StoreSlot slot = slots[i];

                if (slot != null && slot.IsOccupied)
                {
                    display.AppendLine(
                        "Slot " + (i + 1) + ": " + slot.ItemType +
                        " | Stock: " + slot.Quantity +
                        " | Price: ₱" + slot.Price +
                        " | Sale chance: " +
                        (currentStore.GetSaleChance(i) * 100f).ToString("F0") + "%");
                }
                else
                {
                    display.AppendLine("Slot " + (i + 1) + ": Empty");
                }
            }

            shelfContentsText.text = display.ToString();
        }
    }

    private void CollectEarnings()
    {
        if (currentStore == null)
            return;

        int amount = currentStore.CollectEarnings();

        if (statusText != null)
            statusText.text = amount > 0
                ? "Collected ₱" + amount + "."
                : "There are no earnings to collect.";

        RefreshDisplays();
    }
}
