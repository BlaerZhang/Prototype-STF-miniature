using UnityEngine;
using JoostenProductions;

/// <summary>
/// 3D自动跑酷控制器
/// 玩家无需任何输入，角色自动在Z轴方向前进
/// 支持起步速度、加速时间和最高速度设置
/// </summary>
public class AutoRunnerLinearController : OverridableMonoBehaviour
{
    [Header("自动跑酷参数")]
    [SerializeField] private float startSpeed = 2f;        // 起步速度（最低速度）
    [SerializeField] private float accelerationTime = 3f;  // 加速时间（从起步速度到最高速度所需时间）
    [SerializeField] private float maxSpeed = 10f;         // 最高速度
    
    [Header("控制设置")]
    [SerializeField] private bool autoStart = true;        // 是否自动开始
    [SerializeField] private bool useRigidbody = true;     // 是否使用Rigidbody物理移动
    
    [Header("调试信息")]
    [SerializeField] private bool showDebugInfo = true;    // 是否显示调试信息
    
    // 私有变量
    private Rigidbody rb;
    private CharacterController characterController;
    private bool isRunning = false;
    private float currentSpeed = 0f;
    private float accelerationStartTime = 0f;
    
    // 当前状态
    public enum RunningState
    {
        Stopped,        // 停止状态
        Accelerating,   // 加速阶段
        MaxSpeed        // 最高速度阶段
    }
    
    private RunningState currentState = RunningState.Stopped;
    
    void Start()
    {
        // 获取移动组件
        rb = GetComponent<Rigidbody>();
        characterController = GetComponent<CharacterController>();
        
        // 验证移动组件
        if (useRigidbody && rb == null)
        {
            Debug.LogWarning("AutoRunnerLinearController: 未找到Rigidbody组件，将尝试使用CharacterController");
            useRigidbody = false;
        }
        
        if (!useRigidbody && characterController == null)
        {
            Debug.LogWarning("AutoRunnerLinearController: 未找到CharacterController组件，将使用Transform移动");
        }
        
        // 初始化
        currentSpeed = 0f;
        currentState = RunningState.Stopped;
        
        if (autoStart)
        {
            StartRunning();
        }
    }
    
    public override void UpdateMe()
    {
        if (!isRunning) return;
        
        UpdateSpeed();
        MoveCharacter();
        
        if (showDebugInfo)
        {
            UpdateDebugInfo();
        }
    }
    
    /// <summary>
    /// 更新速度逻辑
    /// </summary>
    private void UpdateSpeed()
    {
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
                    currentSpeed = maxSpeed;
                    currentState = RunningState.MaxSpeed;
                    Debug.Log("AutoRunnerLinearController: 达到最高速度");
                }
                else
                {
                    // 线性插值从起步速度到最高速度
                    currentSpeed = Mathf.Lerp(startSpeed, maxSpeed, accelerationProgress);
                }
                break;
                
            case RunningState.MaxSpeed:
                currentSpeed = maxSpeed;
                break;
        }
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
            Vector3 newPosition = rb.position + transform.TransformDirection(movement);
            rb.MovePosition(newPosition);
        }
        else if (characterController != null)
        {
            // 使用CharacterController移动
            characterController.Move(transform.TransformDirection(movement));
        }
        else
        {
            // 使用Transform移动
            transform.Translate(movement);
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
        
        Debug.Log($"AutoRunnerLinearController: 开始跑酷 - 起步速度: {startSpeed}, 加速时间: {accelerationTime}s, 最高速度: {maxSpeed}");
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
        
        Debug.Log("AutoRunnerLinearController: 停止跑酷");
    }
    
    /// <summary>
    /// 暂停跑酷（保持当前速度）
    /// </summary>
    public void PauseRunning()
    {
        isRunning = false;
        Debug.Log("AutoRunnerLinearController: 暂停跑酷");
    }
    
    /// <summary>
    /// 恢复跑酷
    /// </summary>
    public void ResumeRunning()
    {
        isRunning = true;
        Debug.Log("AutoRunnerLinearController: 恢复跑酷");
    }
    
    /// <summary>
    /// 重置跑酷（重新开始加速）
    /// </summary>
    public void ResetRunning()
    {
        StopRunning();
        StartRunning();
    }
    
    /// <summary>
    /// 设置起步速度
    /// </summary>
    public void SetStartSpeed(float speed)
    {
        startSpeed = Mathf.Max(0f, speed);
        if (startSpeed > maxSpeed)
        {
            maxSpeed = startSpeed;
        }
    }
    
    /// <summary>
    /// 设置最高速度
    /// </summary>
    public void SetMaxSpeed(float speed)
    {
        maxSpeed = Mathf.Max(startSpeed, speed);
    }
    
    /// <summary>
    /// 设置加速时间
    /// </summary>
    public void SetAccelerationTime(float time)
    {
        accelerationTime = Mathf.Max(0.1f, time);
    }
    
    /// <summary>
    /// 获取当前速度
    /// </summary>
    public float GetCurrentSpeed()
    {
        return currentSpeed;
    }
    
    /// <summary>
    /// 获取当前状态
    /// </summary>
    public RunningState GetCurrentState()
    {
        return currentState;
    }
    
    /// <summary>
    /// 获取加速进度 (0-1)
    /// </summary>
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
        // 这里可以添加更多调试信息的显示
        // 例如在Scene视图中显示速度信息等
    }
    
    /// <summary>
    /// 在Scene视图中绘制调试信息
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!showDebugInfo) return;
        
        // 绘制移动方向
        Gizmos.color = Color.blue;
        Vector3 forwardDirection = transform.TransformDirection(Vector3.forward);
        Gizmos.DrawLine(transform.position, transform.position + forwardDirection * 3f);
        
        // 绘制速度指示器
        if (Application.isPlaying && isRunning)
        {
            // 根据当前状态改变颜色
            switch (currentState)
            {
                case RunningState.Stopped:
                    Gizmos.color = Color.red;
                    break;
                case RunningState.Accelerating:
                    Gizmos.color = Color.yellow;
                    break;
                case RunningState.MaxSpeed:
                    Gizmos.color = Color.green;
                    break;
            }
            
            // 绘制速度向量
            Vector3 speedVector = forwardDirection * (currentSpeed / maxSpeed) * 5f;
            Gizmos.DrawLine(transform.position + Vector3.up * 2f, 
                           transform.position + Vector3.up * 2f + speedVector);
            
            // 绘制状态指示器
            Gizmos.DrawWireCube(transform.position + Vector3.up * 3f, Vector3.one * 0.5f);
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
} 