using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using TemuGameplay.Core;

namespace TemuGameplay.UI
{
    /// <summary>
    /// 血量UI控制器
    /// 管理血量显示、动画效果和预览扣血数值
    /// </summary>
    public class HealthUI : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private Slider healthSlider;
        [SerializeField] private TextMeshProUGUI healthText; // 显示 "Health: 80/100"
        [SerializeField] private TextMeshProUGUI damagePreviewText; // 显示预览扣血 "Will lose: 15 health"
        
        [Header("Animation Settings")]
        [SerializeField] private float animationDuration = 0.5f;
        [SerializeField] private Ease animationEase = Ease.OutQuad;
        [SerializeField] private bool enableShakeEffect = true;
        [SerializeField] private float shakeStrength = 10f;
        [SerializeField] private float shakeDuration = 0.3f;
        
        [Header("Preview Display Mode")]
        [SerializeField] private bool useFuzzyDamagePreview = false; // 是否使用模糊显示模式
        [SerializeField] private int lowRiskThreshold = 5;    // 低风险阈值
        [SerializeField] private int highRiskThreshold = 20;  // 高风险阈值
        
        [Header("Color Settings")]
        [SerializeField] private Color healthyColor = Color.green;
        [SerializeField] private Color mediumColor = Color.yellow;
        [SerializeField] private Color lowColor = Color.red;
        [SerializeField] private Color previewTextColor = Color.red;
        [SerializeField] private Color lowRiskColor = Color.green;    // 低风险颜色
        [SerializeField] private Color mediumRiskColor = Color.yellow; // 中风险颜色
        [SerializeField] private Color highRiskColor = Color.red;     // 高风险颜色
        
        [Header("Thresholds")]
        [SerializeField] private float mediumHealthThreshold = 0.6f;
        [SerializeField] private float lowHealthThreshold = 0.3f;

        private HealthSystem healthSystem;
        private GameplayManager gameplayManager;
        private Image healthFillImage;
        private Vector3 originalPosition;

        private void Awake()
        {
            // 获取slider的fill image
            if (healthSlider != null)
            {
                healthFillImage = healthSlider.fillRect.GetComponent<Image>();
                originalPosition = transform.localPosition;
            }

            // 设置预览文本颜色
            if (damagePreviewText != null)
            {
                damagePreviewText.color = previewTextColor;
            }
        }

        private void Start()
        {
            // 查找系统组件
            healthSystem = FindObjectOfType<HealthSystem>();
            gameplayManager = FindObjectOfType<GameplayManager>();
            
            if (healthSystem == null)
            {
                Debug.LogError("HealthSystem not found!");
                return;
            }

            // 监听血量事件
            healthSystem.OnHealthChanged += OnHealthChanged;
            healthSystem.OnDamageTaken += OnDamageTaken;

            // 监听游戏玩法事件来更新预览
            if (gameplayManager != null)
            {
                gameplayManager.OnLevelChanged += OnLevelChanged;
                // 监听玩家集合变化来更新预览
                if (gameplayManager.CurrentSet != null)
                {
                    gameplayManager.CurrentSet.OnTraitCountersUpdated += UpdateDamagePreview;
                }
            }

            // 初始化UI
            InitializeUI();
            UpdateDamagePreview();
        }

        private void InitializeUI()
        {
            if (healthSystem == null) return;

            // 设置slider
            if (healthSlider != null)
            {
                healthSlider.minValue = 0;
                healthSlider.maxValue = healthSystem.MaxHealth;
                healthSlider.value = healthSystem.CurrentHealth;
            }

            // 更新显示
            UpdateHealthDisplay();
            UpdateHealthColor();
        }

        private void OnHealthChanged(int currentHealth, int maxHealth)
        {
            // 动画更新slider值
            if (healthSlider != null)
            {
                healthSlider.DOValue(currentHealth, animationDuration)
                    .SetEase(animationEase);
            }

            // 更新文本和颜色
            UpdateHealthDisplay();
            UpdateHealthColor();
        }

        private void OnDamageTaken(int damageAmount)
        {
            // 震动效果
            if (enableShakeEffect)
            {
                transform.DOShakePosition(shakeDuration, shakeStrength, 10, 90, false, true)
                    .SetEase(Ease.OutElastic);
            }

            // 显示伤害数值（可选的浮动文本效果）
            ShowDamageText(damageAmount);
        }

        private void OnLevelChanged(Data.LevelRequirement newLevel)
        {
            // 新关卡时更新预览
            UpdateDamagePreview();
        }

        private void UpdateHealthDisplay()
        {
            if (healthSystem == null) return;

            // 更新血量文本
            if (healthText != null)
            {
                healthText.text = $"Health: {healthSystem.CurrentHealth}/{healthSystem.MaxHealth}";
            }
        }

        private void UpdateHealthColor()
        {
            if (healthSystem == null || healthFillImage == null) return;

            float healthPercentage = healthSystem.HealthPercentage;
            Color targetColor;

            if (healthPercentage >= mediumHealthThreshold)
            {
                targetColor = healthyColor;
            }
            else if (healthPercentage >= lowHealthThreshold)
            {
                targetColor = mediumColor;
            }
            else
            {
                targetColor = lowColor;
            }

            // 平滑颜色过渡
            healthFillImage.DOColor(targetColor, animationDuration);
        }

        private void UpdateDamagePreview()
        {
            if (damagePreviewText == null || healthSystem == null || gameplayManager == null) return;

            var currentLevel = gameplayManager.CurrentLevel;
            var currentSet = gameplayManager.CurrentSet;

            if (currentLevel == null || currentSet == null)
            {
                damagePreviewText.text = "";
                return;
            }

            // 计算预览伤害（使用新的完整计算方法）
            int previewDamage = healthSystem.CalculateDamageFromLevelRequirement(
                currentLevel, 
                currentSet.TraitCounters
            );

            // 计算金色要求的治疗
            int goldenHeal = 0;
            if (currentLevel.GoldenRequirements != null)
            {
                var metGoldenRequirements = currentLevel.CheckGoldenRequirements(currentSet.TraitCounters);
                goldenHeal = metGoldenRequirements.Count * 10; // 每个金色要求恢复10血量
            }

            // 计算实际的血量变化（考虑先扣血再恢复的顺序）
            int currentHealth = healthSystem.CurrentHealth;
            int maxHealth = healthSystem.MaxHealth;
            
            // 模拟先扣血
            int healthAfterDamage = Mathf.Max(0, currentHealth - previewDamage);
            // 再模拟恢复
            int finalHealth = Mathf.Min(maxHealth, healthAfterDamage + goldenHeal);
            // 计算净变化
            int netHealthChange = finalHealth - currentHealth;

            // 根据显示模式选择不同的显示方式
            if (useFuzzyDamagePreview)
            {
                UpdateFuzzyDamagePreview(netHealthChange);
            }
            else
            {
                UpdateDetailedDamagePreview(previewDamage, goldenHeal, netHealthChange);
            }
        }

        private void UpdateFuzzyDamagePreview(int netHealthChange)
        {
            // 停止任何动画
            damagePreviewText.transform.DOKill();
            damagePreviewText.transform.localScale = Vector3.one;

            // 只有当净伤害为负（实际会掉血）时才显示风险等级
            if (netHealthChange >= 0)
            {
                // 不掉血或者还能回血，不显示任何内容
                damagePreviewText.text = "";
                return;
            }

            // 计算实际掉血量（取绝对值）
            int actualDamage = Mathf.Abs(netHealthChange);
            
            string riskText;
            Color riskColor;

            if (actualDamage <= lowRiskThreshold)
            {
                riskText = "Low Risk of Danger";
                riskColor = lowRiskColor;
            }
            else if (actualDamage >= highRiskThreshold)
            {
                riskText = "High Risk of Danger";
                riskColor = highRiskColor;
                
                // 高风险时添加轻微脉冲动画
                damagePreviewText.transform.DOScale(1.05f, 1f)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine);
            }
            else
            {
                riskText = "Medium Risk of Danger";
                riskColor = mediumRiskColor;
            }

            damagePreviewText.text = riskText;
            damagePreviewText.color = riskColor;
        }

        private void UpdateDetailedDamagePreview(int previewDamage, int goldenHeal, int netHealthChange)
        {
            // 恢复原始颜色
            damagePreviewText.color = previewTextColor;

            if (previewDamage > 0 && goldenHeal > 0)
            {
                damagePreviewText.text = $"Will lose: {previewDamage} health\n✨Will heal: {goldenHeal} health\nNet: {(netHealthChange >= 0 ? "+" : "")}{netHealthChange}";
                
                // 脉冲动画
                damagePreviewText.transform.DOScale(1.1f, 0.5f)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine);
            }
            else if (previewDamage > 0)
            {
                damagePreviewText.text = $"Will lose: {previewDamage} health";
                
                // 脉冲动画提醒
                damagePreviewText.transform.DOScale(1.1f, 0.5f)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine);
            }
            else if (goldenHeal > 0)
            {
                damagePreviewText.text = $"✨Will heal: {goldenHeal} health\nRequirements met ✓";
                
                // 停止脉冲动画
                damagePreviewText.transform.DOKill();
                damagePreviewText.transform.localScale = Vector3.one;
            }
            else
            {
                damagePreviewText.text = "Requirements met ✓";
                
                // 停止脉冲动画
                damagePreviewText.transform.DOKill();
                damagePreviewText.transform.localScale = Vector3.one;
            }
        }

        private void ShowDamageText(int damage)
        {
            // 简单的伤害显示效果
            if (damagePreviewText != null)
            {
                var originalText = damagePreviewText.text;
                damagePreviewText.text = $"-{damage}";
                
                // 伤害文本动画
                damagePreviewText.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 5, 0.5f)
                    .OnComplete(() => {
                        damagePreviewText.text = originalText;
                        UpdateDamagePreview(); // 重新计算预览
                    });
            }
        }

        /// <summary>
        /// 手动更新预览（供外部调用）
        /// </summary>
        public void RefreshDamagePreview()
        {
            UpdateDamagePreview();
        }

        /// <summary>
        /// 设置UI可见性
        /// </summary>
        public void SetUIVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        private void OnDestroy()
        {
            // 清理事件监听
            if (healthSystem != null)
            {
                healthSystem.OnHealthChanged -= OnHealthChanged;
                healthSystem.OnDamageTaken -= OnDamageTaken;
            }

            if (gameplayManager != null)
            {
                gameplayManager.OnLevelChanged -= OnLevelChanged;
                if (gameplayManager.CurrentSet != null)
                {
                    gameplayManager.CurrentSet.OnTraitCountersUpdated -= UpdateDamagePreview;
                }
            }

            // 停止所有DOTween动画
            transform.DOKill();
            if (healthSlider != null) healthSlider.DOKill();
            if (healthFillImage != null) healthFillImage.DOKill();
            if (damagePreviewText != null) damagePreviewText.transform.DOKill();
        }

        /// <summary>
        /// 设置伤害预览显示模式
        /// </summary>
        public void SetFuzzyDamagePreview(bool enabled)
        {
            useFuzzyDamagePreview = enabled;
            UpdateDamagePreview();
        }

        /// <summary>
        /// 切换伤害预览显示模式
        /// </summary>
        public void ToggleDamagePreviewMode()
        {
            SetFuzzyDamagePreview(!useFuzzyDamagePreview);
        }

        /// <summary>
        /// 设置风险阈值
        /// </summary>
        public void SetRiskThresholds(int lowThreshold, int highThreshold)
        {
            lowRiskThreshold = lowThreshold;
            highRiskThreshold = highThreshold;
            if (useFuzzyDamagePreview)
            {
                UpdateDamagePreview();
            }
        }

        #region Context Menu Methods
        [ContextMenu("Test Damage Animation")]
        public void TestDamageAnimation()
        {
            if (Application.isPlaying)
            {
                OnDamageTaken(15);
            }
        }

        [ContextMenu("Refresh Preview")]
        public void TestRefreshPreview()
        {
            if (Application.isPlaying)
            {
                UpdateDamagePreview();
            }
        }

        [ContextMenu("Toggle Fuzzy Preview Mode")]
        public void TestToggleFuzzyMode()
        {
            if (Application.isPlaying)
            {
                ToggleDamagePreviewMode();
                Debug.Log($"Fuzzy preview mode: {(useFuzzyDamagePreview ? "Enabled" : "Disabled")}");
            }
        }

        [ContextMenu("Test Low Risk")]
        public void TestLowRisk()
        {
            if (Application.isPlaying && useFuzzyDamagePreview)
            {
                UpdateFuzzyDamagePreview(-3); // 模拟低风险
            }
        }

        [ContextMenu("Test Medium Risk")]
        public void TestMediumRisk()
        {
            if (Application.isPlaying && useFuzzyDamagePreview)
            {
                UpdateFuzzyDamagePreview(-12); // 模拟中风险
            }
        }

        [ContextMenu("Test High Risk")]
        public void TestHighRisk()
        {
            if (Application.isPlaying && useFuzzyDamagePreview)
            {
                UpdateFuzzyDamagePreview(-25); // 模拟高风险
            }
        }
        #endregion
    }
} 