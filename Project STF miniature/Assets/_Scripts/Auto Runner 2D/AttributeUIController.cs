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
    
    // 更新UI显示
    public void UpdateUI(int value, int minValue, int maxValue, bool canIncrease, bool canDecrease)
    {
        // 更新数值文本
        if (valueText != null)
        {
            valueText.text = value.ToString();
            Debug.Log("valueText.text: " + valueText.text);
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
    
    // UI组件字典，用于快速查找
    private Dictionary<string, AttributeUIGroup> uiGroupDict = new Dictionary<string, AttributeUIGroup>();
    
    private void Start()
    {
        InitializeUI();
        SetupEventListeners();
        
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
                uiGroupDict[group.attributeName] = group;
                
                // 设置属性名称文本
                if (group.nameText != null && string.IsNullOrEmpty(group.nameText.text))
                {
                    group.nameText.text = group.attributeName;
                }
            }
        }
    }
    
    private void SetupEventListeners()
    {
        // 为每个UI组设置按钮事件
        foreach (var group in attributeUIGroups)
        {
            string attributeName = group.attributeName;
            
            // +按钮事件
            if (group.increaseButton != null)
            {
                group.increaseButton.onClick.AddListener(() => OnIncreaseButtonClicked(attributeName));
            }
            
            // -按钮事件
            if (group.decreaseButton != null)
            {
                group.decreaseButton.onClick.AddListener(() => OnDecreaseButtonClicked(attributeName));
            }
        }
        
        // 重置按钮事件
        if (resetButton != null)
        {
            resetButton.onClick.AddListener(OnResetButtonClicked);
        }
    }
    
    #region 按钮点击事件
    private void OnIncreaseButtonClicked(string attributeName)
    {
        if (attributeSystem == null) return;
        
        bool success = false;
        
        switch (attributeName.ToLower())
        {
            case "acceleration":
            case "加速":
                success = attributeSystem.IncreaseAcceleration();
                break;
            case "braking":
            case "刹车":
                success = attributeSystem.IncreaseBraking();
                break;
            case "vision":
            case "视野":
                success = attributeSystem.IncreaseVision();
                break;
            case "steering":
            case "转向":
                success = attributeSystem.IncreaseSteering();
                break;
        }
        
        // 播放音效
        PlaySound(success ? increaseSound : errorSound);
    }
    
    private void OnDecreaseButtonClicked(string attributeName)
    {
        if (attributeSystem == null) return;
        
        bool success = false;
        
        switch (attributeName.ToLower())
        {
            case "acceleration":
            case "加速":
                success = attributeSystem.DecreaseAcceleration();
                break;
            case "braking":
            case "刹车":
                success = attributeSystem.DecreaseBraking();
                break;
            case "vision":
            case "视野":
                success = attributeSystem.DecreaseVision();
                break;
            case "steering":
            case "转向":
                success = attributeSystem.DecreaseSteering();
                break;
        }
        
        // 播放音效
        PlaySound(success ? decreaseSound : errorSound);
    }
    
    private void OnResetButtonClicked()
    {
        if (attributeSystem != null)
        {
            attributeSystem.ResetAllAttributes();
            PlaySound(decreaseSound); // 使用减少音效作为重置音效
        }
    }
    #endregion
    
    #region 事件响应
    private void OnAttributeValueChanged(string attributeName, int newValue)
    {
        if (attributeSystem == null) return;
        
        // 更新对应的UI组
        if (uiGroupDict.TryGetValue(attributeName, out AttributeUIGroup uiGroup))
        {
            UpdateAttributeUI(attributeName, uiGroup);
        }
        else
        {
            // 尝试中文名称匹配
            string chineseName = GetChineseAttributeName(attributeName);
            if (uiGroupDict.TryGetValue(chineseName, out uiGroup))
            {
                UpdateAttributeUI(attributeName, uiGroup);
            }
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
    }
    #endregion
    
    #region 辅助方法
    private void UpdateAttributeUI(string attributeName, AttributeUIGroup uiGroup)
    {
        if (attributeSystem == null) return;
        
        AttributeData attributeData = GetAttributeData(attributeName);
        if (attributeData == null) return;
        
        // 检查是否可以增加/减少
        bool canIncrease = attributeData.currentValue < attributeData.maxValue;
        bool canDecrease = attributeData.currentValue > attributeData.minValue;
        
        // 如果使用总点数限制，还需要检查剩余点数
        if (attributeSystem.GetRemainingPoints() <= 0)
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
    
    private AttributeData GetAttributeData(string attributeName)
    {
        switch (attributeName.ToLower())
        {
            case "acceleration":
                return attributeSystem.GetAcceleration();
            case "braking":
                return attributeSystem.GetBraking();
            case "vision":
                return attributeSystem.GetVision();
            case "steering":
                return attributeSystem.GetSteering();
            default:
                return null;
        }
    }
    
    private string GetChineseAttributeName(string englishName)
    {
        switch (englishName.ToLower())
        {
            case "acceleration": return "加速";
            case "braking": return "刹车";
            case "vision": return "视野";
            case "steering": return "转向";
            default: return englishName;
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
        if (attributeSystem == null) return;
        
        OnAttributeValueChanged("acceleration", attributeSystem.GetAccelerationValue());
        OnAttributeValueChanged("braking", attributeSystem.GetBrakingValue());
        OnAttributeValueChanged("vision", attributeSystem.GetVisionValue());
        OnAttributeValueChanged("steering", attributeSystem.GetSteeringValue());
        
        if (attributeSystem.GetRemainingPoints() != int.MaxValue)
        {
            OnTotalPointsChanged(
                attributeSystem.GetAccelerationValue() + attributeSystem.GetBrakingValue() + 
                attributeSystem.GetVisionValue() + attributeSystem.GetSteeringValue(),
                20 // 这里应该从attributeSystem获取，但当前没有公开接口
            );
        }
    }
    #endregion
} 