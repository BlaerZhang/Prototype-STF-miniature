using UnityEngine;

/// <summary>
/// 技能数据结构
/// 包含技能的基本属性和动画参数设置
/// </summary>
[System.Serializable]
public class Skill
{
    [Header("技能基础属性")]
    public string skillName = "技能";           // 技能名称
    public float duration = 3f;                 // 持续时间（秒）
    public float cooldown = 10f;                // 冷却时间（秒）
    public float energyCost = 20f;              // 精力消耗
    
    [Header("动画参数")]
    public AnimationParameterType parameterType = AnimationParameterType.Bool;
    public string animationParameterName = "IsSkillActive";  // 动画机参数名
    
    [Header("技能效果（可选）")]
    [Tooltip("是否影响移动速度")]
    public bool affectsMovementSpeed = false;
    [Tooltip("移动速度倍数（仅当affectsMovementSpeed为true时生效）")]
    public float speedMultiplier = 1.5f;
    
    // 运行时状态（不在Inspector中显示）
    [System.NonSerialized] public bool isActive = false;           // 是否激活
    [System.NonSerialized] public bool isOnCooldown = false;       // 是否在冷却中
    [System.NonSerialized] public float remainingDuration = 0f;    // 剩余持续时间
    [System.NonSerialized] public float remainingCooldown = 0f;    // 剩余冷却时间
    [System.NonSerialized] public float currentCooldownModifier = 1f; // 当前冷却修饰符
    
    /// <summary>
    /// 动画参数类型
    /// </summary>
    public enum AnimationParameterType
    {
        Bool,       // 布尔值参数
        Trigger     // 触发器参数
    }
    
    /// <summary>
    /// 检查技能是否可以使用
    /// </summary>
    public bool CanUse(float currentEnergy)
    {
        return !isActive && !isOnCooldown && currentEnergy >= energyCost;
    }
    
    /// <summary>
    /// 激活技能
    /// </summary>
    public void Activate()
    {
        // 外部系统已经检查过CanUse，这里直接激活
        isActive = true;
        remainingDuration = duration;
        
        Debug.Log($"技能 {skillName} 激活，持续时间: {duration}秒");
    }
    
    /// <summary>
    /// 结束技能
    /// </summary>
    public void Deactivate()
    {
        if (!isActive) return;
        
        isActive = false;
        isOnCooldown = true;
        remainingCooldown = cooldown;
        remainingDuration = 0f;
        
        Debug.Log($"技能 {skillName} 结束，开始冷却: {cooldown}秒");
    }
    
    /// <summary>
    /// 更新技能状态（每帧调用）
    /// </summary>
    public void UpdateSkill()
    {
        UpdateSkill(1f);
    }
    
    /// <summary>
    /// 更新技能状态（每帧调用）
    /// </summary>
    /// <param name="cooldownModifier">冷却时间修饰符 (小于1加速冷却)</param>
    public void UpdateSkill(float cooldownModifier)
    {
        // 保存当前的冷却修饰符用于UI显示
        currentCooldownModifier = cooldownModifier;
        
        // 更新持续时间
        if (isActive)
        {
            remainingDuration -= Time.deltaTime;
            if (remainingDuration <= 0f)
            {
                Deactivate();
            }
        }
        
        // 更新冷却时间（应用修饰符）
        if (isOnCooldown)
        {
            remainingCooldown -= Time.deltaTime * (1f / Mathf.Max(0.1f, cooldownModifier));
            if (remainingCooldown <= 0f)
            {
                isOnCooldown = false;
                Debug.Log($"技能 {skillName} 冷却完成");
            }
        }
    }
    
    /// <summary>
    /// 获取冷却进度 (0-1)
    /// </summary>
    public float GetCooldownProgress()
    {
        if (!isOnCooldown) return 1f;
        return 1f - (remainingCooldown / cooldown);
    }
    
    /// <summary>
    /// 获取持续时间进度 (0-1)
    /// </summary>
    public float GetDurationProgress()
    {
        if (!isActive) return 0f;
        return 1f - (remainingDuration / duration);
    }
    
    /// <summary>
    /// 重置技能状态（用于调试）
    /// </summary>
    public void Reset()
    {
        isActive = false;
        isOnCooldown = false;
        remainingDuration = 0f;
        remainingCooldown = 0f;
    }
    
    /// <summary>
    /// 获取用于UI显示的剩余冷却时间
    /// </summary>
    public float GetDisplayRemainingCooldown()
    {
        if (!isOnCooldown) return 0f;
        // 应用冷却修饰符来计算显示时间
        return remainingCooldown * currentCooldownModifier;
    }
} 