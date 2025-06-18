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
        [System.Serializable]
        public enum QuantityMode
        {
            Global,     // 全局设置：所有物品使用相同数量
            Individual, // 单独设置：只使用列表中配置的物品
            Mixed       // 混合设置：有配置的物品使用单独设置，其余使用全局设置
        }

        [Header("数量配置模式")]
        [SerializeField] private QuantityMode quantityMode = QuantityMode.Mixed;
        
        [Header("全局数量设置")]
        [SerializeField] private int defaultMaxQuantity = 3;
        
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

            switch (quantityMode)
            {
                case QuantityMode.Global:
                    ApplyGlobalQuantitySettings();
                    break;
                case QuantityMode.Individual:
                    ApplyIndividualQuantitySettings();
                    break;
                case QuantityMode.Mixed:
                    ApplyMixedQuantitySettings();
                    break;
            }

            Debug.Log($"Applied quantity settings to {gameplayManager.ItemDatabase.AllItems.Count} items using {quantityMode} mode");
            
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

        private void ApplyMixedQuantitySettings()
        {
            var database = gameplayManager.ItemDatabase;
            var allItems = database.AllItems;
            var configuredItemIds = new HashSet<string>();
            
            // 首先应用单个物品配置
            foreach (var setting in itemQuantities)
            {
                var item = database.GetItem(setting.itemId);
                if (item != null)
                {
                    item.SetMaxQuantity(setting.maxQuantity);
                    item.SetCurrentQuantity(setting.currentQuantity);
                    configuredItemIds.Add(setting.itemId);
                    
                    Debug.Log($"[个性化] Set {item.ItemName}: {setting.currentQuantity}/{setting.maxQuantity}");
                }
                else
                {
                    Debug.LogWarning($"Item not found: {setting.itemId} ({setting.itemName})");
                }
            }
            
            // 然后对未配置的物品应用全局设置
            int globallySetCount = 0;
            foreach (var item in allItems)
            {
                if (!configuredItemIds.Contains(item.ItemId))
                {
                    item.SetMaxQuantity(defaultMaxQuantity);
                    globallySetCount++;
                }
            }
            
            Debug.Log($"[混合设置] 个性化配置: {configuredItemIds.Count} 个物品, 全局配置: {globallySetCount} 个物品 (默认数量: {defaultMaxQuantity})");
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
            var configuredItems = new HashSet<string>();
            
            // 收集已配置的物品ID
            foreach (var setting in itemQuantities)
            {
                configuredItems.Add(setting.itemId);
            }
            
            foreach (var item in allItems)
            {
                string status = item.IsAvailable ? "✅" : "❌";
                string configType = "";
                
                if (quantityMode == QuantityMode.Mixed)
                {
                    configType = configuredItems.Contains(item.ItemId) ? "[个性化]" : "[全局]";
                }
                
                Debug.Log($"{status} {configType} {item.ItemName}: {item.CurrentQuantity}/{item.MaxQuantity}");
            }
        }
        #endregion

        #region Preset Configurations
        [ContextMenu("Apply Preset: Low Stock (Global)")]
        public void ApplyPresetLowStock()
        {
            defaultMaxQuantity = 2;
            quantityMode = QuantityMode.Global;
            if (Application.isPlaying) ApplyQuantitySettings();
        }

        [ContextMenu("Apply Preset: Medium Stock (Global)")]
        public void ApplyPresetMediumStock()
        {
            defaultMaxQuantity = 5;
            quantityMode = QuantityMode.Global;
            if (Application.isPlaying) ApplyQuantitySettings();
        }

        [ContextMenu("Apply Preset: High Stock (Global)")]
        public void ApplyPresetHighStock()
        {
            defaultMaxQuantity = 10;
            quantityMode = QuantityMode.Global;
            if (Application.isPlaying) ApplyQuantitySettings();
        }

        [ContextMenu("Apply Preset: Mixed Default")]
        public void ApplyPresetMixedDefault()
        {
            defaultMaxQuantity = 3;
            quantityMode = QuantityMode.Mixed;
            if (Application.isPlaying) ApplyQuantitySettings();
        }
        #endregion

        // Inspector显示帮助信息
        [ContextMenu("Show Help")]
        public void ShowHelp()
        {
            Debug.Log(@"
=== 物品数量配置帮助 ===

配置模式:
1. Global (全局): 所有物品使用相同的默认数量
2. Individual (单独): 只有列表中配置的物品会被设置，其他物品保持原样
3. Mixed (混合): 列表中的物品使用单独配置，其余物品使用全局设置 ⭐推荐

设置方法:
- 选择配置模式
- 设置默认最大数量 (用于全局或混合模式)
- 在列表中添加需要特殊配置的物品 (用于单独或混合模式)

快捷操作:
- 右键 → 'Auto Fill Item List': 自动填充当前所有物品
- 右键 → 'Apply Preset': 应用预设配置
- 右键 → 'Show Current Quantities': 显示当前数量状态 (混合模式会标注配置类型)

注意事项:
- Item ID 必须与数据库中的物品ID完全匹配
- 混合模式最灵活，可以对重要物品单独配置，其他使用默认值
- 修改后需要重新运行或点击 'Apply Quantity Settings'
");
        }
    }
} 