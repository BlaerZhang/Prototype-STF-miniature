using UnityEngine;
using Unity.Mathematics;
using UnityEngine.Splines;
using JoostenProductions;

[RequireComponent(typeof(AutoRunnerController))]
public class TankAIController : OverridableMonoBehaviour
{
    [Header("AI控制参数")]
    [SerializeField] private SplineContainer splineContainer;
    [SerializeField] private float waypointThreshold = 1f;  // 到达waypoint的距离阈值
    [SerializeField] private bool loopPath = true;          // 是否循环路径
    [SerializeField] private bool autoStart = true;        // 是否自动开始
    
    [Header("AI行为参数")]
    [SerializeField] private float steeringSensitivity = 2f; // 转向敏感度
    [SerializeField] private float accelerationMultiplier = 1f;    // 加速倍数（油门）
    [SerializeField] private float brakeMultiplier = 0.8f;         // 刹车倍数
    [SerializeField] private float forwardThreshold = 0.1f;        // 前进阈值（目标需要在多前方才加速）
    
    [Header("避障系统")]
    [SerializeField] private float visionDistance = 3f;    // 视距
    [SerializeField] private float tankWidth = 1f;         // 坦克宽度（CircleCast半径）
    [SerializeField] private LayerMask obstacleLayerMask = -1; // 障碍物图层
    [SerializeField] private float avoidanceStrength = 1f; // 避障强度（0-1，越高越优先避障）
    [SerializeField] private float brakeOnObstacle = 0.5f; // 遇到障碍物时的刹车强度
    
    [Header("调试选项")]
    [SerializeField] private bool showDebugGizmos = true;
    [SerializeField] private Color pathColor = Color.yellow;
    [SerializeField] private Color targetColor = Color.red;
    [SerializeField] private Color directionColor = Color.cyan;
    [SerializeField] private Color visionColor = Color.blue;
    [SerializeField] private Color obstacleColor = Color.red;
    
    private AutoRunnerController tankController;
    private bool isActive = false;
    private int currentWaypointIndex = 0;  // 当前目标knot索引
    
    // 当前目标位置
    private Vector3 currentTarget;
    private Vector3 lastPosition;
    private float stuckTimer = 0f;
    private const float stuckThreshold = 2f; // 卡住检测时间
    
    // 避障相关
    private bool hasObstacle = false;
    private Vector3 obstaclePoint;
    private Vector3 obstacleNormal;
    private Vector3 avoidanceDirection;
    
    void Start()
    {
        // 获取坦克控制器组件
        tankController = GetComponent<AutoRunnerController>();
        
        if (tankController == null)
        {
            Debug.LogError("TankAIController需要AutoRunnerController组件!");
            return;
        }
        
        if (splineContainer == null)
        {
            Debug.LogError("TankAIController需要SplineContainer!");
            return;
        }
        
        lastPosition = transform.position;
        
        if (autoStart)
        {
            StartAI();
        }
    }
    
    public override void UpdateMe()
    {
        if (!isActive || splineContainer == null || tankController == null)
            return;
            
        UpdateTarget();
        DetectObstacles();
        ControlTank();
        CheckIfStuck();
    }
    
    private void UpdateTarget()
    {
        if (splineContainer.Spline.Count == 0) return;
        
        // 获取当前目标knot的世界坐标
        BezierKnot currentKnot = splineContainer.Spline[currentWaypointIndex];
        currentTarget = splineContainer.transform.TransformPoint(currentKnot.Position);
        
        // 检查是否到达当前目标
        float distanceToTarget = Vector3.Distance(transform.position, currentTarget);
        
        if (distanceToTarget < waypointThreshold)
        {
            // 移动到下一个waypoint
            AdvanceToNextWaypoint();
        }
    }
    
    private void DetectObstacles()
    {
        Vector3 tankForward = transform.up;
        Vector3 rayOrigin = transform.position;
        
        // 使用CircleCast2D检测前方障碍物
        RaycastHit2D hit = Physics2D.CircleCast(
            rayOrigin, 
            tankWidth * 0.5f,  // 半径为坦克宽度的一半
            tankForward, 
            visionDistance, 
            obstacleLayerMask
        );
        
        if (hit.collider != null)
        {
            hasObstacle = true;
            obstaclePoint = hit.point;
            obstacleNormal = hit.normal;
            
            // 计算反射方向作为避障方向
            Vector3 incidentDirection = tankForward;
            avoidanceDirection = Vector3.Reflect(incidentDirection, obstacleNormal);
        }
        else
        {
            hasObstacle = false;
        }
    }
    
    private void AdvanceToNextWaypoint()
    {
        currentWaypointIndex++;
        
        // 处理路径结束
        if (currentWaypointIndex >= splineContainer.Spline.Count)
        {
            if (loopPath)
            {
                currentWaypointIndex = 0;
            }
            else
            {
                currentWaypointIndex = splineContainer.Spline.Count - 1;
                StopAI(); // 停止AI
            }
        }
    }
    
    private void ControlTank()
    {
        Vector3 directionToTarget = (currentTarget - transform.position).normalized;
        Vector3 tankForward = transform.up; // 坦克前进方向
        
        // 计算最终转向方向（混合waypoint方向和避障方向）
        Vector3 finalDirection = directionToTarget;
        
        if (hasObstacle)
        {
            // 混合waypoint方向和避障方向
            finalDirection = Vector3.Lerp(directionToTarget, avoidanceDirection, avoidanceStrength).normalized;
        }
        
        // 计算前进方向的匹配度（基于最终方向）
        float forwardDot = Vector3.Dot(tankForward, finalDirection);
        
        // 油门/刹车控制逻辑
        float accelerationInput = 0f;
        
        if (hasObstacle)
        {
            // 遇到障碍物时减速
            accelerationInput = -brakeOnObstacle;
        }
        else if (forwardDot > forwardThreshold)
        {
            // 目标在前方，使用油门加速
            accelerationInput = forwardDot * accelerationMultiplier;
        }
        else if (forwardDot < -forwardThreshold)
        {
            // 目标在后方，使用刹车减速
            accelerationInput = forwardDot * brakeMultiplier; // 负值表示刹车
        }
        // forwardDot在阈值内时，既不加速也不刹车，让坦克自然减速
        
        // 计算转向输入 (基于最终方向的叉积)
        Vector3 cross = Vector3.Cross(tankForward, finalDirection);
        float turnInput = -cross.z * steeringSensitivity;
        
        // 限制输入范围
        accelerationInput = Mathf.Clamp(accelerationInput, -1f, 1f);
        turnInput = Mathf.Clamp(turnInput, -1f, 1f);
        
        // 应用输入到坦克控制器
        tankController.SetAIInput(accelerationInput, turnInput);
    }
    
    private void CheckIfStuck()
    {
        // 检测坦克是否卡住不动
        float distanceMoved = Vector3.Distance(transform.position, lastPosition);
        
        if (distanceMoved < 0.1f) // 移动距离很小
        {
            stuckTimer += Time.deltaTime;
            
            if (stuckTimer > stuckThreshold)
            {
                // 尝试解除卡住状态：前进到下一个waypoint
                AdvanceToNextWaypoint();
                stuckTimer = 0f;
            }
        }
        else
        {
            stuckTimer = 0f;
        }
        
        lastPosition = transform.position;
    }
    
    public void StartAI()
    {
        isActive = true;
        currentWaypointIndex = 0;
        stuckTimer = 0f;
        
        // 设置坦克控制器为AI模式
        if (tankController != null)
        {
            tankController.SetControlMode(ControlMode.AI);
        }
        
        Debug.Log("Tank AI 开始运行 - 仅前进/刹车模式 + 避障系统");
    }
    
    public void StopAI()
    {
        isActive = false;
        
        // 恢复手动控制模式
        if (tankController != null)
        {
            tankController.SetControlMode(ControlMode.Manual);
            tankController.SetAIInput(0f, 0f); // 清零输入
        }
        
        Debug.Log("Tank AI 停止运行");
    }
    
    public void SetSpline(SplineContainer newSpline)
    {
        splineContainer = newSpline;
        currentWaypointIndex = 0;
        stuckTimer = 0f;
    }
    
    public bool IsActive()
    {
        return isActive;
    }
    
    public float GetCurrentProgress()
    {
        if (splineContainer.Spline.Count == 0) return 0f;
        return (float)currentWaypointIndex / splineContainer.Spline.Count;
    }
    
    public int GetCurrentWaypointIndex()
    {
        return currentWaypointIndex;
    }
    
    public int GetTotalWaypoints()
    {
        return splineContainer != null ? splineContainer.Spline.Count : 0;
    }
    
    // 获取避障状态（用于外部查询）
    public bool HasObstacle()
    {
        return hasObstacle;
    }
    
    public Vector3 GetObstaclePoint()
    {
        return obstaclePoint;
    }
    
    private void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;
        
        // 绘制spline路径和knot点
        if (splineContainer != null && splineContainer.Spline.Count > 0)
        {
            // 绘制knot点（实际的waypoints）
            for (int i = 0; i < splineContainer.Spline.Count; i++)
            {
                BezierKnot knot = splineContainer.Spline[i];
                Vector3 knotPosition = splineContainer.transform.TransformPoint(knot.Position);
                
                // 当前目标用不同颜色显示
                if (isActive && i == currentWaypointIndex)
                {
                    Gizmos.color = targetColor;
                    Gizmos.DrawWireSphere(knotPosition, waypointThreshold);
                }
                else
                {
                    Gizmos.color = pathColor;
                    Gizmos.DrawWireSphere(knotPosition, 0.3f);
                }
                
                // 绘制knot之间的连线
                if (i < splineContainer.Spline.Count - 1)
                {
                    BezierKnot nextKnot = splineContainer.Spline[i + 1];
                    Vector3 nextPosition = splineContainer.transform.TransformPoint(nextKnot.Position);
                    Gizmos.color = pathColor;
                    Gizmos.DrawLine(knotPosition, nextPosition);
                }
                else if (loopPath && splineContainer.Spline.Count > 2)
                {
                    // 如果是循环路径，连接最后一个和第一个knot
                    BezierKnot firstKnot = splineContainer.Spline[0];
                    Vector3 firstPosition = splineContainer.transform.TransformPoint(firstKnot.Position);
                    Gizmos.color = pathColor;
                    Gizmos.DrawLine(knotPosition, firstPosition);
                }
            }
        }
        
        // 绘制避障系统可视化
        if (Application.isPlaying)
        {
            // 绘制视野范围
            Gizmos.color = visionColor;
            Vector3 visionEnd = transform.position + transform.up * visionDistance;
            Gizmos.DrawLine(transform.position, visionEnd);
            Gizmos.DrawWireSphere(visionEnd, tankWidth * 0.5f);
            
            // 绘制坦克检测范围
            Gizmos.color = visionColor;
            Gizmos.DrawWireSphere(transform.position, tankWidth * 0.5f);
            
            if (hasObstacle)
            {
                // 绘制障碍物检测点
                Gizmos.color = obstacleColor;
                Gizmos.DrawWireSphere(obstaclePoint, 0.3f);
                
                // 绘制障碍物法向量
                Gizmos.color = obstacleColor;
                Gizmos.DrawLine(obstaclePoint, obstaclePoint + obstacleNormal * 1.5f);
                
                // 绘制避障方向
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(transform.position, transform.position + avoidanceDirection * 2f);
            }
        }
        
        // 绘制当前目标和方向
        if (isActive && Application.isPlaying)
        {
            // 到当前目标的连线
            Gizmos.color = targetColor;
            Gizmos.DrawLine(transform.position, currentTarget);
            
            // 坦克前进方向
            Gizmos.color = directionColor;
            Gizmos.DrawLine(transform.position, transform.position + transform.up * 2f);
            
            // 显示AI状态信息
            if (tankController != null)
            {
                Vector2 currentInput = tankController.GetCurrentInput();
                
                // 显示加速/刹车状态
                if (currentInput.x > 0)
                    Gizmos.color = Color.green;  // 加速 - 绿色
                else if (currentInput.x < 0)
                    Gizmos.color = Color.red;    // 刹车 - 红色
                else
                    Gizmos.color = Color.white;  // 空档 - 白色
                
                // 显示输入向量
                Vector3 inputVector = new Vector3(currentInput.y, currentInput.x, 0) * 2f;
                Gizmos.DrawLine(transform.position + Vector3.right * 2f, 
                               transform.position + Vector3.right * 2f + inputVector);
            }
            
            // 显示waypoint信息
            Gizmos.color = Color.magenta;
            Vector3 textPosition = transform.position + Vector3.up * 3f;
            // 在Scene视图中显示当前waypoint索引（通过绘制小方块表示）
            for (int i = 0; i < currentWaypointIndex + 1; i++)
            {
                Gizmos.DrawWireCube(textPosition + Vector3.right * i * 0.3f, Vector3.one * 0.2f);
            }
        }
    }
    
    private void OnDisable()
    {
        // 确保在禁用时停止AI
        if (isActive)
        {
            StopAI();
        }
    }
} 