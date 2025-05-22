using UnityEngine;
using UnityEngine.InputSystem;
using JoostenProductions;

public enum ControlMode
{
    Manual,  // 手动控制
    AI       // AI控制
}

public class AutoRunnerController : OverridableMonoBehaviour
{
    [Header("坦克控制参数")]
    [SerializeField] private float acceleration = 1000f;      // 前进加速度力量
    [SerializeField] private float brakeForce = 1500f;       // 刹车力量（比前进更强）
    [SerializeField] private float turnSpeed = 180f;         // 转向速度（度/秒）
    [SerializeField] private float frictionCoefficient = 500f; // 摩擦系数
    
    [Header("可选参数")]
    [SerializeField] private float dragCoefficient = 2f; // 拖拽系数
    
    [Header("控制模式")]
    [SerializeField] private ControlMode controlMode = ControlMode.Manual;
    
    private Rigidbody2D rb2D;
    private float horizontalInput;  // X轴输入（前进/刹车）
    private float verticalInput;    // Y轴输入（转向）

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 获取Rigidbody2D组件
        rb2D = GetComponent<Rigidbody2D>();
        
        if (rb2D == null)
        {
            Debug.LogError("AutoRunnerController需要Rigidbody2D组件!");
            return;
        }
        
        // 设置拖拽以提供更好的控制感觉
        rb2D.linearDamping = dragCoefficient;
    }

    public override void UpdateMe()
    {
        // 根据控制模式读取输入
        if (controlMode == ControlMode.Manual)
        {
            ReadManualInput();
        }
        // AI模式下输入由外部设置，这里不需要读取
    }

    public override void FixedUpdateMe()
    {
        // 应用物理控制
        HandleMovement();
        HandleRotation();
        HandleFriction();
    }
    
    private void ReadManualInput()
    {
        // 使用New Input System读取输入
        horizontalInput = 0f;
        verticalInput = 0f;
        
        if (Keyboard.current != null)
        {
            // 前进/刹车输入
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
                horizontalInput += 1f;  // 前进
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
                horizontalInput -= 1f;  // 刹车
                
            // 转向输入
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
                verticalInput += 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
                verticalInput -= 1f;
        }
    }
    
    // AI控制接口
    public void SetAIInput(float horizontal, float vertical)
    {
        if (controlMode == ControlMode.AI)
        {
            horizontalInput = Mathf.Clamp(horizontal, -1f, 1f);
            verticalInput = Mathf.Clamp(vertical, -1f, 1f);
        }
    }
    
    public void SetControlMode(ControlMode newMode)
    {
        controlMode = newMode;
        
        // 切换到AI模式时清零输入
        if (controlMode == ControlMode.AI)
        {
            horizontalInput = 0f;
            verticalInput = 0f;
        }
    }
    
    public ControlMode GetControlMode()
    {
        return controlMode;
    }
    
    // 获取当前输入值（用于调试）
    public Vector2 GetCurrentInput()
    {
        return new Vector2(horizontalInput, verticalInput);
    }
    
    private void HandleMovement()
    {
        if (rb2D == null) return;
        
        if (horizontalInput > 0f)
        {
            // 前进：向前施加力
            Vector2 forceDirection = transform.up * horizontalInput * acceleration;
            rb2D.AddForce(forceDirection * Time.fixedDeltaTime);
        }
        else if (horizontalInput < 0f)
        {
            // 刹车：向后施加力来减速（但不会让坦克后退）
            Vector2 currentVelocity = rb2D.linearVelocity;
            Vector2 forwardDirection = transform.up;
            
            // 只有当坦克在前进时才应用刹车力
            float forwardSpeed = Vector2.Dot(currentVelocity, forwardDirection);
            if (forwardSpeed > 0.1f)  // 有一定前进速度时才刹车
            {
                Vector2 brakeDirection = -forwardDirection * Mathf.Abs(horizontalInput) * brakeForce;
                rb2D.AddForce(brakeDirection * Time.fixedDeltaTime);
            }
        }
    }
    
    private void HandleRotation()
    {
        if (rb2D == null) return;
        
        // 转向逻辑（不需要加速度，直接设置角速度）
        float rotationAmount = -verticalInput * turnSpeed * Time.fixedDeltaTime;
        transform.Rotate(0, 0, rotationAmount);
    }
    
    private void HandleFriction()
    {
        if (rb2D == null) return;
        
        // 当没有输入时应用摩擦力使坦克停下
        if (Mathf.Approximately(horizontalInput, 0f))
        {
            Vector2 frictionForce = -rb2D.linearVelocity.normalized * frictionCoefficient * Time.fixedDeltaTime;
            
            // 避免过度摩擦导致反向运动
            if (frictionForce.magnitude > rb2D.linearVelocity.magnitude)
            {
                rb2D.linearVelocity = Vector2.zero;
            }
            else
            {
                rb2D.AddForce(frictionForce);
            }
        }
    }
    
    // 可选：在Scene视图中显示调试信息
    private void OnDrawGizmos()
    {
        if (Application.isPlaying && rb2D != null)
        {
            // 显示速度向量
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + (Vector3)rb2D.linearVelocity);
            
            // 显示朝向
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + transform.up * 2f);
            
            // 显示控制模式和当前输入状态
            if (horizontalInput > 0)
                Gizmos.color = Color.green;  // 前进 - 绿色
            else if (horizontalInput < 0)
                Gizmos.color = Color.red;    // 刹车 - 红色
            else
                Gizmos.color = controlMode == ControlMode.AI ? Color.yellow : Color.white;  // 空档
                
            Gizmos.DrawWireCube(transform.position + Vector3.up * 3f, Vector3.one * 0.5f);
        }
    }
}
