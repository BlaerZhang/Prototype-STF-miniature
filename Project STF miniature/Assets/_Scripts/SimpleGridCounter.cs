using UnityEngine; 
using System;

public class SimpleGridCounter : MonoBehaviour
{
    public int gridCount = 0;
    public static Action<int> OnGridCountChanged;
    public static Action<int> OnTimeSpentforGrid;

    void Start()
    {
        
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Fog"))
        {
            Debug.Log("Fog entered");
            gridCount++;
            OnGridCountChanged?.Invoke(gridCount);
            OnTimeSpentforGrid?.Invoke(other.GetComponent<SimpleRequirementsGenerator>().timeCost);
        }
    }
}
