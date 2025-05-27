# Auto Runner Linear 自动跑酷控制器

这是一套为Unity 3D项目设计的简易自动跑酷控制器系统，玩家无需任何输入，角色会自动在Z轴方向前进。

## 功能特性

### 核心功能
- ✅ **自动前进**: 角色自动在Z轴方向移动，无需玩家输入
- ✅ **起步速度**: 可设置角色开始移动的最低速度
- ✅ **加速时间**: 可设置从起步速度到最高速度所需的时间
- ✅ **最高速度**: 可设置角色能达到的最高移动速度
- ✅ **平滑加速**: 支持线性插值和动画曲线加速

### 高级功能（AdvancedAutoRunnerController）
- 🎬 **动画控制**: 自动控制角色跑步动画
- 🔊 **音效系统**: 支持脚步声、开始音效、达到最高速度音效
- ✨ **粒子效果**: 支持加速、最高速度、灰尘等粒子效果
- 📷 **相机跟随**: 自动相机跟随功能
- 📊 **事件系统**: 提供开始、停止、达到最高速度等事件
- 🎮 **UI集成**: 内置速度显示UI

## 脚本说明

### 1. AutoRunnerLinearController.cs
基础版本的自动跑酷控制器，提供核心的自动移动功能。

**主要属性:**
- `startSpeed`: 起步速度（最低速度）
- `accelerationTime`: 加速时间
- `maxSpeed`: 最高速度
- `autoStart`: 是否自动开始
- `useRigidbody`: 是否使用Rigidbody物理移动

**主要方法:**
- `StartRunning()`: 开始跑酷
- `StopRunning()`: 停止跑酷
- `PauseRunning()`: 暂停跑酷
- `ResumeRunning()`: 恢复跑酷
- `ResetRunning()`: 重置跑酷

### 2. AdvancedAutoRunnerController.cs
高级版本的自动跑酷控制器，包含完整的音效、动画、粒子效果等功能。

**额外功能:**
- 动画控制（需要Animator组件）
- 音效播放（需要AudioSource组件）
- 粒子效果控制（需要ParticleSystem组件）
- 相机跟随
- 事件回调系统

### 3. AutoRunnerExample.cs
使用示例脚本，展示如何使用控制器并提供UI控制界面。

## 使用方法

### 基础设置

1. **添加控制器脚本**
   ```csharp
   // 将AutoRunnerLinearController或AdvancedAutoRunnerController添加到角色GameObject上
   ```

2. **配置移动组件**
   - 推荐使用Rigidbody进行物理移动
   - 也支持CharacterController或Transform移动

3. **设置参数**
   ```csharp
   startSpeed = 2f;        // 起步速度
   accelerationTime = 3f;  // 加速时间（秒）
   maxSpeed = 10f;         // 最高速度
   ```

### 高级设置（AdvancedAutoRunnerController）

1. **动画设置**
   - 添加Animator组件
   - 设置Speed和IsRunning参数
   - 配置跑步动画

2. **音效设置**
   - 添加AudioSource组件
   - 设置脚步声、开始音效等AudioClip

3. **粒子效果**
   - 添加ParticleSystem组件
   - 配置加速、最高速度、灰尘等效果

4. **相机跟随**
   - 启用enableCameraFollow
   - 设置cameraOffset偏移量

### 代码示例

```csharp
// 获取控制器组件
AutoRunnerLinearController controller = GetComponent<AutoRunnerLinearController>();

// 开始跑酷
controller.StartRunning();

// 设置参数
controller.SetStartSpeed(3f);
controller.SetMaxSpeed(15f);
controller.SetAccelerationTime(2f);

// 获取当前状态
float currentSpeed = controller.GetCurrentSpeed();
var state = controller.GetCurrentState();
float progress = controller.GetAccelerationProgress();
```

### 事件监听（高级版本）

```csharp
AdvancedAutoRunnerController advancedController = GetComponent<AdvancedAutoRunnerController>();

// 订阅事件
advancedController.OnStartRunning += () => Debug.Log("开始跑酷!");
advancedController.OnReachMaxSpeed += () => Debug.Log("达到最高速度!");
advancedController.OnStopRunning += () => Debug.Log("停止跑酷!");
advancedController.OnSpeedChanged += (speed) => Debug.Log($"速度变化: {speed}");
```

## 控制说明

### 键盘控制（AutoRunnerExample）
- **空格键**: 开始跑酷
- **S键**: 停止跑酷
- **R键**: 重置跑酷
- **1-4键**: 快速设置速度（5, 10, 15, 20）

### 运行状态
- **Stopped**: 停止状态
- **Accelerating**: 加速阶段
- **MaxSpeed**: 最高速度阶段

## 技术细节

### 移动方式
1. **Rigidbody移动** (推荐)
   - 使用`rb.MovePosition()`进行物理移动
   - 支持碰撞检测和物理交互

2. **CharacterController移动**
   - 使用`characterController.Move()`
   - 适合角色控制器

3. **Transform移动**
   - 直接修改Transform.position
   - 最简单但不支持物理交互

### 坐标系统
- 默认在世界坐标系的Z轴正方向移动
- 可通过`constrainToZAxis`参数控制是否限制在Z轴
- 支持本地坐标系移动

### 性能优化
- 使用OverridableMonoBehaviour基类（JoostenProductions）
- 合理的Update频率控制
- 可选的调试信息显示

## 依赖项

- **Unity 2021.3+**
- **JoostenProductions.OverridableMonoBehaviour** (项目中已包含)

## 注意事项

1. **组件依赖**: 确保GameObject上有适当的移动组件（Rigidbody/CharacterController）
2. **参数验证**: 所有参数都有合理性检查，防止无效值
3. **事件清理**: 记得在OnDestroy中取消事件订阅
4. **性能考虑**: 大量对象时建议关闭不必要的调试功能

## 扩展建议

1. **障碍物检测**: 可以添加前方障碍物检测和自动避障
2. **路径跟随**: 结合Spline系统实现路径跟随
3. **速度变化**: 添加不同地形的速度影响
4. **能量系统**: 添加体力/能量消耗机制
5. **多人同步**: 支持网络多人游戏

## 版本历史

- **v1.0**: 基础自动跑酷功能
- **v1.1**: 添加高级功能（动画、音效、粒子效果）
- **v1.2**: 添加相机跟随和事件系统

## 支持

如有问题或建议，请查看项目文档或联系开发团队。 