/*
=== ItemUI Prefab 详细设置指南 ===

这个指南详细说明如何创建完整的ItemUI Prefab，包含数量显示功能。

## 1. GameObject层级结构：

ItemUI_Prefab (GameObject)                    [尺寸: 100x100]
├── ItemUI.cs (Script Component)
├── Image (Component) - 背景
├── ItemIcon (Image GameObject)              [锚点: 拉伸，边距: 5,5,5,20]
├── ItemButton (Button GameObject)           [锚点: 拉伸，边距: 0,0,0,0]
│   └── (无子对象，透明按钮)
├── SelectedFrame (GameObject)               [锚点: 拉伸，边距: -2,-2,-2,-2]
│   └── Image (Component) - 选中边框
└── QuantityText (TextMeshProUGUI)          [锚点: 右下角]

## 2. 详细组件设置：

### Root GameObject - ItemUI_Prefab
- RectTransform: 100x100
- Image: 背景色 (0.2, 0.2, 0.2, 1) 深灰色
- ItemUI Script: 挂载脚本

### ItemIcon (Image)
- RectTransform: 
  - 锚点: Stretch (拉伸)
  - 边距: Left=5, Top=5, Right=5, Bottom=20
- Image: 
  - Source Image: 留空 (代码设置)
  - Color: 白色 (代码会根据特质改变)
  - Preserve Aspect: 勾选

### ItemButton (Button)
- RectTransform: 
  - 锚点: Stretch (拉伸)
  - 边距: 全部为0
- Button:
  - Target Graphic: Button本身的Image
  - Transition: Color Tint
  - Normal: 透明 (0,0,0,0)
  - Highlighted: 半透明白 (1,1,1,0.1)
  - Pressed: 半透明白 (1,1,1,0.2)
  - Disabled: 透明 (0,0,0,0)
- Image: 透明背景

### SelectedFrame (GameObject)
- RectTransform:
  - 锚点: Stretch (拉伸)
  - 边距: Left=-2, Top=-2, Right=-2, Bottom=-2
- Image:
  - Color: 亮绿色 (0,1,0,1)
  - Image Type: Sliced (如果有9-slice sprite)
- 默认: SetActive(false)

### QuantityText (TextMeshProUGUI)
- RectTransform:
  - 锚点: 右下角 (1,0)
  - 锚点位置: (0, 0)
  - 大小: 30x20
  - 位置: (-15, 10) 相对右下角
- TextMeshProUGUI:
  - Text: "3" (示例)
  - Font: Arial SDF
  - Font Size: 14
  - Color: 白色
  - Alignment: 右下对齐
  - Auto Size: 可选开启

### QuantityPanel (GameObject) - 可选
- RectTransform: 与QuantityText相同位置和大小
- Image: 半透明黑色背景 (0,0,0,0.5)
- 为数量文字提供背景对比度

## 3. 脚本引用设置：

在ItemUI组件中设置引用：
- Item Icon: 拖拽 ItemIcon GameObject
- Item Button: 拖拽 ItemButton GameObject  
- Selected Frame: 拖拽 SelectedFrame GameObject
- Quantity Text: 拖拽 QuantityText GameObject
- Quantity Panel: 拖拽 QuantityPanel GameObject (可选)

## 4. 颜色状态说明：

### 数量文本颜色：
- 白色: 正常数量 (>1)
- 橙色: 数量较少 (=1)
- 红色: 无库存 (=0)

### 物品图标状态：
- 正常: Alpha = 1.0
- 无库存: Alpha = 0.5 (变灰)

### 选中边框：
- 隐藏: 未选中状态
- 显示绿色边框: 已选中状态

### 数量显示逻辑：
- 可用物品网格: 显示数量 (便于玩家了解库存)
- 选中物品网格: 隐藏数量 (避免误解为提交多个)

## 5. 预制件保存：

1. 创建完整的GameObject层级
2. 设置所有组件属性
3. 连接ItemUI脚本的所有引用
4. 拖拽到Project窗口保存为Prefab
5. 在ItemGridUI中设置Item UI Prefab引用

## 6. 测试验证：

运行时可以通过以下方式测试：
- 右键ItemUI → "Test Consume One" (消耗一个)
- 右键ItemUI → "Reset Quantity" (重置数量)
- 右键GameplayManager → "Show Item Quantities" (显示所有数量)

=== 设置完成后ItemUI将完美显示数量和状态 ===
*/

using UnityEngine;

namespace TemuGameplay
{
    /// <summary>
    /// ItemUI Prefab设置指南 - 仅包含设置说明
    /// 不需要挂载到任何GameObject上
    /// </summary>
    public class ItemUI_Prefab_Guide : MonoBehaviour
    {
        [Header("ItemUI Prefab Guide")]
        [TextArea(5, 10)]
        public string prefabGuide = "请查看脚本注释中的完整ItemUI Prefab设置指南";

        [ContextMenu("Show ItemUI Prefab Guide")]
        public void ShowItemUIPrefabGuide()
        {
            Debug.Log(@"
=== ItemUI Prefab 设置指南 ===

GameObject层级结构:
ItemUI_Prefab (100x100)
├── ItemUI.cs Script
├── ItemIcon (Image) - 拉伸，边距5,5,5,20
├── ItemButton (Button) - 拉伸，透明
├── SelectedFrame (GameObject) - 拉伸，绿色边框
└── QuantityText (TMP) - 右下角，白色文字

数量显示颜色:
- 白色: 正常数量 (>1)
- 橙色: 数量较少 (=1)  
- 红色: 无库存 (=0)

详细设置请查看脚本文件顶部的注释！
");
        }
    }
} 