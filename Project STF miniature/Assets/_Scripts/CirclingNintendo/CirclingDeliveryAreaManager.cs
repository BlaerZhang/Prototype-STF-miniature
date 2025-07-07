using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CirclingDeliveryAreaManager : MonoBehaviour
{
    public static CirclingDeliveryAreaManager Instance { get; private set; }
    
    [Header("配置")]
    [SerializeField] private List<CirclingDeliverySubmitArea> deliveryAreas = new List<CirclingDeliverySubmitArea>();
    
    private Dictionary<CirclingDeliverySubmitArea, bool> areaAvailability = new Dictionary<CirclingDeliverySubmitArea, bool>();

    void Awake()
    {
        // 单例模式
        if (Instance == null)
        {
            Instance = this;
            InitializeAreas();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        CirclingNPC.OnDeliveryQuestGenerated += HandleDeliveryQuestRequest;
        CirclingDeliverySubmitArea.OnDelivered += HandleDeliveryCompleted;
    }

    void OnDisable()
    {
        CirclingNPC.OnDeliveryQuestGenerated -= HandleDeliveryQuestRequest;
        CirclingDeliverySubmitArea.OnDelivered -= HandleDeliveryCompleted;
    }

    void InitializeAreas()
    {
        // 如果没有手动分配，自动查找场景中的所有送货区
        if (deliveryAreas.Count == 0)
        {
            deliveryAreas = FindObjectsOfType<CirclingDeliverySubmitArea>().ToList();
        }

        // 初始化可用性字典
        areaAvailability.Clear();
        foreach (var area in deliveryAreas)
        {
            areaAvailability[area] = true; // true = 可用
        }

        Debug.Log($"初始化送货区管理器: 找到 {deliveryAreas.Count} 个送货区");
    }

    void HandleDeliveryQuestRequest(string npcName, Sprite npcSprite, int originalIndex)
    {
        // 寻找一个可用的送货区
        CirclingDeliverySubmitArea availableArea = GetAvailableArea();
        
        if (availableArea != null)
        {
            // 标记该区域为占用
            areaAvailability[availableArea] = false;
            
            // 直接调用该区域的处理方法，绕过原来的index系统
            availableArea.HandleDeliveryQuest(npcName, npcSprite);
            
            Debug.Log($"为NPC {npcName} 分配送货区: {availableArea.name}");
        }
        else
        {
            Debug.LogWarning($"没有可用的送货区给NPC {npcName}！所有送货区都被占用了。");
        }
    }

    void HandleDeliveryCompleted(string npcName)
    {
        // 找到对应的送货区并释放它
        foreach (var kvp in areaAvailability.ToList())
        {
            if (!kvp.Value && kvp.Key.IsCurrentlyHandling(npcName))
            {
                areaAvailability[kvp.Key] = true; // 标记为可用
                Debug.Log($"送货区 {kvp.Key.name} 已释放 (NPC: {npcName})");
                break;
            }
        }
    }

    CirclingDeliverySubmitArea GetAvailableArea()
    {
        // 返回一个随机的可用的送货区
        List<CirclingDeliverySubmitArea> availableAreas = new List<CirclingDeliverySubmitArea>();
        
        // 收集所有可用的送货区
        foreach (var kvp in areaAvailability)
        {
            if (kvp.Value) // true = 可用
            {
                availableAreas.Add(kvp.Key);
            }
        }
        
        // 如果没有可用区域，返回null
        if (availableAreas.Count == 0)
        {
            return null;
        }
        
        // 随机选择一个可用区域
        int randomIndex = Random.Range(0, availableAreas.Count);
        return availableAreas[randomIndex];
    }

    // 调试方法
    [Header("调试信息")]
    [SerializeField] private bool showDebugInfo = true;
    
    void OnGUI()
    {
        if (!showDebugInfo) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label("送货区状态:");
        
        foreach (var kvp in areaAvailability)
        {
            string status = kvp.Value ? "可用" : "占用";
            GUILayout.Label($"{kvp.Key.name}: {status}");
        }
        
        GUILayout.EndArea();
    }
} 