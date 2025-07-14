using System;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable]
public class CirclingItemForSale
{
    [Tooltip("The item type for sale")]
    public CirclingItemType itemType;
    [Tooltip("The quantity of the item for sale")]
    public int quantity;
    [Tooltip("The price of the item for sale")]
    public int price;
}
