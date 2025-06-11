using System.Collections.Generic;
using UnityEngine;
using TemuGameplay.Data;

namespace TemuGameplay.ScriptableObjects
{
    [CreateAssetMenu(fileName = "LevelConfig", menuName = "Temu Gameplay/Level Config")]
    public class LevelConfigSO : ScriptableObject
    {
        [System.Serializable]
        public class TraitRequirement
        {
            public TraitType traitType;
            public int minCount;
        }

        [SerializeField] private string levelName;
        [TextArea(2, 4)]
        [SerializeField] private string levelDescription;
        [SerializeField] private List<TraitRequirement> requirements = new List<TraitRequirement>();
        [SerializeField] private int maxSetSize = -1; // -1 表示无限制

        public string LevelName => levelName;
        public string LevelDescription => levelDescription;
        public List<TraitRequirement> Requirements => requirements;
        public int MaxSetSize => maxSetSize;

        public LevelRequirement ToLevelRequirement()
        {
            LevelRequirement level = new LevelRequirement(levelName);
            
            foreach (var req in requirements)
            {
                level.AddRequirement(req.traitType, req.minCount);
            }

            return level;
        }

        // 编辑器工具方法
        [ContextMenu("Validate Requirements")]
        private void ValidateRequirements()
        {
            HashSet<TraitType> seenTraits = new HashSet<TraitType>();
            bool hasDuplicates = false;

            foreach (var req in requirements)
            {
                if (seenTraits.Contains(req.traitType))
                {
                    Debug.LogError($"Duplicate trait requirement found: {req.traitType}");
                    hasDuplicates = true;
                }
                else
                {
                    seenTraits.Add(req.traitType);
                }

                if (req.minCount <= 0)
                {
                    Debug.LogWarning($"Trait {req.traitType} has non-positive requirement: {req.minCount}");
                }
            }

            if (!hasDuplicates && requirements.Count > 0)
            {
                Debug.Log("All trait requirements are valid!");
            }
            else if (requirements.Count == 0)
            {
                Debug.LogWarning("No requirements defined for this level!");
            }
        }
    }
} 