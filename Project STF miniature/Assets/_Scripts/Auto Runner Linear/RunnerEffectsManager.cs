using UnityEngine;

/// <summary>
/// 跑酷特效管理器 - 处理粒子效果、动画和音频
/// </summary>
public class RunnerEffectsManager : MonoBehaviour
{
    [Header("动画设置")]
    [SerializeField] private Animator characterAnimator;   // 角色动画控制器
    [SerializeField] private string speedParameterName = "Speed"; // 速度参数名
    [SerializeField] private string isRunningParameterName = "IsRunning"; // 是否跑步参数名
    [SerializeField] private string stumbleAnimationTrigger = "Stumble"; // 踉跄动画触发器名称
    [SerializeField] private string slowingAnimationBool = "isSlowing"; // 减速动画bool值名称
    [SerializeField] private float animationSpeedMultiplier = 1f; // 动画速度倍数
    
    [Header("音效设置")]
    [SerializeField] private AudioSource audioSource;     // 音频源
    [SerializeField] private AudioClip startRunningSound; // 开始跑步音效
    [SerializeField] private AudioClip maxSpeedSound;     // 达到最高速度音效
    [SerializeField] private AudioClip footstepSound;     // 脚步声
    [SerializeField] private AudioClip obstacleHitSound;  // 障碍物碰撞音效
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
    
    // 私有变量
    private AutoRunnerLinearController runnerController;
    private Camera mainCamera;
    private float lastFootstepTime = 0f;
    
    /// <summary>
    /// 初始化特效管理器
    /// </summary>
    public void Initialize(AutoRunnerLinearController controller)
    {
        runnerController = controller;
        
        // 初始化组件
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        if (characterAnimator == null)
        {
            characterAnimator = GetComponentInChildren<Animator>();
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
    }
    
    /// <summary>
    /// 更新特效
    /// </summary>
    public void UpdateEffects(AutoRunnerLinearController.RunningState currentState, float currentSpeed, float maxSpeed)
    {
        UpdateAnimation(currentState, currentSpeed, maxSpeed);
        UpdateAudio(currentSpeed, maxSpeed);
        UpdateParticles(currentState, currentSpeed);
        UpdateCamera();
    }
    
    /// <summary>
    /// 更新动画
    /// </summary>
    private void UpdateAnimation(AutoRunnerLinearController.RunningState currentState, float currentSpeed, float maxSpeed)
    {
        if (characterAnimator == null) return;
        
        bool isRunning = currentState != AutoRunnerLinearController.RunningState.Stopped;
        
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
    private void UpdateAudio(float currentSpeed, float maxSpeed)
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
    private void UpdateParticles(AutoRunnerLinearController.RunningState currentState, float currentSpeed)
    {
        // 加速粒子效果
        if (accelerationParticles != null)
        {
            if (currentState == AutoRunnerLinearController.RunningState.Accelerating && !accelerationParticles.isPlaying)
            {
                accelerationParticles.Play();
            }
            else if (currentState != AutoRunnerLinearController.RunningState.Accelerating && accelerationParticles.isPlaying)
            {
                accelerationParticles.Stop();
            }
        }
        
        // 最高速度粒子效果
        if (maxSpeedParticles != null)
        {
            if (currentState == AutoRunnerLinearController.RunningState.MaxSpeed && !maxSpeedParticles.isPlaying)
            {
                maxSpeedParticles.Play();
            }
            else if (currentState != AutoRunnerLinearController.RunningState.MaxSpeed && maxSpeedParticles.isPlaying)
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
                emission.rateOverTime = Mathf.Max(1f, currentSpeed * 2f);
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
    /// 播放开始跑步效果
    /// </summary>
    public void PlayStartRunningEffects()
    {
        if (audioSource != null && startRunningSound != null)
        {
            audioSource.PlayOneShot(startRunningSound);
        }
    }
    
    /// <summary>
    /// 播放达到最高速度效果
    /// </summary>
    public void PlayMaxSpeedEffects()
    {
        if (audioSource != null && maxSpeedSound != null)
        {
            audioSource.PlayOneShot(maxSpeedSound);
        }
    }
    
    /// <summary>
    /// 播放障碍物碰撞音效
    /// </summary>
    public void PlayObstacleHitSound()
    {
        if (audioSource != null && obstacleHitSound != null)
        {
            audioSource.PlayOneShot(obstacleHitSound, 0.7f);
        }
    }
    
    /// <summary>
    /// 播放踉跄动画
    /// </summary>
    public void PlayStumbleAnimation()
    {
        if (characterAnimator != null && !string.IsNullOrEmpty(stumbleAnimationTrigger))
        {
            try
            {
                characterAnimator.SetTrigger(stumbleAnimationTrigger);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"触发踉跄动画失败: {stumbleAnimationTrigger}, 错误: {e.Message}");
            }
        }
    }

    /// <summary>
    /// 播放减速动画
    /// </summary>
    public void StartSlowingAnimation()
    {
        if (characterAnimator != null && !string.IsNullOrEmpty(slowingAnimationBool))
        {
            characterAnimator.SetBool(slowingAnimationBool, true);
        }
    }

    /// <summary>
    /// 重置减速动画
    /// </summary>
    public void ResetSlowingAnimation()
    {
        if (characterAnimator != null && !string.IsNullOrEmpty(slowingAnimationBool))
        {
            characterAnimator.SetBool(slowingAnimationBool, false);
        }
    }
    
    /// <summary>
    /// 停止所有效果
    /// </summary>
    public void StopAllEffects()
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
} 