using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 跑酷障碍物系统 - 处理障碍物检测和效果
/// </summary>
public class RunnerObstacleSystem : MonoBehaviour
{
    [Header("障碍物检测设置")]
    [SerializeField] private bool enableObstacleSystem = true;   // 是否启用障碍物系统
    [SerializeField] private float obstacleDetectionDistance = 0.5f; // 障碍物检测距离
    [SerializeField] private float obstacleDetectionRadius = 0.3f;   // 障碍物检测半径
    [SerializeField] private LayerMask obstacleLayerMask = -1;       // 障碍物图层遮罩
    
    [Header("障碍物效果设置")]
    [SerializeField] private float stumbleEffectDuration = 2f;  // 踉跄效果持续时间（Low/High障碍物）
    [SerializeField] private float slowingEffectDuration = 3f;  // 减速效果持续时间（Slowing障碍物）
    [SerializeField] private float slowingSpeedMultiplier = 0.5f; // 减速时的速度倍数
    
    // 障碍物效果类型
    public enum ObstacleEffectType
    {
        None,           // 无效果
        Stumble,        // 踉跄（Low/High障碍物）
        Slowing         // 减速（Slowing障碍物）
    }
    
    // 私有变量
    private AutoRunnerLinearController runnerController;
    private RunnerEffectsManager effectsManager;
    private HashSet<Collider> detectedObstacles = new HashSet<Collider>();
    private bool isInObstacleEffect = false;
    private ObstacleEffectType currentObstacleEffect = ObstacleEffectType.None;
    private float obstacleEffectEndTime = 0f;
    private float preObstacleSpeed = 0f;
    private bool debugMode = false;
    
    // 公开属性
    public bool IsInObstacleEffect => isInObstacleEffect;
    public ObstacleEffectType CurrentEffectType => currentObstacleEffect;
    public float PreObstacleSpeed => preObstacleSpeed;
    public float SlowingSpeedMultiplier => slowingSpeedMultiplier;
    public HashSet<Collider> DetectedObstacles => detectedObstacles;
    
    /// <summary>
    /// 初始化障碍物系统
    /// </summary>
    public void Initialize(AutoRunnerLinearController controller, RunnerEffectsManager effects)
    {
        runnerController = controller;
        effectsManager = effects;
        debugMode = controller != null && controller.GetType().GetField("enableDebug") != null;
    }
    
    /// <summary>
    /// 更新障碍物检测
    /// </summary>
    public void UpdateObstacleDetection(Vector3 position, Vector3 direction, bool constrainToZAxis)
    {
        if (!enableObstacleSystem) return;
        
        // 更新障碍物效果
        if (isInObstacleEffect && Time.time >= obstacleEffectEndTime)
        {
            EndObstacleEffect();
        }
        
        // 计算检测起点和方向
        Vector3 detectionOrigin = position + Vector3.up * 0.5f;
        Vector3 detectionDirection = constrainToZAxis ? Vector3.forward : direction;
        
        // 使用球形射线检测
        RaycastHit[] hits = Physics.SphereCastAll(
            detectionOrigin,
            obstacleDetectionRadius,
            detectionDirection,
            obstacleDetectionDistance,
            obstacleLayerMask
        );
        
        // 清理已经远离的障碍物
        CleanupDetectedObstacles(detectionOrigin, detectionDirection);
        
        // 处理新检测到的障碍物
        foreach (RaycastHit hit in hits)
        {
            Collider hitCollider = hit.collider;
            
            // 如果这个障碍物还没有被检测过
            if (!detectedObstacles.Contains(hitCollider))
            {
                Obstacles obstacleComponent = hitCollider.GetComponent<Obstacles>();
                if (obstacleComponent != null)
                {
                    // 标记为已检测
                    detectedObstacles.Add(hitCollider);
                    
                    // 触发障碍物效果
                    OnObstacleHit(obstacleComponent);
                    
                    if (debugMode)
                    {
                        Debug.Log($"检测到障碍物: {hitCollider.name} 距离: {hit.distance:F2}");
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// 清理已经远离的障碍物记录
    /// </summary>
    private void CleanupDetectedObstacles(Vector3 origin, Vector3 direction)
    {
        List<Collider> toRemove = new List<Collider>();
        
        foreach (Collider obstacle in detectedObstacles)
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
            if (distanceInDirection < -1f || distanceInDirection > obstacleDetectionDistance + 2f)
            {
                toRemove.Add(obstacle);
            }
        }
        
        foreach (Collider obstacle in toRemove)
        {
            detectedObstacles.Remove(obstacle);
        }
    }
    
    /// <summary>
    /// 处理障碍物碰撞
    /// </summary>
    public void OnObstacleHit(Obstacles obstacleComponent)
    {
        if (!enableObstacleSystem || obstacleComponent == null) return;
        
        // 如果已经在障碍物效果中，根据策略决定是否重新触发
        if (isInObstacleEffect)
        {
            if (debugMode)
            {
                Debug.Log("已在障碍物效果中，忽略新的障碍物");
            }
            return;
        }
        
        // 获取第一个障碍物类型（如果有多个类型，只处理第一个）
        if (obstacleComponent.obstacleTypes != null && obstacleComponent.obstacleTypes.Count > 0)
        {
            ObstacleType obstacleType = obstacleComponent.obstacleTypes[0];
            StartObstacleEffect(obstacleType);
        }
    }
    
    /// <summary>
    /// 开始障碍物效果
    /// </summary>
    public void StartObstacleEffect(ObstacleType obstacleType)
    {
        if (!enableObstacleSystem || runnerController == null || !runnerController.IsRunning) return;
        
        // 在任意障碍物效果持续期间不再触发新的障碍物效果
        if (isInObstacleEffect)
        {
            if (debugMode)
            {
                Debug.Log($"障碍物效果进行中({currentObstacleEffect})，忽略新的{obstacleType}效果");
            }
            return;
        }
        
        // 检查技能免疫
        if (IsImmuneToObstacle(obstacleType))
        {
            if (debugMode)
            {
                Debug.Log($"技能免疫生效，忽略{obstacleType}效果");
            }
            return;
        }
        
        // 记录当前速度（用于减速效果）
        preObstacleSpeed = runnerController.CurrentSpeed;
        
        // 根据障碍物类型设置效果
        switch (obstacleType)
        {
            case ObstacleType.Low:
            case ObstacleType.High:
                StartStumbleEffect();
                break;
                
            case ObstacleType.Slowing:
                StartSlowingEffect();
                break;
        }
        
        // 播放碰撞音效
        if (effectsManager != null)
        {
            effectsManager.PlayObstacleHitSound();
        }
        
        if (debugMode)
        {
            Debug.Log($"触发障碍物效果 - {obstacleType}, 当前速度: {runnerController.CurrentSpeed:F2}");
        }
    }
    
    /// <summary>
    /// 检查是否对指定障碍物类型免疫
    /// </summary>
    private bool IsImmuneToObstacle(ObstacleType obstacleType)
    {
        if (runnerController == null) return false;
        
        switch (obstacleType)
        {
            case ObstacleType.Low:
                // Jump技能持续期间不受Low障碍物效果
                return runnerController.IsSkillActive("Jump");
                
            case ObstacleType.High:
                // Roll技能持续期间不受High障碍物效果
                return runnerController.IsSkillActive("Roll");
                
            case ObstacleType.Slowing:
                // Dash技能持续期间不受Slowing障碍物效果
                return runnerController.IsSkillActive("Dash");
                
            default:
                return false;
        }
    }
    
    /// <summary>
    /// 开始踉跄效果（Low/High障碍物）
    /// </summary>
    private void StartStumbleEffect()
    {
        isInObstacleEffect = true;
        currentObstacleEffect = ObstacleEffectType.Stumble;
        obstacleEffectEndTime = Time.time + stumbleEffectDuration;
        
        // 触发踉跄动画
        if (effectsManager != null)
        {
            effectsManager.PlayStumbleAnimation();
        }
        
        if (debugMode)
        {
            Debug.Log($"开始踉跄效果，持续时间: {stumbleEffectDuration}秒");
        }
    }
    
    /// <summary>
    /// 开始减速效果（Slowing障碍物）
    /// </summary>
    private void StartSlowingEffect()
    {
        isInObstacleEffect = true;
        currentObstacleEffect = ObstacleEffectType.Slowing;
        obstacleEffectEndTime = Time.time + slowingEffectDuration;
        
        if (debugMode)
        {
            Debug.Log($"开始减速效果，持续时间: {slowingEffectDuration}秒，速度倍数: {slowingSpeedMultiplier}");
        }
    }
    
    /// <summary>
    /// 结束障碍物效果
    /// </summary>
    public void EndObstacleEffect()
    {
        if (!isInObstacleEffect) return;
        
        ObstacleEffectType previousEffect = currentObstacleEffect;
        
        // 重置障碍物效果状态
        isInObstacleEffect = false;
        currentObstacleEffect = ObstacleEffectType.None;
        obstacleEffectEndTime = 0f;
        
        // 重新开始加速
        if (runnerController != null)
        {
            runnerController.RestartAcceleration();
        }
        
        if (debugMode)
        {
            Debug.Log($"障碍物效果结束: {previousEffect}，重新开始加速");
        }
    }
    
    /// <summary>
    /// 重置障碍物系统
    /// </summary>
    public void Reset()
    {
        isInObstacleEffect = false;
        currentObstacleEffect = ObstacleEffectType.None;
        obstacleEffectEndTime = 0f;
        detectedObstacles.Clear();
    }
    
    /// <summary>
    /// 设置障碍物系统启用状态
    /// </summary>
    public void SetEnabled(bool enabled)
    {
        enableObstacleSystem = enabled;
        
        // 如果禁用时正在效果中，结束效果
        if (!enabled && isInObstacleEffect)
        {
            EndObstacleEffect();
        }
    }
    
    /// <summary>
    /// 获取障碍物效果剩余时间
    /// </summary>
    public float GetEffectRemainingTime()
    {
        if (!isInObstacleEffect) return 0f;
        return Mathf.Max(0f, obstacleEffectEndTime - Time.time);
    }
    
    /// <summary>
    /// 获取障碍物图层遮罩
    /// </summary>
    public LayerMask GetLayerMask()
    {
        return obstacleLayerMask;
    }
} 