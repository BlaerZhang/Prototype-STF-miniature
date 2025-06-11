using System;
using UnityEngine;

namespace TemuGameplay.Data
{
    [Serializable]
    public class TraitCounter
    {
        [SerializeField] private TraitType traitType;
        [SerializeField] private int currentCount;
        [SerializeField] private int totalCount;

        public TraitType TraitType => traitType;
        public int CurrentCount => currentCount;
        public int TotalCount => totalCount;
        
        public float Progress => totalCount > 0 ? (float)currentCount / totalCount : 0f;

        public TraitCounter(TraitType type, int total)
        {
            traitType = type;
            totalCount = total;
            currentCount = 0;
        }

        public void SetCurrentCount(int count)
        {
            currentCount = Mathf.Clamp(count, 0, totalCount);
        }

        public void IncrementCount()
        {
            currentCount = Mathf.Min(currentCount + 1, totalCount);
        }

        public void DecrementCount()
        {
            currentCount = Mathf.Max(currentCount - 1, 0);
        }

        public void Reset()
        {
            currentCount = 0;
        }
    }
} 