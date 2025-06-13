using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using TemuGameplay.Data;
using TMPro;

namespace TemuGameplay.UI
{
    public class ItemUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [Header("UI Components")]
        [SerializeField] private Image itemIcon;
        [SerializeField] private Button itemButton;
        [SerializeField] private GameObject selectedFrame; // 选中状态的边框
        [SerializeField] private TextMeshProUGUI quantityText; // 数量显示文本（右下角）
        [SerializeField] private GameObject quantityPanel; // 数量文本背后的Panel（可选）
        
        private Item itemData;
        private bool isSelected;
        private bool showQuantity = true; // 是否显示数量

        public Item ItemData => itemData;
        public bool IsSelected => isSelected;

        public event Action<ItemUI> OnItemHover;
        public event Action<ItemUI> OnItemExitHover;
        public event Action<ItemUI> OnItemClick;

        private void Awake()
        {
            if (itemButton == null)
                itemButton = GetComponent<Button>();
                
            if (selectedFrame != null)
                selectedFrame.SetActive(false);
        }

        public void Initialize(Item item, bool showQuantityDisplay = true)
        {
            itemData = item;
            showQuantity = showQuantityDisplay;
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            if (itemData == null) return;

            // 设置图标
            if (itemIcon != null && itemData.Icon != null)
            {
                itemIcon.sprite = itemData.Icon;
                itemIcon.color = GetTraitColor(itemData.Traits.Count > 0 ? itemData.Traits[0] : TraitType.Hiking);
            }
            else if (itemIcon != null)
            {
                // 如果没有图标，显示默认颜色
                itemIcon.color = GetTraitColor(itemData.Traits.Count > 0 ? itemData.Traits[0] : TraitType.Hiking);
            }
            
            // 更新数量显示
            UpdateQuantityDisplay();
            
            // 更新按钮可用状态
            UpdateButtonState();
        }
        
        /// <summary>
        /// 更新数量显示
        /// </summary>
        private void UpdateQuantityDisplay()
        {
            if (quantityText != null && itemData != null)
            {
                // 根据showQuantity标志决定是否显示数量
                bool shouldShow = showQuantity;
                quantityText.gameObject.SetActive(shouldShow);
                
                // 同时控制数量Panel的显示（如果存在）
                if (quantityPanel != null)
                {
                    quantityPanel.SetActive(shouldShow);
                }
                
                if (shouldShow)
                {
                    quantityText.text = itemData.QuantityText;
                    
                    // 根据数量调整文本颜色
                    if (itemData.CurrentQuantity <= 0)
                    {
                        quantityText.color = Color.red; // 没有数量时显示红色
                    }
                    else if (itemData.CurrentQuantity <= 1)
                    {
                        quantityText.color = new Color(1f, 0.5f, 0f, 1f); // 数量很少时显示橙色
                    }
                    else
                    {
                        quantityText.color = Color.white; // 正常数量显示白色
                    }
                }
            }
        }
        
        /// <summary>
        /// 更新按钮可用状态
        /// </summary>
        private void UpdateButtonState()
        {
            if (itemButton != null && itemData != null)
            {
                bool isAvailable = itemData.IsAvailable;
                itemButton.interactable = isAvailable;
                
                // 如果没有库存，让整个物品UI变灰
                if (itemIcon != null)
                {
                    Color iconColor = itemIcon.color;
                    iconColor.a = isAvailable ? 1f : 0.5f;
                    itemIcon.color = iconColor;
                }
            }
        }
        
        /// <summary>
        /// 刷新显示（从外部调用）
        /// </summary>
        public void RefreshDisplay()
        {
            UpdateDisplay();
        }
        
        /// <summary>
        /// 设置是否显示数量
        /// </summary>
        public void SetShowQuantity(bool show)
        {
            showQuantity = show;
            UpdateQuantityDisplay();
        }

        public void SetSelected(bool selected)
        {
            isSelected = selected;
            if (selectedFrame != null)
            {
                selectedFrame.SetActive(selected);
            }
        }

        // 根据主要trait获取颜色
        private Color GetTraitColor(TraitType trait)
        {
            switch (trait)
            {
                case TraitType.Hiking: return new Color(0.4f, 0.8f, 0.4f, 1f); // 绿色
                case TraitType.Running: return new Color(0.8f, 0.4f, 0.4f, 1f); // 红色
                case TraitType.Nutrition: return new Color(0.4f, 0.4f, 0.4f, 1f); // 深灰色
                case TraitType.Climbing: return new Color(0.6f, 0.6f, 1f, 1f); // 深蓝色
                case TraitType.Sitting: return new Color(1f, 0.6f, 0.6f, 1f); // 粉红
                case TraitType.Entertainment: return new Color(1f, 0.8f, 0.2f, 1f); // 金色
                case TraitType.Survival: return new Color(0.8f, 0.6f, 0.2f, 1f); // 橙黄
                case TraitType.Food: return new Color(1f, 0.6f, 0.6f, 1f); // 粉红
                case TraitType.Water: return new Color(0.3f, 0.7f, 1f, 1f); // 水蓝
                case TraitType.Tool: return new Color(0.8f, 0.8f, 0.8f, 1f); // 深灰色
                case TraitType.Medic: return new Color(0.8f, 0.2f, 0.2f, 1f); // 十字架红
                case TraitType.Booster: return new Color(0.8f, 0.8f, 0.2f, 1f); // 电浆绿
                case TraitType.Movement: return new Color(0.8f, 0.2f, 0.8f, 1f); // 亮黄色
                default: return Color.white;
            }
        }

        #region Event Handlers
        public void OnPointerEnter(PointerEventData eventData)
        {
            OnItemHover?.Invoke(this);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            OnItemExitHover?.Invoke(this);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            // 只有当物品可用时才响应点击
            if (itemData != null && itemData.IsAvailable)
            {
                OnItemClick?.Invoke(this);
            }
        }
        #endregion
        
        #region Context Menu Methods (Inspector调试用)
        [ContextMenu("Test Consume One")]
        public void TestConsumeOne()
        {
            if (itemData != null)
            {
                bool consumed = itemData.ConsumeOne();
                Debug.Log($"Consumed {itemData.ItemName}: {consumed}, Remaining: {itemData.CurrentQuantity}");
                RefreshDisplay();
            }
        }
        
        [ContextMenu("Reset Quantity")]
        public void TestResetQuantity()
        {
            if (itemData != null)
            {
                itemData.ResetQuantity();
                Debug.Log($"Reset {itemData.ItemName} quantity to: {itemData.CurrentQuantity}");
                RefreshDisplay();
            }
        }
        #endregion
    }
} 