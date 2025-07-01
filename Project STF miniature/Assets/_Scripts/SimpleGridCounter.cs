using UnityEngine; 
using System;

public class SimpleGridCounter : MonoBehaviour
{
    public int gridCount = 0;
    public static Action<int> OnGridCountChanged;

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
        }
    }
}
