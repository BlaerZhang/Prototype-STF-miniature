using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TemuGameplay.Core;

namespace TemuGameplay.UI
{
    /// <summary>
    /// 提交按钮UI控制器
    /// 处理关卡提交验证逻辑
    /// </summary>
    public class SubmitButtonUI : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private Button submitButton;
        [SerializeField] private TextMeshProUGUI buttonText;
        [SerializeField] private GameObject feedbackPanel; // 反馈面板（可选）
        [SerializeField] private TextMeshProUGUI feedbackText; // 反馈文本（可选）
        
        [Header("Button Text Settings")]
        [SerializeField] private string defaultText = "Submit";
        [SerializeField] private string processingText = "Checking...";
        [SerializeField] private string successText = "Success!";
        [SerializeField] private string failText = "Try Again";
        
        [Header("Feedback Settings")]
        [SerializeField] private float feedbackDisplayTime = 2f;
        [SerializeField] private bool autoHideFeedback = true;

        private GameplayManager gameplayManager;
        private bool isProcessing = false;

        private void Awake()
        {
            if (submitButton == null)
                submitButton = GetComponent<Button>();
                
            if (buttonText == null && submitButton != null)
                buttonText = submitButton.GetComponentInChildren<TextMeshProUGUI>();
        }

        private void Start()
        {
            gameplayManager = FindObjectOfType<GameplayManager>();
            if (gameplayManager == null)
            {
                Debug.LogError("GameplayManager not found!");
                SetButtonInteractable(false);
                return;
            }

            // 设置按钮事件
            if (submitButton != null)
            {
                submitButton.onClick.AddListener(OnSubmitButtonClicked);
            }

            // 监听提交结果
            gameplayManager.OnLevelSubmitted += OnLevelSubmitted;
            gameplayManager.OnLevelChanged += OnLevelChanged;

            // 初始化UI
            UpdateButtonText(defaultText);
            HideFeedback();
        }

        private void OnSubmitButtonClicked()
        {
            if (isProcessing || gameplayManager == null) return;

            Debug.Log("Submit button clicked");
            
            // 设置处理状态
            SetProcessingState(true);
            
            // 提交验证
            bool result = gameplayManager.SubmitAnswer();
            
            // 注意：结果会通过OnLevelSubmitted事件回调处理
        }

        private void OnLevelSubmitted(bool isCompleted)
        {
            SetProcessingState(false);
            
            if (isCompleted)
            {
                ShowSuccess();
            }
            else
            {
                ShowFailure();
            }
        }

        private void OnLevelChanged(Data.LevelRequirement newLevel)
        {
            // 新关卡加载，重置按钮状态
            SetProcessingState(false);
            UpdateButtonText(defaultText);
            HideFeedback();
            
            Debug.Log($"New level loaded: {newLevel.LevelName}");
        }

        private void SetProcessingState(bool processing)
        {
            isProcessing = processing;
            SetButtonInteractable(!processing);
            
            if (processing)
            {
                UpdateButtonText(processingText);
            }
        }

        private void ShowSuccess()
        {
            UpdateButtonText(successText);
            ShowFeedback("🎉 Level Completed!", Color.green);
            
            // 短暂延迟后重置按钮文本
            Invoke(nameof(ResetButtonText), 1f);
        }

        private void ShowFailure()
        {
            UpdateButtonText(failText);
            
            // 获取未满足的要求
            var unmetRequirements = gameplayManager.GetUnmetRequirements();
            string feedbackMessage = "❌ Requirements not met:";
            if (unmetRequirements.Count > 0)
            {
                feedbackMessage += "\n" + string.Join("\n", unmetRequirements);
            }
            
            ShowFeedback(feedbackMessage, Color.red);
            
            // 短暂延迟后重置按钮文本
            Invoke(nameof(ResetButtonText), 2f);
        }

        private void ResetButtonText()
        {
            UpdateButtonText(defaultText);
        }

        private void UpdateButtonText(string text)
        {
            if (buttonText != null)
            {
                buttonText.text = text;
            }
        }

        private void SetButtonInteractable(bool interactable)
        {
            if (submitButton != null)
            {
                submitButton.interactable = interactable;
            }
        }

        private void ShowFeedback(string message, Color color)
        {
            if (feedbackPanel != null)
            {
                feedbackPanel.SetActive(true);
            }
            
            if (feedbackText != null)
            {
                feedbackText.text = message;
                feedbackText.color = color;
            }
            
            // 自动隐藏反馈
            if (autoHideFeedback)
            {
                CancelInvoke(nameof(HideFeedback));
                Invoke(nameof(HideFeedback), feedbackDisplayTime);
            }
        }

        private void HideFeedback()
        {
            if (feedbackPanel != null)
            {
                feedbackPanel.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (submitButton != null)
            {
                submitButton.onClick.RemoveListener(OnSubmitButtonClicked);
            }
            
            if (gameplayManager != null)
            {
                gameplayManager.OnLevelSubmitted -= OnLevelSubmitted;
                gameplayManager.OnLevelChanged -= OnLevelChanged;
            }
        }

        // Inspector可调用的方法
        [ContextMenu("Test Submit")]
        public void TestSubmit()
        {
            if (Application.isPlaying)
            {
                OnSubmitButtonClicked();
            }
        }

        [ContextMenu("Force Reset")]
        public void ForceReset()
        {
            SetProcessingState(false);
            UpdateButtonText(defaultText);
            HideFeedback();
        }

        // 公共方法，可供其他脚本调用
        public void TriggerSubmit()
        {
            OnSubmitButtonClicked();
        }

        public void SetButtonEnabled(bool enabled)
        {
            SetButtonInteractable(enabled);
        }

        public bool IsProcessing()
        {
            return isProcessing;
        }
    }
} 