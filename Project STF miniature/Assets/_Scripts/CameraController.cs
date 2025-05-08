using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class CameraController : MonoBehaviour
{
    [Header("相机引用")]
    [SerializeField] private CinemachineCamera playerCamera;
    [SerializeField] private CinemachineCamera overviewCamera;
    
    [Header("相机设置")]
    [SerializeField] private float dragSpeed = 0.01f;
    [SerializeField] private Transform player;

    private Mouse mouse;
    private bool isDragging = false;
    private Vector2 lastMousePosition;
    private Vector3 originalOverviewPosition;
    private bool isPlayerCameraActive = true;

    private void Start()
    {
        mouse = Mouse.current;
        
        // 初始化相机优先级
        playerCamera.Priority = 10;
        overviewCamera.Priority = 0;
        
        // 记录overview相机的初始位置
        originalOverviewPosition = overviewCamera.transform.position;
    }

    private void Update()
    {
        // 检测鼠标滚轮输入
        float scrollValue = mouse.scroll.ReadValue().y;
        
        if (scrollValue != 0)
        {
            // 向上滚动切换到玩家相机
            if (scrollValue > 0 && !isPlayerCameraActive)
            {
                SwitchToPlayerCamera();
            }
            // 向下滚动切换到overview相机
            else if (scrollValue < 0 && isPlayerCameraActive)
            {
                SwitchToOverviewCamera();
            }
        }

        // 只在overview相机激活时处理拖拽
        if (!isPlayerCameraActive)
        {
            HandleOverviewCameraDrag();
        }
    }

    private void HandleOverviewCameraDrag()
    {
        // 开始拖拽
        if (mouse.rightButton.wasPressedThisFrame)
        {
            isDragging = true;
        }
        // 结束拖拽
        else if (mouse.rightButton.wasReleasedThisFrame)
        {
            isDragging = false;
        }

        // 处理拖拽移动
        if (isDragging)
        {
            Vector2 mouseDelta = mouse.delta.ReadValue() * dragSpeed;
            
            // 移动相机
            Vector3 position = overviewCamera.transform.position;
            position.x -= mouseDelta.x;
            position.y -= mouseDelta.y;
            overviewCamera.transform.position = position;
        }
    }

    private void SwitchToPlayerCamera()
    {
        playerCamera.Priority = 10;
        overviewCamera.Priority = 0;
        isPlayerCameraActive = true;
    }

    private void SwitchToOverviewCamera()
    {
        playerCamera.Priority = 0;
        overviewCamera.Priority = 10;
        isPlayerCameraActive = false;

        // 重置overview相机位置
        Vector3 newPosition = player.position;
        newPosition.z = originalOverviewPosition.z; // 保持原始z轴距离
        overviewCamera.transform.position = newPosition;
    }
} 