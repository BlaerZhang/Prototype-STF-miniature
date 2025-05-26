# 属性点系统使用说明

## 概述
这个简易的加点系统包含两个主要组件：
- `AttributePointSystem`: 核心数据管理器，处理属性值的逻辑
- `AttributeUIController`: UI控制器，连接界面与数据系统

## 功能特性
- 四种属性：加速、刹车、视野、转向
- 每个属性值为整数，范围：1-10（可配置）
- 支持UI按钮 (+/-) 调节数值
- 实时UI更新和状态管理
- 可选的总点数限制功能
- 音效支持
- 按钮状态视觉反馈

## 使用步骤

### 1. 创建属性系统GameObject
```
1. 在场景中创建一个空的GameObject，命名为"AttributeSystem"
2. 添加AttributePointSystem组件
3. 在Inspector中配置各属性的最大值、最小值等参数
```

### 2. 设置UI
```
1. 创建UI Canvas（如果还没有）
2. 为每个属性创建UI组，包含：
   - 属性名称文本 (TextMeshPro)
   - 数值显示文本 (TextMeshPro)
   - 增加按钮 (+)
   - 减少按钮 (-)
   - 可选：进度条 (Slider)
```

### 3. 配置UIController
```
1. 在某个GameObject上添加AttributeUIController组件
2. 将AttributePointSystem引用拖入相应字段
3. 配置attributeUIGroups列表：
   - 为每个属性创建一个AttributeUIGroup
   - 设置attributeName（"加速"、"刹车"、"视野"、"转向"）
   - 拖入对应的UI组件引用
```

## 脚本接口

### AttributePointSystem 主要方法
```csharp
// 获取属性值
int GetAccelerationValue()
int GetBrakingValue()
int GetVisionValue()
int GetSteeringValue()

// 修改属性值
bool IncreaseAcceleration()
bool DecreaseAcceleration()
// ... 其他属性类似

// 其他功能
void ResetAllAttributes()
int GetRemainingPoints()
bool HasRemainingPoints()
```

### 事件监听
```csharp
// 监听属性变化
AttributePointSystem.OnAttributeChanged += (attributeName, newValue) => {
    Debug.Log($"{attributeName} 变更为: {newValue}");
};

// 监听总点数变化
AttributePointSystem.OnTotalPointsChanged += (current, max) => {
    Debug.Log($"点数: {current}/{max}");
};
```

## 配置选项

### AttributePointSystem 配置
- **属性最大值/最小值**: 在Inspector中调整每个属性的范围
- **总点数限制**: 勾选`useTotalPointsLimit`并设置`totalPointsLimit`来启用点数限制

### AttributeUIController 配置
- **按钮颜色**: 设置正常和禁用状态的按钮颜色
- **音效**: 添加AudioSource和音效文件
- **总点数显示**: 可选择显示已用点数和剩余点数

## 扩展建议

### 1. 与游戏系统集成
将属性值应用到游戏中：
```csharp
// 在其他脚本中获取属性值
AttributePointSystem attrSystem = FindFirstObjectByType<AttributePointSystem>();
float speedMultiplier = attrSystem.GetAccelerationValue() * 0.1f;
```

### 2. 数据持久化
添加保存/加载功能：
```csharp
// 保存数据到PlayerPrefs
PlayerPrefs.SetInt("Acceleration", GetAccelerationValue());

// 加载数据
int savedAcceleration = PlayerPrefs.GetInt("Acceleration", 1);
```

### 3. 更多UI元素
- 添加属性描述文本
- 添加属性图标
- 添加动画效果
- 添加确认对话框

## 注意事项
1. 确保UI组件使用TextMeshPro而不是legacy Text组件
2. 按钮的onClick事件会自动设置，无需手动配置
3. AttributeName字段支持中文和英文，建议统一使用一种
4. 如果不需要总点数限制，保持`useTotalPointsLimit`为false即可 