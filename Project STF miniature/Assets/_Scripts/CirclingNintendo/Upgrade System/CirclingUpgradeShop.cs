using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine.Rendering;
using System;
using System.Linq; // Added for ToList()

public class CirclingUpgradeShop : MonoBehaviour
{
    public SerializedDictionary<SerializedDictionary<int, CirclingUpgrade>, int> upgradesSlotsWithIndex;

    public List<CirclingUpgrade> upgradesInSlots;

    public static Action<List<CirclingUpgrade>> OnUpgradesInSlotsGenerated;

    void OnEnable()
    {
        CirclingUpgradeShopUI.OnUpgradeSold += OnUpgradeSold;
    }

    void OnDisable()
    {
        CirclingUpgradeShopUI.OnUpgradeSold -= OnUpgradeSold;
    }

    void Start()
    {
        GenerateUpgradesForSale();
    }

    public void GenerateUpgradesForSale()
    {
        upgradesInSlots = new List<CirclingUpgrade>();
        foreach(var slot in upgradesSlotsWithIndex)
        {
            if (slot.Value < slot.Key.Count + 1)
            {
                var _upgradeForSale = slot.Key[slot.Value];
                upgradesInSlots.Add(_upgradeForSale);
            }
        }

        OnUpgradesInSlotsGenerated?.Invoke(upgradesInSlots);
    }

    void OnUpgradeSold(CirclingUpgrade upgrade)
    {
        // Update the Index of the slot
        foreach(var slot in upgradesSlotsWithIndex.ToList()) // ToList() 避免修改正在迭代的集合
        {
            if(slot.Key.ContainsValue(upgrade))
            {
                upgradesSlotsWithIndex[slot.Key] = slot.Value + 1;
                GenerateUpgradesForSale();
            }
        }
    }
}

