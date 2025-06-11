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
        
        [Header("Level Naming")]
        public string[] levelNamePrefixes = { "Challenge", "Mission", "Trial", "Test", "Task" };
        public string[] levelNameSuffixes = { "Alpha", "Beta", "Gamma", "Delta", "Epsilon" };
        
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
            
            if (levelNamePrefixes == null || levelNamePrefixes.Length == 0)
            {
                levelNamePrefixes = new string[] { "Random Challenge" };
            }
            
            if (levelNameSuffixes == null || levelNameSuffixes.Length == 0)
            {
                levelNameSuffixes = new string[] { "I", "II", "III", "IV", "V" };
            }
        }
    }
} 