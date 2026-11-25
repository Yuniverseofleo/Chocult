using UnityEngine;

public class StoreUpgradeManager : MonoBehaviour
{
    [Header("Upgrade Levels")]
    [SerializeField, Min(0)] private int shelfSizeLevel;
    [SerializeField, Min(0)] private int visibilityLevel;
    [SerializeField, Min(0)] private int bulkSalesLevel;

    [Header("Upgrade Costs (Level 1 to 5)")]
    [SerializeField] private int[] shelfSizeCosts = { 500, 1000, 2000, 3500, 5000 };
    [SerializeField] private int[] visibilityCosts = { 750, 1500, 2500, 4000, 6000 };
    [SerializeField] private int[] bulkSalesCosts = { 1000, 2000, 3500, 5000, 8000 };

    public int GetSlotCapacity() => 2 + shelfSizeLevel * 2;
    public float GetSaleChanceBonus() => visibilityLevel * 0.05f;
    public int GetSaleQuantity() => 1 + bulkSalesLevel;

    public bool PurchaseShelfSizeUpgrade(PlayerCurrency currency) =>
        PurchaseUpgrade(currency, shelfSizeCosts, ref shelfSizeLevel);

    public bool PurchaseVisibilityUpgrade(PlayerCurrency currency) =>
        PurchaseUpgrade(currency, visibilityCosts, ref visibilityLevel);

    public bool PurchaseBulkSalesUpgrade(PlayerCurrency currency) =>
        PurchaseUpgrade(currency, bulkSalesCosts, ref bulkSalesLevel);

    private bool PurchaseUpgrade(PlayerCurrency currency, int[] costs, ref int level)
    {
        if (currency == null || costs == null || level >= costs.Length) return false;
        if (!currency.SpendMoney(costs[level])) return false;
        level++;
        return true;
    }
}
