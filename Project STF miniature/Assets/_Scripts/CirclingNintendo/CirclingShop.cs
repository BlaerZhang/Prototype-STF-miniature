using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System;

public class CirclingShop : MonoBehaviour
{
    public List<SerializedDictionary<CirclingItemForSale, float>> itemsPools;
    public List<CirclingItemForSale> itemsInSlots;
    public static Action<List<CirclingItemForSale>> OnItemsForSaleGenerated;
    private CirclingShopUI _shopUI;

    void Start()
    {
        _shopUI = GetComponent<CirclingShopUI>();
        GenerateItemsForSale();
    }

    void OnEnable()
    {
        UITimerText.OnShopRefreshingTime += GenerateItemsForSale;
    }
    
    void OnDisable()
    {
        UITimerText.OnShopRefreshingTime -= GenerateItemsForSale;
    }

    public void GenerateItemsForSale()
    {
        itemsInSlots = new List<CirclingItemForSale>();
        foreach (var pool in itemsPools)
        {
            var _itemForSale = GenerateItemFromPool(pool);
            if (_itemForSale != null)
            {
                itemsInSlots.Add(_itemForSale);
            }
        }

        OnItemsForSaleGenerated?.Invoke(itemsInSlots);
        _shopUI.GenerateItemSlotUI(itemsInSlots);
    }

    public CirclingItemForSale GenerateItemFromPool(SerializedDictionary<CirclingItemForSale, float> itemsPool)
    {
        // Based on the Weight per itemForSale, generate a random itemForSale
        float _totalWeight = 0f;
        foreach (var item in itemsPool)
        {
            _totalWeight += item.Value;
        }

        float _randomValue = UnityEngine.Random.Range(0f, _totalWeight);
        float _cumulativeWeight = 0f;
        foreach (var item in itemsPool)
        {
            _cumulativeWeight += item.Value; // Add the weight to the cumulative weight
            if (_randomValue <= _cumulativeWeight) // If the random value is less than the cumulative weight, return the itemForSale
            {
                return item.Key;
            }
        }

        return null;
    }
}
