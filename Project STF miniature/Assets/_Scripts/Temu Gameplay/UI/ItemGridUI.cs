using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TemuGameplay.Data;
using TemuGameplay.Core;

namespace TemuGameplay.UI
{
    public class ItemGridUI : MonoBehaviour
    {
        [Header("Grid Settings")]
        [SerializeField] private Transform availableItemsGrid; // 底部可用物品网格
        [SerializeField] private Transform selectedItemsGrid; // 中间已选择物品网格
        [SerializeField] private GameObject itemUIPrefab; // ItemUI预制件
        
        [Header("UI References")]
        [SerializeField] private ItemDetailUI itemDetailUI; // 右侧详情显示

        private GameplayManager gameplayManager;
        private List<ItemUI> availableItemUIs = new List<ItemUI>();
        private List<ItemUI> selectedItemUIs = new List<ItemUI>();

        private void Start()
        {
            gameplayManager = FindObjectOfType<GameplayManager>();
            if (gameplayManager == null)
            {
                Debug.LogError("GameplayManager not found!");
                return;
            }

            // 监听事件
            gameplayManager.CurrentSet.OnItemAdded += OnItemAdded;
            gameplayManager.CurrentSet.OnItemRemoved += OnItemRemoved;
            gameplayManager.OnItemQuantitiesChanged += OnItemQuantitiesChanged;

            // 初始化网格
            InitializeAvailableItemsGrid();
            UpdateSelectedItemsGrid();
        }

        private void InitializeAvailableItemsGrid()
        {
            if (availableItemsGrid == null || itemUIPrefab == null) return;

            // 清除现有的UI
            ClearGrid(availableItemsGrid, availableItemUIs);

            // 获取所有可用物品
            var allItems = gameplayManager.ItemDatabase.AllItems;

            foreach (var item in allItems)
            {
                CreateItemUI(item, availableItemsGrid, availableItemUIs, true);
            }
        }

        private void UpdateSelectedItemsGrid()
        {
            if (selectedItemsGrid == null || itemUIPrefab == null) return;

            // 清除现有的UI
            ClearGrid(selectedItemsGrid, selectedItemUIs);

            // 获取已选择的物品
            var selectedItems = gameplayManager.CurrentSet.SelectedItems;

            foreach (var item in selectedItems)
            {
                CreateItemUI(item, selectedItemsGrid, selectedItemUIs, false);
            }
        }

        private void CreateItemUI(Item item, Transform parent, List<ItemUI> uiList, bool isAvailableGrid)
        {
            GameObject itemObj = Instantiate(itemUIPrefab, parent);
            ItemUI itemUI = itemObj.GetComponent<ItemUI>();

            if (itemUI == null)
            {
                Debug.LogError($"ItemUI component not found on prefab {itemUIPrefab.name}");
                Destroy(itemObj);
                return;
            }

            // 初始化ItemUI
            // 可用物品网格显示数量，选中物品网格不显示数量
            itemUI.Initialize(item, isAvailableGrid);

            // 设置选中状态
            if (isAvailableGrid)
            {
                bool isSelected = gameplayManager.CurrentSet.HasItem(item);
                itemUI.SetSelected(isSelected);
            }
            else
            {
                itemUI.SetSelected(true); // 已选择网格中的都是选中状态
            }

            // 绑定事件
            itemUI.OnItemHover += OnItemUIHover;
            itemUI.OnItemExitHover += OnItemUIExitHover;
            itemUI.OnItemClick += (clickedItemUI) => OnItemUIClick(clickedItemUI, isAvailableGrid);

            uiList.Add(itemUI);
        }

        private void ClearGrid(Transform gridParent, List<ItemUI> uiList)
        {
            // 清理事件绑定
            foreach (var itemUI in uiList)
            {
                if (itemUI != null)
                {
                    itemUI.OnItemHover -= OnItemUIHover;
                    itemUI.OnItemExitHover -= OnItemUIExitHover;
                    // 注意：lambda表达式事件无法直接解绑，这里通过销毁GameObject来清理
                }
            }

            // 销毁GameObject
            foreach (Transform child in gridParent)
            {
                if (Application.isPlaying)
                    Destroy(child.gameObject);
                else
                    DestroyImmediate(child.gameObject);
            }

            uiList.Clear();
        }

        #region Event Handlers
        private void OnItemUIHover(ItemUI itemUI)
        {
            if (itemDetailUI != null && itemUI.ItemData != null)
            {
                itemDetailUI.OnItemHover(itemUI.ItemData);
            }
        }

        private void OnItemUIExitHover(ItemUI itemUI)
        {
            if (itemDetailUI != null)
            {
                itemDetailUI.OnItemExitHover();
            }
        }

        private void OnItemUIClick(ItemUI itemUI, bool isAvailableGrid)
        {
            if (itemUI.ItemData == null) return;

            if (isAvailableGrid)
            {
                // 可用物品网格：切换选择状态
                if (itemUI.IsSelected)
                {
                    // 当前已选中，尝试移除
                    gameplayManager.RemoveItemFromSet(itemUI.ItemData.ItemId);
                }
                else
                {
                    // 当前未选中，检查是否有库存
                    if (!itemUI.ItemData.IsAvailable)
                    {
                        Debug.LogWarning($"Cannot select {itemUI.ItemData.ItemName}: out of stock!");
                        return;
                    }
                    
                    // 尝试添加
                    gameplayManager.AddItemToSet(itemUI.ItemData.ItemId);
                }
            }
            else
            {
                // 已选择物品网格：移除选择
                gameplayManager.RemoveItemFromSet(itemUI.ItemData.ItemId);
            }
        }

        private void OnItemAdded(Item item)
        {
            // 更新可用物品网格中对应项的选中状态
            var availableItemUI = availableItemUIs.FirstOrDefault(ui => ui.ItemData.ItemId == item.ItemId);
            if (availableItemUI != null)
            {
                availableItemUI.SetSelected(true);
            }

            // 刷新已选择物品网格
            UpdateSelectedItemsGrid();
        }

        private void OnItemRemoved(Item item)
        {
            // 更新可用物品网格中对应项的选中状态
            var availableItemUI = availableItemUIs.FirstOrDefault(ui => ui.ItemData.ItemId == item.ItemId);
            if (availableItemUI != null)
            {
                availableItemUI.SetSelected(false);
            }

            // 刷新已选择物品网格
            UpdateSelectedItemsGrid();
        }

        private void OnItemQuantitiesChanged()
        {
            // 当物品数量发生变化时，刷新所有ItemUI的显示
            foreach (var itemUI in availableItemUIs)
            {
                if (itemUI != null)
                {
                    itemUI.RefreshDisplay();
                }
            }

            foreach (var itemUI in selectedItemUIs)
            {
                if (itemUI != null)
                {
                    itemUI.RefreshDisplay();
                }
            }

            Debug.Log("Refreshed all item quantities display");
        }
        #endregion

        #region Public Methods
        // 手动刷新所有网格
        public void RefreshAllGrids()
        {
            InitializeAvailableItemsGrid();
            UpdateSelectedItemsGrid();
        }

        // 设置ItemDetailUI引用
        public void SetItemDetailUI(ItemDetailUI detailUI)
        {
            itemDetailUI = detailUI;
        }
        #endregion

        private void OnDestroy()
        {
            if (gameplayManager?.CurrentSet != null)
            {
                gameplayManager.CurrentSet.OnItemAdded -= OnItemAdded;
                gameplayManager.CurrentSet.OnItemRemoved -= OnItemRemoved;
            }

            if (gameplayManager != null)
            {
                gameplayManager.OnItemQuantitiesChanged -= OnItemQuantitiesChanged;
            }

            // 清理所有UI
            ClearGrid(availableItemsGrid, availableItemUIs);
            ClearGrid(selectedItemsGrid, selectedItemUIs);
        }

        // Inspector工具方法
        [ContextMenu("Refresh Grids")]
        public void RefreshGridsFromInspector()
        {
            if (Application.isPlaying)
            {
                RefreshAllGrids();
            }
            else
            {
                Debug.Log("只能在运行时刷新网格");
            }
        }
    }
} 