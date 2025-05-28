using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public class AttributeUIGroup
{
    [Header("UI组件")]
    public string attributeName;        // 属性名称（用于标识）
    public Button increaseButton;       // +按钮
    public Button decreaseButton;       // -按钮
    public TextMeshProUGUI valueText;   // 数值显示文本
    public TextMeshProUGUI nameText;    // 属性名称文本（可选）
    public Slider valueSlider;          // 进度条（可选）
    
    [Header("按钮状态设置")]
    public Color normalButtonColor = Color.white;
    public Color disabledButtonColor = Color.gray;
    
    // 跟踪是否已为此组设置了监听器
    [System.NonSerialized]
    public bool hasSetupListeners = false;
    
    // 更新UI显示
    public void UpdateUI(int value, int minValue, int maxValue, bool canIncrease, bool canDecrease)
    {
        // 更新数值文本
        if (valueText != null)
        {
            valueText.text = value.ToString();
        }
        
        // 更新进度条
        if (valueSlider != null)
        {
            valueSlider.minValue = minValue;
            valueSlider.maxValue = maxValue;
            valueSlider.value = value;
        }
        
        // 更新按钮状态
        UpdateButtonState(increaseButton, canIncrease);
        UpdateButtonState(decreaseButton, canDecrease);
    }
    
    private void UpdateButtonState(Button button, bool interactable)
    {
        if (button != null)
        {
            button.interactable = interactable;
            
            // 更改按钮颜色以提供视觉反馈
            Image buttonImage = button.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = interactable ? normalButtonColor : disabledButtonColor;
            }
        }
    }
}

public class AttributeUIController : MonoBehaviour
{
    [Header("属性点系统引用")]
    [SerializeField] private AttributePointSystem attributeSystem;
    
    [Header("UI组件")]
    [SerializeField] private List<AttributeUIGroup> attributeUIGroups = new List<AttributeUIGroup>();
    
    [Header("总点数显示（可选）")]
    [SerializeField] private TextMeshProUGUI totalPointsText;
    [SerializeField] private TextMeshProUGUI remainingPointsText;
    [SerializeField] private Button resetButton;
    
    [Header("音效设置（可选）")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip increaseSound;
    [SerializeField] private AudioClip decreaseSound;
    [SerializeField] private AudioClip errorSound;
    
    [Header("动态UI创建（可选）")]
    [SerializeField] private bool autoCreateUIForAttributes = false;
    [SerializeField] private GameObject attributeUIPrefab;
    [SerializeField] private Transform attributeUIContainer;
    
    [Header("调试")]
    [SerializeField] private bool logDebugInfo = false;
    
    // UI组件字典，用于快速查找
    private Dictionary<string, AttributeUIGroup> uiGroupDict = new Dictionary<string, AttributeUIGroup>();
    
    private void Start()
    {
        // 如果没有手动指定属性系统，尝试自动查找
        if (attributeSystem == null)
        {
            attributeSystem = FindFirstObjectByType<AttributePointSystem>();
            if (attributeSystem == null)
            {
                Debug.LogError("AttributeUIController: 未找到AttributePointSystem组件！");
                return;
            }
        }
        
        InitializeUI();
        SetupEventListeners();
        
        // 初始化UI显示
        UpdateAllAttributeUI();
        
        if (logDebugInfo)
        {
            Debug.Log($"AttributeUIController: 初始化完成，UI组数量: {attributeUIGroups.Count}");
        }
    }
    
    private void OnEnable()
    {
        // 订阅属性变化事件
        AttributePointSystem.OnAttributeChanged += OnAttributeValueChanged;
        AttributePointSystem.OnTotalPointsChanged += OnTotalPointsChanged;
    }
    
    private void OnDisable()
    {
        // 取消订阅事件
        AttributePointSystem.OnAttributeChanged -= OnAttributeValueChanged;
        AttributePointSystem.OnTotalPointsChanged -= OnTotalPointsChanged;
    }
    
    private void InitializeUI()
    {
        // 建立UI组件字典
        uiGroupDict.Clear();
        foreach (var group in attributeUIGroups)
        {
            if (!string.IsNullOrEmpty(group.attributeName))
            {
                uiGroupDict[group.attributeName.ToLower()] = group;
                
                // 设置属性名称文本
                if (group.nameText != null && string.IsNullOrEmpty(group.nameText.text))
                {
                    group.nameText.text = group.attributeName;
                }
            }
        }
        
        // 自动创建所有属性的UI（如果启用）
        if (autoCreateUIForAttributes && attributeUIPrefab != null && attributeUIContainer != null)
        {
            CreateUIForAttributes();
        }
    }
    
    private void CreateUIForAttributes()
    {
        // 获取所有属性
        var attributes = attributeSystem.GetAllAttributes();
        
        // 检查每个属性是否已有UI
        foreach (var attribute in attributes)
        {
            if (!uiGroupDict.ContainsKey(attribute.attributeName.ToLower()))
            {
                // 为没有UI的属性创建UI
                CreateUIForAttribute(attribute);
            }
        }
    }
    
    private void CreateUIForAttribute(AttributeData attribute)
    {
        // 实例化UI预制体
        GameObject uiObject = Instantiate(attributeUIPrefab, attributeUIContainer);
        uiObject.name = $"UI_{attribute.attributeName}";
        
        // 查找并配置UI组件
        AttributeUIGroup newGroup = new AttributeUIGroup();
        newGroup.attributeName = attribute.attributeName;
        
        // 查找按钮和文本组件
        newGroup.increaseButton = uiObject.transform.Find("IncreaseButton")?.GetComponent<Button>();
        newGroup.decreaseButton = uiObject.transform.Find("DecreaseButton")?.GetComponent<Button>();
        newGroup.valueText = uiObject.transform.Find("ValueText")?.GetComponent<TextMeshProUGUI>();
        newGroup.nameText = uiObject.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
        newGroup.valueSlider = uiObject.transform.Find("ValueSlider")?.GetComponent<Slider>();
        
        // 设置名称文本
        if (newGroup.nameText != null)
        {
            newGroup.nameText.text = attribute.attributeName;
        }
        
        // 添加到列表和字典
        attributeUIGroups.Add(newGroup);
        uiGroupDict[attribute.attributeName.ToLower()] = newGroup;
        
        // 标记为未设置监听器，让SetupEventListeners处理
        newGroup.hasSetupListeners = false;
        
        // 更新UI显示
        UpdateAttributeUI(attribute.attributeName, newGroup);
        
        if (logDebugInfo)
        {
            Debug.Log($"AttributeUIController: 为属性 {attribute.attributeName} 创建UI成功");
        }
    }
    
    private void SetupEventListeners()
    {
        // 为每个UI组设置按钮事件
        foreach (var group in attributeUIGroups)
        {
            // 如果已经设置过监听器，则跳过
            if (group.hasSetupListeners)
                continue;
                
            string attributeName = group.attributeName;
            
            // +按钮事件
            if (group.increaseButton != null)
            {
                // 先移除所有已有的监听器，确保不会重复
                group.increaseButton.onClick.RemoveAllListeners();
                group.increaseButton.onClick.AddListener(() => OnIncreaseButtonClicked(attributeName));
            }
            
            // -按钮事件
            if (group.decreaseButton != null)
            {
                // 先移除所有已有的监听器，确保不会重复
                group.decreaseButton.onClick.RemoveAllListeners();
                group.decreaseButton.onClick.AddListener(() => OnDecreaseButtonClicked(attributeName));
            }
            
            // 标记为已设置监听器
            group.hasSetupListeners = true;
            
            if (logDebugInfo)
            {
                Debug.Log($"AttributeUIController: 为属性 {attributeName} 设置按钮事件");
            }
        }
        
        // 重置按钮事件
        if (resetButton != null)
        {
            resetButton.onClick.RemoveAllListeners();
            resetButton.onClick.AddListener(OnResetButtonClicked);
        }
    }
    
    #region 按钮点击事件
    private void OnIncreaseButtonClicked(string attributeName)
    {
        if (attributeSystem == null) return;
        
        if (logDebugInfo)
        {
            Debug.Log($"AttributeUIController: 点击增加按钮 - 属性: {attributeName}");
        }
        
        // 使用通用方法增加属性值
        bool success = attributeSystem.IncreaseAttribute(attributeName);
        
        // 播放音效
        PlaySound(success ? increaseSound : errorSound);
    }
    
    private void OnDecreaseButtonClicked(string attributeName)
    {
        if (attributeSystem == null) return;
        
        if (logDebugInfo)
        {
            Debug.Log($"AttributeUIController: 点击减少按钮 - 属性: {attributeName}");
        }
        
        // 使用通用方法减少属性值
        bool success = attributeSystem.DecreaseAttribute(attributeName);
        
        // 播放音效
        PlaySound(success ? decreaseSound : errorSound);
    }
    
    private void OnResetButtonClicked()
    {
        if (attributeSystem != null)
        {
            if (logDebugInfo)
            {
                Debug.Log("AttributeUIController: 点击重置按钮");
            }
            
            attributeSystem.ResetAllAttributes();
            PlaySound(decreaseSound); // 使用减少音效作为重置音效
        }
    }
    #endregion
    
    #region 事件响应
    private void OnAttributeValueChanged(string attributeName, int newValue)
    {
        if (attributeSystem == null) return;
        
        if (logDebugInfo)
        {
            Debug.Log($"AttributeUIController: 属性值变化 - {attributeName}: {newValue}");
        }
        
        // 更新对应的UI组
        string lowerName = attributeName.ToLower();
        if (uiGroupDict.TryGetValue(lowerName, out AttributeUIGroup uiGroup))
        {
            UpdateAttributeUI(attributeName, uiGroup);
        }
    }
    
    private void OnTotalPointsChanged(int currentPoints, int maxPoints)
    {
        // 更新总点数显示
        if (totalPointsText != null)
        {
            totalPointsText.text = $"Used Points: {currentPoints}/{maxPoints}";
        }
        
        // 更新剩余点数显示
        if (remainingPointsText != null)
        {
            int remaining = maxPoints - currentPoints;
            remainingPointsText.text = $"Remaining Points: {remaining}";
        }
        
        if (logDebugInfo)
        {
            Debug.Log($"AttributeUIController: 总点数变化 - 当前: {currentPoints}, 最大: {maxPoints}");
        }
        
        // 因为总点数变化可能影响所有属性的可增加状态，更新所有UI
        UpdateAllAttributeUI();
    }
    #endregion
    
    #region 辅助方法
    private void UpdateAttributeUI(string attributeName, AttributeUIGroup uiGroup)
    {
        if (attributeSystem == null) return;
        
        AttributeData attributeData = attributeSystem.GetAttribute(attributeName);
        if (attributeData == null) return;
        
        // 检查是否可以增加/减少
        bool canIncrease = attributeData.currentValue < attributeData.maxValue;
        bool canDecrease = attributeData.currentValue > attributeData.minValue;
        
        // 如果使用总点数限制，还需要检查剩余点数
        if (!attributeSystem.HasRemainingPoints())
        {
            canIncrease = false;
        }
        
        // 更新UI
        uiGroup.UpdateUI(
            attributeData.currentValue,
            attributeData.minValue,
            attributeData.maxValue,
            canIncrease,
            canDecrease
        );
    }
    
    private void UpdateAllAttributeUI()
    {
        if (attributeSystem == null) return;
        
        // 获取所有属性
        var attributes = attributeSystem.GetAllAttributes();
        
        // 更新每个属性的UI
        foreach (var attribute in attributes)
        {
            string lowerName = attribute.attributeName.ToLower();
            if (uiGroupDict.TryGetValue(lowerName, out AttributeUIGroup uiGroup))
            {
                UpdateAttributeUI(attribute.attributeName, uiGroup);
            }
        }
    }
    
    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
    
    // 手动刷新所有UI（用于调试或特殊情况）
    [ContextMenu("刷新所有UI")]
    public void RefreshAllUI()
    {
        UpdateAllAttributeUI();
        
        // 更新总点数
        if (attributeSystem != null)
        {
            int totalPoints = 0;
            int maxPoints = 0;
            
            // 这里假设GetRemainingPoints方法返回int.MaxValue表示没有限制
        if (attributeSystem.GetRemainingPoints() != int.MaxValue)
        {
                // 计算已使用点数和总点数
                var attributes = attributeSystem.GetAllAttributes();
                foreach (var attribute in attributes)
                {
                    totalPoints += attribute.currentValue;
                }
                
                maxPoints = totalPoints + attributeSystem.GetRemainingPoints();
                OnTotalPointsChanged(totalPoints, maxPoints);
            }
        }
    }
    #endregion
} 