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
    public List<Image> questIcons;
    public Sprite deliveryQuestIcon;
    
    [Header("Fruit Quest")]
    [SerializeField] [Range(0, 1)] private float fruitQuestProbability = 0.5f;
    private float DeliveryQuestProbability => 1 - fruitQuestProbability;
    private Dictionary<CirclingItemType, int> fruitQuestRequiredItems;

    public static Action<string, Sprite, int> OnDeliveryQuestGenerated;
    
    void OnEnable()
    {
        CirclingDeliverySubmitArea.OnDelivered += CompleteDeliveryQuest;
    }
    
    void OnDisable()
    {
        CirclingDeliverySubmitArea.OnDelivered -= CompleteDeliveryQuest;
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
        // Generate 2 types of fruits(index 0-5), each with 1 quantity
        fruitQuestRequiredItems = new Dictionary<CirclingItemType, int>();
        int fruitType1 = Random.Range(0, 5);
        int fruitType2 = Random.Range(0, 5);
        while (fruitType1 == fruitType2)
        {
            fruitType2 = Random.Range(0, 5);
        }
        fruitQuestRequiredItems.Add((CirclingItemType)fruitType1, 1);
        fruitQuestRequiredItems.Add((CirclingItemType)fruitType2, 1);

        // Update Quest Icon
        questIcons[0].gameObject.SetActive(true);
        questIcons[0].sprite = CirclingResourceManager.Instance.GetItemSprite((CirclingItemType)fruitType1);
        questIcons[1].gameObject.SetActive(true);
        questIcons[1].sprite = CirclingResourceManager.Instance.GetItemSprite((CirclingItemType)fruitType2);
    }

    private void GenerateDeliveryQuest()
    {
        isInDeliveryQuest = true;
        int randomDeliveryQuestIndex = Random.Range(0, 3);
        OnDeliveryQuestGenerated?.Invoke(npcName, GetComponent<Image>().sprite, randomDeliveryQuestIndex);

        // Update Quest Icon
        questIcons[0].gameObject.SetActive(true);
        questIcons[0].sprite = deliveryQuestIcon;
        questIcons[1].gameObject.SetActive(false);
    }

    private void TryCompleteFruitQuest()
    {
        if (!isInFruitQuest) return;
        
        //Check if the player has all the required items
        foreach (var item in fruitQuestRequiredItems)
        {
            if (CirclingResourceManager.Instance.GetItemCount(item.Key) < item.Value) 
            {
                Debug.Log($"Player does not have all the required items: {item.Key} x {item.Value}");
                return;
            }
        }
        //Remove the items from the player's inventory
        foreach (var item in fruitQuestRequiredItems)
        {
            CirclingResourceManager.Instance.RemoveItem(item.Key, item.Value);
        }
        //Complete the quest
        CompleteQuest();
    }

    private void CompleteDeliveryQuest(string npcName)
    {
        if (npcName != this.npcName) return;
        CompleteQuest();
    }

    public void CompleteQuest()
    {
        // Reset quest status
        isInFruitQuest = false;
        isInDeliveryQuest = false;

        // Reset Quest Icon
        questIcons[0].gameObject.SetActive(false);
        questIcons[1].gameObject.SetActive(false);

        // Play Particle Effect

        // Coupon Reward
        CirclingResourceManager.Instance.AddItem(CirclingItemType.Coupon, 1);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player")) TryCompleteFruitQuest();
    }   

}