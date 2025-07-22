using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

namespace SpinWheel
{
    /// <summary>
    /// 转盘使用示例
    /// 展示如何使用SpinWheelController进行抽奖
    /// </summary>
    public class SpinWheelExample : MonoBehaviour
    {
        [Header("组件引用")]
        [LabelText("转盘控制器")]
        [SerializeField] private SpinWheelController spinWheelController;
        
        [LabelText("开始抽奖按钮")]
        [SerializeField] private Button spinButton;
        
        [LabelText("结果显示文本")]
        [SerializeField] private Text resultText;
        
        [Header("奖池配置")]
        [LabelText("当前使用的奖池")]
        [SerializeField] private SpinWheelPrizePool currentPrizePool;
        
        [Header("测试奖池列表")]
        [LabelText("可选择的奖池")]
        [SerializeField] private SpinWheelPrizePool[] availablePrizePools;
        
        private void Start()
        {
            SetupUI();
            SetupEvents();
        }
        
        private void SetupUI()
        {
            if (resultText != null)
            {
                resultText.text = "点击按钮开始抽奖！";
            }
            
            if (spinButton != null)
            {
                spinButton.onClick.AddListener(OnSpinButtonClicked);
            }
        }
        
        private void SetupEvents()
        {
            if (spinWheelController != null)
            {
                spinWheelController.OnSpinStart += OnSpinStarted;
                spinWheelController.OnSpinComplete += OnSpinCompleted;
            }
        }
        
        private void OnSpinButtonClicked()
        {
            if (currentPrizePool == null)
            {
                Debug.LogWarning("请先设置奖池配置！");
                if (resultText != null)
                    resultText.text = "请先设置奖池配置！";
                return;
            }
            
            // 开始抽奖
            spinWheelController.StartSpin(currentPrizePool);
        }
        
        private void OnSpinStarted()
        {
            Debug.Log("转盘开始旋转...");
            if (resultText != null)
                resultText.text = "转盘旋转中...";
            
            if (spinButton != null)
                spinButton.interactable = false;
        }
        
        private void OnSpinCompleted(PrizeItem winningPrize)
        {
            Debug.Log($"抽奖完成！获得奖项: {winningPrize.prizeName}");
            if (resultText != null)
                resultText.text = $"恭喜获得: {winningPrize.prizeName}!";
            
            if (spinButton != null)
                spinButton.interactable = true;
        }
        
        // 编辑器测试方法
        [Button("快速测试抽奖")]
        [ShowIf("@Application.isPlaying && currentPrizePool != null")]
        private void QuickTestSpin()
        {
            OnSpinButtonClicked();
        }
        
        [Button("切换奖池")]
        [ShowIf("@availablePrizePools != null && availablePrizePools.Length > 0")]
        private void SwitchPrizePool()
        {
            if (availablePrizePools == null || availablePrizePools.Length == 0) return;
            
            // 循环切换奖池
            int currentIndex = System.Array.IndexOf(availablePrizePools, currentPrizePool);
            int nextIndex = (currentIndex + 1) % availablePrizePools.Length;
            currentPrizePool = availablePrizePools[nextIndex];
            
            Debug.Log($"切换到奖池: {currentPrizePool.poolName}");
        }
        
        private void OnDestroy()
        {
            if (spinButton != null)
                spinButton.onClick.RemoveListener(OnSpinButtonClicked);
            
            if (spinWheelController != null)
            {
                spinWheelController.OnSpinStart -= OnSpinStarted;
                spinWheelController.OnSpinComplete -= OnSpinCompleted;
            }
        }
    }
} 