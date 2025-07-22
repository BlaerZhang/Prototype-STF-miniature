# 抽奖转盘系统 (Spin Wheel System)

一个完全解耦的Unity抽奖转盘系统，支持权重配置、动画效果和自定义UI。

## 功能特性

- ✅ 基于权重的概率抽奖
- ✅ 使用OdinInspector的SerializedDictionary配置奖池
- ✅ 基于UGUI的转盘界面
- ✅ 使用DOTween的平滑旋转动画
- ✅ 支持自定义奖项显示预制体
- ✅ 完全解耦的设计，易于集成
- ✅ Action回调机制

## 快速开始

### 1. 创建奖池配置

1. 在Project窗口右键 → Create → Spin Wheel → Prize Pool
2. 配置奖池：
   - 设置奖池名称
   - 添加奖项（名称、图标、权重）
   - 点击"计算权重分布"按钮

### 2. 设置转盘UI

1. 创建一个Canvas和UI GameObject
2. 添加以下组件到转盘GameObject：
   - `SpinWheelController`
   - `SpinWheelUI`
3. 在SpinWheelUI中配置：
   - 转盘背景图片（圆形Sprite）
   - 转盘容器（旋转的部分）
   - 指针图片
   - 奖项显示预制体（可选）

### 3. 基本使用代码

```csharp
using SpinWheel;

public class MyGameManager : MonoBehaviour
{
    [SerializeField] private SpinWheelController spinWheel;
    [SerializeField] private SpinWheelPrizePool prizePool;
    
    void Start()
    {
        // 订阅事件
        spinWheel.OnSpinComplete += OnPrizeWon;
    }
    
    public void StartLottery()
    {
        // 开始抽奖
        spinWheel.StartSpin(prizePool);
    }
    
    private void OnPrizeWon(PrizeItem prize)
    {
        Debug.Log($"获得奖品: {prize.prizeName}");
        // 处理中奖逻辑
    }
}
```

## 组件说明

### SpinWheelPrizePool (ScriptableObject)
奖池配置文件，包含所有奖项信息和权重。

**主要属性：**
- `poolName`: 奖池名称
- `prizes`: 奖项字典 (使用SerializedDictionary)

**主要方法：**
- `CalculateWeightDistribution()`: 计算权重分布
- `GetRandomPrize()`: 根据权重随机获取奖项
- `GetAngleForPrize()`: 获取奖项对应的角度

### SpinWheelController
转盘主控制器，处理抽奖逻辑和动画。

**主要属性：**
- `spinDuration`: 转盘旋转时间
- `minSpinRotations/maxSpinRotations`: 旋转圈数范围

**主要方法：**
- `StartSpin(prizePool)`: 开始抽奖
- `StopSpin()`: 停止旋转

**事件回调：**
- `OnSpinStart`: 开始旋转时触发
- `OnSpinComplete`: 抽奖完成时触发

### SpinWheelUI
转盘UI管理器，处理视觉显示。

**主要属性：**
- `wheelBackground`: 转盘背景图片
- `wheelContainer`: 转盘容器（旋转部分）
- `pointer`: 指针图片
- `prizeDisplayPrefab`: 奖项显示预制体

### PrizeDisplayComponent
奖项显示组件，用于自定义奖项显示预制体。

**接口实现：**
- 实现`IPrizeDisplay`接口
- `SetPrizeInfo(prize)`: 设置奖项信息

## 自定义奖项显示

### 方法1：使用PrizeDisplayComponent
1. 创建一个GameObject作为预制体
2. 添加`PrizeDisplayComponent`组件
3. 配置Text和Image组件引用
4. 将预制体赋值给SpinWheelUI

### 方法2：实现IPrizeDisplay接口
```csharp
public class CustomPrizeDisplay : MonoBehaviour, IPrizeDisplay
{
    public void SetPrizeInfo(PrizeItem prize)
    {
        // 自定义显示逻辑
    }
}
```

## 高级配置

### 转盘动画设置
- `spinDuration`: 控制旋转时间
- `minSpinRotations/maxSpinRotations`: 控制旋转圈数
- 使用`Ease.OutQuart`缓动效果模拟真实转盘

### UI布局设置
- `wheelRadius`: 转盘半径
- `prizeDisplayRadius`: 奖项显示半径
- `divisionLineColor/Width`: 分割线样式

## 注意事项

1. **权重计算**：权重会自动归一化，无需手动计算百分比
2. **角度计算**：系统自动根据权重计算每个奖项的角度范围
3. **UI层级**：确保转盘在正确的Canvas和Sorting Order
4. **性能考虑**：LineRenderer用于分割线，数量过多时注意性能
5. **DOTween依赖**：确保项目中已导入DOTween

## 示例场景

参考`SpinWheelExample.cs`了解完整的使用示例，包括：
- UI按钮交互
- 事件处理
- 多奖池切换
- 结果显示

## 故障排除

### 常见问题：
1. **转盘不显示**：检查wheelContainer是否正确设置
2. **分割线不显示**：确认LineRenderer材质和颜色设置
3. **奖项位置错误**：检查prizeDisplayRadius设置
4. **动画不流畅**：调整spinDuration和缓动曲线

### 调试技巧：
- 使用Inspector查看权重分布计算结果
- 开启Console查看抽奖日志
- 使用编辑器测试按钮快速验证功能 