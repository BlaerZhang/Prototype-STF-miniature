using UnityEngine;
using UnityEngine.UI;
using Unity.Cinemachine;
using JoostenProductions;
using UnityEngine.InputSystem;
/// <summary>
/// 极简Cinemachine摄像机管理器
/// 通过修改优先级实现摄像机切换，支持键盘和UI按钮触发
/// </summary>
public class SimpleCameraManager : OverridableMonoBehaviour
{
    [Header("摄像机列表")]
    [SerializeField] private CinemachineCamera[] cameras;
    
    [Header("键盘控制")]
    [SerializeField] private bool enableKeyboardControl = true;
    [SerializeField] private Key[] switchKeys = { Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4, Key.Digit5 };
    [SerializeField] private Key nextCameraKey = Key.Tab;
    [SerializeField] private Key previousCameraKey = Key.LeftShift;
    
    [Header("UI按钮 (可选)")]
    [SerializeField] private Button[] cameraButtons;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button previousButton;
    
    [Header("设置")]
    [SerializeField] private int activeCameraPriority = 10;
    [SerializeField] private int inactiveCameraPriority = 0;
    [SerializeField] private int startCameraIndex = 0;
    
    private int currentCameraIndex = 0;
    
    #region Unity生命周期
    
    private void Start()
    {
        ValidateSetup();
        InitializeCameras();
        InitializeButtons();
        SwitchToCamera(startCameraIndex);
    }
    
    public override void UpdateMe()
    {
        if (enableKeyboardControl)
        {
            HandleKeyboardInput();
        }
    }
    
    #endregion
    
    #region 初始化
    
    private void ValidateSetup()
    {
        if (cameras == null || cameras.Length == 0)
        {
            Debug.LogError("[SimpleCameraManager] 没有设置摄像机！请在Inspector中添加Cinemachine摄像机。");
            enabled = false;
            return;
        }
        
        // 过滤空引用
        cameras = System.Array.FindAll(cameras, cam => cam != null);
        
        if (cameras.Length == 0)
        {
            Debug.LogError("[SimpleCameraManager] 所有摄像机引用都为空！");
            enabled = false;
            return;
        }
        
        // 确保起始索引有效
        startCameraIndex = Mathf.Clamp(startCameraIndex, 0, cameras.Length - 1);
    }
    
    private void InitializeCameras()
    {
        // 设置所有摄像机为非激活状态
        for (int i = 0; i < cameras.Length; i++)
        {
            cameras[i].Priority = inactiveCameraPriority;
        }
    }
    
    private void InitializeButtons()
    {
        // 设置摄像机切换按钮
        if (cameraButtons != null)
        {
            for (int i = 0; i < cameraButtons.Length && i < cameras.Length; i++)
            {
                if (cameraButtons[i] != null)
                {
                    int index = i; // 闭包变量
                    cameraButtons[i].onClick.AddListener(() => SwitchToCamera(index));
                }
            }
        }
        
        // 设置下一个/上一个按钮
        if (nextButton != null)
            nextButton.onClick.AddListener(SwitchToNextCamera);
            
        if (previousButton != null)
            previousButton.onClick.AddListener(SwitchToPreviousCamera);
    }
    
    #endregion
    
    #region 输入处理
    
    private void HandleKeyboardInput()
    {
        // 数字键直接切换
        for (int i = 0; i < switchKeys.Length && i < cameras.Length; i++)
        {
            if (Keyboard.current[switchKeys[i]].wasPressedThisFrame)
            {
                SwitchToCamera(i);
                return;
            }
        }
        
        // Tab键切换到下一个
        if (Keyboard.current[nextCameraKey].wasPressedThisFrame)
        {
            SwitchToNextCamera();
        }
        
        // Shift键切换到上一个
        if (Keyboard.current[previousCameraKey].wasPressedThisFrame)
        {
            SwitchToPreviousCamera();
        }
    }
    
    #endregion
    
    #region 摄像机切换
    
    /// <summary>
    /// 切换到指定索引的摄像机
    /// </summary>
    /// <param name="index">摄像机索引</param>
    public void SwitchToCamera(int index)
    {
        if (index < 0 || index >= cameras.Length)
        {
            Debug.LogWarning($"[SimpleCameraManager] 无效的摄像机索引: {index}");
            return;
        }
        
        if (cameras[index] == null)
        {
            Debug.LogWarning($"[SimpleCameraManager] 摄像机 {index} 为空引用");
            return;
        }
        
        // 设置所有摄像机为非激活状态
        for (int i = 0; i < cameras.Length; i++)
        {
            cameras[i].Priority = inactiveCameraPriority;
        }
        
        // 激活目标摄像机
        cameras[index].Priority = activeCameraPriority;
        currentCameraIndex = index;
        
        Debug.Log($"[SimpleCameraManager] 切换到摄像机 {index}: {cameras[index].name}");
    }
    
    /// <summary>
    /// 切换到下一个摄像机
    /// </summary>
    public void SwitchToNextCamera()
    {
        int nextIndex = (currentCameraIndex + 1) % cameras.Length;
        SwitchToCamera(nextIndex);
    }
    
    /// <summary>
    /// 切换到上一个摄像机
    /// </summary>
    public void SwitchToPreviousCamera()
    {
        int previousIndex = (currentCameraIndex - 1 + cameras.Length) % cameras.Length;
        SwitchToCamera(previousIndex);
    }
    
    /// <summary>
    /// 通过摄像机名称切换
    /// </summary>
    /// <param name="cameraName">摄像机名称</param>
    public void SwitchToCameraByName(string cameraName)
    {
        for (int i = 0; i < cameras.Length; i++)
        {
            if (cameras[i] != null && cameras[i].name == cameraName)
            {
                SwitchToCamera(i);
                return;
            }
        }
        
        Debug.LogWarning($"[SimpleCameraManager] 未找到名为 '{cameraName}' 的摄像机");
    }
    
    #endregion
    
    #region 公共方法
    
    /// <summary>
    /// 获取当前激活的摄像机
    /// </summary>
    /// <returns>当前摄像机</returns>
    public CinemachineCamera GetCurrentCamera()
    {
        if (currentCameraIndex >= 0 && currentCameraIndex < cameras.Length)
            return cameras[currentCameraIndex];
        return null;
    }
    
    /// <summary>
    /// 获取当前摄像机索引
    /// </summary>
    /// <returns>当前摄像机索引</returns>
    public int GetCurrentCameraIndex()
    {
        return currentCameraIndex;
    }
    
    /// <summary>
    /// 获取摄像机总数
    /// </summary>
    /// <returns>摄像机数量</returns>
    public int GetCameraCount()
    {
        return cameras.Length;
    }
    
    /// <summary>
    /// 添加新的摄像机到管理列表
    /// </summary>
    /// <param name="camera">要添加的摄像机</param>
    public void AddCamera(CinemachineCamera camera)
    {
        if (camera == null) return;
        
        System.Array.Resize(ref cameras, cameras.Length + 1);
        cameras[cameras.Length - 1] = camera;
        camera.Priority = inactiveCameraPriority;
    }
    
    /// <summary>
    /// 从管理列表中移除摄像机
    /// </summary>
    /// <param name="camera">要移除的摄像机</param>
    public void RemoveCamera(CinemachineCamera camera)
    {
        if (camera == null) return;
        
        var list = new System.Collections.Generic.List<CinemachineCamera>(cameras);
        list.Remove(camera);
        cameras = list.ToArray();
    }
    
    #endregion
} 