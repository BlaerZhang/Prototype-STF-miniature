using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CirclingDeliveryAreaManager : MonoBehaviour
{
    public static CirclingDeliveryAreaManager Instance { get; private set; }
    
    [Header("配置")]
    [SerializeField] private List<CirclingDeliverySubmitArea> deliveryAreas = new List<CirclingDeliverySubmitArea>();
    
    // NPC名称到送货区域的映射
    private Dictionary<string, CirclingDeliverySubmitArea> npcToAreaMapping = new Dictionary<string, CirclingDeliverySubmitArea>();

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

        // 初始化NPC到送货区的映射
        BuildNPCToAreaMapping();

        Debug.Log($"初始化送货区管理器: 找到 {deliveryAreas.Count} 个送货区，映射了 {npcToAreaMapping.Count} 个NPC");
    }

    void BuildNPCToAreaMapping()
    {
        npcToAreaMapping.Clear();
        
        foreach (var area in deliveryAreas)
        {
            string boundNPCName = area.GetBoundNPCName();
            if (!string.IsNullOrEmpty(boundNPCName))
            {
                npcToAreaMapping[boundNPCName] = area;
                Debug.Log($"绑定NPC {boundNPCName} 到送货区 {area.name}");
            }
        }
    }

    void HandleDeliveryQuestRequest(string senderNpcName, Sprite npcSprite, int originalIndex)
    {
        // 获取所有可用的送货区（排除发送者自己的区域）
        List<CirclingDeliverySubmitArea> availableAreas = GetAvailableAreas(senderNpcName);
        
        if (availableAreas.Count > 0)
        {
            // 随机选择一个可用的送货区
            int randomIndex = Random.Range(0, availableAreas.Count);
            CirclingDeliverySubmitArea selectedArea = availableAreas[randomIndex];
            
            // 添加任务到选中的送货区
            selectedArea.AddDeliveryQuest(senderNpcName, npcSprite);
            
            Debug.Log($"为NPC {senderNpcName} 分配送货区: {selectedArea.name} (绑定NPC: {selectedArea.GetBoundNPCName()})");
        }
        else
        {
            Debug.LogWarning($"没有可用的送货区给NPC {senderNpcName}！");
        }
    }

    void HandleDeliveryCompleted(string deliveredNpcName, string targetNpcName)
    {
        Debug.Log($"送货完成: {deliveredNpcName} 的任务已送达到 {targetNpcName}");
        
        // 这里可以添加更多的完成逻辑
        // 例如：给玩家奖励、更新统计数据等
    }

    List<CirclingDeliverySubmitArea> GetAvailableAreas(string senderNpcName)
    {
        List<CirclingDeliverySubmitArea> availableAreas = new List<CirclingDeliverySubmitArea>();
        
        foreach (var area in deliveryAreas)
        {
            string boundNPCName = area.GetBoundNPCName();
            
            // 排除发送者自己的区域
            if (boundNPCName != senderNpcName)
            {
                availableAreas.Add(area);
            }
        }
        
        return availableAreas;
    }

    // 根据NPC名称获取对应的送货区
    public CirclingDeliverySubmitArea GetAreaForNPC(string npcName)
    {
        npcToAreaMapping.TryGetValue(npcName, out CirclingDeliverySubmitArea area);
        return area;
    }

    // 调试方法
    [Header("调试信息")]
    [SerializeField] private bool showDebugInfo = true;
    
    void OnGUI()
    {
        if (!showDebugInfo) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 400, 300));
        GUILayout.Label("送货区状态:");
        
        foreach (var area in deliveryAreas)
        {
            string boundNPC = area.GetBoundNPCName();
            int questCount = area.GetQuestCount();
            GUILayout.Label($"{area.name} (绑定: {boundNPC}) - 任务数: {questCount}");
        }
        
        GUILayout.EndArea();
    }
} 