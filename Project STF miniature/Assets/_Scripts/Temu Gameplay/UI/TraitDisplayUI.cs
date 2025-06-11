using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using TemuGameplay.Data;
using TemuGameplay.Core;

namespace TemuGameplay.UI
{
    public class TraitDisplayUI : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private TextMeshProUGUI traitCounterText;
        [SerializeField] private TextMeshProUGUI levelObjectiveText;
        
        [Header("Display Settings")]
        [SerializeField] private bool showOnlyActiveTraits = true; // 只显示有物品的traits
        [SerializeField] private bool showZeroCountTraits = false; // 是否显示0计数的traits
        
        private GameplayManager gameplayManager;

        private void Start()
        {
            gameplayManager = FindObjectOfType<GameplayManager>();
            if (gameplayManager == null)
            {
                Debug.LogError("GameplayManager not found!");
                return;
            }

            // 监听更新事件
            gameplayManager.CurrentSet.OnTraitCountersUpdated += UpdateTraitDisplay;
            gameplayManager.OnLevelChanged += UpdateLevelObjectiveDisplay;
            gameplayManager.OnLevelSubmitted += OnLevelSubmitted;

            // 初始更新
            UpdateTraitDisplay();
            UpdateLevelObjectiveDisplay(gameplayManager.CurrentLevel);
        }

        private void UpdateTraitDisplay()
        {
            if (traitCounterText == null || gameplayManager == null) return;

            var traitCounters = gameplayManager.CurrentSet.TraitCounters;
            var displayLines = new List<string>();

            foreach (var counter in traitCounters.Values)
            {
                // 根据设置决定是否显示这个trait
                bool shouldShow = false;
                
                if (showOnlyActiveTraits)
                {
                    // 只显示有总数且当前数量不为零的traits
                    shouldShow = counter.TotalCount > 0 && counter.CurrentCount > 0;
                }
                else
                {
                    // 只显示有总数且当前数量不为零的traits，除非设置显示0计数
                    shouldShow = showZeroCountTraits || (counter.CurrentCount > 0 && counter.TotalCount > 0);
                }

                if (shouldShow)
                {
                    string traitName = GetTraitDisplayName(counter.TraitType);
                    string line = $"{traitName}: {counter.CurrentCount}/{counter.TotalCount}";
                    displayLines.Add(line);
                }
            }

            traitCounterText.text = string.Join("\n", displayLines);
            
            // 同时更新关卡目标显示，确保实时显示进度
            if (gameplayManager.CurrentLevel != null)
            {
                UpdateLevelObjectiveDisplay(gameplayManager.CurrentLevel);
            }
        }

        private void UpdateLevelObjectiveDisplay(LevelRequirement level)
        {
            if (levelObjectiveText == null || level == null) return;

            var objectiveLines = new List<string>();
            objectiveLines.Add($"Objective: {level.LevelName}");
            objectiveLines.Add(""); // Empty line

            foreach (var requirement in level.RequiredTraits)
            {
                string traitName = GetTraitDisplayName(requirement.Key);
                int currentCount = 0;
                
                if (gameplayManager?.CurrentSet?.TraitCounters?.ContainsKey(requirement.Key) == true)
                {
                    currentCount = gameplayManager.CurrentSet.TraitCounters[requirement.Key].CurrentCount;
                }

                // Color based on requirement completion
                string statusIcon = currentCount >= requirement.Value ? "√" : "×";
                string line = $"{statusIcon}{traitName}: {currentCount}/{requirement.Value}";
                objectiveLines.Add(line);
            }

            levelObjectiveText.text = string.Join("\n", objectiveLines);
        }

        private string GetTraitDisplayName(TraitType trait)
        {
            switch (trait)
            {
                case TraitType.Hiking: return "Hiking";
                case TraitType.Running: return "Running";
                case TraitType.Nutrition: return "Nutrition";
                case TraitType.Climbing: return "Climbing";
                case TraitType.Sitting: return "Sitting";
                case TraitType.Entertainment: return "Entertainment";
                case TraitType.Survival: return "Survival";
                case TraitType.Food: return "Food";
                case TraitType.Water: return "Water";
                case TraitType.Tool: return "Tool";
                default: return trait.ToString();
            }
        }

        // 手动刷新方法
        public void RefreshDisplay()
        {
            UpdateTraitDisplay();
            if (gameplayManager?.CurrentLevel != null)
            {
                UpdateLevelObjectiveDisplay(gameplayManager.CurrentLevel);
            }
        }

        private void OnLevelSubmitted(bool isCompleted)
        {
            if (isCompleted)
            {
                Debug.Log("🎉 UI: Level submitted and completed!");
                // 可以在这里添加完成效果，比如显示成功动画
            }
            else
            {
                Debug.Log("❌ UI: Level submitted but not completed");
                // 可以在这里添加失败提示效果
            }
        }

        private void OnDestroy()
        {
            if (gameplayManager?.CurrentSet != null)
            {
                gameplayManager.CurrentSet.OnTraitCountersUpdated -= UpdateTraitDisplay;
            }
            
            if (gameplayManager != null)
            {
                gameplayManager.OnLevelChanged -= UpdateLevelObjectiveDisplay;
                gameplayManager.OnLevelSubmitted -= OnLevelSubmitted;
            }
        }

        // 可以在Inspector中调用的工具方法
        [ContextMenu("Toggle Show Only Active Traits")]
        public void ToggleShowOnlyActiveTraits()
        {
            showOnlyActiveTraits = !showOnlyActiveTraits;
            UpdateTraitDisplay();
        }

        [ContextMenu("Toggle Show Zero Count Traits")]
        public void ToggleShowZeroCountTraits()
        {
            showZeroCountTraits = !showZeroCountTraits;
            UpdateTraitDisplay();
        }
    }
} 