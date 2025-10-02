using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System;
using System.Linq;
using SpinWheel;

public class CirclingShop : MonoBehaviour
{
    private SpinWheelController spinWheelController;
    [SerializeField] private SpinWheelPrizePool mysteryBoxPrizePool;
    public List<SerializedDictionary<CirclingItemForSale, float>> itemsPools;
    public List<CirclingItemForSale> itemsInSlots;
    public static Action<List<CirclingItemForSale>> OnItemsForSaleGenerated;
    public static Action<CirclingShop> OnShopRefreshed;
    private CirclingShopUI _shopUI;

    void Start()
    {
        _shopUI = GetComponent<CirclingShopUI>();
        spinWheelController = FindObjectOfType<SpinWheelController>();
        GenerateItemsForSale();
    }

    void OnEnable()
    {
        UITimerText.OnShopRefreshingTime += GenerateItemsForSale;
        UITimerText.OnDayChanged += RefillShopRefresh;
        CirclingShopUI.OnMysteryBoxPurchased += DrawMysteryBox;
    }
    
    void OnDisable()
    {
        UITimerText.OnShopRefreshingTime -= GenerateItemsForSale;
        UITimerText.OnDayChanged -= RefillShopRefresh;
        CirclingShopUI.OnMysteryBoxPurchased -= DrawMysteryBox;
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

    public void DrawMysteryBox(CirclingShopUI shopUI)
    {
        if (shopUI == _shopUI)
        {
            spinWheelController.StartSpin(mysteryBoxPrizePool);
        }
    }

    void RefillShopRefresh()
    {
        // If the player has the upgrade "Shop Manual Refresh", refill the shop refresh with the level of the upgrade
        int _shopRefreshLevel = 0;
        if (CirclingUpgradeManager.Instance.upgrades.Any(upgrade => upgrade.UpgradeName == "Shop Manual Refresh"))
        {
            _shopRefreshLevel = CirclingUpgradeManager.Instance.upgrades.First(upgrade => upgrade.UpgradeName == "Shop Manual Refresh").upgradeLevel;
        }

        CirclingResourceManager.Instance.SetItemCount(CirclingItemType.ShopRefresh, _shopRefreshLevel);
    }

    public void ManualRefreshShop()
    {
        if (CirclingResourceManager.Instance.GetItemCount(CirclingItemType.ShopRefresh) > 0)
        {
            CirclingResourceManager.Instance.RemoveItem(CirclingItemType.ShopRefresh, 1);
            GenerateItemsForSale();
            OnShopRefreshed?.Invoke(this);
        }
    }
}
