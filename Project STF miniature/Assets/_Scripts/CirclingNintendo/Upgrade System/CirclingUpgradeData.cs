using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CirclingUpgradeData", menuName = "Scriptable Objects/CirclingUpgradeData")]
public class CirclingUpgradeData : ScriptableObject
{
    public string upgradeName;
    public string upgradeDescription;
    public CirclingUpgradeType upgradeType;
    public int upgradeMaxLevel;
    public List<int> upgradeCostPerLevel;
}

public enum CirclingUpgradeType
{
    AffairShop,
    TradeShop,
    LotteryShop,
    TrafficShop,
}
