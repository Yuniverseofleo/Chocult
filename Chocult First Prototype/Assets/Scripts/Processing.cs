
using UnityEngine;
using System.Collections;

public class Processing : MonoBehaviour, IInteractable
{
    [Header("Station Settings")]
    [SerializeField] private ItemType inputItem;
    [SerializeField] private ItemType outputItem;
    [SerializeField] private float processingTime = 3.5f;
    [SerializeField] private bool plantingstation = false;
    [SerializeField] private ItemType processingitem;
    [SerializeField] private ItemType midprocessingitem;

    private bool isProcessing = false;
    private bool processingItemAdded = false;
    private ItemType? currentItem = null;

    private PlayerInventory playerInventory;

    [System.Obsolete]
    void Start()
    {
        playerInventory = FindObjectOfType<PlayerInventory>();
    }

    public void Interact()
    {
        if (playerInventory == null)
            return;

        // If currently processing, do nothing
        if (isProcessing)
        {
            Debug.Log("Still processing...");
            return;
        }

        // Station is empty
        if (currentItem == null)
        {
            if (playerInventory.HasItem(inputItem))
            {
                playerInventory.RemoveItem(inputItem);
                currentItem = inputItem;

                // Normal stations start immediately
                if (!plantingstation)
                {
                    StartCoroutine(ProcessItem());
                }
                else
                {
                    Debug.Log("You need to add " + processingitem + ".");
                }
            }
            else
            {
                Debug.Log("You need a " + inputItem + " to use this station.");
            }
        }

        // Planting station is waiting for its processing item
        else if (plantingstation &&
                 currentItem == inputItem &&
                 !processingItemAdded)
        {
            if (playerInventory.HasItem(processingitem))
            {
                playerInventory.RemoveItem(processingitem);
                processingItemAdded = true;

                StartCoroutine(ProcessItem());
            }
            else
            {
                Debug.Log("You need a " + processingitem + " to use this station.");
            }
        }

        // Output is ready to collect
        else if (currentItem == outputItem)
        {
            bool added = playerInventory.AddItem(outputItem);

            if (added)
            {
                Debug.Log("Collected " + outputItem);
                currentItem = null;
                processingItemAdded = false;
            }
            else
            {
                Debug.Log("Inventory full! Cannot collect " + outputItem);
            }
        }
    }

    private IEnumerator ProcessItem()
    {
        isProcessing = true;
        Debug.Log("Processing " + inputItem + "...");

        yield return new WaitForSeconds(processingTime);

        currentItem = outputItem;
        isProcessing = false;

        Debug.Log("Finished " + outputItem + "!");
    }
}
