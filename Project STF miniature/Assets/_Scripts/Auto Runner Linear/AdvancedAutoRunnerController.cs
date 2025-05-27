using UnityEngine;
using JoostenProductions;
using System.Collections.Generic;

/// <summary>
/// 高级3D自动跑酷控制器
/// 包含动画、音效、粒子效果、技能系统等完整功能
/// </summary>
public class AdvancedAutoRunnerController : OverridableMonoBehaviour
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
    
    [Header("技能系统")]
    [SerializeField] private SkillSystem skillSystem;      // 技能系统组件
    [SerializeField] private bool enableSkillSystem = true; // 是否启用技能系统
    
    [Header("动画设置")]
    [SerializeField] private Animator characterAnimator;   // 角色动画控制器
    [SerializeField] private string speedParameterName = "Speed"; // 速度参数名
    [SerializeField] private string isRunningParameterName = "IsRunning"; // 是否跑步参数名
    [SerializeField] private float animationSpeedMultiplier = 1f; // 动画速度倍数
    
    [Header("音效设置")]
    [SerializeField] private AudioSource audioSource;     // 音频源
    [SerializeField] private AudioClip startRunningSound; // 开始跑步音效
    [SerializeField] private AudioClip maxSpeedSound;     // 达到最高速度音效
    [SerializeField] private AudioClip footstepSound;     // 脚步声
    [SerializeField] private float footstepInterval = 0.5f; // 脚步声间隔
    
    [Header("粒子效果")]
    [SerializeField] private ParticleSystem accelerationParticles; // 加速粒子效果
    [SerializeField] private ParticleSystem maxSpeedParticles;     // 最高速度粒子效果
    [SerializeField] private ParticleSystem dustParticles;        // 灰尘粒子效果
    
    [Header("相机跟随")]
    [SerializeField] private bool enableCameraFollow = true;      // 是否启用相机跟随
    [SerializeField] private Transform cameraTarget;              // 相机跟随目标
    [SerializeField] private Vector3 cameraOffset = new Vector3(0, 2, -5); // 相机偏移
    [SerializeField] private float cameraFollowSpeed = 5f;        // 相机跟随速度
    
    [Header("调试信息")]
    [SerializeField] private bool showDebugInfo = true;    // 是否显示调试信息
    [SerializeField] private bool showSpeedUI = true;      // 是否显示速度UI
    
    [Header("障碍物效果系统")]
    [SerializeField] private bool enableObstacleSystem = true;  // 是否启用障碍物系统
    [SerializeField] private float stumbleEffectDuration = 2f;  // 踉跄效果持续时间（Low/High障碍物）
    [SerializeField] private float slowingEffectDuration = 3f;  // 减速效果持续时间（Slowing障碍物）
    [SerializeField] private float slowingSpeedMultiplier = 0.5f; // 减速时的速度倍数
    [SerializeField] private string stumbleAnimationTrigger = "Stumble"; // 踉跄动画触发器名称
    [SerializeField] private AudioClip obstacleHitSound;        // 障碍物碰撞音效
    [SerializeField] private float obstacleDetectionDistance = 0.5f; // 障碍物检测距离
    [SerializeField] private float obstacleDetectionRadius = 0.3f;   // 障碍物检测半径
    [SerializeField] private LayerMask obstacleLayerMask = -1;       // 障碍物图层遮罩
    
    // 私有变量
    private Rigidbody rb;
    private CharacterController characterController;
    private bool isRunning = false;
    private float currentSpeed = 0f;
    private float baseMaxSpeed = 0f;                       // 基础最高速度（用于技能修改）
    private float accelerationStartTime = 0f;
    private float lastFootstepTime = 0f;
    private Camera mainCamera;
    
    // 障碍物效果状态
    private bool isInObstacleEffect = false;               // 是否处于障碍物效果中
    private ObstacleEffectType currentObstacleEffect = ObstacleEffectType.None;
    private float obstacleEffectEndTime = 0f;              // 障碍物效果结束时间
    private float preObstacleSpeed = 0f;                   // 障碍物效果前的速度
    private HashSet<Collider> detectedObstacles = new HashSet<Collider>(); // 已检测过的障碍物
    
    // 当前状态
    public enum RunningState
    {
        Stopped,        // 停止状态
        Accelerating,   // 加速阶段
        MaxSpeed        // 最高速度阶段
    }
    
    // 障碍物效果类型
    public enum ObstacleEffectType
    {
        None,           // 无效果
        Stumble,        // 踉跄（Low/High障碍物）
        Slowing         // 减速（Slowing障碍物）
    }
    
    private RunningState currentState = RunningState.Stopped;
    
    // 事件
    public System.Action OnStartRunning;
    public System.Action OnReachMaxSpeed;
    public System.Action OnStopRunning;
    public System.Action<float> OnSpeedChanged;
    
    // 技能系统属性
    public SkillSystem SkillSystem => skillSystem;
    public bool IsSkillSystemEnabled => enableSkillSystem && skillSystem != null;
    
    void Start()
    {
        InitializeComponents();
        InitializeSettings();
        InitializeSkillSystem();
        
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
        
        // 获取动画组件
        if (characterAnimator == null)
        {
            characterAnimator = GetComponentInChildren<Animator>();
        }
        
        // 获取音频组件
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        // 获取技能系统组件
        if (enableSkillSystem && skillSystem == null)
        {
            skillSystem = GetComponent<SkillSystem>();
            if (skillSystem == null)
            {
                skillSystem = gameObject.AddComponent<SkillSystem>();
            }
        }
        
        // 获取主相机
        if (enableCameraFollow)
        {
            mainCamera = Camera.main;
            if (cameraTarget == null)
            {
                cameraTarget = transform;
            }
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
        lastFootstepTime = 0f;
    }
    
    /// <summary>
    /// 初始化技能系统
    /// </summary>
    private void InitializeSkillSystem()
    {
        if (!IsSkillSystemEnabled) return;
        
        // 订阅技能系统事件
        skillSystem.OnSkillActivated += OnSkillActivated;
        skillSystem.OnSkillDeactivated += OnSkillDeactivated;
        skillSystem.OnEnergyChanged += OnEnergyChanged;
        
        if (showDebugInfo)
        {
            Debug.Log("AdvancedAutoRunnerController: 技能系统已初始化");
        }
    }
    
    public override void UpdateMe()
    {
        if (!isRunning) return;
        
        UpdateObstacleEffects();
        DetectObstacles();
        UpdateSpeed();
        MoveCharacter();
        UpdateAnimation();
        UpdateAudio();
        UpdateParticles();
        UpdateCamera();
        
        if (showDebugInfo)
        {
            UpdateDebugInfo();
        }
    }
    
    /// <summary>
    /// 更新障碍物效果
    /// </summary>
    private void UpdateObstacleEffects()
    {
        if (!enableObstacleSystem || !isInObstacleEffect) return;
        
        // 检查障碍物效果是否结束
        if (Time.time >= obstacleEffectEndTime)
        {
            EndObstacleEffect();
        }
    }
    
    /// <summary>
    /// 检测前方障碍物
    /// </summary>
    private void DetectObstacles()
    {
        if (!enableObstacleSystem) return;
        
        // 计算检测起点和方向
        Vector3 detectionOrigin = transform.position + Vector3.up * 0.5f; // 稍微向上偏移避免地面干扰
        Vector3 detectionDirection = constrainToZAxis ? Vector3.forward : transform.forward;
        
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
                    
                    if (showDebugInfo)
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
    /// 更新速度逻辑
    /// </summary>
    private void UpdateSpeed()
    {
        float previousSpeed = currentSpeed;
        
        // 如果处于障碍物效果中，使用特殊的速度逻辑
        if (isInObstacleEffect)
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
    /// 更新正常状态下的速度
    /// </summary>
    private void UpdateNormalSpeed()
    {
        // 计算当前有效的最高速度（考虑技能效果）
        float effectiveMaxSpeed = CalculateEffectiveMaxSpeed();
        
        switch (currentState)
        {
            case RunningState.Stopped:
                currentSpeed = 0f;
                break;
                
            case RunningState.Accelerating:
                // 计算加速进度 (0到1)
                float accelerationProgress = (Time.time - accelerationStartTime) / accelerationTime;
                
                if (accelerationProgress >= 1f)
                {
                    // 加速完成，进入最高速度阶段
                    currentSpeed = effectiveMaxSpeed;
                    currentState = RunningState.MaxSpeed;
                    OnReachMaxSpeed?.Invoke();
                    PlayMaxSpeedEffects();
                    Debug.Log("AdvancedAutoRunnerController: 达到最高速度");
                }
                else
                {
                    // 使用动画曲线进行加速
                    float curveValue = accelerationCurve.Evaluate(accelerationProgress);
                    currentSpeed = Mathf.Lerp(startSpeed, effectiveMaxSpeed, curveValue);
                }
                break;
                
            case RunningState.MaxSpeed:
                currentSpeed = effectiveMaxSpeed;
                break;
        }
    }
    
    /// <summary>
    /// 更新障碍物效果状态下的速度
    /// </summary>
    private void UpdateObstacleEffectSpeed()
    {
        switch (currentObstacleEffect)
        {
            case ObstacleEffectType.Stumble:
                // 踉跄状态：保持最低速度
                currentSpeed = startSpeed;
                break;
                
            case ObstacleEffectType.Slowing:
                // 减速状态：速度减半
                float targetSpeed = preObstacleSpeed * slowingSpeedMultiplier;
                currentSpeed = Mathf.Max(targetSpeed, startSpeed); // 不低于最低速度
                break;
        }
    }
    
    /// <summary>
    /// 计算有效的最高速度（考虑技能效果）
    /// </summary>
    private float CalculateEffectiveMaxSpeed()
    {
        float effectiveSpeed = baseMaxSpeed;
        
        if (IsSkillSystemEnabled)
        {
            // 检查所有激活的技能，应用速度修改
            foreach (var skill in skillSystem.Skills)
            {
                if (skill.isActive && skill.affectsMovementSpeed)
                {
                    effectiveSpeed *= skill.speedMultiplier;
                }
            }
        }
        
        return effectiveSpeed;
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
    /// 更新动画
    /// </summary>
    private void UpdateAnimation()
    {
        if (characterAnimator == null) return;
        
        // 设置速度参数
        if (!string.IsNullOrEmpty(speedParameterName))
        {
            float normalizedSpeed = currentSpeed / maxSpeed;
            characterAnimator.SetFloat(speedParameterName, normalizedSpeed * animationSpeedMultiplier);
        }
        
        // 设置是否跑步参数
        if (!string.IsNullOrEmpty(isRunningParameterName))
        {
            characterAnimator.SetBool(isRunningParameterName, isRunning && currentSpeed > 0.1f);
        }
    }
    
    /// <summary>
    /// 更新音效
    /// </summary>
    private void UpdateAudio()
    {
        if (audioSource == null) return;
        
        // 播放脚步声
        if (footstepSound != null && currentSpeed > 0.1f)
        {
            float adjustedInterval = footstepInterval / (currentSpeed / maxSpeed + 0.5f);
            
            if (Time.time - lastFootstepTime >= adjustedInterval)
            {
                audioSource.pitch = Random.Range(0.8f, 1.2f);
                audioSource.PlayOneShot(footstepSound, 0.3f);
                lastFootstepTime = Time.time;
            }
        }
    }
    
    /// <summary>
    /// 更新粒子效果
    /// </summary>
    private void UpdateParticles()
    {
        // 加速粒子效果
        if (accelerationParticles != null)
        {
            if (currentState == RunningState.Accelerating && !accelerationParticles.isPlaying)
            {
                accelerationParticles.Play();
            }
            else if (currentState != RunningState.Accelerating && accelerationParticles.isPlaying)
            {
                accelerationParticles.Stop();
            }
        }
        
        // 最高速度粒子效果
        if (maxSpeedParticles != null)
        {
            if (currentState == RunningState.MaxSpeed && !maxSpeedParticles.isPlaying)
            {
                maxSpeedParticles.Play();
            }
            else if (currentState != RunningState.MaxSpeed && maxSpeedParticles.isPlaying)
            {
                maxSpeedParticles.Stop();
            }
        }
        
        // 灰尘粒子效果
        if (dustParticles != null)
        {
            if (currentSpeed > 0.1f && !dustParticles.isPlaying)
            {
                dustParticles.Play();
            }
            else if (currentSpeed <= 0.1f && dustParticles.isPlaying)
            {
                dustParticles.Stop();
            }
            
            // 根据速度调整粒子发射率
            if (dustParticles.isPlaying)
            {
                var emission = dustParticles.emission;
                emission.rateOverTime = (currentSpeed / maxSpeed) * 20f;
            }
        }
    }
    
    /// <summary>
    /// 更新相机跟随
    /// </summary>
    private void UpdateCamera()
    {
        if (!enableCameraFollow || mainCamera == null || cameraTarget == null) return;
        
        Vector3 targetPosition = cameraTarget.position + cameraOffset;
        mainCamera.transform.position = Vector3.Lerp(
            mainCamera.transform.position, 
            targetPosition, 
            cameraFollowSpeed * Time.deltaTime
        );
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
        PlayStartRunningEffects();
        
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
        StopAllEffects();
        
        Debug.Log("AdvancedAutoRunnerController: 停止跑酷");
    }
    
    /// <summary>
    /// 播放开始跑步效果
    /// </summary>
    private void PlayStartRunningEffects()
    {
        if (audioSource != null && startRunningSound != null)
        {
            audioSource.PlayOneShot(startRunningSound);
        }
    }
    
    /// <summary>
    /// 播放达到最高速度效果
    /// </summary>
    private void PlayMaxSpeedEffects()
    {
        if (audioSource != null && maxSpeedSound != null)
        {
            audioSource.PlayOneShot(maxSpeedSound);
        }
    }
    
    /// <summary>
    /// 停止所有效果
    /// </summary>
    private void StopAllEffects()
    {
        if (accelerationParticles != null && accelerationParticles.isPlaying)
        {
            accelerationParticles.Stop();
        }
        
        if (maxSpeedParticles != null && maxSpeedParticles.isPlaying)
        {
            maxSpeedParticles.Stop();
        }
        
        if (dustParticles != null && dustParticles.isPlaying)
        {
            dustParticles.Stop();
        }
    }
    
    // 公共接口方法（与基础版本相同）
    public void PauseRunning()
    {
        isRunning = false;
        Debug.Log("AdvancedAutoRunnerController: 暂停跑酷");
    }
    
    public void ResumeRunning()
    {
        isRunning = true;
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
        maxSpeed = baseMaxSpeed; // 保持向后兼容
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
    /// 更新调试信息
    /// </summary>
    private void UpdateDebugInfo()
    {
        // 可以在这里添加更多调试信息
    }
    
    /// <summary>
    /// 在Scene视图中绘制调试信息
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!showDebugInfo) return;
        
        // 绘制移动方向
        Gizmos.color = Color.blue;
        Vector3 forwardDirection = constrainToZAxis ? Vector3.forward : transform.TransformDirection(Vector3.forward);
        Gizmos.DrawLine(transform.position, transform.position + forwardDirection * 3f);
        
        // 绘制速度指示器
        if (Application.isPlaying && isRunning)
        {
            // 根据当前状态改变颜色
            Color stateColor;
            switch (currentState)
            {
                case RunningState.Stopped:
                    stateColor = Color.red;
                    break;
                case RunningState.Accelerating:
                    stateColor = Color.yellow;
                    break;
                case RunningState.MaxSpeed:
                    stateColor = Color.green;
                    break;
                default:
                    stateColor = Color.white;
                    break;
            }
            
            // 如果处于障碍物效果中，修改颜色
            if (isInObstacleEffect)
            {
                switch (currentObstacleEffect)
                {
                    case ObstacleEffectType.Stumble:
                        stateColor = Color.magenta; // 踉跄效果用紫色
                        break;
                    case ObstacleEffectType.Slowing:
                        stateColor = Color.cyan; // 减速效果用青色
                        break;
                }
            }
            
            Gizmos.color = stateColor;
            
            // 绘制速度向量
            Vector3 speedVector = forwardDirection * (currentSpeed / maxSpeed) * 5f;
            Gizmos.DrawLine(transform.position + Vector3.up * 2f, 
                           transform.position + Vector3.up * 2f + speedVector);
            
            // 绘制状态指示器
            Gizmos.DrawWireCube(transform.position + Vector3.up * 3f, Vector3.one * 0.5f);
            
            // 如果处于障碍物效果中，绘制效果范围
            if (isInObstacleEffect)
            {
                Gizmos.color = stateColor;
                Gizmos.DrawWireSphere(transform.position + Vector3.up * 4f, 1f);
                
                // 绘制剩余时间指示器
                float remainingTime = GetObstacleEffectRemainingTime();
                float totalDuration = currentObstacleEffect == ObstacleEffectType.Stumble ? 
                                    stumbleEffectDuration : slowingEffectDuration;
                float progress = 1f - (remainingTime / totalDuration);
                
                // 绘制进度圆弧（简化版）
                Vector3 center = transform.position + Vector3.up * 4f;
                float radius = 1.2f;
                int segments = Mathf.RoundToInt(progress * 16f);
                for (int i = 0; i < segments; i++)
                {
                    float angle1 = (i / 16f) * 360f * Mathf.Deg2Rad;
                    float angle2 = ((i + 1) / 16f) * 360f * Mathf.Deg2Rad;
                    Vector3 p1 = center + new Vector3(Mathf.Sin(angle1), 0, Mathf.Cos(angle1)) * radius;
                    Vector3 p2 = center + new Vector3(Mathf.Sin(angle2), 0, Mathf.Cos(angle2)) * radius;
                    Gizmos.DrawLine(p1, p2);
                }
            }
        }
        
        // 绘制相机跟随目标
        if (enableCameraFollow && cameraTarget != null)
        {
            Gizmos.color = Color.cyan;
            Vector3 cameraPos = cameraTarget.position + cameraOffset;
            Gizmos.DrawWireCube(cameraPos, Vector3.one * 0.5f);
            Gizmos.DrawLine(cameraTarget.position, cameraPos);
        }
        
        // 绘制障碍物检测范围
        if (enableObstacleSystem && Application.isPlaying)
        {
            Vector3 detectionOrigin = transform.position + Vector3.up * 0.5f;
            Vector3 detectionDirection = constrainToZAxis ? Vector3.forward : transform.forward;
            
            // 绘制检测射线
            Gizmos.color = Color.orange;
            Gizmos.DrawLine(detectionOrigin, detectionOrigin + detectionDirection * obstacleDetectionDistance);
            
            // 绘制检测球体范围
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f); // 半透明橙色
            Vector3 endPoint = detectionOrigin + detectionDirection * obstacleDetectionDistance;
            Gizmos.DrawWireSphere(endPoint, obstacleDetectionRadius);
            
            // 绘制已检测到的障碍物
            Gizmos.color = Color.red;
            foreach (Collider obstacle in detectedObstacles)
            {
                if (obstacle != null)
                {
                    Gizmos.DrawWireCube(obstacle.bounds.center, obstacle.bounds.size);
                }
            }
            
            // 绘制技能免疫状态
            if (IsSkillSystemEnabled && HasAnyImmunity())
            {
                Vector3 immunityCenter = transform.position + Vector3.up * 5f;
                
                // 为每个激活的免疫技能绘制护盾
                if (IsSkillActive("Jump"))
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawWireSphere(immunityCenter + Vector3.left * 1.5f, 0.8f);
                    // 绘制"Jump"标识
                    Gizmos.DrawLine(immunityCenter + Vector3.left * 1.5f + Vector3.up * 0.8f,
                                   immunityCenter + Vector3.left * 1.5f + Vector3.up * 1.2f);
                }
                
                if (IsSkillActive("Roll"))
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawWireSphere(immunityCenter, 0.8f);
                    // 绘制"Roll"标识
                    Gizmos.DrawWireCube(immunityCenter + Vector3.up * 1.0f, Vector3.one * 0.3f);
                }
                
                if (IsSkillActive("Dash"))
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireSphere(immunityCenter + Vector3.right * 1.5f, 0.8f);
                    // 绘制"Dash"标识（箭头状）
                    Vector3 dashPos = immunityCenter + Vector3.right * 1.5f;
                    Gizmos.DrawLine(dashPos + Vector3.back * 0.5f, dashPos + Vector3.forward * 0.5f);
                    Gizmos.DrawLine(dashPos + Vector3.forward * 0.3f, dashPos + Vector3.forward * 0.5f + Vector3.left * 0.2f);
                    Gizmos.DrawLine(dashPos + Vector3.forward * 0.3f, dashPos + Vector3.forward * 0.5f + Vector3.right * 0.2f);
                }
                
                // 绘制总体免疫护盾
                Gizmos.color = new Color(1f, 1f, 1f, 0.2f); // 半透明白色
                Gizmos.DrawWireSphere(immunityCenter, 2f);
            }
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
        footstepInterval = Mathf.Max(0.1f, footstepInterval);
        cameraFollowSpeed = Mathf.Max(0.1f, cameraFollowSpeed);
        animationSpeedMultiplier = Mathf.Max(0.1f, animationSpeedMultiplier);
        
        // 障碍物系统参数验证
        stumbleEffectDuration = Mathf.Max(0.1f, stumbleEffectDuration);
        slowingEffectDuration = Mathf.Max(0.1f, slowingEffectDuration);
        slowingSpeedMultiplier = Mathf.Clamp(slowingSpeedMultiplier, 0.1f, 1f);
        // obstacleDetectionDistance = Mathf.Max(0.1f, obstacleDetectionDistance);
        obstacleDetectionRadius = Mathf.Max(0.01f, obstacleDetectionRadius);
    }
    
    /// <summary>
    /// GUI显示速度信息
    /// </summary>
    private void OnGUI()
    {
        if (!showSpeedUI || !Application.isPlaying) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 250));
        GUILayout.Label($"当前速度: {currentSpeed:F2} / {maxSpeed:F2}");
        GUILayout.Label($"状态: {currentState}");
        GUILayout.Label($"加速进度: {GetAccelerationProgress():P0}");
        
        // 显示障碍物效果信息
        if (enableObstacleSystem)
        {
            GUILayout.Space(10);
            GUILayout.Label("=== 障碍物系统 ===");
            if (isInObstacleEffect)
            {
                float remainingTime = obstacleEffectEndTime - Time.time;
                GUILayout.Label($"当前效果: {currentObstacleEffect}");
                GUILayout.Label($"剩余时间: {remainingTime:F1}秒");
                GUILayout.Label($"效果前速度: {preObstacleSpeed:F2}");
            }
            else
            {
                GUILayout.Label("无障碍物效果");
            }
            
            // 显示技能免疫状态
            if (IsSkillSystemEnabled)
            {
                GUILayout.Space(5);
                GUILayout.Label("=== 技能免疫状态 ===");
                bool hasImmunity = false;
                
                if (IsSkillActive("Jump"))
                {
                    GUILayout.Label("🛡️ Jump: 免疫Low障碍物");
                    hasImmunity = true;
                }
                
                if (IsSkillActive("Roll"))
                {
                    GUILayout.Label("🛡️ Roll: 免疫High障碍物");
                    hasImmunity = true;
                }
                
                if (IsSkillActive("Dash"))
                {
                    GUILayout.Label("🛡️ Dash: 免疫Slowing障碍物");
                    hasImmunity = true;
                }
                
                if (!hasImmunity)
                {
                    GUILayout.Label("无技能免疫");
                }
            }
        }
        
        // 显示技能系统信息
        if (IsSkillSystemEnabled)
        {
            GUILayout.Space(10);
            GUILayout.Label("=== 技能系统 ===");
            GUILayout.Label($"精力: {skillSystem.Energy.CurrentEnergy:F1}/{skillSystem.Energy.MaxEnergy}");
            GUILayout.Label($"激活技能数: {skillSystem.ActiveSkillCount}");
        }
        
        if (GUILayout.Button(isRunning ? "停止" : "开始"))
        {
            if (isRunning)
                StopRunning();
            else
                StartRunning();
        }
        
        if (GUILayout.Button("重置"))
        {
            ResetRunning();
        }
        
        // 技能系统调试按钮
        if (IsSkillSystemEnabled)
        {
            if (GUILayout.Button("重置技能系统"))
            {
                skillSystem.ResetSkillSystem();
            }
        }
        
        // 障碍物系统调试按钮
        if (enableObstacleSystem && Application.isPlaying)
        {
            GUILayout.Space(5);
            if (GUILayout.Button("测试踉跄效果"))
            {
                StartObstacleEffect(ObstacleType.Low);
            }
            if (GUILayout.Button("测试减速效果"))
            {
                StartObstacleEffect(ObstacleType.Slowing);
            }
            if (isInObstacleEffect && GUILayout.Button("结束障碍物效果"))
            {
                EndObstacleEffect();
            }
        }
        
        GUILayout.EndArea();
    }
    
    // ===== 技能系统事件处理 =====
    
    /// <summary>
    /// 技能激活事件处理
    /// </summary>
    /// <param name="skill">激活的技能</param>
    private void OnSkillActivated(Skill skill)
    {
        if (showDebugInfo)
        {
            Debug.Log($"AdvancedAutoRunnerController: 技能 '{skill.skillName}' 已激活");
        }
        
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
    /// <param name="skill">结束的技能</param>
    private void OnSkillDeactivated(Skill skill)
    {
        if (showDebugInfo)
        {
            Debug.Log($"AdvancedAutoRunnerController: 技能 '{skill.skillName}' 已结束");
        }
        
        // 如果技能影响移动速度，立即更新速度
        if (skill.affectsMovementSpeed && currentState == RunningState.MaxSpeed)
        {
            // 在最高速度状态下，立即应用新的速度
            currentSpeed = CalculateEffectiveMaxSpeed();
            OnSpeedChanged?.Invoke(currentSpeed);
        }
    }
    
    /// <summary>
    /// 精力变化事件处理
    /// </summary>
    /// <param name="currentEnergy">当前精力</param>
    /// <param name="maxEnergy">最大精力</param>
    private void OnEnergyChanged(float currentEnergy, float maxEnergy)
    {
        // 可以在这里添加精力变化的视觉效果或音效
        if (showDebugInfo && currentEnergy <= 0f)
        {
            Debug.Log("AdvancedAutoRunnerController: 精力耗尽");
        }
    }
    
    // ===== 技能系统公共接口 =====
    
    /// <summary>
    /// 尝试激活技能
    /// </summary>
    /// <param name="skillIndex">技能索引</param>
    /// <returns>是否成功激活</returns>
    public bool TryActivateSkill(int skillIndex)
    {
        if (!IsSkillSystemEnabled) return false;
        return skillSystem.TryActivateSkill(skillIndex);
    }
    
    /// <summary>
    /// 根据技能名称激活技能
    /// </summary>
    /// <param name="skillName">技能名称</param>
    /// <returns>是否成功激活</returns>
    public bool TryActivateSkill(string skillName)
    {
        if (!IsSkillSystemEnabled) return false;
        return skillSystem.TryActivateSkill(skillName);
    }
    
    /// <summary>
    /// 检查技能是否激活
    /// </summary>
    /// <param name="skillIndex">技能索引</param>
    /// <returns>技能是否激活</returns>
    public bool IsSkillActive(int skillIndex)
    {
        if (!IsSkillSystemEnabled) return false;
        return skillSystem.IsSkillActive(skillIndex);
    }
    
    /// <summary>
    /// 根据技能名称检查技能是否激活
    /// </summary>
    /// <param name="skillName">技能名称</param>
    /// <returns>技能是否激活</returns>
    public bool IsSkillActive(string skillName)
    {
        if (!IsSkillSystemEnabled) return false;
        return skillSystem.IsSkillActive(skillName);
    }
    
    /// <summary>
    /// 获取技能对象
    /// </summary>
    /// <param name="skillIndex">技能索引</param>
    /// <returns>技能对象</returns>
    public Skill GetSkill(int skillIndex)
    {
        if (!IsSkillSystemEnabled) return null;
        return skillSystem.GetSkill(skillIndex);
    }
    
    /// <summary>
    /// 根据技能名称获取技能对象
    /// </summary>
    /// <param name="skillName">技能名称</param>
    /// <returns>技能对象</returns>
    public Skill GetSkill(string skillName)
    {
        if (!IsSkillSystemEnabled) return null;
        return skillSystem.GetSkill(skillName);
    }
    
    /// <summary>
    /// 获取当前精力百分比
    /// </summary>
    /// <returns>精力百分比 (0-1)</returns>
    public float GetEnergyPercentage()
    {
        if (!IsSkillSystemEnabled) return 1f;
        return skillSystem.Energy.EnergyPercentage;
    }
    
    /// <summary>
    /// 获取当前精力值
    /// </summary>
    /// <returns>当前精力值</returns>
    public float GetCurrentEnergy()
    {
        if (!IsSkillSystemEnabled) return 0f;
        return skillSystem.Energy.CurrentEnergy;
    }
    
    /// <summary>
    /// 获取基础最高速度（不包含技能效果）
    /// </summary>
    /// <returns>基础最高速度</returns>
    public float GetBaseMaxSpeed()
    {
        return baseMaxSpeed;
    }
    
    /// <summary>
    /// 获取有效最高速度（包含技能效果）
    /// </summary>
    /// <returns>有效最高速度</returns>
    public float GetEffectiveMaxSpeed()
    {
        return CalculateEffectiveMaxSpeed();
    }
    
    /// <summary>
    /// 清理资源
    /// </summary>
    private void OnDestroy()
    {
        // 清理技能系统事件订阅
        if (IsSkillSystemEnabled)
        {
            skillSystem.OnSkillActivated -= OnSkillActivated;
            skillSystem.OnSkillDeactivated -= OnSkillDeactivated;
            skillSystem.OnEnergyChanged -= OnEnergyChanged;
        }
    }
    
    /// <summary>
    /// 开始障碍物效果
    /// </summary>
    /// <param name="obstacleType">障碍物类型</param>
    public void StartObstacleEffect(ObstacleType obstacleType)
    {
        if (!enableObstacleSystem || !isRunning) return;
        
        // 功能1：在任意障碍物效果持续期间不再触发新的障碍物效果
        if (isInObstacleEffect)
        {
            if (showDebugInfo)
            {
                Debug.Log($"障碍物效果进行中({currentObstacleEffect})，忽略新的{obstacleType}效果");
            }
            return;
        }
        
        // 功能2：检查技能免疫
        if (IsImmuneToObstacle(obstacleType))
        {
            if (showDebugInfo)
            {
                Debug.Log($"技能免疫生效，忽略{obstacleType}效果");
            }
            return;
        }
        
        // 记录当前速度（用于减速效果）
        preObstacleSpeed = currentSpeed;
        
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
        PlayObstacleHitSound();
        
        if (showDebugInfo)
        {
            Debug.Log($"AdvancedAutoRunnerController: 触发障碍物效果 - {obstacleType}, 当前速度: {currentSpeed:F2}");
        }
    }
    
    /// <summary>
    /// 检查是否对指定障碍物类型免疫
    /// </summary>
    /// <param name="obstacleType">障碍物类型</param>
    /// <returns>是否免疫</returns>
    private bool IsImmuneToObstacle(ObstacleType obstacleType)
    {
        if (!IsSkillSystemEnabled) return false;
        
        switch (obstacleType)
        {
            case ObstacleType.Low:
                // Jump技能持续期间不受Low障碍物效果
                return IsSkillActive("Jump");
                
            case ObstacleType.High:
                // Roll技能持续期间不受High障碍物效果
                return IsSkillActive("Roll");
                
            case ObstacleType.Slowing:
                // Dash技能持续期间不受Slowing障碍物效果
                return IsSkillActive("Dash");
                
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
        if (characterAnimator != null && !string.IsNullOrEmpty(stumbleAnimationTrigger))
        {
            try
            {
                characterAnimator.SetTrigger(stumbleAnimationTrigger);
            }
            catch (System.Exception e)
            {
                if (showDebugInfo)
                {
                    Debug.LogWarning($"触发踉跄动画失败: {stumbleAnimationTrigger}, 错误: {e.Message}");
                }
            }
        }
        
        if (showDebugInfo)
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
        
        if (showDebugInfo)
        {
            Debug.Log($"开始减速效果，持续时间: {slowingEffectDuration}秒，速度倍数: {slowingSpeedMultiplier}");
        }
    }
    
    /// <summary>
    /// 结束障碍物效果
    /// </summary>
    private void EndObstacleEffect()
    {
        if (!isInObstacleEffect) return;
        
        ObstacleEffectType previousEffect = currentObstacleEffect;
        
        // 重置障碍物效果状态
        isInObstacleEffect = false;
        currentObstacleEffect = ObstacleEffectType.None;
        obstacleEffectEndTime = 0f;
        
        // 重新开始加速
        RestartAcceleration();
        
        if (showDebugInfo)
        {
            Debug.Log($"障碍物效果结束: {previousEffect}，重新开始加速");
        }
    }
    
    /// <summary>
    /// 重新开始加速
    /// </summary>
    private void RestartAcceleration()
    {
        if (!isRunning) return;
        
        currentState = RunningState.Accelerating;
        accelerationStartTime = Time.time;
        
        if (showDebugInfo)
        {
            Debug.Log("重新开始加速阶段");
        }
    }
    
    /// <summary>
    /// 播放障碍物碰撞音效
    /// </summary>
    private void PlayObstacleHitSound()
    {
        if (audioSource != null && obstacleHitSound != null)
        {
            audioSource.PlayOneShot(obstacleHitSound, 0.7f);
        }
    }
    
    /// <summary>
    /// 处理障碍物碰撞（由Trigger调用）
    /// </summary>
    /// <param name="obstacleComponent">障碍物组件</param>
    public void OnObstacleHit(Obstacles obstacleComponent)
    {
        if (!enableObstacleSystem || !isRunning || obstacleComponent == null) return;
        
        // 如果已经在障碍物效果中，根据策略决定是否重新触发
        if (isInObstacleEffect)
        {
            // 策略：重新开始计时（可以根据需求调整）
            if (showDebugInfo)
            {
                Debug.Log("已在障碍物效果中，重新开始计时");
            }
        }
        
        // 获取第一个障碍物类型（如果有多个类型，只处理第一个）
        if (obstacleComponent.obstacleTypes != null && obstacleComponent.obstacleTypes.Count > 0)
        {
            ObstacleType obstacleType = obstacleComponent.obstacleTypes[0];
            StartObstacleEffect(obstacleType);
        }
    }
    
    /// <summary>
    /// 检查是否处于障碍物效果中
    /// </summary>
    /// <returns>是否处于障碍物效果中</returns>
    public bool IsInObstacleEffect()
    {
        return isInObstacleEffect;
    }
    
    /// <summary>
    /// 获取当前障碍物效果类型
    /// </summary>
    /// <returns>当前障碍物效果类型</returns>
    public ObstacleEffectType GetCurrentObstacleEffect()
    {
        return currentObstacleEffect;
    }
    
    /// <summary>
    /// 获取障碍物效果剩余时间
    /// </summary>
    /// <returns>剩余时间（秒）</returns>
    public float GetObstacleEffectRemainingTime()
    {
        if (!isInObstacleEffect) return 0f;
        return Mathf.Max(0f, obstacleEffectEndTime - Time.time);
    }
    
    /// <summary>
    /// 强制结束障碍物效果（调试用）
    /// </summary>
    public void ForceEndObstacleEffect()
    {
        if (isInObstacleEffect)
        {
            EndObstacleEffect();
        }
    }
    
    /// <summary>
    /// 设置障碍物系统启用状态
    /// </summary>
    /// <param name="enabled">是否启用</param>
    public void SetObstacleSystemEnabled(bool enabled)
    {
        enableObstacleSystem = enabled;
        
        // 如果禁用时正在效果中，结束效果
        if (!enabled && isInObstacleEffect)
        {
            EndObstacleEffect();
        }
    }
    
    /// <summary>
    /// 检查对特定障碍物类型是否免疫
    /// </summary>
    /// <param name="obstacleType">障碍物类型</param>
    /// <returns>是否免疫</returns>
    public bool IsImmuneToObstacleType(ObstacleType obstacleType)
    {
        return IsImmuneToObstacle(obstacleType);
    }
    
    /// <summary>
    /// 获取当前激活的免疫技能列表
    /// </summary>
    /// <returns>免疫技能名称列表</returns>
    public System.Collections.Generic.List<string> GetActiveImmunitySkills()
    {
        var immunitySkills = new System.Collections.Generic.List<string>();
        
        if (!IsSkillSystemEnabled) return immunitySkills;
        
        if (IsSkillActive("Jump"))
            immunitySkills.Add("Jump");
            
        if (IsSkillActive("Roll"))
            immunitySkills.Add("Roll");
            
        if (IsSkillActive("Dash"))
            immunitySkills.Add("Dash");
            
        return immunitySkills;
    }
    
    /// <summary>
    /// 检查是否有任何免疫效果激活
    /// </summary>
    /// <returns>是否有免疫效果</returns>
    public bool HasAnyImmunity()
    {
        if (!IsSkillSystemEnabled) return false;
        
        return IsSkillActive("Jump") || IsSkillActive("Roll") || IsSkillActive("Dash");
    }
    
    /// <summary>
    /// 获取对特定障碍物类型提供免疫的技能名称
    /// </summary>
    /// <param name="obstacleType">障碍物类型</param>
    /// <returns>提供免疫的技能名称，如果没有则返回null</returns>
    public string GetImmunitySkillForObstacle(ObstacleType obstacleType)
    {
        if (!IsSkillSystemEnabled) return null;
        
        switch (obstacleType)
        {
            case ObstacleType.Low:
                return IsSkillActive("Jump") ? "Jump" : null;
                
            case ObstacleType.High:
                return IsSkillActive("Roll") ? "Roll" : null;
                
            case ObstacleType.Slowing:
                return IsSkillActive("Dash") ? "Dash" : null;
                
            default:
                return null;
        }
    }
} 