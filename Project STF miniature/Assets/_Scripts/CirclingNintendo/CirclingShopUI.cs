using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class CirclingShopUI : MonoBehaviour
{
    public GameObject itemForSalePrefab;
    public Transform itemSlotsParent;

    public void GenerateItemSlotUI(List<CirclingItemForSale> itemsForSale)
    {
        // Clear the itemSlotsParent
        foreach (Transform child in itemSlotsParent)
        {
            Destroy(child.gameObject);
        }

        foreach (var itemForSale in itemsForSale)
        {
            // Instantiate the itemForSalePrefab and set the itemForSale to the itemForSalePrefab
            var _itemSlot = Instantiate(itemForSalePrefab, itemSlotsParent);

            // Set up the itemForSalePrefab
            _itemSlot.transform.Find("Price Text").GetComponent<TMP_Text>().text = itemForSale.price.ToString();
            _itemSlot.transform.Find("Item Icon/Item Quantity Text").GetComponent<TMP_Text>().text = $"x{itemForSale.quantity}";
            _itemSlot.transform.Find("Item Icon").GetComponent<Image>().sprite = CirclingResourceManager.Instance.GetItemSprite(itemForSale.itemType);
            _itemSlot.GetComponent<Button>().onClick.AddListener(() => OnItemSlotClicked(itemForSale, _itemSlot));
        }
    }

    void OnItemSlotClicked(CirclingItemForSale itemForSale, GameObject itemSlot)
    {
        if (CirclingResourceManager.Instance.TryBuyItem(itemForSale.itemType, itemForSale.quantity, itemForSale.price))
        {
            // Show the success message
            Debug.Log($"Bought {itemForSale.itemType} x{itemForSale.quantity} for {itemForSale.price} coupons");

            // Disable the itemSlot
            if (itemForSale.itemType == CirclingItemType.MysteryBox) return;
            itemSlot.GetComponent<Button>().interactable = false;
            itemSlot.transform.Find("Price Text").GetComponent<TMP_Text>().text = "SOLD";
        }
        else
        {
            // Show the error message
            Debug.LogError($"Not enough resources to buy {itemForSale.itemType} x{itemForSale.quantity} for {itemForSale.price} coupons");
            
            // Flash the price text
            itemSlot.transform.Find("Price Text").GetComponent<TMP_Text>().DOColor(Color.red, 0.5f).SetEase(Ease.Flash, 4, 0.5f);
            itemSlot.transform.DOShakePosition(0.5f, 10, 10, 0, false, true);
        }
    }

}