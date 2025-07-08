using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using System;
using UnityEngine.Rendering;

public class CirclingResourceManager : MonoBehaviour
{
    [ShowInInspector]
    public Dictionary<CirclingItemType, int> itemCount = new()
    {
        {CirclingItemType.Watermelon, 0},
        {CirclingItemType.Strawberry, 0},
        {CirclingItemType.Grape, 0},
        {CirclingItemType.Apple, 0},
        {CirclingItemType.Banana, 0},
        {CirclingItemType.Mango, 0},
        {CirclingItemType.Coupon, 0},
        {CirclingItemType.Red, 0},
        {CirclingItemType.Yellow, 0},
        {CirclingItemType.Blue, 0},
        {CirclingItemType.Green, 0},
        {CirclingItemType.Orange, 0},
        {CirclingItemType.Purple, 0},
        {CirclingItemType.FireFlower, 0},
    };

    public SerializedDictionary<CirclingItemType, Sprite> itemSprites;

    public static CirclingResourceManager Instance;

    /// <param name="itemType">物品类型</param>
    /// <param name="count">物品数量</param>
    /// <param name="changedCount">变化数量，正数表示增加，负数表示减少</param>
    public static Action<CirclingItemType, int, int> OnItemCountChanged;

    public void AddItem(CirclingItemType itemType, int count)
    {
        itemCount[itemType] = Mathf.Max(itemCount[itemType] + count, 0);
        OnItemCountChanged?.Invoke(itemType, itemCount[itemType], count);
    }

    public void RemoveItem(CirclingItemType itemType, int count)
    {
        itemCount[itemType] = Mathf.Max(itemCount[itemType] - count, 0);
        OnItemCountChanged?.Invoke(itemType, itemCount[itemType], -count);
    }

    public void AddOneItem(int itemTypeIndex)
    {
        AddItem((CirclingItemType)itemTypeIndex, 1);
    }

    public void RemoveOneItem(int itemTypeIndex)
    {
        RemoveItem((CirclingItemType)itemTypeIndex, 1);
    }

    public int GetItemCount(CirclingItemType itemType)
    {
        return itemCount[itemType];
    }

    public Sprite GetItemSprite(CirclingItemType itemType)
    {
        return itemSprites[itemType];
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    void Start()
    {
        foreach (var item in itemCount)
        {
            OnItemCountChanged?.Invoke(item.Key, item.Value, 0);
        }
    }
}
