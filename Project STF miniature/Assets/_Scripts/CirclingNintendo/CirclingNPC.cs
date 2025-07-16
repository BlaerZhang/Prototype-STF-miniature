using UnityEngine;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using UnityEngine.UI;

public class CirclingNPC : MonoBehaviour
{
    public string npcName;
    public bool isInFruitQuest = false;
    public bool isInDeliveryQuest = false;
    public bool IsInQuest => isInFruitQuest || isInDeliveryQuest;

    [Header("Quest UI")]
    public List<Image> fruitRequirementsIconUIs;
    public Image questStatusIconUI;
    public Sprite questStatusIconSprite_InFruitQuest;
    public Sprite questStatusIconSprite_FruitQuestSubmittable;
    public Sprite questStatusIconSprite_InDeliveryQuest;
    public Sprite questStatusIconSprite_NewQuest;
    
    [Header("Fruit Quest")]
    [SerializeField] [Range(0, 1)] private float fruitQuestProbability = 0.5f;
    private float DeliveryQuestProbability => 1 - fruitQuestProbability;
    public bool isFruitQuestRequiredItemsRandom = true;
    public CirclingItemType fruitQuestRequiredFixedItem1 = CirclingItemType.Watermelon;
    public CirclingItemType fruitQuestRequiredFixedItem2 = CirclingItemType.Apple;
    private Dictionary<CirclingItemType, int> fruitQuestRequiredItems;
    public int fruitQuestCouponRewardCount = 2;

    [Header("Delivery Quest")]
    public int deliveryQuestCouponRewardCount = 1;
    public static Action<string, Sprite, int> OnDeliveryQuestGenerated;
    
    void OnEnable()
    {
        CirclingDeliverySubmitArea.OnDelivered += CompleteDeliveryQuest;
        CirclingResourceManager.OnItemCountChanged += OnItemCountChanged;
    }
    
    void OnDisable()
    {
        CirclingDeliverySubmitArea.OnDelivered -= CompleteDeliveryQuest;
        CirclingResourceManager.OnItemCountChanged -= OnItemCountChanged;
    }

    void Start()
    {
        // Reset Quest Icon
        fruitRequirementsIconUIs[0].gameObject.SetActive(!isFruitQuestRequiredItemsRandom);
        fruitRequirementsIconUIs[1].gameObject.SetActive(!isFruitQuestRequiredItemsRandom);
        fruitRequirementsIconUIs[0].sprite = CirclingResourceManager.Instance.GetItemSprite(fruitQuestRequiredFixedItem1);
        fruitRequirementsIconUIs[1].sprite = CirclingResourceManager.Instance.GetItemSprite(fruitQuestRequiredFixedItem2);
        questStatusIconUI.sprite = questStatusIconSprite_NewQuest;
    }
    

    public void GenerateRandomQuest()
    {
        if (IsInQuest) return;

        if (Random.Range(0f, 1f) < fruitQuestProbability)
        {
            GenerateFruitQuest();
        }
        else
        {
            GenerateDeliveryQuest();
        }
    }

    private void GenerateFruitQuest()
    {
        isInFruitQuest = true;
        fruitQuestRequiredItems = new Dictionary<CirclingItemType, int>();
        int fruitType1;
        int fruitType2;
        
         if (isFruitQuestRequiredItemsRandom)
        {
            // Generate 2 types of fruits(index 0-5), each with 1 quantity
            fruitType1 = Random.Range(0, 5);
            fruitType2 = Random.Range(0, 5);
            while (fruitType1 == fruitType2)
            {
                fruitType2 = Random.Range(0, 5);
            }
            fruitQuestRequiredItems.Add((CirclingItemType)fruitType1, 1);
            fruitQuestRequiredItems.Add((CirclingItemType)fruitType2, 1);
        }
        else
        {
            fruitType1 = (int)fruitQuestRequiredFixedItem1;
            fruitType2 = (int)fruitQuestRequiredFixedItem2;
            fruitQuestRequiredItems.Add((CirclingItemType)fruitType1, 1);
            fruitQuestRequiredItems.Add((CirclingItemType)fruitType2, 1);
        }

        // Update Quest Icon
        fruitRequirementsIconUIs[0].gameObject.SetActive(true);
        fruitRequirementsIconUIs[0].sprite = CirclingResourceManager.Instance.GetItemSprite((CirclingItemType)fruitType1);
        fruitRequirementsIconUIs[1].gameObject.SetActive(true);
        fruitRequirementsIconUIs[1].sprite = CirclingResourceManager.Instance.GetItemSprite((CirclingItemType)fruitType2);
        questStatusIconUI.sprite = questStatusIconSprite_InFruitQuest;
    }

    private void GenerateDeliveryQuest()
    {
        isInDeliveryQuest = true;
        int randomDeliveryQuestIndex = Random.Range(0, 3);
        OnDeliveryQuestGenerated?.Invoke(npcName, GetComponent<Image>().sprite, randomDeliveryQuestIndex);

        // Update Quest Icon
        questStatusIconUI.sprite = questStatusIconSprite_InDeliveryQuest;
    }

    public bool CheckFruitQuestSubmittable()
    {
        if (!isInFruitQuest) return false;

        foreach (var item in fruitQuestRequiredItems)
        {
            if (CirclingResourceManager.Instance.GetItemCount(item.Key) < item.Value) 
            {
                return false;
            }
        }
        return true;
    }

    private void OnItemCountChanged(CirclingItemType itemType, int itemCount, int changedCount)
    {
        if (isInFruitQuest)
        {
            questStatusIconUI.sprite =  CheckFruitQuestSubmittable() ? 
            questStatusIconSprite_FruitQuestSubmittable : questStatusIconSprite_InFruitQuest;
        }
    }

    private void TryCompleteFruitQuest()
    {
        if (!isInFruitQuest) return;
        
        //Check if the player has all the required items
        if (!CheckFruitQuestSubmittable()) return;

        //Remove the items from the player's inventory
        foreach (var item in fruitQuestRequiredItems)
        {
            CirclingResourceManager.Instance.RemoveItem(item.Key, item.Value);
        }
        //Complete the quest
        CompleteQuest(fruitQuestCouponRewardCount);
    }

    private void CompleteDeliveryQuest(string npcName)
    {
        if (npcName != this.npcName) return;
        CompleteQuest(deliveryQuestCouponRewardCount);
    }

    public void CompleteQuest(int couponRewardCount = 1)
    {
        // Reset quest status
        isInFruitQuest = false;
        isInDeliveryQuest = false;

        // Reset Quest Icon
        fruitRequirementsIconUIs[0].gameObject.SetActive(!isFruitQuestRequiredItemsRandom);
        fruitRequirementsIconUIs[1].gameObject.SetActive(!isFruitQuestRequiredItemsRandom);
        questStatusIconUI.sprite = questStatusIconSprite_NewQuest;

        // Coupon Reward
        CirclingResourceManager.Instance.AddItem(CirclingItemType.Coupon, couponRewardCount);
    }

    public void OnNPCGridClicked()
    {
        if (isInFruitQuest) TryCompleteFruitQuest();
        else if (!IsInQuest) GenerateRandomQuest();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player")) TryCompleteFruitQuest();
    }   

}