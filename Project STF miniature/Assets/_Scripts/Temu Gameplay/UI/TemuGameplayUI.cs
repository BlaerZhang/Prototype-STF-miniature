using UnityEngine;
using TemuGameplay.Core;

namespace TemuGameplay.UI
{
    /// <summary>
    /// 拼多多玩法主UI管理器
    /// 统一管理所有UI组件，提供统一的接口
    /// </summary>
    public class TemuGameplayUI : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private TraitDisplayUI traitDisplayUI;
        [SerializeField] private ItemDetailUI itemDetailUI;
        [SerializeField] private ItemGridUI itemGridUI;
        
        [Header("Auto Find Components")]
        [SerializeField] private bool autoFindComponents = true;

        private GameplayManager gameplayManager;

        private void Awake()
        {
            if (autoFindComponents)
            {
                FindUIComponents();
            }
        }

        private void Start()
        {
            gameplayManager = FindObjectOfType<GameplayManager>();
            if (gameplayManager == null)
            {
                Debug.LogError("GameplayManager not found!");
                return;
            }

            // 确保所有UI组件都已连接
            ConnectUIComponents();

            // 监听游戏状态变化
            gameplayManager.OnLevelSubmitted += OnLevelSubmitted;
        }

        private void FindUIComponents()
        {
            if (traitDisplayUI == null)
                traitDisplayUI = FindObjectOfType<TraitDisplayUI>();

            if (itemDetailUI == null)
                itemDetailUI = FindObjectOfType<ItemDetailUI>();

            if (itemGridUI == null)
                itemGridUI = FindObjectOfType<ItemGridUI>();
        }

        private void ConnectUIComponents()
        {
            // 连接ItemGridUI和ItemDetailUI
            if (itemGridUI != null && itemDetailUI != null)
            {
                itemGridUI.SetItemDetailUI(itemDetailUI);
            }
        }

        private void OnLevelSubmitted(bool isValid)
        {
            if (isValid)
            {
                OnLevelCompleted();
            }
            else
            {
                OnLevelIncomplete();
            }
        }

        private void OnLevelCompleted()
        {
            Debug.Log("🎉 UI: Level completed!");
            // 这里可以添加完成关卡的UI效果
            // 比如显示完成动画、播放音效等
        }

        private void OnLevelIncomplete()
        {
            Debug.Log("❌ UI: Level requirements not met");
            // 关卡未完成，可以在这里添加提示UI
        }

        #region Public Methods
        /// <summary>
        /// 刷新所有UI显示
        /// </summary>
        public void RefreshAllUI()
        {
            if (traitDisplayUI != null)
                traitDisplayUI.RefreshDisplay();

            if (itemGridUI != null)
                itemGridUI.RefreshAllGrids();

            if (itemDetailUI != null)
                itemDetailUI.ClearDisplay();
        }

        /// <summary>
        /// 显示物品详情
        /// </summary>
        public void ShowItemDetail(Data.Item item)
        {
            if (itemDetailUI != null)
                itemDetailUI.ShowItemDetail(item);
        }

        /// <summary>
        /// 清除物品详情显示
        /// </summary>
        public void ClearItemDetail()
        {
            if (itemDetailUI != null)
                itemDetailUI.ClearDisplay();
        }

        /// <summary>
        /// 获取UI组件引用
        /// </summary>
        public TraitDisplayUI GetTraitDisplayUI() => traitDisplayUI;
        public ItemDetailUI GetItemDetailUI() => itemDetailUI;
        public ItemGridUI GetItemGridUI() => itemGridUI;
        #endregion

        private void OnDestroy()
        {
            if (gameplayManager != null)
            {
                gameplayManager.OnLevelSubmitted -= OnLevelSubmitted;
            }
        }

        #region Inspector Methods
        [ContextMenu("Find All UI Components")]
        public void FindAllUIComponents()
        {
            FindUIComponents();
            Debug.Log("已查找所有UI组件");
        }

        [ContextMenu("Refresh All UI")]
        public void RefreshAllUIFromInspector()
        {
            if (Application.isPlaying)
            {
                RefreshAllUI();
                Debug.Log("已刷新所有UI");
            }
            else
            {
                Debug.Log("只能在运行时刷新UI");
            }
        }

        [ContextMenu("Validate UI Setup")]
        public void ValidateUISetup()
        {
            bool allValid = true;

            if (traitDisplayUI == null)
            {
                Debug.LogError("TraitDisplayUI is missing!");
                allValid = false;
            }

            if (itemDetailUI == null)
            {
                Debug.LogError("ItemDetailUI is missing!");
                allValid = false;
            }

            if (itemGridUI == null)
            {
                Debug.LogError("ItemGridUI is missing!");
                allValid = false;
            }

            if (allValid)
            {
                Debug.Log("✓ 所有UI组件都已正确设置");
            }
        }
        #endregion
    }
} 