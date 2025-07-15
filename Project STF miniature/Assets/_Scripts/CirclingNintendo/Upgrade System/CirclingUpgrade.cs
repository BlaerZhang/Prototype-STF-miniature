using UnityEngine;

[System.Serializable]
public class CirclingUpgrade
{
    public CirclingUpgradeData upgradeData;
    public int upgradeLevel;
    public string UpgradeName => upgradeData.upgradeName;
    public string UpgradeDescription => upgradeData.upgradeDescription;
    public int UpgradeCost => upgradeData.upgradeCostPerLevel[upgradeLevel - 1];

    public CirclingUpgrade(CirclingUpgradeData upgradeData)
    {
        this.upgradeData = upgradeData;
        this.upgradeLevel = 1;
    }
}
