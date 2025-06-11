using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TemuGameplay.Data;

namespace TemuGameplay.Core
{
    public class ItemDatabase
    {
        private List<Item> allItems;
        private Dictionary<string, Item> itemLookup;
        private Dictionary<TraitType, List<Item>> itemsByTrait;

        public List<Item> AllItems => allItems;

        public ItemDatabase()
        {
            allItems = new List<Item>();
            itemLookup = new Dictionary<string, Item>();
            itemsByTrait = new Dictionary<TraitType, List<Item>>();
            InitializeTraitLists();
        }

        private void InitializeTraitLists()
        {
            foreach (TraitType trait in Enum.GetValues(typeof(TraitType)))
            {
                itemsByTrait[trait] = new List<Item>();
            }
        }

        public void AddItem(Item item)
        {
            if (item == null || itemLookup.ContainsKey(item.ItemId))
            {
                Debug.LogWarning($"Item with ID {item?.ItemId} already exists or is null");
                return;
            }

            allItems.Add(item);
            itemLookup[item.ItemId] = item;

            // 添加到trait索引
            foreach (var trait in item.Traits)
            {
                if (itemsByTrait.ContainsKey(trait))
                {
                    itemsByTrait[trait].Add(item);
                }
            }
        }

        public void RemoveItem(string itemId)
        {
            if (!itemLookup.ContainsKey(itemId)) return;

            Item item = itemLookup[itemId];
            allItems.Remove(item);
            itemLookup.Remove(itemId);

            // 从trait索引中移除
            foreach (var trait in item.Traits)
            {
                if (itemsByTrait.ContainsKey(trait))
                {
                    itemsByTrait[trait].Remove(item);
                }
            }
        }

        public Item GetItem(string itemId)
        {
            return itemLookup.ContainsKey(itemId) ? itemLookup[itemId] : null;
        }

        public List<Item> GetItemsWithTrait(TraitType trait)
        {
            return itemsByTrait.ContainsKey(trait) ? new List<Item>(itemsByTrait[trait]) : new List<Item>();
        }

        public List<Item> GetItemsWithTraits(List<TraitType> traits)
        {
            if (traits == null || traits.Count == 0) return new List<Item>();

            return allItems.Where(item => traits.Any(trait => item.HasTrait(trait))).ToList();
        }

        public Dictionary<TraitType, int> GetTraitTotalCounts()
        {
            Dictionary<TraitType, int> counts = new Dictionary<TraitType, int>();

            foreach (TraitType trait in Enum.GetValues(typeof(TraitType)))
            {
                counts[trait] = itemsByTrait[trait].Count;
            }

            return counts;
        }

        public void Clear()
        {
            allItems.Clear();
            itemLookup.Clear();
            foreach (var list in itemsByTrait.Values)
            {
                list.Clear();
            }
        }

        // 用于测试的示例数据
        public void LoadSampleData()
        {
            Clear();

            // Create sample items (sports theme) with quantities
            var sampleItems = new List<Item>
            {
                new Item("hk_001", "Hiking Boots", new List<TraitType> { TraitType.Hiking }, null, "Professional outdoor hiking boots", 5),
                new Item("hk_002", "Hiking Backpack", new List<TraitType> { TraitType.Hiking }, null, "Large capacity hiking backpack", 3),
                new Item("hk_003", "Hiking Poles", new List<TraitType> { TraitType.Hiking, TraitType.Tool }, null, "Carbon fiber hiking poles", 4),
                new Item("run_001", "Running Shoes", new List<TraitType> { TraitType.Running }, null, "Professional marathon running shoes", 6),
                new Item("run_002", "Sports Drink", new List<TraitType> { TraitType.Running, TraitType.Nutrition }, null, "Electrolyte sports drink", 8),
                new Item("nut_001", "Protein Powder", new List<TraitType> { TraitType.Nutrition, TraitType.Food }, null, "High quality whey protein powder", 2),
                new Item("nut_002", "Energy Bar", new List<TraitType> { TraitType.Nutrition, TraitType.Food }, null, "Oat and nut energy bar", 10),
                new Item("surv_001", "Emergency Blanket", new List<TraitType> { TraitType.Survival }, null, "Emergency thermal blanket", 3),
                new Item("water_001", "Water Filter", new List<TraitType> { TraitType.Water, TraitType.Tool }, null, "Portable water filter", 2)
            };

            foreach (var item in sampleItems)
            {
                AddItem(item);
            }
        }
        
        /// <summary>
        /// 重置所有物品的数量到最大值
        /// </summary>
        public void ResetAllQuantities()
        {
            foreach (var item in allItems)
            {
                item.ResetQuantity();
            }
            Debug.Log($"Reset quantities for {allItems.Count} items");
        }
        
        /// <summary>
        /// 设置所有物品的最大数量
        /// </summary>
        public void SetAllMaxQuantities(int maxQuantity)
        {
            foreach (var item in allItems)
            {
                item.SetMaxQuantity(maxQuantity);
            }
            Debug.Log($"Set max quantity {maxQuantity} for {allItems.Count} items");
        }
        
        /// <summary>
        /// 获取库存不足的物品列表（数量<=0）
        /// </summary>
        public List<Item> GetOutOfStockItems()
        {
            return allItems.Where(item => !item.IsAvailable).ToList();
        }
        
        /// <summary>
        /// 获取库存充足的物品列表（数量>0）
        /// </summary>
        public List<Item> GetAvailableItems()
        {
            return allItems.Where(item => item.IsAvailable).ToList();
        }
    }
} 