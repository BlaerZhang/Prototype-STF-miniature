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
    
    [Header("需求数量配置")]
    public int maxRequiredCount = 10;
    public int minRequiredCount = 1;
    
    [Header("需求数量概率分布")]
    [Tooltip("X轴: 0到1映射到min-max范围\nY轴: 概率权重（越高越容易被选中）")]
    public AnimationCurve requiredCountDistribution = AnimationCurve.Linear(0f, 1f, 1f, 0.5f);
    
    [Space]
    [Tooltip("曲线采样精度，值越大越精确但性能略差")]
    [Range(10, 100)]
    public int curveSampleCount = 50;

    [Header("权重配置 (消耗时间 -> 权重)")]
    [Tooltip("消耗时间：生成几种不同的消耗时间\n权重：该数量被选中的概率权重")]
    public List<TypeCountWeight> timeCostWeights = new List<TypeCountWeight>()
    {
        new TypeCountWeight { typeCount = 1, weight = 4f },
        new TypeCountWeight { typeCount = 2, weight = 4f },
        new TypeCountWeight { typeCount = 3, weight = 2f }
    };
    
    [Header("布局")]
    public float displayYOffset = 0.1f;
    
    [Header("颜色配置")]
    public List<Color> requiredColors = new List<Color>();
    
    [Header("调试")]
    public bool enableDebugLog = false;
    
    // 运行时转换为Dictionary便于查找
    private Dictionary<int, float> weightDict;
    // 曲线概率查找表
    private float[] cumulativeProbabilities;
    private int[] countValues;
    [SerializeField] public int timeCost = 1;
    
    void Start() => TryGenerateRequirements();

    void TryGenerateRequirements()
    {
        if (!IsValidConfiguration()) return;
        
        int typeCount = GetRandomTypeCount();
        this.timeCost = GetRandomTimeCost();  // 修复：将结果赋值给成员变量
        typeCount = Mathf.Min(typeCount, requiredColors.Count);
        
        LogDebug($"生成 {typeCount} 种需求类型，时间消耗 {this.timeCost} 分钟");
        
        CreateRequirementObjects(typeCount);
    }

    bool IsValidConfiguration()
    {
        // 简化验证：链式检查
        return CheckNotNull(requirementPrefab, "RequirementPrefab未设置") &&
               CheckTypeCountWeights() &&
               CheckRequiredCountRange() &&
               CheckList(requiredColors, "颜色列表") &&
               CheckList(timeCostWeights, "消耗时间列表");
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

    bool CheckRequiredCountRange()
    {
        if (minRequiredCount <= 0)
        {
            Debug.LogError("最小需求数量必须大于0");
            return false;
        }
        
        if (maxRequiredCount < minRequiredCount)
        {
            Debug.LogError("最大需求数量不能小于最小需求数量");
            return false;
        }
        
        // 构建曲线概率查找表
        BuildCurveProbabilityTable();
        
        return true;
    }

    void BuildCurveProbabilityTable()
    {
        int range = maxRequiredCount - minRequiredCount + 1;
        countValues = new int[range];
        float[] weights = new float[range];
        
        // 为每个可能的数量值计算权重
        for (int i = 0; i < range; i++)
        {
            int count = minRequiredCount + i;
            float normalizedPos = range > 1 ? (float)i / (range - 1) : 0f;
            float curveValue = requiredCountDistribution.Evaluate(normalizedPos);
            
            countValues[i] = count;
            weights[i] = Mathf.Max(0f, curveValue); // 确保权重非负
        }
        
        // 构建累积概率表
        float totalWeight = weights.Sum();
        if (totalWeight <= 0)
        {
            Debug.LogWarning("曲线权重总和为0，使用均匀分布");
            for (int i = 0; i < weights.Length; i++)
                weights[i] = 1f;
            totalWeight = weights.Sum();
        }
        
        cumulativeProbabilities = new float[range];
        float cumulativeWeight = 0f;
        for (int i = 0; i < range; i++)
        {
            cumulativeWeight += weights[i];
            cumulativeProbabilities[i] = cumulativeWeight / totalWeight;
        }
        
        if (enableDebugLog)
        {
            LogDebug("需求数量概率分布:");
            for (int i = 0; i < range; i++)
        {
                float prob = i == 0 ? cumulativeProbabilities[i] : 
                            cumulativeProbabilities[i] - cumulativeProbabilities[i-1];
                LogDebug($"  数量{countValues[i]}: {prob:P1}");
            }
        }
    }

    int GetRandomRequiredCount()
    {
        if (cumulativeProbabilities == null || cumulativeProbabilities.Length == 0)
        {
            // 兜底：均匀分布
            return Random.Range(minRequiredCount, maxRequiredCount + 1);
        }
        
        float randomValue = Random.Range(0f, 1f);
        
        for (int i = 0; i < cumulativeProbabilities.Length; i++)
        {
            if (randomValue <= cumulativeProbabilities[i])
            {
                int selectedCount = countValues[i];
                LogDebug($"随机值{randomValue:F3} -> 选中数量{selectedCount}");
                return selectedCount;
            }
        }
        
        // 兜底：返回最大值
        return maxRequiredCount;
    }

    int GetRandomTimeCost()
    {
        float totalWeight = timeCostWeights.Sum(x => x.weight);
        float randomValue = Random.Range(0f, totalWeight);
        
        LogDebug($"时间消耗权重配置: {string.Join(",", timeCostWeights.Select(x => $"{x.typeCount}分钟:{x.weight}"))} 总计:{totalWeight:F1} 随机值:{randomValue:F1}");

        float currentWeight = 0f;
        foreach (var item in timeCostWeights)
        {
            currentWeight += item.weight;
            if (randomValue <= currentWeight)  // 修复：使用 <= 而不是 <
            {
                LogDebug($"选中{item.typeCount}分钟 (权重:{item.weight})");
                return item.typeCount;
            }
        }

        // 浮点精度兜底：返回最大时间消耗
        var maxTimeCost = timeCostWeights.Max(x => x.typeCount);
        LogDebug($"兜底返回最大时间消耗: {maxTimeCost}");
        return maxTimeCost;
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
            
            int requiredAmount = GetRandomRequiredCount(); // 使用曲线分布
            SetRequirementData(requirement, colors[i], requiredAmount, i + 1);
        }

        CreateTimeCostObject(timeCost);
    }

    void CreateTimeCostObject(int timeCost)
    {
        var position = transform.position + Vector3.up * (- 1 * displayYOffset);
        var requirement = Instantiate(requirementPrefab, position, Quaternion.identity, transform);
        requirement.GetComponentInChildren<TMP_Text>().text = timeCost.ToString() + "<size=0.6>min</size>";
        requirement.GetComponentInChildren<SpriteRenderer>().enabled = false;
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
        Debug.Log("=== 类型数量权重分布 ===");
        
        foreach (var item in typeCountWeights.OrderBy(x => x.typeCount))
        {
            float percentage = (item.weight / total) * 100f;
            Debug.Log($"{item.typeCount}种类型: 权重{item.weight}, 概率{percentage:F1}%");
        }
    }
    
    [ContextMenu("预览时间消耗权重分布")]
    void PreviewTimeCostWeightDistribution()
    {
        if (!CheckList(timeCostWeights, "时间消耗权重配置")) return;
        
        float total = timeCostWeights.Sum(x => x.weight);
        Debug.Log("=== 时间消耗权重分布 ===");
        
        foreach (var item in timeCostWeights.OrderBy(x => x.typeCount))
        {
            float percentage = (item.weight / total) * 100f;
            Debug.Log($"{item.typeCount}分钟: 权重{item.weight}, 概率{percentage:F1}%");
        }
    }
    
    [ContextMenu("预览需求数量分布")]
    void PreviewRequiredCountDistribution()
        {
        if (!CheckRequiredCountRange()) return;
        
        Debug.Log("=== 需求数量概率分布 ===");
        
        // 重新构建概率表（以防参数改变）
        BuildCurveProbabilityTable();
        
        for (int i = 0; i < countValues.Length; i++)
        {
            float prob = i == 0 ? cumulativeProbabilities[i] : 
                        cumulativeProbabilities[i] - cumulativeProbabilities[i-1];
            Debug.Log($"数量{countValues[i]}: {prob:P1}");
        }
    }
    
    [ContextMenu("测试曲线分布(100次)")]
    void TestCurveDistribution()
            {
        if (!CheckRequiredCountRange()) return;
        
        BuildCurveProbabilityTable();
        
        Dictionary<int, int> results = new Dictionary<int, int>();
        int testCount = 100;
        
        for (int i = 0; i < testCount; i++)
        {
            int count = GetRandomRequiredCount();
            if (results.ContainsKey(count))
                results[count]++;
            else
                results[count] = 1;
        }
        
        Debug.Log($"=== 曲线分布测试结果 ({testCount}次) ===");
        foreach (var kvp in results.OrderBy(x => x.Key))
        {
            float percentage = (float)kvp.Value / testCount * 100f;
            Debug.Log($"数量{kvp.Key}: {kvp.Value}次 ({percentage:F1}%)");
        }
    }
}
