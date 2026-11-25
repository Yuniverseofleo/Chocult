using System.Collections;
using UnityEngine;

public class StoreSaleSimulator : MonoBehaviour
{
    [SerializeField] private HonestyStore store;
    [SerializeField, Min(1f)] private float saleCheckInterval = 10f;
    [SerializeField] private bool simulateWhenGameIsPaused;

    private Coroutine simulationRoutine;

    private void OnEnable()
    {
        simulationRoutine = StartCoroutine(SimulateSales());
    }

    private void OnDisable()
    {
        if (simulationRoutine != null) StopCoroutine(simulationRoutine);
    }

    private IEnumerator SimulateSales()
    {
        while (true)
        {
            if (simulateWhenGameIsPaused)
                yield return new WaitForSecondsRealtime(saleCheckInterval);
            else
                yield return new WaitForSeconds(saleCheckInterval);

            if (store != null) store.ProcessSalesTick();
        }
    }
}
