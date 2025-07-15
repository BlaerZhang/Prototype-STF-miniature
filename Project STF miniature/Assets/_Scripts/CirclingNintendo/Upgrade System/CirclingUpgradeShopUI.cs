using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class CirclingUpgradeShopUI : MonoBehaviour
{
    public GameObject upgradeForSalePrefab;
    public Transform upgradeSlotsParent;
    public GameObject upgradeShopPanel;
    public static Action<CirclingUpgrade> OnUpgradeSold;

    void OnEnable()
    {
        CirclingUpgradeShop.OnUpgradesInSlotsGenerated += GenerateUpgradeSlotUI;
    }

    void OnDisable()
    {
        CirclingUpgradeShop.OnUpgradesInSlotsGenerated -= GenerateUpgradeSlotUI;
    }

    public void GenerateUpgradeSlotUI(List<CirclingUpgrade> upgradesForSale)
    {
        // Clear the upgradeSlotsParent
        foreach (Transform child in upgradeSlotsParent)
        {
            Destroy(child.gameObject);
        }

        foreach (var upgradeForSale in upgradesForSale)
        {
            if (upgradeForSale == null) continue;
            // Instantiate the upgradeForSalePrefab and set the upgradeForSale to the upgradeForSalePrefab
            var _upgradeSlot = Instantiate(upgradeForSalePrefab, upgradeSlotsParent);
            var itemTypeToPay = upgradeForSale.upgradeData.upgradeType switch
        {
            CirclingUpgradeType.AffairShop => CirclingItemType.AffairShopCoupon,
            CirclingUpgradeType.TradeShop => CirclingItemType.TradeShopCoupon,
            CirclingUpgradeType.LotteryShop => CirclingItemType.LotteryShopCoupon,
            CirclingUpgradeType.TrafficShop => CirclingItemType.TrafficShopCoupon,
        };
            // Set up the itemForSalePrefab
            _upgradeSlot.transform.Find("Price Text").GetComponent<TMP_Text>().text = upgradeForSale.UpgradeCost.ToString();
            _upgradeSlot.transform.Find("Price Text/Price Icon").GetComponent<Image>().sprite = CirclingResourceManager.Instance.GetItemSprite(itemTypeToPay);
            _upgradeSlot.transform.Find("Upgrade Name").GetComponent<TMP_Text>().text = upgradeForSale.UpgradeName;
            _upgradeSlot.transform.Find("Upgrade Description").GetComponent<TMP_Text>().text = upgradeForSale.UpgradeDescription;
            _upgradeSlot.GetComponent<Button>().onClick.AddListener(() => OnUpgradeSlotClicked(upgradeForSale, _upgradeSlot));
        }
    }

    void OnUpgradeSlotClicked(CirclingUpgrade upgradeForSale, GameObject upgradeSlot)
    {
        var itemTypeToPay = upgradeForSale.upgradeData.upgradeType switch
        {
            CirclingUpgradeType.AffairShop => CirclingItemType.AffairShopCoupon,
            CirclingUpgradeType.TradeShop => CirclingItemType.TradeShopCoupon,
            CirclingUpgradeType.LotteryShop => CirclingItemType.LotteryShopCoupon,
            CirclingUpgradeType.TrafficShop => CirclingItemType.TrafficShopCoupon,
        };

        if (CirclingResourceManager.Instance.TryPayItem(itemTypeToPay, upgradeForSale.UpgradeCost))
        {
            // Show the success message
            Debug.Log($"Bought {upgradeForSale.UpgradeName} for {upgradeForSale.UpgradeCost} coupons");

            // Call the OnUpgradeSold event
            OnUpgradeSold?.Invoke(upgradeForSale);

            // Play the buy sound
            AudioManager.Instance.PlaySound(AudioManager.Instance.soundClips["Buy"]);
        }
        else
        {
            // Show the error message
            Debug.LogError($"Not enough resources to buy {upgradeForSale.UpgradeName} for {upgradeForSale.UpgradeCost} coupons");
            
            // Flash the price text
            upgradeSlot.transform.Find("Price Text").GetComponent<TMP_Text>().DOColor(Color.red, 0.25f).SetEase(Ease.Flash, 4, 0.25f);
            upgradeSlot.GetComponent<Image>().DOColor(Color.red, 0.25f).SetEase(Ease.Flash, 4, 0.25f);
            upgradeSlot.transform.DOShakePosition(0.5f, 10, 10, 0, false, true);

            // Play the error sound
            AudioManager.Instance.PlaySound(AudioManager.Instance.soundClips["Error"]);
        }
    }

}