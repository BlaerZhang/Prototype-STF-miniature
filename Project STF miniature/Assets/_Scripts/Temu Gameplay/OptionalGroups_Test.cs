using UnityEngine;
using TemuGameplay.Core;
using TemuGameplay.Data;

namespace TemuGameplay
{
    /// <summary>
    /// 任选组功能测试工具
    /// 用于测试和调试新的任选requirements功能
    /// </summary>
    public class OptionalGroups_Test : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private TraitType[] testGroupTraits = { TraitType.Climbing, TraitType.Entertainment, TraitType.Survival };
        [SerializeField] private int[] testGroupCounts = { 2, 1, 3 };
        [SerializeField] private int requiredTypesCount = 2;
        [SerializeField] private float groupDamage = 20f;
        
        private GameplayManager gameplayManager;

        private void Start()
        {
            gameplayManager = FindObjectOfType<GameplayManager>();
        }

        [ContextMenu("Force Add Test Optional Group")]
        public void ForceAddTestOptionalGroup()
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

            // 创建测试任选组
            var optionalGroup = new OptionalRequirementGroup("Test Group", requiredTypesCount, groupDamage);
            
            int count = Mathf.Min(testGroupTraits.Length, testGroupCounts.Length);
            for (int i = 0; i < count; i++)
            {
                optionalGroup.AddTraitRequirement(testGroupTraits[i], testGroupCounts[i]);
            }

            // 添加到当前关卡
            gameplayManager.CurrentLevel.AddOptionalGroup(optionalGroup);
            
            Debug.Log($"✅ Added test optional group with {count} traits, need {requiredTypesCount} types");
            
            // 触发UI更新
            RefreshAllUI();
        }

        [ContextMenu("Clear All Optional Groups")]
        public void ClearAllOptionalGroups()
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

            gameplayManager.CurrentLevel.OptionalGroups.Clear();
            Debug.Log("🗑️ Cleared all optional groups");
            
            RefreshAllUI();
        }

        [ContextMenu("Test Group Damage Calculation")]
        public void TestGroupDamageCalculation()
        {
            if (!Application.isPlaying)
            {
                Debug.Log("请在运行时使用此功能");
                return;
            }

            if (gameplayManager?.CurrentLevel?.OptionalGroups?.Count == 0)
            {
                Debug.Log("❌ No optional groups found. Add one first.");
                return;
            }

            var currentTraitCounters = gameplayManager.CurrentSet.TraitCounters;
            
            foreach (var group in gameplayManager.CurrentLevel.OptionalGroups)
            {
                int metTypes = group.CheckMetTypesCount(currentTraitCounters);
                float damage = group.CalculateGroupDamage(currentTraitCounters);
                bool satisfied = group.IsGroupSatisfied(currentTraitCounters);
                
                Debug.Log($"📊 Group '{group.GroupName}':");
                Debug.Log($"  - Met types: {metTypes}/{group.RequiredTypesCount}");
                Debug.Log($"  - Satisfied: {satisfied}");
                Debug.Log($"  - Damage: {damage}/{group.GroupDamageAmount}");
                
                var traitStatus = group.GetTraitStatus(currentTraitCounters);
                foreach (var (trait, current, required, met) in traitStatus)
                {
                    string status = met ? "✅" : "❌";
                    Debug.Log($"    {status} {trait}: {current}/{required}");
                }
            }
        }

        [ContextMenu("Regenerate with Optional Groups Enabled")]
        public void RegenerateWithOptionalGroups()
        {
            if (!Application.isPlaying)
            {
                Debug.Log("请在运行时使用此功能");
                return;
            }

            // 确保任选组概率为100%用于测试
            var randomSettings = gameplayManager.GetRandomSettings();
            float originalChance = randomSettings.optionalGroupChance;
            randomSettings.optionalGroupChance = 1f;
            
            // 重新生成关卡
            gameplayManager.RerollRequirements();
            
            // 恢复原始概率
            randomSettings.optionalGroupChance = originalChance;
            
            Debug.Log("🎲 Regenerated level with optional groups guaranteed");
        }

        [ContextMenu("Show Current Level Info")]
        public void ShowCurrentLevelInfo()
        {
            if (!Application.isPlaying)
            {
                Debug.Log("请在运行时使用此功能");
                return;
            }

            if (gameplayManager?.CurrentLevel == null)
            {
                Debug.Log("❌ No current level");
                return;
            }

            var level = gameplayManager.CurrentLevel;
            
            Debug.Log($"📋 Level: {level.LevelName}");
            Debug.Log($"  - Independent Requirements: {level.RequiredTraits.Count}");
            Debug.Log($"  - Optional Groups: {level.OptionalGroups.Count}");
            Debug.Log($"  - Golden Requirements: {level.GoldenRequirements.Count}");
            
            foreach (var req in level.RequiredTraits)
            {
                Debug.Log($"    ⚪ {req.Key}: {req.Value}");
            }
            
            foreach (var group in level.OptionalGroups)
            {
                Debug.Log($"    🔵 {group.GroupName}: {group.TraitRequirements.Count} traits, need {group.RequiredTypesCount}");
            }
            
            foreach (var golden in level.GoldenRequirements)
            {
                Debug.Log($"    ✨ {golden.Key}: {golden.Value}");
            }
        }

        private void RefreshAllUI()
        {
            // 触发UI更新
            var traitDisplay = FindObjectOfType<TemuGameplay.UI.TraitDisplayUI>();
            if (traitDisplay != null)
            {
                traitDisplay.RefreshDisplay();
            }
            
            var healthUI = FindObjectOfType<TemuGameplay.UI.HealthUI>();
            if (healthUI != null)
            {
                healthUI.RefreshDamagePreview();
            }
            
            Debug.Log("🔄 UI refreshed");
        }

        [ContextMenu("Quick Test Setup")]
        public void QuickTestSetup()
        {
            if (!Application.isPlaying)
            {
                Debug.Log("请在运行时使用此功能");
                return;
            }

            // 清除当前选择
            gameplayManager.ResetCurrentSet();
            
            // 生成带任选组的关卡
            RegenerateWithOptionalGroups();
            
            Debug.Log("⚡ Quick test setup complete!");
        }
    }
} 