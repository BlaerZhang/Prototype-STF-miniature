using UnityEngine;
using System;

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
    [SerializeField] private AttributeData acceleration = new AttributeData("加速", 1, 10, 1);
    [SerializeField] private AttributeData braking = new AttributeData("刹车", 1, 10, 1);
    [SerializeField] private AttributeData vision = new AttributeData("视野", 1, 10, 1);
    [SerializeField] private AttributeData steering = new AttributeData("转向", 1, 10, 1);
    
    [Header("总点数限制（可选）")]
    [SerializeField] private bool useTotalPointsLimit = false;
    [SerializeField] private int totalPointsLimit = 20;
    [SerializeField] private int currentTotalPoints = 4; // 初始总点数（4个属性各1点）
    
    // 事件：当属性值改变时触发
    public static event Action<string, int> OnAttributeChanged;
    public static event Action<int, int> OnTotalPointsChanged; // (当前点数, 最大点数)
    
    private void Start()
    {
        // 初始化时发送当前值
        UpdateAllAttributeEvents();
        UpdateTotalPointsEvent();
    }
    
    #region 属性访问器
    public AttributeData GetAcceleration() => acceleration;
    public AttributeData GetBraking() => braking;
    public AttributeData GetVision() => vision;
    public AttributeData GetSteering() => steering;
    
    public int GetAccelerationValue() => acceleration.currentValue;
    public int GetBrakingValue() => braking.currentValue;
    public int GetVisionValue() => vision.currentValue;
    public int GetSteeringValue() => steering.currentValue;
    #endregion
    
    #region 属性修改方法
    public bool IncreaseAcceleration()
    {
        if (CanIncreaseAttribute())
        {
            if (acceleration.IncreaseValue())
            {
                if (useTotalPointsLimit) currentTotalPoints++;
                OnAttributeChanged?.Invoke("acceleration", acceleration.currentValue);
                UpdateTotalPointsEvent();
                return true;
            }
        }
        return false;
    }
    
    public bool DecreaseAcceleration()
    {
        if (acceleration.DecreaseValue())
        {
            if (useTotalPointsLimit) currentTotalPoints--;
            OnAttributeChanged?.Invoke("acceleration", acceleration.currentValue);
            UpdateTotalPointsEvent();
            return true;
        }
        return false;
    }
    
    public bool IncreaseBraking()
    {
        if (CanIncreaseAttribute())
        {
            if (braking.IncreaseValue())
            {
                if (useTotalPointsLimit) currentTotalPoints++;
                OnAttributeChanged?.Invoke("braking", braking.currentValue);
                UpdateTotalPointsEvent();
                return true;
            }
        }
        return false;
    }
    
    public bool DecreaseBraking()
    {
        if (braking.DecreaseValue())
        {
            if (useTotalPointsLimit) currentTotalPoints--;
            OnAttributeChanged?.Invoke("braking", braking.currentValue);
            UpdateTotalPointsEvent();
            return true;
        }
        return false;
    }
    
    public bool IncreaseVision()
    {
        if (CanIncreaseAttribute())
        {
            if (vision.IncreaseValue())
            {
                if (useTotalPointsLimit) currentTotalPoints++;
                OnAttributeChanged?.Invoke("vision", vision.currentValue);
                UpdateTotalPointsEvent();
                return true;
            }
        }
        return false;
    }
    
    public bool DecreaseVision()
    {
        if (vision.DecreaseValue())
        {
            if (useTotalPointsLimit) currentTotalPoints--;
            OnAttributeChanged?.Invoke("vision", vision.currentValue);
            UpdateTotalPointsEvent();
            return true;
        }
        return false;
    }
    
    public bool IncreaseSteering()
    {
        if (CanIncreaseAttribute())
        {
            if (steering.IncreaseValue())
            {
                if (useTotalPointsLimit) currentTotalPoints++;
                OnAttributeChanged?.Invoke("steering", steering.currentValue);
                UpdateTotalPointsEvent();
                return true;
            }
        }
        return false;
    }
    
    public bool DecreaseSteering()
    {
        if (steering.DecreaseValue())
        {
            if (useTotalPointsLimit) currentTotalPoints--;
            OnAttributeChanged?.Invoke("steering", steering.currentValue);
            UpdateTotalPointsEvent();
            return true;
        }
        return false;
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
        OnAttributeChanged?.Invoke("acceleration", acceleration.currentValue);
        OnAttributeChanged?.Invoke("braking", braking.currentValue);
        OnAttributeChanged?.Invoke("vision", vision.currentValue);
        OnAttributeChanged?.Invoke("steering", steering.currentValue);
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
        acceleration.SetValue(1);
        braking.SetValue(1);
        vision.SetValue(1);
        steering.SetValue(1);
        
        if (useTotalPointsLimit)
        {
            currentTotalPoints = 4; // 4个属性各1点
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