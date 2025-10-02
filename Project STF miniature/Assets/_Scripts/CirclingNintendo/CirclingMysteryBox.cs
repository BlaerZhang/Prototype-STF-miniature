using UnityEngine;
using SpinWheel;

public class CirclingMysteryBox : MonoBehaviour
{
    private SpinWheelController spinWheelController;
    [SerializeField] private SpinWheelPrizePool prizePool;
    
    void Start()
    {
        spinWheelController = FindObjectOfType<SpinWheelController>();
    }

    public void Draw()
    {
        spinWheelController.StartSpin(prizePool);
    }
}
