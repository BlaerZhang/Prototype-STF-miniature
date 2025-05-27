# 技能系统使用指南

## 概述

技能系统是为Auto Runner Linear项目开发的简易技能管理系统，集成在`AdvancedAutoRunnerController`中。系统包含精力管理、技能冷却、动画控制和UI集成等功能。

## 核心组件

### 1. Skill.cs - 技能数据结构
```csharp
[System.Serializable]
public class Skill
{
    public string skillName;                    // 技能名称
    public float duration;                      // 持续时间（秒）
    public float cooldown;                      // 冷却时间（秒）
    public float energyCost;                    // 精力消耗
    public AnimationParameterType parameterType; // 动画参数类型（Bool/Trigger）
    public string animationParameterName;       // 动画机参数名
    public bool affectsMovementSpeed;           // 是否影响移动速度
    public float speedMultiplier;               // 速度倍数
}
```

### 2. EnergySystem.cs - 精力系统
```csharp
[System.Serializable]
public class EnergySystem
{
    public float MaxEnergy;                     // 精力最大值
    public float CurrentEnergy;                 // 当前精力值
    public float EnergyRegenRate;               // 精力回复速度（每秒）
}
```

### 3. SkillSystem.cs - 技能系统管理器
管理所有技能的激活、冷却和动画控制。

### 4. AdvancedAutoRunnerController.cs - 高级控制器
集成了技能系统的完整跑酷控制器。

## 快速开始

### 1. 基础设置

1. 在角色GameObject上添加`AdvancedAutoRunnerController`组件
2. 确保`Enable Skill System`选项已勾选
3. 系统会自动添加`SkillSystem`组件

### 2. 配置技能

在Inspector中的`SkillSystem`组件中：

```csharp
// 技能列表配置示例
Skills[0]:
  - Skill Name: "冲刺"
  - Duration: 3.0
  - Cooldown: 10.0
  - Energy Cost: 20.0
  - Parameter Type: Bool
  - Animation Parameter Name: "IsSprinting"
  - Affects Movement Speed: true
  - Speed Multiplier: 1.5

Skills[1]:
  - Skill Name: "跳跃"
  - Duration: 1.0
  - Cooldown: 5.0
  - Energy Cost: 15.0
  - Parameter Type: Trigger
  - Animation Parameter Name: "Jump"
  - Affects Movement Speed: false
```

### 3. 配置精力系统

```csharp
Energy System:
  - Max Energy: 100
  - Energy Regen Rate: 5.0 (每秒回复5点精力)
```

### 4. 配置UI（可选）

```csharp
UI References:
  - Energy Slider: 精力条UI
  - Skill UI Elements: 技能图标和冷却遮罩
```

## 代码使用示例

### 激活技能

```csharp
// 通过索引激活技能
bool success = advancedController.TryActivateSkill(0);

// 通过名称激活技能
bool success = advancedController.TryActivateSkill("冲刺");
```

### 检查技能状态

```csharp
// 检查技能是否激活
bool isActive = advancedController.IsSkillActive(0);
bool isActive = advancedController.IsSkillActive("冲刺");

// 获取技能对象
Skill skill = advancedController.GetSkill(0);
if (skill != null)
{
    Debug.Log($"技能状态: {skill.isActive}");
    Debug.Log($"剩余冷却: {skill.remainingCooldown}");
}
```

### 精力系统操作

```csharp
// 获取精力信息
float currentEnergy = advancedController.GetCurrentEnergy();
float energyPercentage = advancedController.GetEnergyPercentage();

// 直接操作精力系统
var energySystem = advancedController.SkillSystem.Energy;
energySystem.FillEnergy();          // 填满精力
energySystem.EmptyEnergy();         // 清空精力
energySystem.RestoreEnergy(50f);    // 恢复50点精力
```

### 事件监听

```csharp
void Start()
{
    var skillSystem = advancedController.SkillSystem;
    
    // 订阅技能事件
    skillSystem.OnSkillActivated += OnSkillActivated;
    skillSystem.OnSkillDeactivated += OnSkillDeactivated;
    skillSystem.OnEnergyChanged += OnEnergyChanged;
}

private void OnSkillActivated(Skill skill)
{
    Debug.Log($"技能 {skill.skillName} 已激活");
}

private void OnSkillDeactivated(Skill skill)
{
    Debug.Log($"技能 {skill.skillName} 已结束");
}

private void OnEnergyChanged(float currentEnergy, float maxEnergy)
{
    Debug.Log($"精力变化: {currentEnergy}/{maxEnergy}");
}
```

## 动画集成

### 1. 动画参数类型

- **Bool**: 技能激活时设为true，结束时设为false
- **Trigger**: 技能激活时触发一次

### 2. 动画机设置

在Animator Controller中添加对应的参数：

```
Parameters:
  - IsSprinting (Bool)
  - Jump (Trigger)
  - IsSkillActive (Bool)
```

### 3. 动画状态机

```
States:
  - Idle -> Sprint (Condition: IsSprinting == true)
  - Sprint -> Idle (Condition: IsSprinting == false)
  - Any State -> Jump (Condition: Jump trigger)
```

## UI集成

### 1. 精力条设置

```csharp
// 在SkillSystemExample中
[SerializeField] private Slider energySlider;

void UpdateUI()
{
    energySlider.value = skillSystem.Energy.CurrentEnergy;
    energySlider.maxValue = skillSystem.Energy.MaxEnergy;
}
```

### 2. 技能按钮设置

```csharp
// 技能按钮配置
[SerializeField] private Button[] skillButtons;
[SerializeField] private Image[] skillCooldownOverlays;
[SerializeField] private Text[] skillCooldownTexts;

void UpdateSkillUI(int skillIndex)
{
    var skill = skillSystem.Skills[skillIndex];
    
    // 更新按钮可交互性
    skillButtons[skillIndex].interactable = skill.CanUse(skillSystem.Energy.CurrentEnergy);
    
    // 更新冷却遮罩
    if (skill.isOnCooldown)
    {
        skillCooldownOverlays[skillIndex].fillAmount = 1f - skill.GetCooldownProgress();
        skillCooldownTexts[skillIndex].text = Mathf.Ceil(skill.remainingCooldown).ToString();
    }
}
```

### 3. DOTween动画（可选）

```csharp
using DG.Tweening;

// 技能激活时的UI动画
private void OnSkillActivated(Skill skill)
{
    int skillIndex = skillSystem.Skills.IndexOf(skill);
    if (skillIndex >= 0)
    {
        // 按钮脉冲效果
        skillButtons[skillIndex].transform.DOPunchScale(Vector3.one * 0.1f, 0.3f);
        
        // 冷却遮罩动画
        skillCooldownOverlays[skillIndex].DOFillAmount(0f, skill.cooldown);
    }
}
```

## 调试功能

### 1. 键盘快捷键

在`SkillSystem`组件中：
- `1`, `2`, `3`: 激活对应技能
- `F1`: 重置技能系统
- `F2`: 填满精力

### 2. GUI调试信息

运行时会显示：
- 当前速度和状态
- 精力值
- 激活技能数量

### 3. Console日志

系统会输出详细的调试信息：
```
技能 '冲刺' 已激活
精力消耗: 20, 剩余: 80.0/100
技能 '冲刺' 已结束，开始冷却: 10秒
```

## 扩展功能

### 1. 添加新技能

```csharp
// 在Inspector中添加新的技能配置
Skills[2]:
  - Skill Name: "护盾"
  - Duration: 5.0
  - Cooldown: 15.0
  - Energy Cost: 30.0
  - Parameter Type: Bool
  - Animation Parameter Name: "HasShield"
  - Affects Movement Speed: false
```

### 2. 自定义技能效果

```csharp
// 在OnSkillActivated事件中添加自定义逻辑
private void OnSkillActivated(Skill skill)
{
    switch (skill.skillName)
    {
        case "冲刺":
            // 冲刺效果已自动应用（速度倍数）
            break;
            
        case "护盾":
            // 自定义护盾逻辑
            EnableShield();
            break;
            
        case "跳跃":
            // 自定义跳跃逻辑
            PerformJump();
            break;
    }
}
```

### 3. 技能组合

```csharp
// 检查多个技能的激活状态
bool canPerformCombo = advancedController.IsSkillActive("冲刺") && 
                       advancedController.IsSkillActive("跳跃");

if (canPerformCombo)
{
    // 执行组合技能
    PerformComboSkill();
}
```

## 性能优化

### 1. 对象池

对于频繁创建的UI元素，建议使用对象池：

```csharp
// 技能效果粒子对象池
public class SkillEffectPool : MonoBehaviour
{
    [SerializeField] private ParticleSystem effectPrefab;
    private Queue<ParticleSystem> pool = new Queue<ParticleSystem>();
    
    public ParticleSystem GetEffect()
    {
        if (pool.Count > 0)
            return pool.Dequeue();
        else
            return Instantiate(effectPrefab);
    }
    
    public void ReturnEffect(ParticleSystem effect)
    {
        effect.Stop();
        pool.Enqueue(effect);
    }
}
```

### 2. 事件优化

避免在Update中频繁检查技能状态，使用事件驱动：

```csharp
// 好的做法：使用事件
skillSystem.OnSkillActivated += UpdateUI;

// 避免：在Update中检查
void Update()
{
    // 避免这样做
    for (int i = 0; i < skills.Count; i++)
    {
        if (skills[i].isActive != lastActiveStates[i])
        {
            UpdateUI();
        }
    }
}
```

## 常见问题

### Q: 技能无法激活？
A: 检查以下条件：
1. 精力是否足够
2. 技能是否在冷却中
3. 技能是否已经激活
4. 技能系统是否启用

### Q: 动画参数无效？
A: 确保：
1. Animator组件存在
2. 动画参数名称正确
3. 参数类型匹配（Bool/Trigger）

### Q: UI不更新？
A: 检查：
1. UI引用是否正确设置
2. 事件是否正确订阅
3. Update方法是否被调用

### Q: 性能问题？
A: 优化建议：
1. 减少Update中的计算
2. 使用事件驱动更新
3. 合理设置UI更新频率

## 总结

技能系统提供了完整的技能管理功能，包括：
- ✅ 精力系统（自动回复、消耗验证）
- ✅ 技能冷却管理
- ✅ 动画集成（Bool/Trigger参数）
- ✅ UI集成（按钮、进度条、冷却遮罩）
- ✅ 事件系统（激活/结束/精力变化）
- ✅ 调试功能（快捷键、GUI、日志）
- ✅ 扩展性（自定义技能效果）

系统设计简洁但功能完整，适合原型开发和快速迭代。 