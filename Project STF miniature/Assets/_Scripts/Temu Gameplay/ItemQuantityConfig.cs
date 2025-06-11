using UnityEngine;
using System.Collections.Generic;
using TemuGameplay.Data;
using TemuGameplay.Core;

namespace TemuGameplay
{
    /// <summary>
    /// 物品数量配置管理器
    /// 提供简单的方式来设置和管理物品的初始数量
    /// </summary>
    public class ItemQuantityConfig : MonoBehaviour
    {
        [Header("全局数量设置")]
        [SerializeField] private int defaultMaxQuantity = 3;
        [SerializeField] private bool useGlobalSetting = false;
        
        [Header("单个物品数量配置")]
        [SerializeField] private List<ItemQuantitySetting> itemQuantities = new List<ItemQuantitySetting>();
        
        [System.Serializable]
        public class ItemQuantitySetting
        {
            [Header("物品信息")]
            public string itemId;
            public string itemName; // 仅用于Inspector显示
            
            [Header("数量设置")]
            [Range(0, 50)]
            public int maxQuantity = 3;
            [Range(0, 50)]
            public int currentQuantity = 3;
            
            [Header("备注")]
            public string notes; // 备注信息
        }

        private GameplayManager gameplayManager;

        private void Start()
        {
            gameplayManager = FindObjectOfType<GameplayManager>();
            if (gameplayManager == null)
            {
                Debug.LogError("GameplayManager not found!");
                return;
            }

            // 等待更长时间，确保ItemDatabase和UI都已经初始化
            Invoke(nameof(ApplyQuantitySettings), 0.5f);
        }

        private void ApplyQuantitySettings()
        {
            if (gameplayManager?.ItemDatabase == null)
            {
                Debug.LogError("ItemDatabase not initialized!");
                return;
            }

            if (useGlobalSetting)
            {
                // 使用全局设置
                ApplyGlobalQuantitySettings();
            }
            else
            {
                // 使用单个物品设置
                ApplyIndividualQuantitySettings();
            }

            Debug.Log($"Applied quantity settings to {gameplayManager.ItemDatabase.AllItems.Count} items");
            
            // 触发UI更新
            if (gameplayManager != null)
            {
                gameplayManager.TriggerItemQuantitiesChangedEvent();
            }
        }

        private void ApplyGlobalQuantitySettings()
        {
            var allItems = gameplayManager.ItemDatabase.AllItems;
            foreach (var item in allItems)
            {
                item.SetMaxQuantity(defaultMaxQuantity);
            }
            
            Debug.Log($"Set all items max quantity to: {defaultMaxQuantity}");
        }

        private void ApplyIndividualQuantitySettings()
        {
            var database = gameplayManager.ItemDatabase;
            
            foreach (var setting in itemQuantities)
            {
                var item = database.GetItem(setting.itemId);
                if (item != null)
                {
                    item.SetMaxQuantity(setting.maxQuantity);
                    item.SetCurrentQuantity(setting.currentQuantity);
                    
                    Debug.Log($"Set {item.ItemName}: {setting.currentQuantity}/{setting.maxQuantity}");
                }
                else
                {
                    Debug.LogWarning($"Item not found: {setting.itemId} ({setting.itemName})");
                }
            }
        }

        #region Context Menu Methods
        [ContextMenu("Apply Quantity Settings")]
        public void ApplyQuantitySettingsFromInspector()
        {
            if (Application.isPlaying)
            {
                ApplyQuantitySettings();
            }
            else
            {
                Debug.Log("只能在运行时应用数量设置");
            }
        }

        [ContextMenu("Auto Fill Item List")]
        public void AutoFillItemList()
        {
            if (!Application.isPlaying)
            {
                Debug.Log("请在运行时使用此功能");
                return;
            }

            if (gameplayManager?.ItemDatabase == null)
            {
                Debug.LogError("ItemDatabase not found!");
                return;
            }

            itemQuantities.Clear();
            var allItems = gameplayManager.ItemDatabase.AllItems;
            
            foreach (var item in allItems)
            {
                var setting = new ItemQuantitySetting
                {
                    itemId = item.ItemId,
                    itemName = item.ItemName,
                    maxQuantity = item.MaxQuantity,
                    currentQuantity = item.CurrentQuantity,
                    notes = $"Traits: {string.Join(", ", item.Traits)}"
                };
                itemQuantities.Add(setting);
            }

            Debug.Log($"Auto filled {itemQuantities.Count} items");
        }

        [ContextMenu("Reset All to Default")]
        public void ResetAllToDefault()
        {
            if (Application.isPlaying && gameplayManager?.ItemDatabase != null)
            {
                gameplayManager.ItemDatabase.SetAllMaxQuantities(defaultMaxQuantity);
                Debug.Log($"Reset all items to default quantity: {defaultMaxQuantity}");
            }
        }

        [ContextMenu("Show Current Quantities")]
        public void ShowCurrentQuantities()
        {
            if (!Application.isPlaying || gameplayManager?.ItemDatabase == null)
            {
                Debug.Log("请在运行时查看数量");
                return;
            }

            Debug.Log("=== 当前物品数量 ===");
            var allItems = gameplayManager.ItemDatabase.AllItems;
            foreach (var item in allItems)
            {
                string status = item.IsAvailable ? "✅" : "❌";
                Debug.Log($"{status} {item.ItemName}: {item.CurrentQuantity}/{item.MaxQuantity}");
            }
        }
        #endregion

        #region Preset Configurations
        [ContextMenu("Apply Preset: Low Stock")]
        public void ApplyPresetLowStock()
        {
            defaultMaxQuantity = 2;
            useGlobalSetting = true;
            if (Application.isPlaying) ApplyQuantitySettings();
        }

        [ContextMenu("Apply Preset: Medium Stock")]
        public void ApplyPresetMediumStock()
        {
            defaultMaxQuantity = 5;
            useGlobalSetting = true;
            if (Application.isPlaying) ApplyQuantitySettings();
        }

        [ContextMenu("Apply Preset: High Stock")]
        public void ApplyPresetHighStock()
        {
            defaultMaxQuantity = 10;
            useGlobalSetting = true;
            if (Application.isPlaying) ApplyQuantitySettings();
        }
        #endregion

        // Inspector显示帮助信息
        [ContextMenu("Show Help")]
        public void ShowHelp()
        {
            Debug.Log(@"
=== 物品数量配置帮助 ===

设置方法:
1. 全局设置: 勾选 'Use Global Setting'，所有物品使用相同数量
2. 单独设置: 取消勾选，在 'Item Quantities' 列表中单独配置每个物品

快捷操作:
- 右键 → 'Auto Fill Item List': 自动填充当前所有物品
- 右键 → 'Apply Preset': 应用预设配置 (低/中/高库存)
- 右键 → 'Show Current Quantities': 显示当前数量状态

注意事项:
- Item ID 必须与数据库中的物品ID完全匹配
- 修改后需要重新运行或点击 'Apply Quantity Settings'
- 运行时的修改会立即生效
");
        }
    }
} 