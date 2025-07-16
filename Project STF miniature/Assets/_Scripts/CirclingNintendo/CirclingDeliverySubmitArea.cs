using Unity.VisualScripting;
using UnityEngine;
using System;   
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

[System.Serializable]
public class DeliveryQuest
{
    public string senderNpcName;
    public Sprite npcSprite;
    
    public DeliveryQuest(string senderName, Sprite sprite)
    {
        senderNpcName = senderName;
        npcSprite = sprite;
    }
}

public class CirclingDeliverySubmitArea : MonoBehaviour
{
    [Header("NPC绑定")]
    [SerializeField] private string boundNPCName; // 该送货区绑定的NPC名称
    
    [Header("UI组件")]
    public Image questIcon;
    public Transform questIconContainer; // 用于显示多个任务图标的容器
    
    [Header("UI循环设置")]
    [SerializeField] private float iconCycleInterval = 0.5f; // 头像切换间隔
    
    [Header("任务队列")]
    [SerializeField] private List<DeliveryQuest> deliveryQuests = new List<DeliveryQuest>();
    
    [Header("Debug")]
    public bool isDebug = false;
    
    // 事件：完成送货时触发 (送货的NPC名称, 目标NPC名称)
    public static Action<string, string> OnDelivered;
    
    private Coroutine iconCycleCoroutine;
    private int currentIconIndex = 0;

    void Start()
    {
        UpdateQuestDisplay();
    }

    // 添加新的送货任务
    public void AddDeliveryQuest(string senderNpcName, Sprite npcSprite)
    {
        DeliveryQuest newQuest = new DeliveryQuest(senderNpcName, npcSprite);
        deliveryQuests.Add(newQuest);
        UpdateQuestDisplay();
        
        Debug.Log($"添加送货任务: {senderNpcName} -> {boundNPCName} (总任务数: {deliveryQuests.Count})");
    }

    // 更新任务显示
    void UpdateQuestDisplay()
    {
        if (deliveryQuests.Count > 0)
        {
            questIcon.gameObject.SetActive(true);
            
            // 停止之前的循环协程
            if (iconCycleCoroutine != null)
            {
                StopCoroutine(iconCycleCoroutine);
            }
            
            // 如果只有一个任务，直接显示
            if (deliveryQuests.Count == 1)
            {
                questIcon.sprite = deliveryQuests[0].npcSprite;
            }
            else
            {
                // 多个任务时开始循环显示
                iconCycleCoroutine = StartCoroutine(CycleIcons());
            }
        }
        else
        {
            questIcon.gameObject.SetActive(false);
            
            // 停止循环协程
            if (iconCycleCoroutine != null)
            {
                StopCoroutine(iconCycleCoroutine);
                iconCycleCoroutine = null;
            }
        }
    }

    // 循环显示头像的协程
    IEnumerator CycleIcons()
    {
        currentIconIndex = 0;
        
        while (deliveryQuests.Count > 0)
        {
            if (currentIconIndex >= deliveryQuests.Count)
            {
                currentIconIndex = 0;
            }
            
            questIcon.sprite = deliveryQuests[currentIconIndex].npcSprite;
            currentIconIndex++;
            
            yield return new WaitForSeconds(iconCycleInterval);
        }
    }

    // 获取绑定的NPC名称
    public string GetBoundNPCName()
    {
        return boundNPCName;
    }

    // 获取当前任务数量
    public int GetQuestCount()
    {
        return deliveryQuests.Count;
    }

    // 检查是否有来自指定NPC的任务
    public bool HasQuestFromNPC(string npcName)
    {
        return deliveryQuests.Any(quest => quest.senderNpcName == npcName);
    }

    // 设置绑定的NPC（用于运行时配置）
    public void SetBoundNPC(string npcName)
    {
        boundNPCName = npcName;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player") && deliveryQuests.Count > 0) 
        {
            CompleteAllDeliveryQuests();
        }
    }

    void CompleteAllDeliveryQuests()
    {
        if (deliveryQuests.Count > 0)
        {
            // 收集所有发送者的名称
            List<string> senderNames = deliveryQuests.Select(quest => quest.senderNpcName).ToList();
            
            Debug.Log($"完成所有送货任务: [{string.Join(", ", senderNames)}] -> {boundNPCName} (共{deliveryQuests.Count}个任务)");
            
            // 为每个任务循环触发事件
            foreach (string senderName in senderNames)
            {
                OnDelivered?.Invoke(senderName, boundNPCName);
                Debug.Log($"  - 触发事件: {senderName} -> {boundNPCName}");
            }
            
            // 清空所有任务
            deliveryQuests.Clear();
            
            // 更新显示
            UpdateQuestDisplay();
        }
    }

    // 清空所有任务（用于重置或调试）
    public void ClearAllQuests()
    {
        deliveryQuests.Clear();
        UpdateQuestDisplay();
    }

    void OnDestroy()
    {
        // 确保协程在对象销毁时停止
        if (iconCycleCoroutine != null)
        {
            StopCoroutine(iconCycleCoroutine);
        }
    }

    // 调试信息
    void OnGUI()
    {
        if (deliveryQuests.Count > 0 && isDebug)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
            GUI.Label(new Rect(screenPos.x, Screen.height - screenPos.y, 100, 20), $"任务: {deliveryQuests.Count}");
            
            // 显示当前循环的头像索引
            if (deliveryQuests.Count > 1)
            {
                GUI.Label(new Rect(screenPos.x, Screen.height - screenPos.y + 20, 100, 20), $"显示: {currentIconIndex}/{deliveryQuests.Count}");
            }
        }
    }
}
