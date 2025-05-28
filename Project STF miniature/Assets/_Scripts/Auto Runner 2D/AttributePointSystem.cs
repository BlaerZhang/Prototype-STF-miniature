using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class AttributeData
{
    [Header("属性设置")]
    public string attributeName;    // 属性名称
    public int currentValue = 1;    // 当前值
    public int minValue = 1;        // 最小值
    public int maxValue = 10;       // 最大值
    
    public AttributeData(string name, int min = 1, int max = 10, int current = 1)
    {
        attributeName = name;
        minValue = min;
        maxValue = max;
        currentValue = Mathf.Clamp(current, min, max);
    }
    
    // 增加属性点
    public bool IncreaseValue()
    {
        if (currentValue < maxValue)
        {
            currentValue++;
            return true;
        }
        return false;
    }
    
    // 减少属性点
    public bool DecreaseValue()
    {
        if (currentValue > minValue)
        {
            currentValue--;
            return true;
        }
        return false;
    }
    
    // 设置值
    public void SetValue(int value)
    {
        currentValue = Mathf.Clamp(value, minValue, maxValue);
    }
    
    // 获取进度百分比
    public float GetProgressPercent()
    {
        return (float)(currentValue - minValue) / (maxValue - minValue);
    }
}

public class AttributePointSystem : MonoBehaviour
{
    [Header("属性配置")]
    [SerializeField] private List<AttributeData> attributes = new List<AttributeData>();
    
    [Header("总点数限制（可选）")]
    [SerializeField] private bool useTotalPointsLimit = false;
    [SerializeField] private int totalPointsLimit = 20;
    [SerializeField] private int currentTotalPoints = 0; // 初始总点数将根据初始属性计算
    
    // 事件：当属性值改变时触发
    public static event Action<string, int> OnAttributeChanged;
    public static event Action<int, int> OnTotalPointsChanged; // (当前点数, 最大点数)
    
    private void Awake()
    {
        // 如果没有属性，添加默认属性
        if (attributes.Count == 0)
        {
            InitializeDefaultAttributes();
        }
        
        // 计算当前总点数
        RecalculateTotalPoints();
    }
    
    private void Start()
    {
        // 初始化时发送当前值
        UpdateAllAttributeEvents();
        UpdateTotalPointsEvent();
    }
    
    // 初始化默认属性
    private void InitializeDefaultAttributes()
    {
        attributes.Add(new AttributeData("Acceleration", 1, 20, 1));
        attributes.Add(new AttributeData("Max Speed", 1, 20, 1));
        attributes.Add(new AttributeData("Skill Cooldown", 1, 20, 1));
        attributes.Add(new AttributeData("Energy Regen", 1, 20, 1));
        attributes.Add(new AttributeData("Max Energy", 1, 20, 1));
    }
    
    // 重新计算当前总点数
    private void RecalculateTotalPoints()
    {
        currentTotalPoints = attributes.Sum(attr => attr.currentValue);
    }
    
    #region 属性管理
    // 添加新属性
    public bool AddAttribute(string name, int min = 1, int max = 10, int current = 1)
    {
        // 检查是否已存在同名属性
        if (GetAttribute(name) != null)
            return false;
            
        attributes.Add(new AttributeData(name, min, max, current));
        
        // 更新总点数
        if (useTotalPointsLimit)
        {
            RecalculateTotalPoints();
            UpdateTotalPointsEvent();
        }
        
        OnAttributeChanged?.Invoke(name, current);
        return true;
    }
    
    // 移除属性
    public bool RemoveAttribute(string name)
    {
        AttributeData attribute = GetAttribute(name);
        if (attribute == null)
            return false;
            
        attributes.Remove(attribute);
        
        // 更新总点数
        if (useTotalPointsLimit)
        {
            RecalculateTotalPoints();
            UpdateTotalPointsEvent();
        }
        
        return true;
    }
    
    // 通过名称获取属性
    public AttributeData GetAttribute(string name)
    {
        return attributes.FirstOrDefault(a => a.attributeName == name);
    }
    
    // 获取所有属性
    public IReadOnlyList<AttributeData> GetAllAttributes()
    {
        return attributes.AsReadOnly();
    }
    
    // 获取属性值
    public int GetAttributeValue(string name)
    {
        AttributeData attribute = GetAttribute(name);
        return attribute != null ? attribute.currentValue : 0;
    }
    
    // 判断属性是否存在
    public bool HasAttribute(string name)
    {
        return GetAttribute(name) != null;
    }
    #endregion
    
    #region 属性修改方法
    // 增加属性值
    public bool IncreaseAttribute(string name)
    {
        AttributeData attribute = GetAttribute(name);
        if (attribute == null)
            return false;
            
        if (!CanIncreaseAttribute())
            return false;
            
        if (attribute.IncreaseValue())
        {
            if (useTotalPointsLimit) currentTotalPoints++;
            OnAttributeChanged?.Invoke(name, attribute.currentValue);
            UpdateTotalPointsEvent();
            return true;
        }
        
        return false;
    }
    
    // 减少属性值
    public bool DecreaseAttribute(string name)
    {
        AttributeData attribute = GetAttribute(name);
        if (attribute == null)
            return false;
            
        if (attribute.DecreaseValue())
        {
            if (useTotalPointsLimit) currentTotalPoints--;
            OnAttributeChanged?.Invoke(name, attribute.currentValue);
            UpdateTotalPointsEvent();
            return true;
        }
        
        return false;
    }
    
    // 设置属性值
    public bool SetAttributeValue(string name, int value)
    {
        AttributeData attribute = GetAttribute(name);
        if (attribute == null)
            return false;
            
        int oldValue = attribute.currentValue;
        attribute.SetValue(value);
        
        if (useTotalPointsLimit)
        {
            currentTotalPoints += (attribute.currentValue - oldValue);
            UpdateTotalPointsEvent();
        }
        
        OnAttributeChanged?.Invoke(name, attribute.currentValue);
        return true;
    }
    #endregion
    
    #region 辅助方法
    private bool CanIncreaseAttribute()
    {
        if (!useTotalPointsLimit) return true;
        return currentTotalPoints < totalPointsLimit;
    }
    
    private void UpdateAllAttributeEvents()
    {
        foreach (var attribute in attributes)
        {
            OnAttributeChanged?.Invoke(attribute.attributeName, attribute.currentValue);
        }
    }
    
    private void UpdateTotalPointsEvent()
    {
        if (useTotalPointsLimit)
        {
            OnTotalPointsChanged?.Invoke(currentTotalPoints, totalPointsLimit);
        }
    }
    
    // 重置所有属性到初始值
    public void ResetAllAttributes()
    {
        foreach (var attribute in attributes)
        {
            attribute.SetValue(1);
        }
        
        if (useTotalPointsLimit)
        {
            currentTotalPoints = attributes.Count; // 每个属性1点
        }
        
        UpdateAllAttributeEvents();
        UpdateTotalPointsEvent();
    }
    
    // 获取剩余可分配点数
    public int GetRemainingPoints()
    {
        if (!useTotalPointsLimit) return int.MaxValue;
        return totalPointsLimit - currentTotalPoints;
    }
    
    // 检查是否还有可分配点数
    public bool HasRemainingPoints()
    {
        return GetRemainingPoints() > 0;
    }
    #endregion
} 