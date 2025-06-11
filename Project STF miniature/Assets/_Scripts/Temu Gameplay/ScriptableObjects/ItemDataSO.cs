using System.Collections.Generic;
using UnityEngine;
using TemuGameplay.Data;

namespace TemuGameplay.ScriptableObjects
{
    [CreateAssetMenu(fileName = "ItemData", menuName = "Temu Gameplay/Item Data")]
    public class ItemDataSO : ScriptableObject
    {
        [System.Serializable]
        public class ItemConfig
        {
            public string itemId;
            public string itemName;
            public List<TraitType> traits;
            public Sprite icon;
            [TextArea(2, 4)]
            public string description;

            public Item ToItem()
            {
                return new Item(itemId, itemName, traits, icon, description);
            }
        }

        [SerializeField] private List<ItemConfig> items = new List<ItemConfig>();

        public List<ItemConfig> Items => items;

        public List<Item> GetAllItems()
        {
            List<Item> result = new List<Item>();
            foreach (var config in items)
            {
                result.Add(config.ToItem());
            }
            return result;
        }

        public Item GetItem(string itemId)
        {
            var config = items.Find(item => item.itemId == itemId);
            return config?.ToItem();
        }

        // 编辑器工具方法
        [ContextMenu("Validate Item IDs")]
        private void ValidateItemIDs()
        {
            HashSet<string> seenIds = new HashSet<string>();
            bool hasDuplicates = false;

            foreach (var item in items)
            {
                if (string.IsNullOrEmpty(item.itemId))
                {
                    Debug.LogError($"Item '{item.itemName}' has empty ID!");
                    continue;
                }

                if (seenIds.Contains(item.itemId))
                {
                    Debug.LogError($"Duplicate item ID found: {item.itemId}");
                    hasDuplicates = true;
                }
                else
                {
                    seenIds.Add(item.itemId);
                }
            }

            if (!hasDuplicates)
            {
                Debug.Log("All item IDs are unique!");
            }
        }
    }
} 