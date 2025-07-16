using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.Linq;
using System;

public class CirclingUpgradeManager : MonoBehaviour
{
    public List<CirclingUpgrade> upgrades;
    public TMP_Text upgradeDisplayUI;

    public static event Action<CirclingUpgrade> OnUpgradeAdded;

    void OnEnable()
    {
        CirclingUpgradeShopUI.OnUpgradeSold += AddUpgrade;
    }

    void OnDisable()
    {
        CirclingUpgradeShopUI.OnUpgradeSold -= AddUpgrade;
    }

    public void AddUpgrade(CirclingUpgrade upgrade)
    {
        // Check if the upgrade is already in the list by comparing upgradeData
        var existingUpgrade = upgrades.FirstOrDefault(u => u.upgradeData == upgrade.upgradeData);
        
        if (existingUpgrade != null)
        {
            // If the upgrade is already in the list, increase the level of the existing upgrade
            existingUpgrade.upgradeLevel++;
        }
        else
        {
            // If the upgrade is not in the list, add it
            upgrades.Add(upgrade);
        }
        UpdateUpgradeDisplay();
        OnUpgradeAdded?.Invoke(upgrade);
    }

    void UpdateUpgradeDisplay()
    {
        string upgradeDisplayText = "<size=24><u>Upgrades</u></size>\n\n";
        foreach(var upgrade in upgrades)
        {
            upgradeDisplayText += $"{upgrade.upgradeData.upgradeName} - Level {upgrade.upgradeLevel}\n";
        }
        upgradeDisplayUI.text = upgradeDisplayText;
    }

    void Start()
    {
        UpdateUpgradeDisplay();
    }
}