using System;
using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;

namespace SpinWheel
{
    public class SpinWheelController : MonoBehaviour
    {
        [Header("转盘设置")]
        [LabelText("转盘UI")]
        [SerializeField] private SpinWheelUI spinWheelUI;
        
        [LabelText("转盘旋转时间"), Range(2f, 10f)]
        [SerializeField] private float spinDuration = 4f;
        
        [LabelText("最小旋转圈数"), Range(3, 10)]
        [SerializeField] private int minSpinRotations = 5;
        
        [LabelText("最大旋转圈数"), Range(3, 15)]
        [SerializeField] private int maxSpinRotations = 8;
        
        [Header("当前状态")]
        [ReadOnly]
        [SerializeField] private bool isSpinning = false;
        
        [ReadOnly]
        [SerializeField] private SpinWheelPrizePool currentPrizePool;
        
        // 事件回调
        public static Action<PrizeItem> OnSpinComplete;
        public static Action OnSpinStart;
        
        private Sequence currentSpinSequence;
        
        private void Awake()
        {
            if (spinWheelUI == null)
                spinWheelUI = GetComponentInChildren<SpinWheelUI>();
        }
        
        /// <summary>
        /// 开始抽奖
        /// </summary>
        /// <param name="prizePool">奖池配置</param>
        public void StartSpin(SpinWheelPrizePool prizePool)
        {
            if (isSpinning)
            {
                Debug.LogWarning("转盘生成完成");
                return;
            }
            
            if (prizePool == null)
            {
                Debug.LogError("奖池配置不能为空");
                return;
            }
            
            currentPrizePool = prizePool;
            
            // 设置UI
            spinWheelUI.SetupWheel(prizePool);
        }
        
        public void PerformSpin()
        {
            isSpinning = true;
            OnSpinStart?.Invoke();
            
            // 先确定中奖奖项
            PrizeItem winningPrize = currentPrizePool.GetRandomPrize();
            
            // 计算目标角度
            float targetAngle = currentPrizePool.GetAngleForPrize(winningPrize);
            
            // 添加多圈旋转
            int rotations = UnityEngine.Random.Range(minSpinRotations, maxSpinRotations + 1);
            float finalAngle = targetAngle + (360f * rotations);
            
            // 执行旋转动画
            // 指针在顶部，我们需要让奖项转到顶部位置
            // 由于转盘是顺时针旋转，需要计算正确的旋转角度
            currentSpinSequence = DOTween.Sequence();
            currentSpinSequence
            .Append(spinWheelUI.WheelTransform.DORotate(
                new Vector3(0, 0, finalAngle), // 正值：让奖项区域转到指针位置
                spinDuration,
                RotateMode.FastBeyond360
            )).SetEase(Ease.OutQuart)
            // 模拟现实转盘的减速效果
            .OnComplete(() => {
                OnSpinCompleted(winningPrize);
            });

            currentSpinSequence.Play();
        }
        
        private void OnSpinCompleted(PrizeItem winningPrize)
        {
            isSpinning = false;
            currentSpinSequence = null;

            // Reset wheel angle
            spinWheelUI.WheelTransform.localRotation = Quaternion.identity;
            
            Debug.Log($"恭喜获得: {winningPrize.prizeName}");
            OnSpinComplete?.Invoke(winningPrize);
        }
        
        /// <summary>
        /// 停止当前旋转（如果需要的话）
        /// </summary>
        public void StopSpin()
        {
            if (currentSpinSequence != null)
            {
                currentSpinSequence.Kill();
                currentSpinSequence = null;
                isSpinning = false;
            }
        }
        
        private void OnDestroy()
        {
            StopSpin();
        }
        
        // 编辑器测试用
        [Button("测试抽奖")]
        [ShowIf("@currentPrizePool != null && !isSpinning")]
        private void TestSpin()
        {
            if (Application.isPlaying && currentPrizePool != null)
            {
                StartSpin(currentPrizePool);
            }
        }
    }
} 