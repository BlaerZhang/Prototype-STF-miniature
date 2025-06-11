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

        public string LevelName => levelName;
        public Dictionary<TraitType, int> RequiredTraits => requiredTraits;

        public LevelRequirement(string name)
        {
            levelName = name;
            requiredTraits = new Dictionary<TraitType, int>();
        }

        public void AddRequirement(TraitType trait, int minCount)
        {
            requiredTraits[trait] = minCount;
        }

        public void RemoveRequirement(TraitType trait)
        {
            requiredTraits.Remove(trait);
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
    }
} 