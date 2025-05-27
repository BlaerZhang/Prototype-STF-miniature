using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 自动跑酷控制器使用示例
/// 展示如何使用AutoRunnerLinearController和AdvancedAutoRunnerController
/// </summary>
public class AutoRunnerExample : MonoBehaviour
{
    [Header("控制器引用")]
    [SerializeField] private AutoRunnerLinearController basicController;
    [SerializeField] private AdvancedAutoRunnerController advancedController;
    
    [Header("UI控制")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button stopButton;
    [SerializeField] private Button resetButton;
    [SerializeField] private Slider speedSlider;
    [SerializeField] private TMP_Text speedText;
    [SerializeField] private TMP_Text stateText;
    
    [Header("测试设置")]
    [SerializeField] private bool useAdvancedController = true;
    [SerializeField] private KeyCode startKey = KeyCode.Space;
    [SerializeField] private KeyCode stopKey = KeyCode.S;
    [SerializeField] private KeyCode resetKey = KeyCode.R;
    
    private void Start()
    {
        InitializeUI();
        SetupEventListeners();
    }
    
    private void Update()
    {
        HandleKeyboardInput();
        UpdateUI();
    }
    
    /// <summary>
    /// 初始化UI
    /// </summary>
    private void InitializeUI()
    {
        if (startButton != null)
        {
            startButton.onClick.AddListener(StartRunning);
        }
        
        if (stopButton != null)
        {
            stopButton.onClick.AddListener(StopRunning);
        }
        
        if (resetButton != null)
        {
            resetButton.onClick.AddListener(ResetRunning);
        }
        
        if (speedSlider != null)
        {
            speedSlider.onValueChanged.AddListener(OnSpeedSliderChanged);
            speedSlider.minValue = 1f;
            speedSlider.maxValue = 20f;
            speedSlider.value = useAdvancedController ? 
                (advancedController != null ? advancedController.GetCurrentSpeed() : 10f) :
                (basicController != null ? basicController.GetCurrentSpeed() : 10f);
        }
    }
    
    /// <summary>
    /// 设置事件监听器
    /// </summary>
    private void SetupEventListeners()
    {
        if (useAdvancedController && advancedController != null)
        {
            advancedController.OnStartRunning += OnStartRunning;
            advancedController.OnReachMaxSpeed += OnReachMaxSpeed;
            advancedController.OnStopRunning += OnStopRunning;
            advancedController.OnSpeedChanged += OnSpeedChanged;
        }
    }
    
    /// <summary>
    /// 处理键盘输入
    /// </summary>
    private void HandleKeyboardInput()
    {
        if (Input.GetKeyDown(startKey))
        {
            StartRunning();
        }
        
        if (Input.GetKeyDown(stopKey))
        {
            StopRunning();
        }
        
        if (Input.GetKeyDown(resetKey))
        {
            ResetRunning();
        }
        
        // 数字键快速设置速度
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SetMaxSpeed(5f);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SetMaxSpeed(10f);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SetMaxSpeed(15f);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            SetMaxSpeed(20f);
        }
    }
    
    /// <summary>
    /// 更新UI显示
    /// </summary>
    private void UpdateUI()
    {
        if (useAdvancedController && advancedController != null)
        {
            UpdateUIForAdvancedController();
        }
        else if (basicController != null)
        {
            UpdateUIForBasicController();
        }
    }
    
    private void UpdateUIForAdvancedController()
    {
        if (speedText != null)
        {
            speedText.text = $"速度: {advancedController.GetCurrentSpeed():F1}";
        }
        
        if (stateText != null)
        {
            stateText.text = $"状态: {advancedController.GetCurrentState()}";
        }
    }
    
    private void UpdateUIForBasicController()
    {
        if (speedText != null)
        {
            speedText.text = $"速度: {basicController.GetCurrentSpeed():F1}";
        }
        
        if (stateText != null)
        {
            stateText.text = $"状态: {basicController.GetCurrentState()}";
        }
    }
    
    /// <summary>
    /// 开始跑酷
    /// </summary>
    public void StartRunning()
    {
        if (useAdvancedController && advancedController != null)
        {
            advancedController.StartRunning();
        }
        else if (basicController != null)
        {
            basicController.StartRunning();
        }
        
        Debug.Log("AutoRunnerExample: 开始跑酷");
    }
    
    /// <summary>
    /// 停止跑酷
    /// </summary>
    public void StopRunning()
    {
        if (useAdvancedController && advancedController != null)
        {
            advancedController.StopRunning();
        }
        else if (basicController != null)
        {
            basicController.StopRunning();
        }
        
        Debug.Log("AutoRunnerExample: 停止跑酷");
    }
    
    /// <summary>
    /// 重置跑酷
    /// </summary>
    public void ResetRunning()
    {
        if (useAdvancedController && advancedController != null)
        {
            advancedController.ResetRunning();
        }
        else if (basicController != null)
        {
            basicController.ResetRunning();
        }
        
        Debug.Log("AutoRunnerExample: 重置跑酷");
    }
    
    /// <summary>
    /// 设置最高速度
    /// </summary>
    public void SetMaxSpeed(float speed)
    {
        if (useAdvancedController && advancedController != null)
        {
            advancedController.SetMaxSpeed(speed);
        }
        else if (basicController != null)
        {
            basicController.SetMaxSpeed(speed);
        }
        
        if (speedSlider != null)
        {
            speedSlider.value = speed;
        }
        
        Debug.Log($"AutoRunnerExample: 设置最高速度为 {speed}");
    }
    
    /// <summary>
    /// 速度滑块变化事件
    /// </summary>
    private void OnSpeedSliderChanged(float value)
    {
        SetMaxSpeed(value);
    }
    
    // 事件回调方法
    private void OnStartRunning()
    {
        Debug.Log("AutoRunnerExample: 收到开始跑酷事件");
    }
    
    private void OnReachMaxSpeed()
    {
        Debug.Log("AutoRunnerExample: 收到达到最高速度事件");
    }
    
    private void OnStopRunning()
    {
        Debug.Log("AutoRunnerExample: 收到停止跑酷事件");
    }
    
    private void OnSpeedChanged(float newSpeed)
    {
        // Debug.Log($"AutoRunnerExample: 速度变化为 {newSpeed:F2}");
    }
    
    /// <summary>
    /// 切换控制器类型
    /// </summary>
    public void ToggleControllerType()
    {
        // 先停止当前控制器
        StopRunning();
        
        // 切换类型
        useAdvancedController = !useAdvancedController;
        
        Debug.Log($"AutoRunnerExample: 切换到 {(useAdvancedController ? "高级" : "基础")} 控制器");
        
        // 重新设置事件监听器
        SetupEventListeners();
    }
    
    /// <summary>
    /// 在GUI中显示控制说明
    /// </summary>
    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(Screen.width - 250, 10, 240, 200));
        GUILayout.Label("=== 控制说明 ===");
        GUILayout.Label($"空格键: 开始跑酷");
        GUILayout.Label($"S键: 停止跑酷");
        GUILayout.Label($"R键: 重置跑酷");
        GUILayout.Label($"1-4键: 快速设置速度");
        GUILayout.Label($"当前控制器: {(useAdvancedController ? "高级" : "基础")}");
        
        if (GUILayout.Button("切换控制器类型"))
        {
            ToggleControllerType();
        }
        
        GUILayout.EndArea();
    }
    
    private void OnDestroy()
    {
        // 清理事件监听器
        if (useAdvancedController && advancedController != null)
        {
            advancedController.OnStartRunning -= OnStartRunning;
            advancedController.OnReachMaxSpeed -= OnReachMaxSpeed;
            advancedController.OnStopRunning -= OnStopRunning;
            advancedController.OnSpeedChanged -= OnSpeedChanged;
        }
    }
} 