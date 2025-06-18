using UnityEngine;
using TMPro;
using System.Linq;
using TemuGameplay.Data;
using UnityEngine.InputSystem;

namespace TemuGameplay.UI
{
    public class ItemDetailUI : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private TextMeshProUGUI itemDetailText;
        [SerializeField] private GameObject detailPanel; // 整个详情面板
        
        [Header("Display Settings")]
        [SerializeField] private bool hideWhenNoItem = true;

        private void Awake()
        {
            if (hideWhenNoItem && detailPanel != null)
            {
                detailPanel.SetActive(false);
            }
            
            ClearDisplay();
        }

        private void Update()
        {
            // follow mouse X position when active
            if (detailPanel.activeSelf)
            {
                Vector2 mousePos = Mouse.current.position.ReadValue();
                detailPanel.transform.position = new Vector3(mousePos.x, detailPanel.transform.position.y, detailPanel.transform.position.z);
            }
        }

        public void ShowItemDetail(Item item)
        {
            if (item == null)
            {
                ClearDisplay();
                return;
            }

            if (hideWhenNoItem && detailPanel != null)
            {
                detailPanel.SetActive(true);
            }

            DisplayItem(item);
        }

        public void ClearDisplay()
        {
            if (itemDetailText != null)
            {
                itemDetailText.text = "";
            }

            if (hideWhenNoItem && detailPanel != null)
            {
                detailPanel.SetActive(false);
            }
        }

        private void DisplayItem(Item item)
        {
            if (itemDetailText == null || item == null) return;

            var displayLines = new System.Collections.Generic.List<string>();

            // 物品名称
            displayLines.Add($"<size=18><b>{item.ItemName}</b></size>");
            displayLines.Add("");

            // Traits
            if (item.Traits != null && item.Traits.Count > 0)
            {
                displayLines.Add("<b>Traits:</b>");
                foreach (var trait in item.Traits)
                {
                    string traitName = GetTraitDisplayName(trait);
                    displayLines.Add($"  • {traitName}");
                }
                displayLines.Add("");
            }

            // Description
            if (!string.IsNullOrEmpty(item.Description))
            {
                displayLines.Add("<b>Description:</b>");
                displayLines.Add($"  {item.Description}");
                displayLines.Add("");
            }

            // ID（可选，用于调试）
            #if UNITY_EDITOR
            displayLines.Add($"<size=10><color=#888888>ID: {item.ItemId}</color></size>");
            #endif

            itemDetailText.text = string.Join("\n", displayLines);
        }

        private string GetTraitDisplayName(TraitType trait)
        {
            switch (trait)
            {
                case TraitType.Hiking: return "Hiking";
                case TraitType.Running: return "Running";
                case TraitType.Nutrition: return "Nutrition";
                case TraitType.Climbing: return "Climbing";
                case TraitType.Sitting: return "Sitting";
                case TraitType.Entertainment: return "Entertainment";
                case TraitType.Survival: return "Survival";
                case TraitType.Food: return "Food";
                case TraitType.Water: return "Water";
                case TraitType.Tool: return "Tool";
                default: return trait.ToString();
            }
        }

        // 外部调用方法
        public void OnItemHover(Item item)
        {
            ShowItemDetail(item);
        }

        public void OnItemExitHover()
        {
            ClearDisplay();
        }

        // Inspector工具方法
        [ContextMenu("Test Display")]
        private void TestDisplay()
        {
            var testItem = new Item(
                "test_001", 
                "Test Item", 
                new System.Collections.Generic.List<TraitType> { TraitType.Hiking, TraitType.Running }, 
                null, 
                "This is a test item with multiple traits."
            );
            
            ShowItemDetail(testItem);
        }

        [ContextMenu("Clear Test")]
        private void ClearTest()
        {
            ClearDisplay();
        }
    }
} 