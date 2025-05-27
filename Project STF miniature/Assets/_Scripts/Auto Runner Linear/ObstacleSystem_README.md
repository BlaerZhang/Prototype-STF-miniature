# 障碍物效果系统使用指南

## 概述

障碍物效果系统是为 `AdvancedAutoRunnerController` 开发的扩展功能，允许玩家角色在经过不同类型的障碍物时触发相应的效果。

## 支持的障碍物类型

### 1. Low 障碍物（低障碍物）
- **效果**：踉跄状态
- **持续时间**：2 秒（可配置）
- **速度影响**：降到最低速度（startSpeed）
- **动画**：触发 "Stumble" 动画触发器
- **结束后**：重新开始加速

### 2. High 障碍物（高障碍物）
- **效果**：踉跄状态（同 Low 障碍物）
- **持续时间**：2 秒（可配置）
- **速度影响**：降到最低速度（startSpeed）
- **动画**：触发 "Stumble" 动画触发器
- **结束后**：重新开始加速

### 3. Slowing 障碍物（减速障碍物）
- **效果**：减速状态
- **持续时间**：3 秒（可配置）
- **速度影响**：速度减半（可配置倍数）
- **动画**：无特殊动画
- **结束后**：重新开始加速

## 新增功能（高级特性）

### 1. 障碍物效果阻塞机制

在任意障碍物效果持续期间，不会触发新的障碍物效果：

```csharp
// 如果当前已有障碍物效果，新的效果将被忽略
if (controller.IsInObstacleEffect())
{
    // 新的障碍物碰撞将被忽略
    return; 
}
```

**设计目的：**
- 防止效果重叠造成的混乱
- 确保玩家体验的一致性
- 避免效果时间的异常重置

### 2. 技能免疫系统

特定技能激活期间可以免疫对应的障碍物效果：

| 技能 | 免疫障碍物类型 | 说明 |
|------|---------------|------|
| Jump | Low 障碍物 | 跳跃期间不受低障碍物影响 |
| Roll | High 障碍物 | 翻滚期间不受高障碍物影响 |
| Dash | Slowing 障碍物 | 冲刺期间不受减速障碍物影响 |

**工作机制：**
```csharp
// 检查技能免疫
private bool IsImmuneToObstacle(ObstacleType obstacleType)
{
    switch (obstacleType)
    {
        case ObstacleType.Low:
            return IsSkillActive("Jump");  // Jump技能激活时免疫
        case ObstacleType.High:
            return IsSkillActive("Roll");  // Roll技能激活时免疫
        case ObstacleType.Slowing:
            return IsSkillActive("Dash");  // Dash技能激活时免疫
    }
}
```

## 快速开始

### 1. 系统设置

在 `AdvancedAutoRunnerController` 组件的 Inspector 中：

```
障碍物效果系统:
  ✓ Enable Obstacle System         // 启用障碍物系统
  - Stumble Effect Duration: 2.0    // 踉跄效果持续时间
  - Slowing Effect Duration: 3.0    // 减速效果持续时间
  - Slowing Speed Multiplier: 0.5   // 减速时的速度倍数
  - Stumble Animation Trigger: "Stumble" // 踉跄动画触发器
  - Obstacle Hit Sound: [音效文件]       // 碰撞音效
  - Obstacle Detection Distance: 0.5     // 障碍物检测距离
  - Obstacle Detection Radius: 0.3       // 障碍物检测半径
  - Obstacle Layer Mask: [层级遮罩]      // 障碍物图层遮罩
```

### 2. 障碍物设置

确保障碍物 GameObject 包含：
1. `Obstacles` 组件，配置 `obstacleTypes` 列表
2. `Collider` 组件（普通 Collider，不需要是 Trigger）
3. 正确的 Layer 设置（对应检测遮罩）

**重要**：无需 Rigidbody 组件，系统使用射线检测！

### 3. 动画设置

在角色的 Animator Controller 中添加：
- **Stumble** (Trigger): 踉跄动画触发器

## 工作原理

### 射线检测流程（无需 Rigidbody）

1. 每帧主动检测前方的障碍物
2. 使用 `Physics.SphereCastAll` 进行球形射线检测
3. 检测范围：前方 `obstacleDetectionDistance` 距离，半径 `obstacleDetectionRadius`
4. 自动去重，防止重复触发
5. 根据障碍物类型触发相应效果

### 效果处理逻辑

```csharp
// 踉跄效果（Low/High）
if (obstacleType == Low || obstacleType == High)
{
    currentSpeed = startSpeed;           // 速度降到最低
    触发 "Stumble" 动画;
    持续 stumbleEffectDuration 秒;
}

// 减速效果（Slowing）
if (obstacleType == Slowing)
{
    currentSpeed = preObstacleSpeed * slowingSpeedMultiplier;  // 速度减半
    持续 slowingEffectDuration 秒;
}

// 效果结束后
重新开始加速阶段;
currentState = RunningState.Accelerating;
```

## 代码使用示例

### 手动触发障碍物效果

```csharp
// 获取控制器引用
AdvancedAutoRunnerController controller = GetComponent<AdvancedAutoRunnerController>();

// 手动触发踉跄效果
controller.StartObstacleEffect(ObstacleType.Low);

// 手动触发减速效果
controller.StartObstacleEffect(ObstacleType.Slowing);
```

### 检查障碍物效果状态

```csharp
// 检查是否处于障碍物效果中
bool inEffect = controller.IsInObstacleEffect();

// 获取当前效果类型
ObstacleEffectType currentEffect = controller.GetCurrentObstacleEffect();

// 获取剩余时间
float remainingTime = controller.GetObstacleEffectRemainingTime();
```

### 检查技能免疫状态

```csharp
// 检查对特定障碍物类型是否免疫
bool immuneToLow = controller.IsImmuneToObstacleType(ObstacleType.Low);
bool immuneToHigh = controller.IsImmuneToObstacleType(ObstacleType.High);
bool immuneToSlowing = controller.IsImmuneToObstacleType(ObstacleType.Slowing);

// 获取当前激活的免疫技能列表
var immunitySkills = controller.GetActiveImmunitySkills();
foreach (string skill in immunitySkills)
{
    Debug.Log($"激活的免疫技能: {skill}");
}

// 检查是否有任何免疫效果激活
bool hasAnyImmunity = controller.HasAnyImmunity();

// 获取对特定障碍物提供免疫的技能名称
string immunitySkill = controller.GetImmunitySkillForObstacle(ObstacleType.Low);
if (immunitySkill != null)
{
    Debug.Log($"对Low障碍物提供免疫的技能: {immunitySkill}");
}
```

### 实际使用场景示例

```csharp
// 障碍物碰撞处理示例
void OnObstacleDetected(ObstacleType obstacleType)
{
    // 检查是否已在效果中
    if (controller.IsInObstacleEffect())
    {
        Debug.Log("已有障碍物效果进行中，忽略新碰撞");
        return;
    }
    
    // 检查技能免疫
    if (controller.IsImmuneToObstacleType(obstacleType))
    {
        string immunitySkill = controller.GetImmunitySkillForObstacle(obstacleType);
        Debug.Log($"技能 {immunitySkill} 提供免疫，忽略 {obstacleType} 障碍物");
        
        // 可以播放免疫效果
        PlayImmunityEffect();
        return;
    }
    
    // 正常处理障碍物效果
    controller.StartObstacleEffect(obstacleType);
}

// 技能使用策略示例
void UseSkillStrategically()
{
    // 检测前方障碍物类型，预先激活对应技能
    ObstacleType upcomingObstacle = DetectUpcomingObstacle();
    
    switch (upcomingObstacle)
    {
        case ObstacleType.Low:
            controller.TryActivateSkill("Jump");
            break;
        case ObstacleType.High:
            controller.TryActivateSkill("Roll");
            break;
        case ObstacleType.Slowing:
            controller.TryActivateSkill("Dash");
            break;
    }
}
```

## 障碍物预制体配置

### Low 障碍物示例（无需 Rigidbody）

```
Low Obstacle GameObject:
  - Obstacles (Script)
    - Obstacle Types: [Low]
  - Box Collider (普通 Collider，非 Trigger)
  - Layer: Obstacles
  - Mesh Renderer
  - Mesh Filter
```

### High 障碍物示例

```
High Obstacle GameObject:
  - Obstacles (Script)
    - Obstacle Types: [High]
  - Box Collider (普通 Collider，非 Trigger)
  - Layer: Obstacles
  - Mesh Renderer
  - Mesh Filter
```

### Slowing 障碍物示例

```
Slowing Obstacle GameObject:
  - Obstacles (Script)
    - Obstacle Types: [Slowing]
  - Box Collider (普通 Collider，非 Trigger)
  - Layer: Obstacles
  - Mesh Renderer
  - Mesh Filter
```

## 调试功能

### 1. GUI 调试信息

运行时会在屏幕左上角显示：
```
=== 障碍物系统 ===
当前效果: Stumble
剩余时间: 1.2秒
效果前速度: 8.50

=== 技能免疫状态 ===
🛡️ Jump: 免疫Low障碍物
🛡️ Roll: 免疫High障碍物
无技能免疫
```

### 2. 调试按钮

GUI 中提供的测试按钮：
- **测试踉跄效果**: 立即触发踉跄效果
- **测试减速效果**: 立即触发减速效果
- **结束障碍物效果**: 强制结束当前效果

### 3. Scene 视图可视化

在 Scene 视图中会显示：
- **橙色射线**: 检测射线方向和距离
- **橙色线框球体**: 检测范围
- **红色线框**: 已检测到的障碍物
- **紫色指示器**: 踉跄效果状态
- **青色指示器**: 减速效果状态
- **进度圆环**: 效果剩余时间
- **技能免疫护盾**: 
  - **绿色球体**: Jump技能激活（免疫Low障碍物）
  - **蓝色球体**: Roll技能激活（免疫High障碍物）
  - **黄色球体**: Dash技能激活（免疫Slowing障碍物）
  - **白色大护盾**: 总体免疫状态

### 4. 高级测试器组件

使用 `ObstacleEffectTester` 组件进行全面测试：

**基础障碍物测试：**
- **T 键**: 触发Low障碍物效果
- **Y 键**: 触发High障碍物效果  
- **U 键**: 触发Slowing障碍物效果
- **I 键**: 强制结束当前效果

**技能激活测试：**
- **J 键**: 激活Jump技能
- **K 键**: 激活Roll技能
- **L 键**: 激活Dash技能

**免疫组合测试：**
- **1 键**: Jump技能 + Low障碍物（应该免疫）
- **2 键**: Roll技能 + High障碍物（应该免疫）
- **3 键**: Dash技能 + Slowing障碍物（应该免疫）
- **4 键**: 重复触发测试（第二个效果应该被阻塞）

**实时状态显示：**
在屏幕右上角显示当前的障碍物效果状态和激活的免疫技能。

## 参数配置

### 基础参数

| 参数 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| Enable Obstacle System | bool | true | 是否启用障碍物系统 |
| Stumble Effect Duration | float | 2.0 | 踉跄效果持续时间（秒） |
| Slowing Effect Duration | float | 3.0 | 减速效果持续时间（秒） |
| Slowing Speed Multiplier | float | 0.5 | 减速时的速度倍数 |
| Stumble Animation Trigger | string | "Stumble" | 踉跄动画触发器名称 |
| Obstacle Hit Sound | AudioClip | null | 障碍物碰撞音效 |
| Obstacle Detection Distance | float | 0.5 | 障碍物检测距离 |
| Obstacle Detection Radius | float | 0.3 | 障碍物检测半径 |
| Obstacle Layer Mask | LayerMask | 0 | 障碍物图层遮罩 |

### 参数限制

- `Stumble Effect Duration`: 最小值 0.1 秒
- `Slowing Effect Duration`: 最小值 0.1 秒
- `Slowing Speed Multiplier`: 范围 0.1 - 1.0
- `Obstacle Detection Distance`: 最小值 0.1 米
- `Obstacle Detection Radius`: 最小值 0.05 米

## 高级功能

### 1. 射线检测优化

检测系统具有以下优化特性：
- **去重机制**: 已检测过的障碍物不会重复触发
- **距离清理**: 远离的障碍物自动从记录中移除
- **球形检测**: 使用 SphereCast 提供更好的检测覆盖

### 2. 层级遮罩配置

```csharp
// 在Inspector中设置Obstacle Layer Mask
// 只检测特定层级的障碍物，提高性能
obstacleLayerMask = LayerMask.GetMask("Obstacles", "Environment");
```

### 3. 效果优先级

多个障碍物同时触发时：
- 按照碰撞顺序处理
- 后续碰撞会重置当前效果

### 4. 系统控制

```csharp
// 启用/禁用障碍物系统
controller.SetObstacleSystemEnabled(false);

// 强制结束效果（调试用）
controller.ForceEndObstacleEffect();
```

## 性能优化

### 1. 碰撞检测优化

- 使用 Trigger Collider 而不是物理碰撞
- 合理设置碰撞层级 Layer Mask
- 避免过多的障碍物重叠

### 2. 音效优化

- 使用 PlayOneShot 避免重复播放
- 控制音效音量避免过响
- 考虑使用对象池管理音效

### 3. 动画优化

- 使用 Trigger 参数避免状态机复杂化
- 合理设置动画 Transition 时间
- 避免在动画中使用过多的事件

## 扩展开发

### 添加新的障碍物类型

1. 在 `ObstacleType` 枚举中添加新类型：
```csharp
public enum ObstacleType
{
    Low,
    High,
    Slowing,
    NewType    // 新增类型
}
```

2. 在 `StartObstacleEffect` 方法中添加处理逻辑：
```csharp
case ObstacleType.NewType:
    StartNewTypeEffect();
    break;
```

3. 实现对应的效果方法：
```csharp
private void StartNewTypeEffect()
{
    isInObstacleEffect = true;
    currentObstacleEffect = ObstacleEffectType.NewType;
    // 自定义效果逻辑
}
```

### 自定义效果处理

```csharp
// 在 UpdateObstacleEffectSpeed 中添加新的速度逻辑
case ObstacleEffectType.NewType:
    // 自定义速度计算
    currentSpeed = CalculateCustomSpeed();
    break;
```

## 常见问题

### Q: 障碍物没有触发效果？
A: 检查以下项目：
1. `Enable Obstacle System` 是否勾选
2. 障碍物是否有 `Obstacles` 组件
3. 障碍物 Layer 是否在 `Obstacle Layer Mask` 中
4. 检测距离和半径设置是否合适
5. 障碍物是否在角色前方的检测范围内

### Q: 检测距离太短或太长？
A: 调整以下参数：
1. `Obstacle Detection Distance`: 检测射线长度
2. `Obstacle Detection Radius`: 检测球体半径
3. 在 Scene 视图中观察橙色检测范围可视化

### Q: 障碍物重复触发效果？
A: 系统有去重机制，如果仍然重复：
1. 检查障碍物是否有重叠的 Collider
2. 确认障碍物移动速度不会导致快速进出检测范围
3. 调整检测距离避免过早触发

### Q: 性能问题（检测卡顿）？
A: 优化建议：
1. 合理设置 `Obstacle Layer Mask`，只检测必要的层级
2. 减少检测距离和半径
3. 确保障碍物数量不过多
4. 使用简单的 Collider 形状（Box、Sphere）

### Q: 踉跄动画没有播放？
A: 确保：
1. Animator Controller 中有 "Stumble" 参数
2. 参数类型设置为 Trigger
3. 动画状态机有对应的 Transition

### Q: 音效没有播放？
A: 检查：
1. AudioSource 组件是否存在
2. Obstacle Hit Sound 是否已设置
3. AudioSource 是否启用
4. 游戏音量设置是否正确

### Q: 效果持续时间不正确？
A: 验证：
1. Duration 参数设置
2. 是否有重复检测导致时间重置
3. 系统时间是否正常

### Q: 无法在没有 Rigidbody 的情况下工作？
A: 新系统完全支持无 Rigidbody：
1. 确保障碍物有普通 Collider（非 Trigger）
2. 设置正确的 Layer 和 Layer Mask
3. 调整检测参数以适应场景
4. 使用 Scene 视图查看检测范围可视化

### Q: 为什么障碍物效果期间其他障碍物不生效？
A: 这是设计特性（阻塞机制）：
1. 防止效果重叠造成混乱
2. 确保玩家体验一致性
3. 如需修改，可以在 `StartObstacleEffect` 方法中调整逻辑

### Q: 技能免疫不起作用？
A: 检查以下项目：
1. 技能系统是否已启用（`Enable Skill System`）
2. 对应技能是否正确激活
3. 技能名称是否正确（"Jump", "Roll", "Dash"）
4. 技能是否仍在持续时间内
5. 查看 Console 输出确认免疫日志

### Q: 如何自定义免疫规则？
A: 修改 `IsImmuneToObstacle` 方法：
```csharp
private bool IsImmuneToObstacle(ObstacleType obstacleType)
{
    switch (obstacleType)
    {
        case ObstacleType.Low:
            // 自定义免疫条件
            return IsSkillActive("Jump") || IsSkillActive("CustomSkill");
        // ... 其他条件
    }
}
```

### Q: 想要禁用阻塞机制怎么办？
A: 注释掉 `StartObstacleEffect` 中的阻塞检查：
```csharp
// 注释掉这部分代码即可允许效果重叠
/*
if (isInObstacleEffect)
{
    return;
}
*/
```

### Q: 如何添加新的免疫技能？
A: 在 `IsImmuneToObstacle` 方法中添加新的技能检查：
```csharp
case ObstacleType.Low:
    return IsSkillActive("Jump") || IsSkillActive("NewSkill");
```
同时更新 GUI 显示和文档。

### Q: 免疫状态的可视化效果不显示？
A: 确保：
1. `Show Debug Info` 已勾选
2. 技能确实处于激活状态
3. Scene 视图中选中了角色对象
4. 可以通过 Console 查看免疫日志确认

### Q: 可以同时激活多个免疫技能吗？
A: 可以，系统支持同时激活多个技能：
- 每个技能提供独立的免疫效果
- GUI 会显示所有激活的免疫技能
- Scene 视图会同时显示多个护盾效果

## 总结

障碍物效果系统提供了完整的障碍物交互功能：

- ✅ 三种障碍物类型支持（Low, High, Slowing）
- ✅ 自动碰撞检测和效果触发
- ✅ 动画集成（踉跄效果）
- ✅ 音效支持
- ✅ 完整的调试功能
- ✅ Scene 视图可视化
- ✅ 参数验证和限制
- ✅ 性能优化设计
- ✅ 扩展性架构
- ✅ **障碍物效果阻塞机制**（防止重叠）
- ✅ **技能免疫系统**（Jump/Roll/Dash 对应免疫）
- ✅ **高级调试工具**（免疫状态可视化）
- ✅ **无 Rigidbody 支持**（射线检测系统）

系统设计简洁高效，易于使用和扩展，为跑酷游戏提供了丰富的障碍物交互体验。

### 核心优势

1. **双重保护机制**: 
   - 障碍物效果期间阻塞新效果
   - 技能激活期间提供免疫保护

2. **智能免疫系统**:
   - Jump 技能免疫 Low 障碍物
   - Roll 技能免疫 High 障碍物  
   - Dash 技能免疫 Slowing 障碍物

3. **完善的反馈系统**:
   - 实时 GUI 状态显示
   - Scene 视图免疫护盾可视化
   - Console 详细日志输出

4. **高度可扩展**:
   - 易于添加新的障碍物类型
   - 灵活的免疫规则配置
   - 模块化设计便于维护 