using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TemuGameplay.Data;

namespace TemuGameplay.Core
{
    public class PlayerSet
    {
        private List<Item> selectedItems;
        private Dictionary<TraitType, TraitCounter> traitCounters;
        private int maxSetSize;

        public List<Item> SelectedItems => selectedItems;
        public Dictionary<TraitType, TraitCounter> TraitCounters => traitCounters;
        public int MaxSetSize => maxSetSize;
        public int CurrentSetSize => selectedItems.Count;
        public bool IsSetFull => maxSetSize > 0 && selectedItems.Count >= maxSetSize;

        public event Action<Item> OnItemAdded;
        public event Action<Item> OnItemRemoved;
        public event Action OnTraitCountersUpdated;

        public PlayerSet(int maxSize = -1)
        {
            selectedItems = new List<Item>();
            traitCounters = new Dictionary<TraitType, TraitCounter>();
            maxSetSize = maxSize;
            InitializeTraitCounters();
        }

        private void InitializeTraitCounters()
        {
            foreach (TraitType trait in Enum.GetValues(typeof(TraitType)))
            {
                traitCounters[trait] = new TraitCounter(trait, 0);
            }
        }

        public void SetTraitTotalCounts(Dictionary<TraitType, int> totalCounts)
        {
            foreach (var kvp in totalCounts)
            {
                if (traitCounters.ContainsKey(kvp.Key))
                {
                    traitCounters[kvp.Key] = new TraitCounter(kvp.Key, kvp.Value);
                }
            }
            UpdateTraitCounters();
        }

        public bool CanAddItem(Item item)
        {
            if (item == null) return false;
            if (HasItem(item)) return false;
            if (IsSetFull) return false;
            return true;
        }

        public bool AddItem(Item item)
        {
            if (!CanAddItem(item)) return false;

            selectedItems.Add(item);
            UpdateTraitCounters();
            OnItemAdded?.Invoke(item);
            return true;
        }

        public bool RemoveItem(Item item)
        {
            if (item == null || !HasItem(item)) return false;

            selectedItems.Remove(item);
            UpdateTraitCounters();
            OnItemRemoved?.Invoke(item);
            return true;
        }

        public bool HasItem(Item item)
        {
            return selectedItems.Any(i => i.ItemId == item.ItemId);
        }

        public void ClearSet()
        {
            var itemsToRemove = new List<Item>(selectedItems);
            selectedItems.Clear();
            UpdateTraitCounters();

            foreach (var item in itemsToRemove)
            {
                OnItemRemoved?.Invoke(item);
            }
        }

        private void UpdateTraitCounters()
        {
            // 重置所有计数器
            foreach (var counter in traitCounters.Values)
            {
                counter.Reset();
            }

            // 统计当前选中物品的traits
            foreach (var item in selectedItems)
            {
                foreach (var trait in item.Traits)
                {
                    if (traitCounters.ContainsKey(trait))
                    {
                        traitCounters[trait].IncrementCount();
                    }
                }
            }

            OnTraitCountersUpdated?.Invoke();
        }

        public void SetMaxSetSize(int maxSize)
        {
            maxSetSize = maxSize;
        }
        
        /// <summary>
        /// 获取选中的物品列表（用于数量消耗）
        /// </summary>
        public List<Item> GetSelectedItems()
        {
            return new List<Item>(selectedItems); // 返回副本以避免外部修改
        }
    }
} 