using UnityEngine;

// Basic currency component for this prototype.
// If your project already has a currency system, replace this reference with that system.
public class PlayerCurrency : MonoBehaviour
{
    [SerializeField, Min(0)] private int balance;
    public int Balance => balance;

    public void AddMoney(int amount)
    {
        if (amount > 0) balance += amount;
    }

    public bool SpendMoney(int amount)
    {
        if (amount < 0 || balance < amount) return false;
        balance -= amount;
        return true;
    }
}
