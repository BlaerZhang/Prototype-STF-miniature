using Unity.VisualScripting;
using UnityEngine;
using System;   
using UnityEngine.UI;

public class CirclingDeliverySubmitArea : MonoBehaviour
{
    private bool isInDeliveryQuest = false;
    private string currentNpcName;
    private Sprite npcSprite;
    public Image questIcon;
    public static Action<string> OnDelivered;

    // 由管理器调用的方法（不再需要index）
    public void HandleDeliveryQuest(string _npcName, Sprite _npcSprite)
    {
        isInDeliveryQuest = true;
        currentNpcName = _npcName;
        npcSprite = _npcSprite;
        questIcon.gameObject.SetActive(true);
        questIcon.sprite = npcSprite;
    }

    // 供管理器查询当前是否处理指定NPC的任务
    public bool IsCurrentlyHandling(string npcName)
    {
        return isInDeliveryQuest && currentNpcName == npcName;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player") && isInDeliveryQuest) 
        {
            CompleteDeliveryQuest();
        }
    }

    void CompleteDeliveryQuest()
    {
        OnDelivered?.Invoke(currentNpcName);

        // Reset Grid UI
        questIcon.gameObject.SetActive(false);

        // Reset Delivery Quest
        currentNpcName = null;
        npcSprite = null;
        isInDeliveryQuest = false;
    }
}
