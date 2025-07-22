using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

namespace SpinWheel
{
    /// <summary>
    /// 奖项显示组件 - 用于预制体中显示奖项信息
    /// 用户可以自定义这个预制体的布局和样式
    /// </summary>
    public class PrizeDisplayComponent : MonoBehaviour, IPrizeDisplay
    {
        [Header("UI组件引用")]
        [LabelText("奖项名称文本")]
        [SerializeField] private Text prizeNameText;
        
        [LabelText("奖项图标")]
        [SerializeField] private Image prizeIcon;
        
        [LabelText("权重显示文本 (可选)")]
        [SerializeField] private Text weightText;
        
        [Header("显示设置")]
        [LabelText("显示权重信息")]
        [SerializeField] private bool showWeight = false;
        
        [LabelText("权重显示格式")]
        [SerializeField] private string weightFormat = "{0:F1}%";
        
        private PrizeItem currentPrize;
        
        public void SetPrizeInfo(PrizeItem prize)
        {
            currentPrize = prize;
            UpdateDisplay();
        }
        
        private void UpdateDisplay()
        {
            if (currentPrize == null) return;
            
            // 设置奖项名称
            if (prizeNameText != null)
            {
                prizeNameText.text = currentPrize.prizeName;
            }
            
            // 设置奖项图标
            if (prizeIcon != null)
            {
                if (currentPrize.prizeIcon != null)
                {
                    prizeIcon.sprite = currentPrize.prizeIcon;
                    prizeIcon.gameObject.SetActive(true);
                }
                else
                {
                    prizeIcon.gameObject.SetActive(false);
                }
            }
            
            // 设置权重显示
            if (weightText != null && showWeight)
            {
                float weightPercentage = currentPrize.normalizedWeight * 100f;
                weightText.text = string.Format(weightFormat, weightPercentage);
                weightText.gameObject.SetActive(true);
            }
            else if (weightText != null)
            {
                weightText.gameObject.SetActive(false);
            }
        }
        
        // 编辑器预览
        [Button("预览显示")]
        [ShowInInspector]
        private void PreviewDisplay()
        {
            if (currentPrize != null)
            {
                UpdateDisplay();
            }
        }
    }
} 