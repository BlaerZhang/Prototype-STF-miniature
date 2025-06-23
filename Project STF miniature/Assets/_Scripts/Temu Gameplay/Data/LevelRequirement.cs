using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TemuGameplay.Data
{
    /// <summary>
    /// 任选要求组 - 玩家只需满足组内指定数量的trait种类即可
    /// </summary>
    [Serializable]
    public class OptionalRequirementGroup
    {
        [SerializeField] private string groupName;
        [SerializeField] private Dictionary<TraitType, int> traitRequirements; // 组内每个trait的数量要求
        [SerializeField] private int requiredTypesCount; // 需要满足的trait种类数量
        [SerializeField] private float groupDamageAmount; // 组的基础扣血量

        public string GroupName => groupName;
        public Dictionary<TraitType, int> TraitRequirements => traitRequirements;
        public int RequiredTypesCount => requiredTypesCount;
        public float GroupDamageAmount => groupDamageAmount;

        public OptionalRequirementGroup(string name, int requiredTypes, float damageAmount = 20f)
        {
            groupName = name;
            traitRequirements = new Dictionary<TraitType, int>();
            requiredTypesCount = requiredTypes;
            groupDamageAmount = damageAmount;
        }

        public void AddTraitRequirement(TraitType trait, int count)
        {
            traitRequirements[trait] = count;
        }

        public void RemoveTraitRequirement(TraitType trait)
        {
            traitRequirements.Remove(trait);
        }

        /// <summary>
        /// 检查组内满足的trait种类数量
        /// </summary>
        public int CheckMetTypesCount(Dictionary<TraitType, TraitCounter> traitCounters)
        {
            int metCount = 0;
            foreach (var requirement in traitRequirements)
            {
                TraitType trait = requirement.Key;
                int requiredCount = requirement.Value;
                int currentCount = traitCounters.ContainsKey(trait) ? traitCounters[trait].CurrentCount : 0;

                if (currentCount >= requiredCount)
                {
                    metCount++;
                }
            }
            return metCount;
        }

        /// <summary>
        /// 检查组是否满足要求
        /// </summary>
        public bool IsGroupSatisfied(Dictionary<TraitType, TraitCounter> traitCounters)
        {
            return CheckMetTypesCount(traitCounters) >= requiredTypesCount;
        }

        /// <summary>
        /// 计算渐进扣血量
        /// </summary>
        public float CalculateGroupDamage(Dictionary<TraitType, TraitCounter> traitCounters)
        {
            int metCount = CheckMetTypesCount(traitCounters);
            if (metCount >= requiredTypesCount)
            {
                return 0f; // 满足要求，不扣血
            }

            // 渐进扣血：根据满足的比例计算
            float satisfactionRatio = (float)metCount / requiredTypesCount;
            float damageRatio = 1f - satisfactionRatio; // 未满足的比例
            return groupDamageAmount * damageRatio;
        }

        /// <summary>
        /// 获取组内每个trait的满足状态
        /// </summary>
        public List<(TraitType trait, int current, int required, bool met)> GetTraitStatus(Dictionary<TraitType, TraitCounter> traitCounters)
        {
            var status = new List<(TraitType, int, int, bool)>();
            foreach (var requirement in traitRequirements)
            {
                TraitType trait = requirement.Key;
                int requiredCount = requirement.Value;
                int currentCount = traitCounters.ContainsKey(trait) ? traitCounters[trait].CurrentCount : 0;
                bool met = currentCount >= requiredCount;
                status.Add((trait, currentCount, requiredCount, met));
            }
            return status;
        }
    }

    [Serializable]
    public class LevelRequirement
    {
        [SerializeField] private string levelName;
        [SerializeField] private Dictionary<TraitType, int> requiredTraits; // 独立要求
        [SerializeField] private Dictionary<TraitType, int> goldenRequirements; // 金色要求，满足后恢复血量
        [SerializeField] private List<OptionalRequirementGroup> optionalGroups; // 任选要求组

        public string LevelName => levelName;
        public Dictionary<TraitType, int> RequiredTraits => requiredTraits;
        public Dictionary<TraitType, int> GoldenRequirements => goldenRequirements;
        public List<OptionalRequirementGroup> OptionalGroups => optionalGroups;

        public LevelRequirement(string name)
        {
            levelName = name;
            requiredTraits = new Dictionary<TraitType, int>();
            goldenRequirements = new Dictionary<TraitType, int>();
            optionalGroups = new List<OptionalRequirementGroup>();
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

        public void AddOptionalGroup(OptionalRequirementGroup group)
        {
            optionalGroups.Add(group);
        }

        public void RemoveOptionalGroup(OptionalRequirementGroup group)
        {
            optionalGroups.Remove(group);
        }

        public bool CheckRequirement(Dictionary<TraitType, TraitCounter> traitCounters)
        {
            // 检查独立要求
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

            // 检查任选组要求
            foreach (var group in optionalGroups)
            {
                if (!group.IsGroupSatisfied(traitCounters))
                {
                    return false;
                }
            }

            return true;
        }

        public List<string> GetUnmetRequirements(Dictionary<TraitType, TraitCounter> traitCounters)
        {
            List<string> unmet = new List<string>();

            // 独立要求
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

            // 任选组要求
            foreach (var group in optionalGroups)
            {
                if (!group.IsGroupSatisfied(traitCounters))
                {
                    int metCount = group.CheckMetTypesCount(traitCounters);
                    unmet.Add($"{group.GroupName}: {metCount}/{group.RequiredTypesCount} types satisfied");
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
        /// 获取所有要求的显示文本（包括金色要求和任选组）
        /// </summary>
        public List<(string text, bool isGolden, bool isOptionalGroup)> GetAllRequirementsDisplay(Dictionary<TraitType, TraitCounter> traitCounters)
        {
            List<(string, bool, bool)> allRequirements = new List<(string, bool, bool)>();

            // 普通要求
            foreach (var requirement in requiredTraits)
            {
                TraitType trait = requirement.Key;
                int requiredCount = requirement.Value;
                int currentCount = traitCounters.ContainsKey(trait) ? traitCounters[trait].CurrentCount : 0;
                string text = $"{trait}: {currentCount}/{requiredCount}";
                allRequirements.Add((text, false, false));
            }

            // 任选组要求
            foreach (var group in optionalGroups)
            {
                int metCount = group.CheckMetTypesCount(traitCounters);
                bool groupSatisfied = group.IsGroupSatisfied(traitCounters);
                string statusIcon = groupSatisfied ? "✓" : "×";
                string text = $"{statusIcon} {group.GroupName}: {metCount}/{group.RequiredTypesCount} types required";
                allRequirements.Add((text, false, true));

                // 显示组内每个trait的详细状态
                var traitStatus = group.GetTraitStatus(traitCounters);
                foreach (var (trait, current, required, met) in traitStatus)
                {
                    string traitIcon = met ? "✓" : "×";
                    string traitText = $"  {traitIcon} {trait}: {current}/{required}";
                    allRequirements.Add((traitText, false, true));
                }
            }

            // 金色要求
            foreach (var requirement in goldenRequirements)
            {
                TraitType trait = requirement.Key;
                int requiredCount = requirement.Value;
                int currentCount = traitCounters.ContainsKey(trait) ? traitCounters[trait].CurrentCount : 0;
                string text = $"✨{trait}: {currentCount}/{requiredCount} (Heal +10)";
                allRequirements.Add((text, true, false));
            }

            return allRequirements;
        }
    }
} 