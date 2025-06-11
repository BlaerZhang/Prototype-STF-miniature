/*
=== Temu Gameplay Demo Setup Guide ===

这是一个完整的特质同步系统设置指南。

## 1. 创建基础GameObject层级结构：

GameManager (Empty GameObject)
├── GameplayManager.cs
├── RandomLevelDemo.cs (用于快捷键测试)
└── ItemDatabase.cs

UI (Empty GameObject)
├── TemuGameplayUI.cs (主UI管理器)
├── Canvas (UI Canvas)
    ├── TraitDisplay (Empty GameObject)
    │   └── TraitDisplayUI.cs
    ├── ItemDetail (Empty GameObject)
    │   └── ItemDetailUI.cs
    ├── ItemGrid (Empty GameObject)
    │   └── ItemGridUI.cs
    └── SubmitButton (Button)
        └── SubmitButtonUI.cs

## 2. 组件配置顺序：

### Step 1: 创建ItemDatabase
1. 创建ItemDatabase ScriptableObject资源
2. 添加体育主题的物品（参考现有的Hiking/Running等物品）
3. 将ItemDatabase拖拽到GameplayManager的itemDatabase字段

### Step 2: 配置GameplayManager
- 选择Level Generation Mode (ScriptableObject 或 RandomGeneration)
- 如果使用ScriptableObject模式，创建LevelRequirement资源
- 如果使用Random模式，配置RandomLevelSettings参数

### Step 3: 设置UI组件
1. 配置TraitDisplayUI：
   - 设置Trait Counter Prefab或Container
   - 设置Level Objective Prefab或Container

2. 配置ItemDetailUI：
   - 设置Detail Panel、Name Text、Traits Text等UI元素

3. 配置ItemGridUI：
   - 设置Available Grid和Selected Grid的Transform
   - 设置Item Prefab (带有ItemUI组件)

4. 配置SubmitButtonUI：
   - 挂载到Button上
   - 设置Button Text引用

### Step 4: 创建ItemUI Prefab
ItemUI Prefab结构：
- Root GameObject (ItemUI.cs)
  - ItemIcon (Image)
  - ItemButton (Button)
  - SelectedFrame (GameObject)
  - QuantityText (TextMeshProUGUI) - 右下角显示数量

### Step 5: 连接所有引用
- TemuGameplayUI会自动查找组件(如果autoFindComponents=true)
- 或者手动拖拽所有UI组件到对应字段

## 3. 测试快捷键：

运行时按键功能：
- R: 重新生成随机关卡
- T: 切换生成模式 (SO ↔ Random)
- Space: 提交答案
- Q: 重置所有物品数量到最大值
- 1: 简单难度 (20%)
- 2: 中等难度 (50%)
- 3: 困难难度 (80%)

## 4. 系统功能：

### 特质计数器显示
- 自动显示当前选中物品的特质统计
- 显示关卡目标要求
- 实时更新计数

### 物品选择系统
- 点击可用物品添加到选中区域
- 点击选中物品移除
- 悬停显示物品详情

### 手动提交验证
- 点击Submit按钮或按Space键提交
- 验证是否达成关卡目标
- 成功后自动消耗选中物品数量(-1)
- 成功后自动刷新关卡

### 物品数量系统
- 每个物品都有固定的最大数量和当前数量
- UI右下角显示当前数量
- 数量不足时物品变灰且不可选择
- 成功提交后消耗选中物品的数量
- 可按Q键重置所有物品数量到最大值

### 随机关卡生成
- 可配置特质类型数量范围
- 可配置每种特质的数量范围
- 难度调节器影响要求数量
- 自动验证关卡可完成性

## 5. 调试功能：

所有主要组件都包含Context Menu方法：
- [右键组件] → 各种测试和验证功能
- Console日志详细显示系统状态
- 屏幕左下角显示当前模式和控制提示

## 6. 扩展建议：

- 添加更多特质类型 (在TraitType枚举中)
- 创建更多主题物品 (体育、科技、生活等)
- 添加音效和动画效果
- 实现关卡进度保存
- 添加计时挑战模式

=== 设置完成后即可开始游戏测试 ===
*/

using UnityEngine;

namespace TemuGameplay
{
    /// <summary>
    /// 演示设置指南 - 这个脚本仅包含设置说明
    /// 实际不需要挂载到任何GameObject上
    /// </summary>
    public class Demo_Setup_Guide : MonoBehaviour
    {
        [Header("Setup Guide")]
        [TextArea(10, 20)]
        public string setupInstructions = "请查看脚本注释中的完整设置指南";

        [ContextMenu("Show Setup Guide")]
        public void ShowSetupGuide()
        {
            Debug.Log(@"
=== Temu Gameplay Demo Setup Guide ===

1. 创建GameObject层级结构 (详见脚本注释)
2. 配置ItemDatabase和GameplayManager
3. 设置UI组件和引用
4. 创建ItemUI Prefab
5. 运行游戏，使用快捷键测试

快捷键：
- R: 重新生成关卡
- T: 切换模式
- Space: 提交答案
- Q: 重置物品数量
- 1/2/3: 难度调节

详细指南请查看脚本文件顶部的注释！
");
        }
    }
} 