using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.Linq;

[System.Serializable]
public class TypeCountWeight
{
    public int typeCount;
    public float weight;
}

public class SimpleRequirementsGenerator : MonoBehaviour
{
    [Header("基础设置")]
    public GameObject requirementPrefab;
    
    [Header("权重配置 (类型数量 -> 权重)")]
    [Tooltip("类型数量：生成几种不同的需求类型\n权重：该数量被选中的概率权重")]
    public List<TypeCountWeight> typeCountWeights = new List<TypeCountWeight>()
    {
        new TypeCountWeight { typeCount = 1, weight = 4f },
        new TypeCountWeight { typeCount = 2, weight = 4f },
        new TypeCountWeight { typeCount = 3, weight = 2f }
    };
    
    [Header("数量和布局")]
    public int maxRequiredCount = 10;
    public int minRequiredCount = 1;
    public float displayYOffset = 0.1f;
    
    [Header("颜色配置")]
    public List<Color> requiredColors = new List<Color>();
    
    [Header("调试")]
    public bool enableDebugLog = false;
    
    // 运行时转换为Dictionary便于查找
    private Dictionary<int, float> weightDict;
    
    void Start() => TryGenerateRequirements();

    void TryGenerateRequirements()
    {
        if (!IsValidConfiguration()) return;
        
        int typeCount = GetRandomTypeCount();
        typeCount = Mathf.Min(typeCount, requiredColors.Count);
        
        LogDebug($"生成 {typeCount} 种需求类型");
        
        CreateRequirementObjects(typeCount);
    }

    bool IsValidConfiguration()
    {
        // 简化验证：链式检查
        return CheckNotNull(requirementPrefab, "RequirementPrefab未设置") &&
               CheckTypeCountWeights() &&
               CheckList(requiredColors, "颜色列表");
    }

    bool CheckNotNull(Object obj, string errorMsg)
    {
        if (obj == null) Debug.LogError(errorMsg);
        return obj != null;
    }

    bool CheckList<T>(List<T> list, string name)
    {
        if (list?.Count > 0) return true;
        Debug.LogError($"{name}为空！");
        return false;
    }

    bool CheckTypeCountWeights()
    {
        if (!CheckList(typeCountWeights, "权重配置")) return false;

        // 检查重复的类型数量
        var typeCounts = typeCountWeights.Select(x => x.typeCount).ToList();
        var duplicates = typeCounts.GroupBy(x => x).Where(g => g.Count() > 1);
        if (duplicates.Any())
        {
            Debug.LogError($"类型数量重复: {string.Join(",", duplicates.Select(g => g.Key))}");
            return false;
        }

        // 检查负权重
        var invalidWeight = typeCountWeights.FirstOrDefault(x => x.weight < 0);
        if (invalidWeight != null)
        {
            Debug.LogError($"权重不能为负数：类型数量{invalidWeight.typeCount}的权重为{invalidWeight.weight}");
            return false;
        }

        // 检查类型数量是否合理
        var invalidTypeCount = typeCountWeights.FirstOrDefault(x => x.typeCount <= 0);
        if (invalidTypeCount != null)
        {
            Debug.LogError($"类型数量必须大于0：{invalidTypeCount.typeCount}");
            return false;
        }

        var totalWeight = typeCountWeights.Sum(x => x.weight);
        if (totalWeight <= 0)
        {
            Debug.LogError("权重总和必须大于0");
            return false;
        }

        var maxTypeCount = typeCountWeights.Max(x => x.typeCount);
        if (requiredColors.Count < maxTypeCount)
        {
            Debug.LogWarning($"颜色数量({requiredColors.Count})少于最大类型数量({maxTypeCount})");
        }

        // 构建运行时字典
        weightDict = typeCountWeights.ToDictionary(x => x.typeCount, x => x.weight);

        return true;
    }

    void CreateRequirementObjects(int typeCount)
    {
        var colors = GetRandomColors(typeCount);
        
        for (int i = 0; i < typeCount; i++)
        {
            var position = transform.position + Vector3.up * (i * displayYOffset);
            var requirement = Instantiate(requirementPrefab, position, Quaternion.identity, transform);
            
            SetRequirementData(requirement, colors[i], Random.Range(minRequiredCount, maxRequiredCount + 1), i + 1);
        }
    }

    void SetRequirementData(GameObject requirement, Color color, int amount, int index)
    {
        requirement.GetComponentInChildren<TMP_Text>().text = amount.ToString();
        requirement.GetComponentInChildren<SpriteRenderer>().color = color;
        
        LogDebug($"需求{index}: 颜色={color}, 数量={amount}");
    }

    Color[] GetRandomColors(int count)
    {
        var availableColors = new List<Color>(requiredColors);
        var selectedColors = new Color[count];
        
        for (int i = 0; i < count; i++)
        {
            int randomIndex = Random.Range(0, availableColors.Count);
            selectedColors[i] = availableColors[randomIndex];
            availableColors.RemoveAt(randomIndex);
        }
        
        return selectedColors;
    }

    int GetRandomTypeCount()
    {
        float totalWeight = weightDict.Values.Sum();
        float randomValue = Random.Range(0f, totalWeight);
        
        LogDebug($"权重配置: {string.Join(",", weightDict.Select(x => $"{x.Key}类型:{x.Value}"))} 总计:{totalWeight:F1} 随机值:{randomValue:F1}");
        
        float currentWeight = 0f;
        foreach (var kvp in weightDict.OrderBy(x => x.Key)) // 按类型数量排序
        {
            currentWeight += kvp.Value;
            if (randomValue < currentWeight)
            {
                LogDebug($"选中{kvp.Key}种类型 (权重:{kvp.Value})");
                return kvp.Key;
            }
        }
        
        // 浮点精度兜底：返回最大类型数量
        var maxTypeCount = weightDict.Keys.Max();
        LogDebug($"兜底返回最大类型数量: {maxTypeCount}");
        return maxTypeCount;
    }

    void LogDebug(string message)
    {
        if (enableDebugLog) Debug.Log($"[需求生成器] {message}");
    }

    // 工具方法
    [ContextMenu("预览权重分布")]
    void PreviewWeightDistribution()
    {
        if (!CheckList(typeCountWeights, "权重配置")) return;
        
        float total = typeCountWeights.Sum(x => x.weight);
        Debug.Log("=== 权重分布 ===");
        
        foreach (var item in typeCountWeights.OrderBy(x => x.typeCount))
        {
            float percentage = (item.weight / total) * 100f;
            Debug.Log($"{item.typeCount}种类型: 权重{item.weight}, 概率{percentage:F1}%");
        }
    }
}
