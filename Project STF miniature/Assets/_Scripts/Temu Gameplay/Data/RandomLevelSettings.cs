using System;
using UnityEngine;

namespace TemuGameplay.Data
{
    [Serializable]
    public class RandomLevelSettings
    {
        [Header("Generation Parameters")]
        [Range(1, 5)]
        public int minTraitTypes = 2;           // 最少要求多少种trait
        
        [Range(1, 8)]
        public int maxTraitTypes = 4;           // 最多要求多少种trait
        
        [Range(1, 10)]
        public int minTraitCount = 1;           // 每个trait的最少数量
        
        [Range(1, 10)]
        public int maxTraitCount = 3;           // 每个trait的最大数量
        
        [Header("Optional Groups")]
        [Range(0f, 1f)]
        public float optionalGroupChance = 0.3f; // 生成任选组的概率 (0-1)
        
        [Range(1, 3)]
        public int maxOptionalGroups = 1;       // 最多生成多少个任选组
        
        [Range(2, 5)]
        public int minGroupSize = 3;            // 任选组最少包含多少种trait
        
        [Range(3, 6)]
        public int maxGroupSize = 4;            // 任选组最多包含多少种trait
        
        [Range(1, 3)]
        public int minRequiredTypes = 2;        // 任选组至少需要满足多少种trait
        
        [Range(15f, 30f)]
        public float groupBaseDamage = 20f;     // 任选组的基础扣血量
        
        [Header("Level Naming")]
        public string[] levelNamePrefixes = { "Challenge", "Mission", "Trial", "Test", "Task" };
        public string[] levelNameSuffixes = { "Alpha", "Beta", "Gamma", "Delta", "Epsilon" };
        public string[] groupNames = { "Mobility", "Combat", "Support", "Utility", "Stealth" }; // 任选组名称
        
        [Header("Advanced Settings")]
        public bool ensureCompletable = true;   // 确保关卡可完成（有足够的物品）
        public bool favorActiveTraits = true;  // 倾向于选择有物品的trait
        
        [Range(0f, 1f)]
        public float difficultyModifier = 0.5f; // 难度修正器 (0=简单, 1=困难)

        // 验证设置的有效性
        public void ValidateSettings()
        {
            minTraitTypes = Mathf.Max(1, minTraitTypes);
            maxTraitTypes = Mathf.Max(minTraitTypes, maxTraitTypes);
            minTraitCount = Mathf.Max(1, minTraitCount);
            maxTraitCount = Mathf.Max(minTraitCount, maxTraitCount);
            
            // 验证任选组设置
            optionalGroupChance = Mathf.Clamp01(optionalGroupChance);
            maxOptionalGroups = Mathf.Max(1, maxOptionalGroups);
            minGroupSize = Mathf.Max(2, minGroupSize);
            maxGroupSize = Mathf.Max(minGroupSize, maxGroupSize);
            minRequiredTypes = Mathf.Max(1, minRequiredTypes);
            minRequiredTypes = Mathf.Min(minRequiredTypes, minGroupSize - 1); // 确保任选组有意义
            groupBaseDamage = Mathf.Max(5f, groupBaseDamage);
            
            if (levelNamePrefixes == null || levelNamePrefixes.Length == 0)
            {
                levelNamePrefixes = new string[] { "Random Challenge" };
            }
            
            if (levelNameSuffixes == null || levelNameSuffixes.Length == 0)
            {
                levelNameSuffixes = new string[] { "I", "II", "III", "IV", "V" };
            }
            
            if (groupNames == null || groupNames.Length == 0)
            {
                groupNames = new string[] { "Group A", "Group B", "Group C" };
            }
        }
    }
} 