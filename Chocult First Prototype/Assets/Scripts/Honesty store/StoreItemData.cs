using UnityEngine;

[CreateAssetMenu(fileName = "StoreItemData", menuName = "Honesty Store/Store Item Data")]
public class StoreItemData : ScriptableObject
{
    [SerializeField] private ItemType itemType;
    [SerializeField, Min(1)] private int defaultPrice = 100;
    [SerializeField, Range(0f, 1f)] private float baseSaleChance = 0.5f;

    public ItemType ItemType => itemType;
    public int DefaultPrice => defaultPrice;
    public float BaseSaleChance => baseSaleChance;
}
