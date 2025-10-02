using UnityEngine;
using SpinWheel;

[RequireComponent(typeof(SpinWheelController))]
public class CirclingLotteryManager : MonoBehaviour
{
    [SerializeField] private SpinWheelController spinWheelController;

    void OnEnable()
    {
        SpinWheelController.OnSpinComplete += OnSpinComplete;
        spinWheelController = GetComponent<SpinWheelController>();
    }

    void OnDisable()
    {
        SpinWheelController.OnSpinComplete -= OnSpinComplete;
    }

    void OnSpinComplete(PrizeItem prize)
    {
        if (prize.prizeContent.Count == 0) return;

        foreach (var item in prize.prizeContent)
        {
            CirclingResourceManager.Instance.AddItem(item.itemType, item.quantity);
        }
    }
}