using UnityEngine;
using JoostenProductions;
using System.Collections.Generic;
using UnityEngine.InputSystem;

/// <summary>
/// 高级3D自动跑酷控制器
/// 包含动画、音效、粒子效果、技能系统等完整功能
/// </summary>
public class AutoRunnerLinearController : OverridableMonoBehaviour
{
    [Header("自动跑酷参数")]
    [SerializeField] private float startSpeed = 2f;        // 起步速度（最低速度）
    [SerializeField] private float accelerationTime = 3f;  // 加速时间（从起步速度到最高速度所需时间）
    [SerializeField] private float maxSpeed = 10f;         // 最高速度
    [SerializeField] private AnimationCurve accelerationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // 加速曲线
    
    [Header("控制设置")]
    [SerializeField] private bool autoStart = true;        // 是否自动开始
    [SerializeField] private bool useRigidbody = true;     // 是否使用Rigidbody物理移动
    [SerializeField] private bool constrainToZAxis = true; // 是否限制只在Z轴移动
    
    [Header("系统组件")]
    [SerializeField] private RunnerEffectsManager effectsManager;   // 特效管理器
    [SerializeField] private SkillSystem skillSystem;              // 技能系统组件
    [SerializeField] private RunnerObstacleSystem obstacleSystem;  // 障碍系统
    [SerializeField] private RunnerAutoCastSystem autoCastSystem;  // 自动施法系统
    [SerializeField] private AttributePointSystem attributePointSystem;  // 属性点系统
    [SerializeField] private bool enableDebug = true;              // 是否启用调试
    
    // 私有变量
    private Rigidbody rb;
    private CharacterController characterController;
    private bool isRunning = false;
    private float currentSpeed = 0f;
    private float baseMaxSpeed = 0f;                       // 基础最高速度（用于技能修改）
    private float accelerationStartTime = 0f;
    
    // 当前状态
    public enum RunningState
    {
        Stopped,        // 停止状态
        Accelerating,   // 加速阶段
        MaxSpeed        // 最高速度阶段
    }
    
    private RunningState currentState = RunningState.Stopped;
    
    // 事件
    public System.Action OnStartRunning;
    public System.Action OnReachMaxSpeed;
    public System.Action OnStopRunning;
    public System.Action<float> OnSpeedChanged;
    
    // 公共属性
    public bool IsRunning => isRunning;
    public float CurrentSpeed => currentSpeed;
    public RunningState CurrentState => currentState;
    public bool IsSkillSystemEnabled => skillSystem != null && skillSystem.enabled;
    
    void Start()
    {
        InitializeComponents();
        InitializeSettings();
        SubscribeToEvents();
        
        if (autoStart)
        {
            StartRunning();
        }
    }
    
    private void InitializeComponents()
    {
        // 获取移动组件
        rb = GetComponent<Rigidbody>();
        characterController = GetComponent<CharacterController>();
        
        // 创建缺失的组件
        if (effectsManager == null)
        {
            effectsManager = GetComponent<RunnerEffectsManager>();
            if (effectsManager == null)
                effectsManager = gameObject.AddComponent<RunnerEffectsManager>();
        }
        
        if (obstacleSystem == null)
        {
            obstacleSystem = GetComponent<RunnerObstacleSystem>();
            if (obstacleSystem == null)
                obstacleSystem = gameObject.AddComponent<RunnerObstacleSystem>();
        }
        
        if (skillSystem == null)
        {
            skillSystem = GetComponent<SkillSystem>();
            if (skillSystem == null)
                skillSystem = gameObject.AddComponent<SkillSystem>();
        }
        
        if (autoCastSystem == null && skillSystem != null)
        {
            autoCastSystem = GetComponent<RunnerAutoCastSystem>();
            if (autoCastSystem == null)
                autoCastSystem = gameObject.AddComponent<RunnerAutoCastSystem>();
            
            // 设置自动施法系统依赖
            autoCastSystem.Initialize(skillSystem, obstacleSystem);
        }
        
        // 验证移动组件
        if (useRigidbody && rb == null)
        {
            Debug.LogWarning("AdvancedAutoRunnerController: 未找到Rigidbody组件，将尝试使用CharacterController");
            useRigidbody = false;
        }
        
        if (!useRigidbody && characterController == null)
        {
            Debug.LogWarning("AdvancedAutoRunnerController: 未找到CharacterController组件，将使用Transform移动");
        }
    }
    
    private void InitializeSettings()
    {
        // 初始化Rigidbody设置
        if (useRigidbody && rb != null)
        {
            if (constrainToZAxis)
            {
                rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY | 
                                RigidbodyConstraints.FreezeRotation;
            }
        }
        
        // 保存基础最高速度
        baseMaxSpeed = maxSpeed;
        
        // 初始化状态
        currentSpeed = 0f;
        currentState = RunningState.Stopped;
        
        // 初始化子系统
        if (effectsManager != null)
            effectsManager.Initialize(this);
        
        if (obstacleSystem != null)
            obstacleSystem.Initialize(this, effectsManager);
        
        if (autoCastSystem != null && skillSystem != null && obstacleSystem != null)
            autoCastSystem.Initialize(skillSystem, obstacleSystem);
    }
    
    private void SubscribeToEvents()
    {
        // 订阅技能系统事件
        if (skillSystem != null)
        {
            skillSystem.OnSkillActivated += OnSkillActivated;
            skillSystem.OnSkillDeactivated += OnSkillDeactivated;
        }
    }
    
    public override void UpdateMe()
    {
        if (!isRunning) 
        {
            if (Keyboard.current[Key.Space].wasPressedThisFrame)
            {
                StartRunning();
            }
            return;
        }
        
        UpdateSpeed();
        MoveCharacter();
        
        // 更新子系统
        if (effectsManager != null)
            effectsManager.UpdateEffects(currentState, currentSpeed, maxSpeed);
        
        if (obstacleSystem != null)
            obstacleSystem.UpdateObstacleDetection(transform.position, GetMoveDirection(), constrainToZAxis);
    }
    
    /// <summary>
    /// 更新速度逻辑
    /// </summary>
    private void UpdateSpeed()
    {
        float previousSpeed = currentSpeed;
        
        // 如果处于障碍物效果中，使用特殊的速度逻辑
        if (obstacleSystem != null && obstacleSystem.IsInObstacleEffect)
        {
            UpdateObstacleEffectSpeed();
        }
        else
        {
            // 正常的速度更新逻辑
            UpdateNormalSpeed();
        }
        
        // 触发速度变化事件
        if (Mathf.Abs(currentSpeed - previousSpeed) > 0.01f)
        {
            OnSpeedChanged?.Invoke(currentSpeed);
        }
    }
    
    /// <summary>
    /// 更新正常移动状态下的速度
    /// </summary>
    private void UpdateNormalSpeed()
    {
        // 计算当前有效的最大速度（包含属性加成）
        float effectiveMaxSpeed = CalculateEffectiveMaxSpeed();
        
        switch (currentState)
        {
            case RunningState.Accelerating:
                // 计算加速进度（0-1）
                float accelerationProgress = GetAccelerationProgress();
                
                // 应用加速度属性
                if (attributePointSystem != null)
                {
                    AttributeData accelAttr = attributePointSystem.GetAttribute("Acceleration");
                    if (accelAttr != null)
                    {
                        // 加速度值越高，加速越快，最高可减少75%加速时间
                        float accelModifier = 1f + (0.75f * accelAttr.GetProgressPercent());
                        accelerationProgress = Mathf.Clamp01(accelerationProgress * accelModifier);
                    }
                }
                
                // 根据曲线计算当前速度百分比
                float speedPercent = accelerationCurve.Evaluate(accelerationProgress);
                
                // 计算当前速度（使用有效最大速度）
                float targetSpeed = Mathf.Lerp(startSpeed, effectiveMaxSpeed, speedPercent);
                
                // 应用新的速度
                if (Mathf.Abs(currentSpeed - targetSpeed) > 0.01f)
                {
                    currentSpeed = targetSpeed;
                    OnSpeedChanged?.Invoke(currentSpeed);
                }
                
                // 检查是否已经达到最高速度
                if (accelerationProgress >= 1f)
                {
                    currentState = RunningState.MaxSpeed;
                    currentSpeed = effectiveMaxSpeed;
                    OnReachMaxSpeed?.Invoke();
                    
                    if (enableDebug)
                        Debug.Log($"AdvancedAutoRunnerController: 已达到最高速度 - {effectiveMaxSpeed}");
                }
                break;
                
            case RunningState.MaxSpeed:
                // 检查最高速度是否已更新
                if (Mathf.Abs(currentSpeed - effectiveMaxSpeed) > 0.01f)
                {
                    currentSpeed = effectiveMaxSpeed;
                    OnSpeedChanged?.Invoke(currentSpeed);
                    
                    if (enableDebug)
                        Debug.Log($"AdvancedAutoRunnerController: 速度已更新 - {currentSpeed}");
                }
                break;
        }
    }
    
    /// <summary>
    /// 更新障碍物效果状态下的速度
    /// </summary>
    private void UpdateObstacleEffectSpeed()
    {
        if (obstacleSystem == null) return;
        
        switch (obstacleSystem.CurrentEffectType)
        {
            case RunnerObstacleSystem.ObstacleEffectType.Stumble:
                // 踉跄状态：保持最低速度
                currentSpeed = startSpeed;
                break;
                
            case RunnerObstacleSystem.ObstacleEffectType.Slowing:
                // 减速状态：速度减半
                float targetSpeed = obstacleSystem.PreObstacleSpeed * obstacleSystem.SlowingSpeedMultiplier;
                currentSpeed = Mathf.Max(targetSpeed, startSpeed); // 不低于最低速度
                break;
        }
    }
    
    /// <summary>
    /// 计算考虑了所有修饰符后的最高速度
    /// </summary>
    private float CalculateEffectiveMaxSpeed()
    {
        // 首先应用属性系统的最大速度加成到基础速度
        float effectiveSpeed = baseMaxSpeed;
        if (attributePointSystem != null)
        {
            AttributeData maxSpeedAttr = attributePointSystem.GetAttribute("Max Speed");
            if (maxSpeedAttr != null)
            {
                // 速度值越高，最大速度越快，最高可增加100%
                float maxSpeedModifier = 1f + maxSpeedAttr.GetProgressPercent();
                effectiveSpeed *= maxSpeedModifier;
            }
        }
        
        // 然后应用技能的速度修饰符
        if (skillSystem != null)
        {
            foreach (var skill in skillSystem.Skills)
            {
                if (skill.isActive && skill.affectsMovementSpeed)
                {
                    effectiveSpeed *= skill.speedMultiplier;
                }
            }
        }
        
        // 最后应用障碍物系统的速度修饰符
        if (obstacleSystem != null)
        {
            effectiveSpeed *= obstacleSystem.SlowingSpeedMultiplier;
        }
        
        return effectiveSpeed;
    }
    
    /// <summary>
    /// 获取移动方向向量
    /// </summary>
    public Vector3 GetMoveDirection()
    {
        return constrainToZAxis ? Vector3.forward : transform.forward;
    }
    
    /// <summary>
    /// 移动角色
    /// </summary>
    private void MoveCharacter()
    {
        Vector3 movement = Vector3.forward * currentSpeed * Time.deltaTime;
        
        if (useRigidbody && rb != null)
        {
            // 使用Rigidbody移动
            if (constrainToZAxis)
            {
                Vector3 newPosition = rb.position + new Vector3(0, 0, movement.z);
                rb.MovePosition(newPosition);
            }
            else
            {
                Vector3 newPosition = rb.position + transform.TransformDirection(movement);
                rb.MovePosition(newPosition);
            }
        }
        else if (characterController != null)
        {
            // 使用CharacterController移动
            if (constrainToZAxis)
            {
                characterController.Move(new Vector3(0, 0, movement.z));
            }
            else
            {
                characterController.Move(transform.TransformDirection(movement));
            }
        }
        else
        {
            // 使用Transform移动
            if (constrainToZAxis)
            {
                transform.Translate(new Vector3(0, 0, movement.z), Space.World);
            }
            else
            {
                transform.Translate(movement);
            }
        }
    }
    
    /// <summary>
    /// 开始跑酷
    /// </summary>
    public void StartRunning()
    {
        if (isRunning) return;
        
        isRunning = true;
        currentSpeed = startSpeed;
        currentState = RunningState.Accelerating;
        accelerationStartTime = Time.time;
        
        OnStartRunning?.Invoke();
        
        if (effectsManager != null)
            effectsManager.PlayStartRunningEffects();
        
        if (enableDebug)
            Debug.Log($"AdvancedAutoRunnerController: 开始跑酷 - 起步速度: {startSpeed}, 加速时间: {accelerationTime}s, 最高速度: {maxSpeed}");
    }
    
    /// <summary>
    /// 停止跑酷
    /// </summary>
    public void StopRunning()
    {
        if (!isRunning) return;
        
        isRunning = false;
        currentSpeed = 0f;
        currentState = RunningState.Stopped;
        
        OnStopRunning?.Invoke();
        
        // 停止所有效果
        if (effectsManager != null)
            effectsManager.StopAllEffects();
        
        // 重置障碍物系统
        if (obstacleSystem != null)
            obstacleSystem.Reset();
        
        // 重置自动施法系统
        if (autoCastSystem != null)
            autoCastSystem.Reset();
        
        if (enableDebug)
            Debug.Log("AdvancedAutoRunnerController: 停止跑酷");
    }
    
    // 公共接口方法
    public void PauseRunning()
    {
        isRunning = false;
        if (enableDebug)
            Debug.Log("AdvancedAutoRunnerController: 暂停跑酷");
    }
    
    public void ResumeRunning()
    {
        isRunning = true;
        if (enableDebug)
            Debug.Log("AdvancedAutoRunnerController: 恢复跑酷");
    }
    
    public void ResetRunning()
    {
        StopRunning();
        StartRunning();
    }
    
    public void SetStartSpeed(float speed)
    {
        startSpeed = Mathf.Max(0f, speed);
        if (startSpeed > maxSpeed)
        {
            maxSpeed = startSpeed;
        }
    }
    
    public void SetMaxSpeed(float speed)
    {
        baseMaxSpeed = Mathf.Max(startSpeed, speed);
        maxSpeed = baseMaxSpeed;
    }
    
    public void SetAccelerationTime(float time)
    {
        accelerationTime = Mathf.Max(0.1f, time);
    }
    
    public float GetCurrentSpeed()
    {
        return currentSpeed;
    }
    
    public RunningState GetCurrentState()
    {
        return currentState;
    }
    
    public float GetAccelerationProgress()
    {
        if (currentState != RunningState.Accelerating)
        {
            return currentState == RunningState.MaxSpeed ? 1f : 0f;
        }
        
        return Mathf.Clamp01((Time.time - accelerationStartTime) / accelerationTime);
    }
    
    /// <summary>
    /// 技能激活事件处理
    /// </summary>
    private void OnSkillActivated(Skill skill)
    {
        // 如果技能影响移动速度，立即更新速度
        if (skill.affectsMovementSpeed && currentState == RunningState.MaxSpeed)
        {
            // 在最高速度状态下，立即应用新的速度
            currentSpeed = CalculateEffectiveMaxSpeed();
            OnSpeedChanged?.Invoke(currentSpeed);
        }
    }
    
    /// <summary>
    /// 技能结束事件处理
    /// </summary>
    private void OnSkillDeactivated(Skill skill)
    {
        // 如果技能影响移动速度，立即更新速度
        if (skill.affectsMovementSpeed && currentState == RunningState.MaxSpeed)
        {
            // 在最高速度状态下，立即应用新的速度
            currentSpeed = CalculateEffectiveMaxSpeed();
            OnSpeedChanged?.Invoke(currentSpeed);
        }
    }
    
    /// <summary>
    /// 技能系统代理方法
    /// </summary>
    public bool TryActivateSkill(string skillName)
    {
        if (skillSystem == null) return false;
        return skillSystem.TryActivateSkill(skillName);
    }
    
    public bool IsSkillActive(string skillName)
    {
        if (skillSystem == null) return false;
        return skillSystem.IsSkillActive(skillName);
    }
    
    /// <summary>
    /// 清理资源
    /// </summary>
    private void OnDestroy()
    {
        // 清理技能系统事件订阅
        if (skillSystem != null)
        {
            skillSystem.OnSkillActivated -= OnSkillActivated;
            skillSystem.OnSkillDeactivated -= OnSkillDeactivated;
        }
    }
    
    /// <summary>
    /// 在Inspector中显示运行时信息
    /// </summary>
    private void OnValidate()
    {
        // 确保参数合理性
        startSpeed = Mathf.Max(0f, startSpeed);
        maxSpeed = Mathf.Max(startSpeed, maxSpeed);
        accelerationTime = Mathf.Max(0.1f, accelerationTime);
    }
    
    /// <summary>
    /// 重新开始加速
    /// </summary>
    public void RestartAcceleration()
    {
        if (!isRunning) return;
        
        // 计算有效最大速度
        float effectiveMaxSpeed = CalculateEffectiveMaxSpeed();
        
        // 如果当前速度已经接近或超过有效最大速度，直接进入最高速度状态
        if (currentSpeed >= effectiveMaxSpeed * 0.95f)
        {
            currentState = RunningState.MaxSpeed;
            currentSpeed = effectiveMaxSpeed;
            OnReachMaxSpeed?.Invoke();
            
            if (enableDebug)
                Debug.Log($"RestartAcceleration: 直接进入最高速度状态 - {effectiveMaxSpeed}");
        }
        else
        {
            // 根据当前速度计算应该从哪个时间点开始加速
            float speedRatio = (currentSpeed - startSpeed) / (effectiveMaxSpeed - startSpeed);
            speedRatio = Mathf.Clamp01(speedRatio);
            
            // 应用加速度属性
            float accelerationModifier = 1f;
            if (attributePointSystem != null)
            {
                AttributeData accelAttr = attributePointSystem.GetAttribute("Acceleration");
                if (accelAttr != null)
                {
                    accelerationModifier = 1f + (0.5f * accelAttr.GetProgressPercent());
                }
            }
            
            // 调整加速时间，考虑属性加成
            float effectiveAccelerationTime = accelerationTime / accelerationModifier;
            
            // 计算应该从多少时间前开始加速才能达到当前速度
            float elapsedAccelerationTime = speedRatio * effectiveAccelerationTime;
            
            currentState = RunningState.Accelerating;
            accelerationStartTime = Time.time - elapsedAccelerationTime;
            
            if (enableDebug)
                Debug.Log($"RestartAcceleration: 从加速状态恢复 - 当前速度: {currentSpeed:F2}, 目标速度: {effectiveMaxSpeed:F2}, 加速进度: {speedRatio:F2}");
        }
    }
} 