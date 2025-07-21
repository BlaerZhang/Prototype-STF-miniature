using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class CirclingShopUI : MonoBehaviour
{
    public GameObject itemForSalePrefab;
    public Transform itemSlotsParent;
    public Button shopRefreshButton;
    public Dictionary<CirclingItemForSale, GameObject> currentItemsForSaleAndSlots;

    [Header("Upgrade Related")]
    public bool isRainbowWhiteBallUnlocked = false;

    void OnEnable()
    {
        CirclingUpgradeManager.OnUpgradeAdded += OnUpgradeAdded;
        CirclingResourceManager.OnItemCountChanged += UpdateRefreshButton;
    }
    
    void OnDisable()
    {
        CirclingUpgradeManager.OnUpgradeAdded -= OnUpgradeAdded;
        CirclingResourceManager.OnItemCountChanged -= UpdateRefreshButton;
    }

    public void GenerateItemSlotUI(List<CirclingItemForSale> itemsForSale)
    {
        // Clear the itemSlotsParent
        foreach (Transform child in itemSlotsParent)
        {
            Destroy(child.gameObject);
        }

        currentItemsForSaleAndSlots = new Dictionary<CirclingItemForSale, GameObject>();

        foreach (var itemForSale in itemsForSale)
        {
            // Instantiate the itemForSalePrefab and set the itemForSale to the itemForSalePrefab
            var _itemSlot = Instantiate(itemForSalePrefab, itemSlotsParent);

            // Set up the itemForSalePrefab
            _itemSlot.transform.Find("Price Text").GetComponent<TMP_Text>().text = itemForSale.price.ToString();
            _itemSlot.transform.Find("Item Icon/Item Quantity Text").GetComponent<TMP_Text>().text = $"x{itemForSale.quantity}";
            _itemSlot.transform.Find("Item Icon").GetComponent<Image>().sprite = CirclingResourceManager.Instance.GetItemSprite(itemForSale.itemType);
            _itemSlot.GetComponent<Button>().onClick.AddListener(() => OnItemSlotClicked(itemForSale, _itemSlot));

            // If white and rainbow ball are not unlocked, disable the itemSlot if the item is a white or rainbow ball
            if (!isRainbowWhiteBallUnlocked && (itemForSale.itemType == CirclingItemType.Black || itemForSale.itemType == CirclingItemType.Rainbow))
            {
                _itemSlot.transform.Find("Price Text").GetComponent<TMP_Text>().text = "LOCKED";
                _itemSlot.GetComponent<Button>().interactable = false;
            }

            currentItemsForSaleAndSlots.Add(itemForSale, _itemSlot);
        }
    }

    void OnItemSlotClicked(CirclingItemForSale itemForSale, GameObject itemSlot)
    {
        if (CirclingResourceManager.Instance.TryBuyItem(itemForSale.itemType, itemForSale.quantity, CirclingItemType.Coupon, itemForSale.price))
        {
            // Show the success message
            Debug.Log($"Bought {itemForSale.itemType} x{itemForSale.quantity} for {itemForSale.price} coupons");

            // Play the buy sound
            AudioManager.Instance.PlaySound(AudioManager.Instance.soundClips["Buy"]);

            // If the item is a mystery box for 3 coupons, don't disable the itemSlot
            if (itemForSale.itemType == CirclingItemType.MysteryBox && itemForSale.price == 3) return;

            // Disable the itemSlot
            itemSlot.GetComponent<Button>().interactable = false;
            itemSlot.transform.Find("Price Text").GetComponent<TMP_Text>().text = "SOLD";
        }
        else
        {
            // Show the error message
            Debug.Log($"Not enough resources to buy {itemForSale.itemType} x{itemForSale.quantity} for {itemForSale.price} coupons");
            
            // Flash the price text
            itemSlot.transform.Find("Price Text").GetComponent<TMP_Text>().DOColor(Color.red, 0.25f).SetEase(Ease.Flash, 4, 0.25f);
            itemSlot.GetComponent<Image>().DOColor(Color.red, 0.25f).SetEase(Ease.Flash, 4, 0.25f);
            itemSlot.transform.DOShakePosition(0.5f, 10, 10, 0, false, true);

            // Play the error sound
            AudioManager.Instance.PlaySound(AudioManager.Instance.soundClips["Error"]);
        }
    }

    void OnUpgradeAdded(CirclingUpgrade upgrade)
    {
        if (upgrade.UpgradeName != "Unlock Rainbow / White Ball") return;
        isRainbowWhiteBallUnlocked = true;

        foreach (var itemForSale in currentItemsForSaleAndSlots.Keys)
        {
            if (itemForSale.itemType == CirclingItemType.Black || itemForSale.itemType == CirclingItemType.Rainbow)
            {
                currentItemsForSaleAndSlots[itemForSale].GetComponent<Button>().interactable = true;
                currentItemsForSaleAndSlots[itemForSale].transform.Find("Price Text").GetComponent<TMP_Text>().text = itemForSale.price.ToString();
            }
        }
    }

    void UpdateRefreshButton(CirclingItemType itemType, int itemCount, int changeCount)
    {
        //Update the shop refresh button
        if (itemType != CirclingItemType.ShopRefresh) return;
        shopRefreshButton.interactable = itemCount > 0;
    }
}  