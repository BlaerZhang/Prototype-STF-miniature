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

            // 使用LevelRequirement的新方法获取所有要求显示
            var allRequirements = level.GetAllRequirementsDisplay(gameplayManager?.CurrentSet?.TraitCounters);
            
            bool hasIndependent = false;
            bool hasOptionalGroups = false;
            bool hasGolden = false;

            // 先显示独立要求
            foreach (var (text, isGolden, isOptionalGroup) in allRequirements)
            {
                if (!isGolden && !isOptionalGroup)
                {
                    if (!hasIndependent)
                    {
                        objectiveLines.Add("Independent Requirements:");
                        hasIndependent = true;
                    }
                    string traitName = text.Split(':')[0];
                    string displayName = GetTraitDisplayName(ParseTraitType(traitName));
                    string count = text.Split(':')[1];
                    objectiveLines.Add($"{text.Replace(traitName, displayName)}");
                }
            }

            // 显示任选组要求
            if (hasIndependent && allRequirements.Any(req => req.isOptionalGroup))
            {
                objectiveLines.Add(""); // Empty line
            }
            
            foreach (var (text, isGolden, isOptionalGroup) in allRequirements)
            {
                if (!isGolden && isOptionalGroup)
                {
                    if (!hasOptionalGroups && !text.StartsWith("  ")) // 只有组标题才显示分组头
                    {
                        objectiveLines.Add("Optional Groups:");
                        hasOptionalGroups = true;
                    }
                    
                    if (text.StartsWith("  ")) // 子项，显示trait名称
                    {
                        string cleanText = text.Trim();
                        string[] parts = cleanText.Split(' ');
                        if (parts.Length >= 2)
                        {
                            string statusIcon = parts[0];
                            string traitName = parts[1].Replace(":", "");
                            string displayName = GetTraitDisplayName(ParseTraitType(traitName));
                            string count = cleanText.Split(':')[1];
                            objectiveLines.Add($"    {statusIcon} {displayName}:{count}");
                        }
                    }
                    else
                    {
                        objectiveLines.Add(text);
                    }
                }
            }

            // 显示金色要求
            var goldenRequirements = allRequirements.Where(req => req.isGolden).ToList();
            if (goldenRequirements.Count > 0)
            {
                objectiveLines.Add(""); // Empty line
                objectiveLines.Add("Golden Requirements:");

                foreach (var (text, isGolden, isOptionalGroup) in goldenRequirements)
                {
                    string processedText = text;
                    // 替换trait名称为显示名称
                    foreach (TraitType trait in System.Enum.GetValues(typeof(TraitType)))
                    {
                        string enumName = trait.ToString();
                        string displayName = GetTraitDisplayName(trait);
                        processedText = processedText.Replace($"✨{enumName}:", $"✨{displayName}:");
                    }
                    
                    string line = $"<color=#FFD700>{processedText}</color>";
                    objectiveLines.Add(line);
                }
            }

            levelObjectiveText.text = string.Join("\n", objectiveLines);
        }
        
        private TraitType ParseTraitType(string traitName)
        {
            if (System.Enum.TryParse<TraitType>(traitName, out TraitType result))
            {
                return result;
            }
            return TraitType.Hiking; // fallback
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