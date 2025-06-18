using System;
using System.Collections.Generic;
using UnityEngine;

namespace TemuGameplay.Data
{
    [Serializable]
    public class LevelRequirement
    {
        [SerializeField] private string levelName;
        [SerializeField] private Dictionary<TraitType, int> requiredTraits;
        [SerializeField] private Dictionary<TraitType, int> goldenRequirements; // 金色要求，满足后恢复血量

        public string LevelName => levelName;
        public Dictionary<TraitType, int> RequiredTraits => requiredTraits;
        public Dictionary<TraitType, int> GoldenRequirements => goldenRequirements;

        public LevelRequirement(string name)
        {
            levelName = name;
            requiredTraits = new Dictionary<TraitType, int>();
            goldenRequirements = new Dictionary<TraitType, int>();
        }

        public void AddRequirement(TraitType trait, int minCount)
        {
            requiredTraits[trait] = minCount;
        }

        public void RemoveRequirement(TraitType trait)
        {
            requiredTraits.Remove(trait);
        }

        public void AddGoldenRequirement(TraitType trait, int minCount)
        {
            goldenRequirements[trait] = minCount;
        }

        public void RemoveGoldenRequirement(TraitType trait)
        {
            goldenRequirements.Remove(trait);
        }

        public bool CheckRequirement(Dictionary<TraitType, TraitCounter> traitCounters)
        {
            foreach (var requirement in requiredTraits)
            {
                TraitType trait = requirement.Key;
                int requiredCount = requirement.Value;

                if (!traitCounters.ContainsKey(trait) || 
                    traitCounters[trait].CurrentCount < requiredCount)
                {
                    return false;
                }
            }

            return true;
        }

        public List<string> GetUnmetRequirements(Dictionary<TraitType, TraitCounter> traitCounters)
        {
            List<string> unmet = new List<string>();

            foreach (var requirement in requiredTraits)
            {
                TraitType trait = requirement.Key;
                int requiredCount = requirement.Value;
                int currentCount = traitCounters.ContainsKey(trait) ? traitCounters[trait].CurrentCount : 0;

                if (currentCount < requiredCount)
                {
                    unmet.Add($"{trait}: {currentCount}/{requiredCount}");
                }
            }

            return unmet;
        }

        /// <summary>
        /// 检查金色要求是否满足
        /// </summary>
        public List<TraitType> CheckGoldenRequirements(Dictionary<TraitType, TraitCounter> traitCounters)
        {
            List<TraitType> metGoldenRequirements = new List<TraitType>();

            foreach (var requirement in goldenRequirements)
            {
                TraitType trait = requirement.Key;
                int requiredCount = requirement.Value;
                int currentCount = traitCounters.ContainsKey(trait) ? traitCounters[trait].CurrentCount : 0;

                if (currentCount >= requiredCount)
                {
                    metGoldenRequirements.Add(trait);
                }
            }

            return metGoldenRequirements;
        }

        /// <summary>
        /// 获取所有要求的显示文本（包括金色要求）
        /// </summary>
        public List<(string text, bool isGolden)> GetAllRequirementsDisplay(Dictionary<TraitType, TraitCounter> traitCounters)
        {
            List<(string, bool)> allRequirements = new List<(string, bool)>();

            // 普通要求
            foreach (var requirement in requiredTraits)
            {
                TraitType trait = requirement.Key;
                int requiredCount = requirement.Value;
                int currentCount = traitCounters.ContainsKey(trait) ? traitCounters[trait].CurrentCount : 0;
                string text = $"{trait}: {currentCount}/{requiredCount}";
                allRequirements.Add((text, false));
            }

            // 金色要求
            foreach (var requirement in goldenRequirements)
            {
                TraitType trait = requirement.Key;
                int requiredCount = requirement.Value;
                int currentCount = traitCounters.ContainsKey(trait) ? traitCounters[trait].CurrentCount : 0;
                string text = $"✨{trait}: {currentCount}/{requiredCount} (Heal +10)";
                allRequirements.Add((text, true));
            }

            return allRequirements;
        }
    }
} 