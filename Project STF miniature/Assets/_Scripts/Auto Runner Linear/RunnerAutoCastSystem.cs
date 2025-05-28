using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 跑酷自动施法系统 - 处理自动技能释放
/// </summary>
public class RunnerAutoCastSystem : MonoBehaviour
{
    [Header("自动施法设置")]
    [SerializeField] private bool enableAutocast = true;            // 是否启用自动施法
    [SerializeField] private float autocastDetectionDistance = 2f;  // 自动施法检测距离
    [SerializeField] private float autocastDetectionRadius = 0.3f;  // 自动施法检测半径
    [SerializeField] private bool debugMode = false;                // 是否显示调试信息
    
    // 自动施法尝试记录类
    private class AutocastAttempt
    {
        public Collider targetObstacle;        // 目标障碍物
        public ObstacleType obstacleType;      // 障碍物类型
        public string targetSkillName;         // 目标技能名称
        public bool isAttempting;              // 是否正在尝试施法
        public float firstDetectionTime;       // 首次检测时间
        
        public AutocastAttempt(Collider obstacle, ObstacleType type, string skillName)
        {
            targetObstacle = obstacle;
            obstacleType = type;
            targetSkillName = skillName;
            isAttempting = true;
            firstDetectionTime = Time.time;
        }
    }
    
    // 私有变量
    private SkillSystem skillSystem;
    private RunnerObstacleSystem obstacleSystem;
    private Dictionary<Collider, AutocastAttempt> autocastAttempts = new Dictionary<Collider, AutocastAttempt>();
    private HashSet<Collider> autocastDetectedObstacles = new HashSet<Collider>();
    
    // 公开属性
    public int ActiveAttemptsCount => autocastAttempts.Count;
    public bool IsEnabled => enableAutocast;
    
    /// <summary>
    /// 初始化自动施法系统
    /// </summary>
    public void Initialize(SkillSystem skills, RunnerObstacleSystem obstacles)
    {
        skillSystem = skills;
        obstacleSystem = obstacles;
    }
    
    /// <summary>
    /// 固定更新 - 处理自动施法逻辑
    /// </summary>
    private void FixedUpdate()
    {
        if (!enableAutocast || skillSystem == null || obstacleSystem == null) return;
        
        // 检测自动施法范围内的障碍物
        DetectAutocastObstacles();
        
        // 处理自动施法尝试
        ProcessAutocastAttempts();
        
        // 清理过期的自动施法记录
        CleanupAutocastAttempts();
    }
    
    /// <summary>
    /// 检测自动施法范围内的障碍物
    /// </summary>
    private void DetectAutocastObstacles()
    {
        // 计算检测起点和方向
        Vector3 detectionOrigin = transform.position + Vector3.up * 0.5f;
        Vector3 detectionDirection = GetDetectionDirection();
        
        // 使用球形射线检测
        RaycastHit[] hits = Physics.SphereCastAll(
            detectionOrigin,
            autocastDetectionRadius,
            detectionDirection,
            autocastDetectionDistance,
            obstacleSystem != null ? obstacleSystem.GetLayerMask() : -1
        );
        
        // 清理已经远离的障碍物
        CleanupAutocastDetectedObstacles(detectionOrigin, detectionDirection);
        
        // 处理新检测到的障碍物
        foreach (RaycastHit hit in hits)
        {
            Collider hitCollider = hit.collider;
            
            // 如果这个障碍物还没有被自动施法系统检测过
            if (!autocastDetectedObstacles.Contains(hitCollider))
            {
                Obstacles obstacleComponent = hitCollider.GetComponent<Obstacles>();
                if (obstacleComponent != null && obstacleComponent.obstacleTypes != null && obstacleComponent.obstacleTypes.Count > 0)
                {
                    // 标记为已检测
                    autocastDetectedObstacles.Add(hitCollider);
                    
                    // 选择最优技能并创建自动施法尝试
                    string bestSkill = SelectBestSkillForObstacle(obstacleComponent.obstacleTypes);
                    if (!string.IsNullOrEmpty(bestSkill))
                    {
                        // 获取主要障碍物类型（用于记录）
                        ObstacleType primaryType = obstacleComponent.obstacleTypes[0];
                        
                        // 创建自动施法尝试
                        AutocastAttempt attempt = new AutocastAttempt(hitCollider, primaryType, bestSkill);
                        autocastAttempts[hitCollider] = attempt;
                        
                        if (debugMode)
                        {
                            Debug.Log($"自动施法: 检测到障碍物 {hitCollider.name}, 选择技能: {bestSkill}, 距离: {hit.distance:F2}");
                        }
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// 获取检测方向
    /// </summary>
    private Vector3 GetDetectionDirection()
    {
        // 假设主要在Z轴移动，可以通过参数调整
        return Vector3.forward;
    }
    
    /// <summary>
    /// 清理已经远离的自动施法检测障碍物记录
    /// </summary>
    private void CleanupAutocastDetectedObstacles(Vector3 origin, Vector3 direction)
    {
        List<Collider> toRemove = new List<Collider>();
        
        foreach (Collider obstacle in autocastDetectedObstacles)
        {
            if (obstacle == null)
            {
                toRemove.Add(obstacle);
                continue;
            }
            
            // 计算障碍物与检测起点的距离
            Vector3 toObstacle = obstacle.bounds.center - origin;
            float distanceInDirection = Vector3.Dot(toObstacle, direction);
            
            // 如果障碍物在后方或距离过远，移除记录
            if (distanceInDirection < -1f || distanceInDirection > autocastDetectionDistance + 2f)
            {
                toRemove.Add(obstacle);
            }
        }
        
        foreach (Collider obstacle in toRemove)
        {
            autocastDetectedObstacles.Remove(obstacle);
        }
    }
    
    /// <summary>
    /// 处理自动施法尝试
    /// </summary>
    private void ProcessAutocastAttempts()
    {
        List<Collider> toRemove = new List<Collider>();
        
        foreach (var kvp in autocastAttempts)
        {
            Collider obstacle = kvp.Key;
            AutocastAttempt attempt = kvp.Value;
            
            // 检查障碍物是否仍然存在
            if (obstacle == null)
            {
                toRemove.Add(obstacle);
                continue;
            }
            
            // 检查障碍物是否已经被普通检测系统检测到
            if (obstacleSystem != null && obstacleSystem.DetectedObstacles.Contains(obstacle))
            {
                if (debugMode)
                {
                    Debug.Log($"自动施法: 障碍物 {obstacle.name} 已被检测系统发现，停止尝试技能 {attempt.targetSkillName}");
                }
                toRemove.Add(obstacle);
                continue;
            }
            
            // 检查障碍物是否已经在身后（停止条件）
            if (IsObstacleBehind(obstacle))
            {
                if (debugMode)
                {
                    Debug.Log($"自动施法: 障碍物 {obstacle.name} 已在身后，停止尝试技能 {attempt.targetSkillName}");
                }
                toRemove.Add(obstacle);
                continue;
            }
            
            // 如果仍在尝试施法
            if (attempt.isAttempting)
            {
                // 尝试释放技能
                bool success = TryActivateSkill(attempt.targetSkillName);
                
                if (success)
                {
                    if (debugMode)
                    {
                        Debug.Log($"自动施法: 成功释放技能 {attempt.targetSkillName} 应对障碍物 {obstacle.name}");
                    }
                    
                    // 成功释放，标记为不再尝试
                    attempt.isAttempting = false;
                }
                else if (debugMode)
                {
                    // 获取失败原因
                    string reason = GetSkillActivationFailureReason(attempt.targetSkillName);
                    Debug.Log($"自动施法: 尝试释放技能 {attempt.targetSkillName} 失败 - {reason}");
                }
            }
        }
        
        // 移除已完成或无效的尝试
        foreach (Collider obstacle in toRemove)
        {
            autocastAttempts.Remove(obstacle);
        }
    }
    
    /// <summary>
    /// 清理过期的自动施法记录
    /// </summary>
    private void CleanupAutocastAttempts()
    {
        List<Collider> toRemove = new List<Collider>();
        
        foreach (var kvp in autocastAttempts)
        {
            Collider obstacle = kvp.Key;
            AutocastAttempt attempt = kvp.Value;
            
            // 移除无效的障碍物记录
            if (obstacle == null)
            {
                toRemove.Add(obstacle);
                continue;
            }
            
            // 如果障碍物已经在身后，移除记录
            if (IsObstacleBehind(obstacle))
            {
                toRemove.Add(obstacle);
                continue;
            }
            
            // 如果技能已经成功释放且不再尝试，移除记录
            if (!attempt.isAttempting)
            {
                // 给一点时间让技能效果生效，然后移除记录
                if (Time.time - attempt.firstDetectionTime > 1f)
                {
                    toRemove.Add(obstacle);
                }
            }
        }
        
        foreach (Collider obstacle in toRemove)
        {
            autocastAttempts.Remove(obstacle);
        }
    }
    
    /// <summary>
    /// 为障碍物类型列表选择最优技能
    /// </summary>
    private string SelectBestSkillForObstacle(List<ObstacleType> obstacleTypes)
    {
        if (skillSystem == null || obstacleTypes == null || obstacleTypes.Count == 0) return null;
        
        List<string> candidateSkills = new List<string>();
        
        // 为每个障碍物类型找到对应的技能
        foreach (ObstacleType type in obstacleTypes)
        {
            string skill = GetSkillNameForObstacleType(type);
            if (!string.IsNullOrEmpty(skill) && !candidateSkills.Contains(skill))
            {
                candidateSkills.Add(skill);
            }
        }
        
        if (candidateSkills.Count == 0) return null;
        
        // 如果只有一个候选技能，直接返回
        if (candidateSkills.Count == 1) return candidateSkills[0];
        
        // 根据精力消耗选择最优技能
        string bestSkill = null;
        float lowestEnergyCost = float.MaxValue;
        List<string> sameEnergyCostSkills = new List<string>();
        
        foreach (string skillName in candidateSkills)
        {
            Skill skill = GetSkill(skillName);
            if (skill != null)
            {
                if (skill.energyCost < lowestEnergyCost)
                {
                    lowestEnergyCost = skill.energyCost;
                    bestSkill = skillName;
                    sameEnergyCostSkills.Clear();
                    sameEnergyCostSkills.Add(skillName);
                }
                else if (Mathf.Approximately(skill.energyCost, lowestEnergyCost))
                {
                    sameEnergyCostSkills.Add(skillName);
                }
            }
        }
        
        // 如果有多个技能具有相同的最低精力消耗，随机选择一个
        if (sameEnergyCostSkills.Count > 1)
        {
            int randomIndex = Random.Range(0, sameEnergyCostSkills.Count);
            bestSkill = sameEnergyCostSkills[randomIndex];
            
            if (debugMode)
            {
                Debug.Log($"自动施法: 多个技能具有相同精力消耗({lowestEnergyCost})，随机选择: {bestSkill}");
            }
        }
        
        return bestSkill;
    }
    
    /// <summary>
    /// 获取障碍物类型对应的技能名称
    /// </summary>
    private string GetSkillNameForObstacleType(ObstacleType obstacleType)
    {
        switch (obstacleType)
        {
            case ObstacleType.Low:
                return "Jump";
            case ObstacleType.High:
                return "Roll";
            case ObstacleType.Slowing:
                return "Dash";
            default:
                return null;
        }
    }
    
    /// <summary>
    /// 检查障碍物是否在角色身后
    /// </summary>
    private bool IsObstacleBehind(Collider obstacle)
    {
        if (obstacle == null) return true;
        
        // 计算障碍物相对于角色的位置
        Vector3 obstaclePosition = obstacle.bounds.center;
        Vector3 characterPosition = transform.position;
        
        // 默认以Z轴为前进方向
        return obstaclePosition.z < characterPosition.z - 0.5f;
    }
    
    /// <summary>
    /// 获取技能对象
    /// </summary>
    private Skill GetSkill(string skillName)
    {
        if (skillSystem == null) return null;
        return skillSystem.GetSkill(skillName);
    }
    
    /// <summary>
    /// 尝试激活技能
    /// </summary>
    private bool TryActivateSkill(string skillName)
    {
        if (skillSystem == null) return false;
        return skillSystem.TryActivateSkill(skillName);
    }
    
    /// <summary>
    /// 获取技能释放失败的原因（用于调试）
    /// </summary>
    private string GetSkillActivationFailureReason(string skillName)
    {
        if (skillSystem == null) return "技能系统未启用";
        
        Skill skill = GetSkill(skillName);
        if (skill == null) return $"技能 {skillName} 不存在";
        
        if (skill.isOnCooldown) return $"技能 {skillName} 正在冷却中";
        
        if (skillSystem.Energy.CurrentEnergy < skill.energyCost) 
            return $"精力不足 (需要:{skill.energyCost}, 当前:{skillSystem.Energy.CurrentEnergy:F1})";
        
        if (skill.isActive) return $"技能 {skillName} 已激活";
        
        return "未知原因";
    }
    
    /// <summary>
    /// 重置自动施法系统
    /// </summary>
    public void Reset()
    {
        autocastAttempts.Clear();
        autocastDetectedObstacles.Clear();
    }
    
    /// <summary>
    /// 设置自动施法系统启用状态
    /// </summary>
    public void SetEnabled(bool enabled)
    {
        enableAutocast = enabled;
        
        // 如果禁用，清理所有自动施法记录
        if (!enabled)
        {
            Reset();
            
            if (debugMode)
            {
                Debug.Log("自动施法系统已禁用，清理所有记录");
            }
        }
    }
    
    /// <summary>
    /// 获取当前自动施法目标信息列表
    /// </summary>
    public List<string> GetTargetInfo()
    {
        List<string> info = new List<string>();
        
        foreach (var kvp in autocastAttempts)
        {
            Collider obstacle = kvp.Key;
            AutocastAttempt attempt = kvp.Value;
            
            if (obstacle != null)
            {
                string status = attempt.isAttempting ? "尝试中" : "已完成";
                float distance = Vector3.Distance(transform.position, obstacle.bounds.center);
                info.Add($"{obstacle.name}: {attempt.targetSkillName} ({status}, 距离:{distance:F1})");
            }
        }
        
        return info;
    }
} 