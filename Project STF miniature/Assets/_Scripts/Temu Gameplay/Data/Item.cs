using System;
using System.Collections.Generic;
using UnityEngine;

namespace TemuGameplay.Data
{
    [Serializable]
    public class Item
    {
        [SerializeField] private string itemId;
        [SerializeField] private string itemName;
        [SerializeField] private List<TraitType> traits;
        [SerializeField] private Sprite icon;
        [SerializeField] private string description;
        
        [Header("数量系统")]
        [SerializeField] private int maxQuantity = 3;  // 最大数量
        [SerializeField] private int currentQuantity;  // 当前数量

        public string ItemId => itemId;
        public string ItemName => itemName;
        public List<TraitType> Traits => traits;
        public Sprite Icon => icon;
        public string Description => description;
        public int MaxQuantity => maxQuantity;
        public int CurrentQuantity => currentQuantity;
        
        // 是否还有可用数量
        public bool IsAvailable => currentQuantity > 0;
        
        // 数量显示文本
        public string QuantityText => $"{currentQuantity}";

        public Item(string id, string name, List<TraitType> itemTraits, Sprite itemIcon = null, string desc = "", int quantity = 3)
        {
            itemId = id;
            itemName = name;
            traits = new List<TraitType>(itemTraits);
            icon = itemIcon;
            description = desc;
            maxQuantity = quantity;
            currentQuantity = quantity;  // 初始化时数量等于最大值
        }

        public bool HasTrait(TraitType trait)
        {
            return traits.Contains(trait);
        }
        
        /// <summary>
        /// 消耗一个物品（成功提交时调用）
        /// </summary>
        /// <returns>是否成功消耗</returns>
        public bool ConsumeOne()
        {
            if (currentQuantity > 0)
            {
                currentQuantity--;
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// 重置数量到最大值
        /// </summary>
        public void ResetQuantity()
        {
            currentQuantity = maxQuantity;
        }
        
        /// <summary>
        /// 设置当前数量
        /// </summary>
        public void SetCurrentQuantity(int quantity)
        {
            currentQuantity = Mathf.Clamp(quantity, 0, maxQuantity);
        }
        
        /// <summary>
        /// 设置最大数量（会重置当前数量）
        /// </summary>
        public void SetMaxQuantity(int quantity)
        {
            maxQuantity = Mathf.Max(0, quantity);
            currentQuantity = maxQuantity;
        }
    }
} 