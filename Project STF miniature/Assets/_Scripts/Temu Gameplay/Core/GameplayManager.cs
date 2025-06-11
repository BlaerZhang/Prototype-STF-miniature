using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TemuGameplay.Data;
using TemuGameplay.ScriptableObjects;

namespace TemuGameplay.Core
{
    public enum LevelGenerationMode
    {
        ScriptableObject,  // 使用SO配置
        RandomGeneration   // 随机生成
    }

    public class GameplayManager : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private ItemDataSO itemDataSO;
        [SerializeField] private LevelConfigSO currentLevelConfig;
        [SerializeField] private int defaultMaxSetSize = -1; // -1 表示无限制
        
        [Header("Level Generation")]
        [SerializeField] private LevelGenerationMode levelMode = LevelGenerationMode.ScriptableObject;
        [SerializeField] private RandomLevelSettings randomSettings = new RandomLevelSettings();

        private ItemDatabase itemDatabase;
        private PlayerSet currentSet;
        private LevelRequirement currentLevel;

        public ItemDatabase ItemDatabase => itemDatabase;
        public PlayerSet CurrentSet => currentSet;
        public LevelRequirement CurrentLevel => currentLevel;

        public event Action<bool> OnLevelSubmitted; // 提交验证结果事件
        public event Action<LevelRequirement> OnLevelChanged;
        public event Action OnItemQuantitiesChanged; // 物品数量变化事件

        private void Awake()
        {
            Initialize();
        }

        private void Initialize()
        {
            // 初始化数据库和玩家集合
            itemDatabase = new ItemDatabase();
            currentSet = new PlayerSet(defaultMaxSetSize);

            // 从ScriptableObject加载数据，如果没有则使用示例数据
            if (itemDataSO != null)
            {
                LoadItemsFromScriptableObject();
            }
            else
            {
                Debug.LogWarning("No ItemDataSO assigned, loading sample data...");
                itemDatabase.LoadSampleData();
            }

            // 设置trait总数
            currentSet.SetTraitTotalCounts(itemDatabase.GetTraitTotalCounts());

            // 监听集合变化
            currentSet.OnItemAdded += OnItemAddedToSet;
            currentSet.OnItemRemoved += OnItemRemovedFromSet;
            currentSet.OnTraitCountersUpdated += OnTraitCountersUpdated;

                    // 设置关卡（根据模式）
        SetupLevel();
    }

    private void SetupLevel()
    {
        switch (levelMode)
        {
            case LevelGenerationMode.ScriptableObject:
                if (currentLevelConfig != null)
                {
                    LoadLevelFromScriptableObject();
                }
                else
                {
                    Debug.LogWarning("No LevelConfigSO assigned, falling back to default level...");
                    SetupDefaultLevel();
                }
                break;
                
            case LevelGenerationMode.RandomGeneration:
                GenerateRandomLevel();
                break;
                
            default:
                SetupDefaultLevel();
                break;
        }
    }

    private void GenerateRandomLevel()
    {
        randomSettings.ValidateSettings();
        
        // 获取所有可用的trait类型和它们的物品数量
        var traitCounts = itemDatabase.GetTraitTotalCounts();
        var availableTraits = traitCounts.Where(kvp => kvp.Value > 0).ToList();
        
        if (availableTraits.Count == 0)
        {
            Debug.LogWarning("No traits available for random generation, using default level");
            SetupDefaultLevel();
            return;
        }
        
        // 决定要使用多少种trait
        int traitTypeCount = UnityEngine.Random.Range(randomSettings.minTraitTypes, randomSettings.maxTraitTypes + 1);
        traitTypeCount = Mathf.Min(traitTypeCount, availableTraits.Count);
        
        // 选择traits
        var selectedTraits = SelectRandomTraits(availableTraits, traitTypeCount);
        
        // 为每个trait分配要求数量
        var requirements = new Dictionary<TraitType, int>();
        foreach (var trait in selectedTraits)
        {
            int maxAvailable = traitCounts[trait];
            int baseRequirement = UnityEngine.Random.Range(randomSettings.minTraitCount, randomSettings.maxTraitCount + 1);
            
            // 应用难度修正器
            float difficultyFactor = 1f + (randomSettings.difficultyModifier * 0.5f);
            int adjustedRequirement = Mathf.RoundToInt(baseRequirement * difficultyFactor);
            
            // 确保不超过可用数量
            if (randomSettings.ensureCompletable)
            {
                adjustedRequirement = Mathf.Min(adjustedRequirement, maxAvailable);
            }
            
            requirements[trait] = Mathf.Max(1, adjustedRequirement);
        }
        
        // 生成关卡名称
        string levelName = GenerateRandomLevelName();
        
        // 创建关卡
        currentLevel = new LevelRequirement(levelName);
        foreach (var req in requirements)
        {
            currentLevel.AddRequirement(req.Key, req.Value);
        }
        
        OnLevelChanged?.Invoke(currentLevel);
        // 移除自动验证，等待手动提交
        
        Debug.Log($"Generated random level: {levelName} with {requirements.Count} trait requirements");
    }

    private List<TraitType> SelectRandomTraits(List<KeyValuePair<TraitType, int>> availableTraits, int count)
    {
        var result = new List<TraitType>();
        var candidates = new List<KeyValuePair<TraitType, int>>(availableTraits);
        
        // 如果启用favorActiveTraits，对有更多物品的trait给予更高权重
        if (randomSettings.favorActiveTraits)
        {
            candidates = candidates.OrderByDescending(kvp => kvp.Value).ToList();
        }
        
        for (int i = 0; i < count && candidates.Count > 0; i++)
        {
            TraitType selectedTrait;
            
            if (randomSettings.favorActiveTraits && i < count / 2)
            {
                // 前半部分从高权重中选择
                int highPriorityRange = Mathf.Max(1, candidates.Count / 3);
                var randomIndex = UnityEngine.Random.Range(0, highPriorityRange);
                selectedTrait = candidates[randomIndex].Key;
            }
            else
            {
                // 随机选择
                var randomIndex = UnityEngine.Random.Range(0, candidates.Count);
                selectedTrait = candidates[randomIndex].Key;
            }
            
            result.Add(selectedTrait);
            candidates.RemoveAll(kvp => kvp.Key == selectedTrait);
        }
        
        return result;
    }

    private string GenerateRandomLevelName()
    {
        var prefix = randomSettings.levelNamePrefixes[UnityEngine.Random.Range(0, randomSettings.levelNamePrefixes.Length)];
        var suffix = randomSettings.levelNameSuffixes[UnityEngine.Random.Range(0, randomSettings.levelNameSuffixes.Length)];
        return $"{prefix} {suffix}";
    }

        private void LoadItemsFromScriptableObject()
        {
            itemDatabase.Clear();
            var items = itemDataSO.GetAllItems();
            
            foreach (var item in items)
            {
                itemDatabase.AddItem(item);
            }
            
            Debug.Log($"Loaded {items.Count} items from ScriptableObject: {itemDataSO.name}");
        }

        private void LoadLevelFromScriptableObject()
        {
            currentLevel = currentLevelConfig.ToLevelRequirement();
            
            // 设置最大集合大小
            if (currentLevelConfig.MaxSetSize > 0)
            {
                currentSet.SetMaxSetSize(currentLevelConfig.MaxSetSize);
            }
            
            OnLevelChanged?.Invoke(currentLevel);
            // 移除自动验证，等待手动提交
            
            Debug.Log($"Loaded level: {currentLevel.LevelName} from ScriptableObject: {currentLevelConfig.name}");
        }

        // 运行时切换物品数据
        public void LoadItemData(ItemDataSO newItemData)
        {
            if (newItemData == null)
            {
                Debug.LogError("Cannot load null ItemDataSO");
                return;
            }

            itemDataSO = newItemData;
            
            // 清空当前选择
            currentSet.ClearSet();
            
            // 重新加载物品
            LoadItemsFromScriptableObject();
            currentSet.SetTraitTotalCounts(itemDatabase.GetTraitTotalCounts());
            
            Debug.Log($"Runtime loaded items from: {itemDataSO.name}");
        }

        // 运行时切换关卡
        public void LoadLevel(LevelConfigSO newLevelConfig)
        {
            if (newLevelConfig == null)
            {
                Debug.LogError("Cannot load null LevelConfigSO");
                return;
            }

            currentLevelConfig = newLevelConfig;
            
            // 清空当前选择
            currentSet.ClearSet();
            
            // 加载新关卡
            LoadLevelFromScriptableObject();
            
            Debug.Log($"Runtime loaded level: {currentLevel.LevelName}");
        }

        private void SetupDefaultLevel()
        {
            currentLevel = new LevelRequirement("Tutorial Level");
            currentLevel.AddRequirement(TraitType.Hiking, 2);
            currentLevel.AddRequirement(TraitType.Running, 1);

            OnLevelChanged?.Invoke(currentLevel);
            // 移除自动验证，等待手动提交
        }

        public void SetLevel(LevelRequirement newLevel)
        {
            currentLevel = newLevel;
            OnLevelChanged?.Invoke(currentLevel);
            // 移除自动验证，等待手动提交
        }

        public bool AddItemToSet(string itemId)
        {
            Item item = itemDatabase.GetItem(itemId);
            if (item == null)
            {
                Debug.LogWarning($"Item with ID {itemId} not found");
                return false;
            }

            return currentSet.AddItem(item);
        }

        public bool RemoveItemFromSet(string itemId)
        {
            Item item = itemDatabase.GetItem(itemId);
            if (item == null)
            {
                Debug.LogWarning($"Item with ID {itemId} not found");
                return false;
            }

            return currentSet.RemoveItem(item);
        }

        public bool ValidateCurrentSet()
        {
            if (currentLevel == null) return false;

            bool isValid = currentLevel.CheckRequirement(currentSet.TraitCounters);
            return isValid;
        }

        public List<string> GetUnmetRequirements()
        {
            if (currentLevel == null) return new List<string>();

            return currentLevel.GetUnmetRequirements(currentSet.TraitCounters);
        }

        public void ResetCurrentSet()
        {
            currentSet.ClearSet();
        }

        public void SetMaxSetSize(int maxSize)
        {
            currentSet.SetMaxSetSize(maxSize);
        }

        private void OnItemAddedToSet(Item item)
        {
            Debug.Log($"Added item: {item.ItemName}");
            // 移除自动验证，仅记录操作
        }

        private void OnItemRemovedFromSet(Item item)
        {
            Debug.Log($"Removed item: {item.ItemName}");
            // 移除自动验证，仅记录操作
        }

        private void OnTraitCountersUpdated()
        {
            // 移除自动验证，trait更新时不触发验证
        }

        // 手动提交验证方法
        public bool SubmitAnswer()
        {
            if (currentLevel == null)
            {
                Debug.LogWarning("No current level to validate");
                return false;
            }

            bool isValid = ValidateCurrentSet();
            OnLevelSubmitted?.Invoke(isValid);

            if (isValid)
            {
                Debug.Log("🎉 Level completed successfully!");
                
                // 成功时消耗选中物品的数量
                ConsumeSelectedItems();
                
                OnLevelCompleted();
                return true;
            }
            else
            {
                var unmet = GetUnmetRequirements();
                if (unmet.Count > 0)
                {
                    Debug.Log($"❌ Requirements not met: {string.Join(", ", unmet)}");
                }
                return false;
            }
        }
        
        /// <summary>
        /// 消耗选中物品的数量
        /// </summary>
        private void ConsumeSelectedItems()
        {
            var selectedItems = currentSet.GetSelectedItems();
            int consumedCount = 0;
            
            foreach (var item in selectedItems)
            {
                if (item.ConsumeOne())
                {
                    consumedCount++;
                    Debug.Log($"Consumed 1x {item.ItemName}, remaining: {item.CurrentQuantity}");
                }
            }
            
            Debug.Log($"Total items consumed: {consumedCount}");
            
            // 触发事件通知UI更新
            OnItemQuantitiesChanged?.Invoke();
        }

        // 关卡完成后的处理
        private void OnLevelCompleted()
        {
            // 清空当前选择
            currentSet.ClearSet();
            
            // 根据当前模式刷新关卡
            RefreshLevel();
        }

        // 刷新关卡（完成后自动加载下一关）
        private void RefreshLevel()
        {
            switch (levelMode)
            {
                case LevelGenerationMode.RandomGeneration:
                    // 随机模式：生成新的随机关卡
                    GenerateRandomLevel();
                    Debug.Log("Generated new random level");
                    break;
                    
                case LevelGenerationMode.ScriptableObject:
                    // SO模式：重新加载当前关卡或加载下一关
                    if (currentLevelConfig != null)
                    {
                        LoadLevelFromScriptableObject();
                        Debug.Log("Reloaded current level from ScriptableObject");
                    }
                    else
                    {
                        SetupDefaultLevel();
                        Debug.Log("Reloaded default level");
                    }
                    break;
                    
                default:
                    SetupDefaultLevel();
                    break;
            }
        }

        private void OnDestroy()
        {
            if (currentSet != null)
            {
                currentSet.OnItemAdded -= OnItemAddedToSet;
                currentSet.OnItemRemoved -= OnItemRemovedFromSet;
                currentSet.OnTraitCountersUpdated -= OnTraitCountersUpdated;
            }
        }

        // 公共方法用于创建自定义关卡
        public LevelRequirement CreateLevel(string levelName, Dictionary<TraitType, int> requirements)
        {
            LevelRequirement level = new LevelRequirement(levelName);
            foreach (var req in requirements)
            {
                level.AddRequirement(req.Key, req.Value);
            }
            return level;
        }

        // 切换关卡生成模式
        public void SetLevelGenerationMode(LevelGenerationMode mode)
        {
            levelMode = mode;
            Debug.Log($"Level generation mode changed to: {mode}");
            
            // 立即应用新模式
            SetupLevel();
        }

        // 重新生成随机关卡（仅在随机模式下有效）
        public void RegenerateRandomLevel()
        {
            if (levelMode == LevelGenerationMode.RandomGeneration)
            {
                // 清空当前选择
                currentSet.ClearSet();
                
                // 生成新关卡
                GenerateRandomLevel();
                
                Debug.Log("Random level regenerated");
            }
            else
            {
                Debug.LogWarning("Can only regenerate in Random Generation mode");
            }
        }

        // 获取当前生成模式
        public LevelGenerationMode GetCurrentMode()
        {
            return levelMode;
        }

        // 获取随机生成设置的引用（用于运行时调整）
        public RandomLevelSettings GetRandomSettings()
        {
            return randomSettings;
        }
        
        /// <summary>
        /// 重置所有物品数量到最大值
        /// </summary>
        public void ResetAllItemQuantities()
        {
            itemDatabase.ResetAllQuantities();
            OnItemQuantitiesChanged?.Invoke();
            Debug.Log("All item quantities reset to maximum");
        }
        
        /// <summary>
        /// 触发物品数量变化事件（用于外部组件）
        /// </summary>
        public void TriggerItemQuantitiesChangedEvent()
        {
            OnItemQuantitiesChanged?.Invoke();
        }
        
        #region Context Menu Methods (Inspector调试用)
        [ContextMenu("Reset All Item Quantities")]
        public void ResetAllItemQuantitiesFromInspector()
        {
            if (Application.isPlaying)
            {
                ResetAllItemQuantities();
            }
            else
            {
                Debug.Log("只能在运行时重置物品数量");
            }
        }
        
        [ContextMenu("Show Item Quantities")]
        public void ShowItemQuantities()
        {
            if (!Application.isPlaying)
            {
                Debug.Log("只能在运行时查看物品数量");
                return;
            }
            
            var availableItems = itemDatabase.GetAvailableItems();
            var outOfStockItems = itemDatabase.GetOutOfStockItems();
            
            Debug.Log($"=== Item Quantities ===");
            Debug.Log($"Available items: {availableItems.Count}");
            foreach (var item in availableItems)
            {
                Debug.Log($"  {item.ItemName}: {item.CurrentQuantity}/{item.MaxQuantity}");
            }
            
            if (outOfStockItems.Count > 0)
            {
                Debug.Log($"Out of stock items: {outOfStockItems.Count}");
                foreach (var item in outOfStockItems)
                {
                    Debug.Log($"  {item.ItemName}: {item.CurrentQuantity}/{item.MaxQuantity}");
                }
            }
        }
        
        [ContextMenu("Set All Items Max Quantity to 5")]
        public void SetAllItemsMaxQuantityTo5()
        {
            if (Application.isPlaying)
            {
                itemDatabase.SetAllMaxQuantities(5);
                OnItemQuantitiesChanged?.Invoke();
            }
            else
            {
                Debug.Log("只能在运行时设置物品数量");
            }
        }
        #endregion
    }
} 