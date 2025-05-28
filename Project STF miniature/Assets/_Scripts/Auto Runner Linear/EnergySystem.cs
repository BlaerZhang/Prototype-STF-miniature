using UnityEngine;
using System;

/// <summary>
/// 精力系统
/// 管理角色的精力值、回复和消耗
/// </summary>
[System.Serializable]
public class EnergySystem
{
    [Header("精力系统设置")]
    [SerializeField] private float maxEnergy = 100f;           // 精力最大值
    [SerializeField] private float energyRegenRate = 5f;       // 精力回复速度（每秒）
    [SerializeField] private float currentEnergy = 100f;       // 当前精力值
    
    [Header("加点系统")]
    [SerializeField] private AttributePointSystem attributePointSystem;  // 属性点系统
    
    [Header("调试信息")]
    [SerializeField] private bool showDebugInfo = true;        // 是否显示调试信息

    // 事件
    public Action<float, float> OnEnergyChanged;    // 精力变化事件 (当前精力, 最大精力)
    public Action OnEnergyEmpty;                    // 精力耗尽事件
    public Action OnEnergyFull;                     // 精力满值事件
    
    // 属性
    public float MaxEnergy => maxEnergy;
    public float CurrentEnergy => currentEnergy;
    public float EnergyPercentage => currentEnergy / maxEnergy;
    public bool IsEmpty => currentEnergy <= 0f;
    public bool IsFull => currentEnergy >= maxEnergy;
    
    /// <summary>
    /// 初始化精力系统
    /// </summary>
    public void Initialize()
    {
        currentEnergy = maxEnergy;
        OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);
        
        if (showDebugInfo)
        {
            Debug.Log($"精力系统初始化 - 最大精力: {maxEnergy}, 回复速度: {energyRegenRate}/秒");
        }
    }
    
    /// <summary>
    /// 更新精力系统（每帧调用）
    /// </summary>
    public void UpdateEnergy()
    {
        // 获取属性加成
        float regenModifier = 1f;
        float maxEnergyModifier = 1f;
        
        if (attributePointSystem != null)
        {
            // 获取能量恢复属性
            AttributeData energyRegenAttr = attributePointSystem.GetAttribute("Energy Regen");
            if (energyRegenAttr != null)
            {
                // 恢复值越高，恢复速度越快，最高可增加100%
                regenModifier = 1f + energyRegenAttr.GetProgressPercent();
            }
            
            // 获取最大能量属性
            AttributeData maxEnergyAttr = attributePointSystem.GetAttribute("Max Energy");
            if (maxEnergyAttr != null)
            {
                // 最大能量值越高，最大能量越大，最高可增加100%
                maxEnergyModifier = 1f + maxEnergyAttr.GetProgressPercent();
            }
        }
        
        // 计算当前最大能量
        float effectiveMaxEnergy = maxEnergy * maxEnergyModifier;
        
        // 自动回复精力（应用属性加成）
        if (currentEnergy < effectiveMaxEnergy)
        {
            float previousEnergy = currentEnergy;
            float effectiveRegenRate = energyRegenRate * regenModifier;
            currentEnergy = Mathf.Min(currentEnergy + effectiveRegenRate * Time.deltaTime, effectiveMaxEnergy);
            
            // 触发事件
            if (Mathf.Abs(currentEnergy - previousEnergy) > 0.01f)
            {
                OnEnergyChanged?.Invoke(currentEnergy, effectiveMaxEnergy);
                
                // 检查是否达到满值
                if (currentEnergy >= effectiveMaxEnergy && previousEnergy < effectiveMaxEnergy)
                {
                    OnEnergyFull?.Invoke();
                    if (showDebugInfo)
                    {
                        Debug.Log("精力已满");
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// 消耗精力
    /// </summary>
    /// <param name="amount">消耗量</param>
    /// <returns>是否成功消耗</returns>
    public bool ConsumeEnergy(float amount)
    {
        if (amount <= 0f) return true;
        
        if (currentEnergy < amount)
        {
            if (showDebugInfo)
            {
                Debug.LogWarning($"精力不足！需要: {amount}, 当前: {currentEnergy:F1}");
            }
            return false;
        }
        
        float previousEnergy = currentEnergy;
        currentEnergy = Mathf.Max(currentEnergy - amount, 0f);
        
        OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);
        
        // 检查是否耗尽
        if (IsEmpty && previousEnergy > 0f)
        {
            OnEnergyEmpty?.Invoke();
            if (showDebugInfo)
            {
                Debug.Log("精力耗尽");
            }
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"消耗精力: {amount}, 剩余: {currentEnergy:F1}/{maxEnergy}");
        }
        
        return true;
    }
    
    /// <summary>
    /// 恢复精力
    /// </summary>
    /// <param name="amount">恢复量</param>
    public void RestoreEnergy(float amount)
    {
        if (amount <= 0f) return;
        
        float previousEnergy = currentEnergy;
        currentEnergy = Mathf.Min(currentEnergy + amount, maxEnergy);
        
        OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);
        
        // 检查是否达到满值
        if (IsFull && previousEnergy < maxEnergy)
        {
            OnEnergyFull?.Invoke();
            if (showDebugInfo)
            {
                Debug.Log("精力已满");
            }
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"恢复精力: {amount}, 当前: {currentEnergy:F1}/{maxEnergy}");
        }
    }
    
    /// <summary>
    /// 检查是否有足够精力
    /// </summary>
    /// <param name="amount">需要的精力量</param>
    /// <returns>是否有足够精力</returns>
    public bool HasEnoughEnergy(float amount)
    {
        return currentEnergy >= amount;
    }
    
    /// <summary>
    /// 设置最大精力值
    /// </summary>
    /// <param name="newMaxEnergy">新的最大精力值</param>
    /// <param name="adjustCurrentEnergy">是否按比例调整当前精力</param>
    public void SetMaxEnergy(float newMaxEnergy, bool adjustCurrentEnergy = false)
    {
        if (newMaxEnergy <= 0f)
        {
            Debug.LogError("最大精力值必须大于0");
            return;
        }
        
        if (adjustCurrentEnergy)
        {
            float ratio = EnergyPercentage;
            maxEnergy = newMaxEnergy;
            currentEnergy = maxEnergy * ratio;
        }
        else
        {
            maxEnergy = newMaxEnergy;
            currentEnergy = Mathf.Min(currentEnergy, maxEnergy);
        }
        
        OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);
        
        if (showDebugInfo)
        {
            Debug.Log($"设置最大精力: {maxEnergy}, 当前精力: {currentEnergy:F1}");
        }
    }
    
    /// <summary>
    /// 设置精力回复速度
    /// </summary>
    /// <param name="newRegenRate">新的回复速度</param>
    public void SetEnergyRegenRate(float newRegenRate)
    {
        energyRegenRate = Mathf.Max(0f, newRegenRate);
        
        if (showDebugInfo)
        {
            Debug.Log($"设置精力回复速度: {energyRegenRate}/秒");
        }
    }
    
    /// <summary>
    /// 立即填满精力
    /// </summary>
    public void FillEnergy()
    {
        float previousEnergy = currentEnergy;
        currentEnergy = maxEnergy;
        
        OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);
        
        if (previousEnergy < maxEnergy)
        {
            OnEnergyFull?.Invoke();
            if (showDebugInfo)
            {
                Debug.Log("精力已填满");
            }
        }
    }
    
    /// <summary>
    /// 清空精力
    /// </summary>
    public void EmptyEnergy()
    {
        float previousEnergy = currentEnergy;
        currentEnergy = 0f;
        
        OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);
        
        if (previousEnergy > 0f)
        {
            OnEnergyEmpty?.Invoke();
            if (showDebugInfo)
            {
                Debug.Log("精力已清空");
            }
        }
    }
    
    /// <summary>
    /// 重置精力系统
    /// </summary>
    public void Reset()
    {
        currentEnergy = maxEnergy;
        OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);
        
        if (showDebugInfo)
        {
            Debug.Log("精力系统已重置");
        }
    }
    
    /// <summary>
    /// 设置属性系统引用
    /// </summary>
    public void SetAttributeSystem(AttributePointSystem aps)
    {
        attributePointSystem = aps;
    }
} 