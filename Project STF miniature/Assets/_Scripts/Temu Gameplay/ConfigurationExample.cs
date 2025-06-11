using UnityEngine;
using TemuGameplay.Core;
using TemuGameplay.Data;
using TemuGameplay.ScriptableObjects;

namespace TemuGameplay
{
    /// <summary>
    /// 示例脚本：展示如何配置和使用ScriptableObject
    /// </summary>
    public class ConfigurationExample : MonoBehaviour
    {
        [Header("物品配置示例")]
        [SerializeField] private ItemDataSO basicItemSet;
        [SerializeField] private ItemDataSO advancedItemSet;
        
        [Header("关卡配置示例")]
        [SerializeField] private LevelConfigSO easyLevel;
        [SerializeField] private LevelConfigSO hardLevel;
        
        private GameplayManager gameplayManager;

        private void Start()
        {
            gameplayManager = FindObjectOfType<GameplayManager>();
            if (gameplayManager == null)
            {
                Debug.LogError("GameplayManager not found!");
                return;
            }
        }

        // 在Inspector中可以调用的方法
        [ContextMenu("Load Basic Item Set")]
        public void LoadBasicItemSet()
        {
            if (basicItemSet != null)
            {
                gameplayManager.LoadItemData(basicItemSet);
                Debug.Log("加载了基础物品集");
            }
        }

        [ContextMenu("Load Advanced Item Set")]
        public void LoadAdvancedItemSet()
        {
            if (advancedItemSet != null)
            {
                gameplayManager.LoadItemData(advancedItemSet);
                Debug.Log("加载了高级物品集");
            }
        }

        [ContextMenu("Load Easy Level")]
        public void LoadEasyLevel()
        {
            if (easyLevel != null)
            {
                gameplayManager.LoadLevel(easyLevel);
                Debug.Log("加载了简单关卡");
            }
        }

        [ContextMenu("Load Hard Level")]
        public void LoadHardLevel()
        {
            if (hardLevel != null)
            {
                gameplayManager.LoadLevel(hardLevel);
                Debug.Log("加载了困难关卡");
            }
        }

        // 演示如何在代码中创建配置
        [ContextMenu("Create Custom Level")]
        public void CreateCustomLevel()
        {
            // 创建自定义关卡
            var customRequirements = new System.Collections.Generic.Dictionary<TraitType, int>
            {
                { TraitType.Hiking, 3 },
                { TraitType.Running, 2 },
                { TraitType.Nutrition, 1 }
            };

            var customLevel = gameplayManager.CreateLevel("自定义关卡", customRequirements);
            gameplayManager.SetLevel(customLevel);
            
            Debug.Log("创建并加载了自定义关卡");
        }
    }
} 