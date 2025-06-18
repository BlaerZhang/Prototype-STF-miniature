using UnityEngine;
using TemuGameplay.Core;
using TemuGameplay.Data;

namespace TemuGameplay
{
    /// <summary>
    /// 金色要求测试工具
    /// 用于调试和验证金色要求功能
    /// </summary>
    public class GoldenRequirement_Test : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private TraitType testTrait = TraitType.Sitting;
        [SerializeField] private int testQuantity = 2;
        
        private GameplayManager gameplayManager;

        private void Start()
        {
            gameplayManager = FindObjectOfType<GameplayManager>();
        }

        [ContextMenu("Force Add Golden Requirement")]
        public void ForceAddGoldenRequirement()
        {
            if (!Application.isPlaying)
            {
                Debug.Log("请在运行时使用此功能");
                return;
            }

            if (gameplayManager?.CurrentLevel == null)
            {
                Debug.Log("❌ No current level found");
                return;
            }

            // 强制添加金色要求
            gameplayManager.CurrentLevel.AddGoldenRequirement(testTrait, testQuantity);
            
            Debug.Log($"✨ Force added golden requirement: {testTrait} x{testQuantity}");
            
            // 触发UI更新
            var traitDisplay = FindObjectOfType<TemuGameplay.UI.TraitDisplayUI>();
            if (traitDisplay != null)
            {
                traitDisplay.RefreshDisplay();
                Debug.Log("🔄 UI refreshed");
            }
            
            // 触发血量UI更新
            var healthUI = FindObjectOfType<TemuGameplay.UI.HealthUI>();
            if (healthUI != null)
            {
                healthUI.RefreshDamagePreview();
                Debug.Log("🔄 Health UI refreshed");
            }
        }

        [ContextMenu("Test Golden Requirement Check")]
        public void TestGoldenRequirementCheck()
        {
            if (!Application.isPlaying || gameplayManager?.CurrentLevel == null)
            {
                Debug.Log("❌ Cannot test - no level or not playing");
                return;
            }

            Debug.Log("=== Golden Requirement Test ===");
            
            var currentLevel = gameplayManager.CurrentLevel;
            var currentSet = gameplayManager.CurrentSet;
            
            // 显示当前金色要求
            if (currentLevel.GoldenRequirements == null || currentLevel.GoldenRequirements.Count == 0)
            {
                Debug.Log("❌ No golden requirements found");
                return;
            }
            
            Debug.Log($"Golden requirements count: {currentLevel.GoldenRequirements.Count}");
            
            foreach (var golden in currentLevel.GoldenRequirements)
            {
                int currentCount = currentSet.TraitCounters.ContainsKey(golden.Key) 
                    ? currentSet.TraitCounters[golden.Key].CurrentCount : 0;
                    
                Debug.Log($"  {golden.Key}: Need {golden.Value}, Have {currentCount}");
            }
            
            // 检查哪些金色要求被满足
            var metRequirements = currentLevel.CheckGoldenRequirements(currentSet.TraitCounters);
            Debug.Log($"Met golden requirements: {metRequirements.Count}");
            
            if (metRequirements.Count > 0)
            {
                foreach (var met in metRequirements)
                {
                    Debug.Log($"  ✅ {met}");
                }
                
                int totalHeal = metRequirements.Count * 10;
                Debug.Log($"Would heal: {totalHeal} health");
            }
            else
            {
                Debug.Log("  No golden requirements met");
            }
        }

        [ContextMenu("Test Health Healing")]
        public void TestHealthHealing()
        {
            if (!Application.isPlaying)
            {
                Debug.Log("请在运行时使用此功能");
                return;
            }

            var healthSystem = gameplayManager?.HealthSystem;
            if (healthSystem == null)
            {
                Debug.Log("❌ No health system found");
                return;
            }

            int healthBefore = healthSystem.CurrentHealth;
            healthSystem.Heal(10);
            int healthAfter = healthSystem.CurrentHealth;
            
            Debug.Log($"🩹 Test heal: {healthBefore} -> {healthAfter} (+{healthAfter - healthBefore})");
        }

        [ContextMenu("Generate Test Level With Golden")]
        public void GenerateTestLevelWithGolden()
        {
            if (!Application.isPlaying || gameplayManager == null)
            {
                Debug.Log("❌ Cannot generate - not playing or no GameplayManager");
                return;
            }

            // 创建测试关卡
            var testLevel = new LevelRequirement("Golden Test Level");
            testLevel.AddRequirement(TraitType.Hiking, 1);
            testLevel.AddRequirement(TraitType.Running, 1);
            
            // 强制添加金色要求
            testLevel.AddGoldenRequirement(TraitType.Sitting, 2);
            testLevel.AddGoldenRequirement(TraitType.Climbing, 1);
            
            // 设置关卡
            gameplayManager.SetLevel(testLevel);
            
            Debug.Log("✨ Generated test level with golden requirements:");
            Debug.Log("  Normal: Hiking x1, Running x1");
            Debug.Log("  Golden: Sitting x2, Climbing x1");
        }

        [ContextMenu("Test Full Health Edge Case")]
        public void TestFullHealthEdgeCase()
        {
            if (!Application.isPlaying || gameplayManager == null)
            {
                Debug.Log("❌ Cannot test - not playing or no GameplayManager");
                return;
            }

            var healthSystem = gameplayManager.HealthSystem;
            if (healthSystem == null)
            {
                Debug.Log("❌ No health system found");
                return;
            }

            // 确保满血
            healthSystem.ResetHealth();
            Debug.Log($"🩹 Reset to full health: {healthSystem.CurrentHealth}/{healthSystem.MaxHealth}");

            // 创建一个测试场景：需要扣10血但也会恢复10血
            var testLevel = new LevelRequirement("Edge Case Test");
            testLevel.AddRequirement(TraitType.Hiking, 2); // 需要2个，假设玩家只有0个 = 扣10血
            testLevel.AddGoldenRequirement(TraitType.Sitting, 1); // 金色要求，假设玩家有1个 = 恢复10血
            
            gameplayManager.SetLevel(testLevel);
            
            Debug.Log("🧪 Test scenario:");
            Debug.Log("  Need: Hiking x2 (will cause 10 damage if unmet)");
            Debug.Log("  Golden: Sitting x1 (will heal 10 if met)");
            Debug.Log("  Expected: Damage first (-10), then heal (+10), net = 0");
            Debug.Log("  Player should end up with 100 health");
        }
    }
} 