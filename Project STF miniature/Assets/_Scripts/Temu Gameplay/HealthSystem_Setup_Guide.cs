using UnityEngine;

namespace TemuGameplay
{
    /// <summary>
    /// 血量系统设置指南
    /// 提供快速设置血量系统的帮助
    /// </summary>
    public class HealthSystem_Setup_Guide : MonoBehaviour
    {
        [Header("快速设置")]
        [SerializeField] private bool autoSetupHealthSystem = true;
        
        [ContextMenu("Show Setup Instructions")]
        public void ShowSetupInstructions()
        {
            Debug.Log(@"
=== 血量系统设置指南 ===

1. 血量系统组件 (HealthSystem):
   - 自动创建：GameplayManager会自动查找或创建血量系统
   - 手动创建：在场景中添加HealthSystem组件到任意GameObject
   - 配置项：
     * Max Health: 100 (最大血量)
     * Current Health: 100 (当前血量)
     * Damage Multiplier: 5.0 (伤害倍数，差值 × 倍数 = 实际扣血)

2. 血量UI (HealthUI):
   - 在UI Canvas下创建血量UI对象
   - 拖拽以下组件到HealthUI脚本：
     * Health Slider: 显示血量条
     * Health Text: 显示血量数值 (血量: 80/100)
     * Damage Preview Text: 显示预览扣血 (将扣除: 15血量)

3. UI组件设置：
   - Slider设置：Min Value = 0, Max Value = 100
   - 确保已导入DOTween插件（用于动画效果）

4. 测试功能：
   - 运行游戏后，选择一些物品（不满足要求）
   - 点击提交按钮
   - 观察血量变化和UI更新

5. 调试工具：
   - HealthSystem右键菜单：Take 10 Damage, Heal 10, Reset Health
   - HealthUI右键菜单：Test Damage Animation, Refresh Preview

扣血计算公式：
- 每个未满足的trait：差值 × 伤害倍数
- 多个trait累加扣血
- 例：需要3个Running，只有1个 → (3-1) × 5 = 10点伤害

金色要求系统：
- 随机生成概率：可在GameplayManager中设置
- 金色要求满足后恢复10点血量
- 显示为：✨Trait: X/Y (Heal +10)
- 按R键重新生成关卡要求

快捷键：
- R键：重新生成关卡要求（Reroll Requirements）
");
        }

        [ContextMenu("Auto Setup Health System")]
        public void AutoSetupHealthSystem()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("请在运行时执行自动设置");
                return;
            }

            var gameplayManager = FindObjectOfType<TemuGameplay.Core.GameplayManager>();
            if (gameplayManager == null)
            {
                Debug.LogError("未找到GameplayManager！");
                return;
            }

            var healthSystem = gameplayManager.HealthSystem;
            if (healthSystem != null)
            {
                Debug.Log("✅ 血量系统已正确设置！");
                Debug.Log($"当前血量: {healthSystem.CurrentHealth}/{healthSystem.MaxHealth}");
                
                var healthUI = FindObjectOfType<TemuGameplay.UI.HealthUI>();
                if (healthUI != null)
                {
                    Debug.Log("✅ 血量UI已找到！");
                    healthUI.RefreshDamagePreview();
                }
                else
                {
                    Debug.LogWarning("❌ 未找到HealthUI组件，请手动设置UI");
                }
            }
            else
            {
                Debug.LogError("❌ 血量系统未正确初始化！");
            }
        }

        private void Start()
        {
            if (autoSetupHealthSystem)
            {
                Invoke(nameof(AutoSetupHealthSystem), 1f);
            }
        }
    }
} 