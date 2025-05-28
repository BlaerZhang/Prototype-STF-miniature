using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine.InputSystem;

/// <summary>
/// 技能系统管理器
/// 管理所有技能的激活、冷却和动画控制
/// </summary>
public class SkillSystem : MonoBehaviour
{
    [Header("技能列表")]
    [SerializeField] private List<Skill> skills = new List<Skill>();
    
    [Header("精力系统")]
    [SerializeField] private EnergySystem energySystem = new EnergySystem();

    [Header("加点系统")]
    [SerializeField] private AttributePointSystem attributePointSystem;
    
    [Header("动画控制")]
    [SerializeField] private Animator characterAnimator;
    
    [Header("UI引用（可选）")]
    [SerializeField] private UnityEngine.UI.Slider energySlider;
    [SerializeField] private List<SkillUIElement> skillUIElements = new List<SkillUIElement>();
    
    [Header("调试设置")]
    [SerializeField] private bool showDebugInfo = true;
    [SerializeField] private KeyCode testSkill1Key = KeyCode.Alpha1;
    [SerializeField] private KeyCode testSkill2Key = KeyCode.Alpha2;
    [SerializeField] private KeyCode testSkill3Key = KeyCode.Alpha3;
    
    // 事件
    public System.Action<Skill> OnSkillActivated;
    public System.Action<Skill> OnSkillDeactivated;
    public System.Action<float, float> OnEnergyChanged;
    
    // 属性
    public EnergySystem Energy => energySystem;
    public List<Skill> Skills => skills;
    public int ActiveSkillCount => skills.Count(s => s.isActive);
    private Keyboard keyboardInput;
    
    void Start()
    {
        InitializeSkillSystem();
        keyboardInput = Keyboard.current;
    }
    
    void Update()
    {
        UpdateSkillSystem();
        HandleDebugInput();
    }
    
    /// <summary>
    /// 初始化技能系统
    /// </summary>
    private void InitializeSkillSystem()
    {
        // 初始化精力系统
        energySystem.Initialize();
        
        // 订阅精力系统事件
        energySystem.OnEnergyChanged += OnEnergySystemChanged;
        energySystem.OnEnergyEmpty += OnEnergyEmpty;
        energySystem.OnEnergyFull += OnEnergyFull;
        
        // 获取动画控制器
        if (characterAnimator == null)
        {
            characterAnimator = GetComponentInChildren<Animator>();
        }
        
        // 初始化技能
        for (int i = 0; i < skills.Count; i++)
        {
            skills[i].Reset();
        }
        
        // 初始化UI
        InitializeUI();
        
        if (showDebugInfo)
        {
            Debug.Log($"技能系统初始化完成 - 技能数量: {skills.Count}");
        }
    }
    
    /// <summary>
    /// 更新技能系统
    /// </summary>
    private void UpdateSkillSystem()
    {
        // 更新精力系统
        energySystem.UpdateEnergy();
        
        // 获取技能冷却时间属性值
        float cooldownModifier = 1f;
        if (attributePointSystem != null)
        {
            AttributeData cooldownAttribute = attributePointSystem.GetAttribute("Skill Cooldown");
            if (cooldownAttribute != null)
            {
                // 冷却值越高，冷却时间越短，最大可减少到原来的75%
                cooldownModifier = 1f - (0.75f * cooldownAttribute.GetProgressPercent());
            }
        }
        
        // 更新所有技能
        foreach (var skill in skills)
        {
            bool wasActive = skill.isActive;
            bool wasOnCooldown = skill.isOnCooldown;
            float previousCooldown = skill.remainingCooldown;
            
            // 应用冷却修饰符
            skill.UpdateSkill(cooldownModifier);
            
            // 检查技能状态变化
            if (wasActive && !skill.isActive)
            {
                // 技能结束
                OnSkillDeactivated?.Invoke(skill);
                UpdateSkillAnimation(skill, false);
                UpdateSkillUI(skill);
            }
            
            // 检查冷却状态变化或冷却时间变化
            if (skill.isOnCooldown && (wasOnCooldown != skill.isOnCooldown || Mathf.Abs(previousCooldown - skill.remainingCooldown) > 0.01f))
            {
                // 冷却中，更新UI
                UpdateSkillUI(skill);
            }
            else if (wasOnCooldown && !skill.isOnCooldown)
            {
                // 冷却结束，更新UI
                UpdateSkillUI(skill);
            }
        }
        
        // 更新UI
        UpdateUI();
    }
    
    /// <summary>
    /// 尝试激活技能
    /// </summary>
    /// <param name="skillIndex">技能索引</param>
    /// <returns>是否成功激活</returns>
    public bool TryActivateSkill(int skillIndex)
    {
        if (skillIndex < 0 || skillIndex >= skills.Count)
        {
            if (showDebugInfo)
            {
                Debug.LogWarning($"无效的技能索引: {skillIndex}");
            }
            return false;
        }
        
        return TryActivateSkill(skills[skillIndex]);
    }
    
    /// <summary>
    /// 尝试激活技能
    /// </summary>
    /// <param name="skill">技能对象</param>
    /// <returns>是否成功激活</returns>
    public bool TryActivateSkill(Skill skill)
    {
        if (skill == null) return false;
        
        // 检查技能是否可以使用
        if (!skill.CanUse(energySystem.CurrentEnergy))
        {
            if (showDebugInfo)
            {
                string reason = skill.isActive ? "技能已激活" : 
                               skill.isOnCooldown ? "技能冷却中" : "精力不足";
                Debug.LogWarning($"无法激活技能 {skill.skillName}: {reason}");
            }
            return false;
        }
        
        // 消耗精力
        if (!energySystem.ConsumeEnergy(skill.energyCost))
        {
            return false;
        }
        
        // 激活技能
        skill.Activate();
        OnSkillActivated?.Invoke(skill);
        UpdateSkillAnimation(skill, true);
        UpdateSkillUI(skill);
        
        if (showDebugInfo)
        {
            Debug.Log($"成功激活技能: {skill.skillName}");
        }
        
        return true;
    }
    
    /// <summary>
    /// 根据技能名称激活技能
    /// </summary>
    /// <param name="skillName">技能名称</param>
    /// <returns>是否成功激活</returns>
    public bool TryActivateSkill(string skillName)
    {
        var skill = skills.FirstOrDefault(s => s.skillName == skillName);
        return TryActivateSkill(skill);
    }
    
    /// <summary>
    /// 强制停止技能
    /// </summary>
    /// <param name="skillIndex">技能索引</param>
    public void ForceStopSkill(int skillIndex)
    {
        if (skillIndex < 0 || skillIndex >= skills.Count) return;
        
        var skill = skills[skillIndex];
        if (skill.isActive)
        {
            skill.Deactivate();
            OnSkillDeactivated?.Invoke(skill);
            UpdateSkillAnimation(skill, false);
            UpdateSkillUI(skill);
        }
    }
    
    /// <summary>
    /// 停止所有技能
    /// </summary>
    public void StopAllSkills()
    {
        foreach (var skill in skills)
        {
            if (skill.isActive)
            {
                skill.Deactivate();
                OnSkillDeactivated?.Invoke(skill);
                UpdateSkillAnimation(skill, false);
                UpdateSkillUI(skill);
            }
        }
    }
    
    /// <summary>
    /// 更新技能动画
    /// </summary>
    private void UpdateSkillAnimation(Skill skill, bool isActive)
    {
        if (characterAnimator == null || string.IsNullOrEmpty(skill.animationParameterName))
            return;
        
        try
        {
            switch (skill.parameterType)
            {
                case Skill.AnimationParameterType.Bool:
                    characterAnimator.SetBool(skill.animationParameterName, isActive);
                    break;
                    
                case Skill.AnimationParameterType.Trigger:
                    if (isActive) // 只在激活时触发
                    {
                        characterAnimator.SetTrigger(skill.animationParameterName);
                    }
                    break;
            }
        }
        catch (System.Exception e)
        {
            if (showDebugInfo)
            {
                Debug.LogWarning($"设置动画参数失败: {skill.animationParameterName}, 错误: {e.Message}");
            }
        }
    }
    
    /// <summary>
    /// 获取技能状态
    /// </summary>
    /// <param name="skillIndex">技能索引</param>
    /// <returns>技能是否激活</returns>
    public bool IsSkillActive(int skillIndex)
    {
        if (skillIndex < 0 || skillIndex >= skills.Count) return false;
        return skills[skillIndex].isActive;
    }
    
    /// <summary>
    /// 根据名称获取技能状态
    /// </summary>
    /// <param name="skillName">技能名称</param>
    /// <returns>技能是否激活</returns>
    public bool IsSkillActive(string skillName)
    {
        var skill = skills.FirstOrDefault(s => s.skillName == skillName);
        return skill?.isActive ?? false;
    }
    
    /// <summary>
    /// 获取技能对象
    /// </summary>
    /// <param name="skillIndex">技能索引</param>
    /// <returns>技能对象</returns>
    public Skill GetSkill(int skillIndex)
    {
        if (skillIndex < 0 || skillIndex >= skills.Count) return null;
        return skills[skillIndex];
    }
    
    /// <summary>
    /// 根据名称获取技能对象
    /// </summary>
    /// <param name="skillName">技能名称</param>
    /// <returns>技能对象</returns>
    public Skill GetSkill(string skillName)
    {
        return skills.FirstOrDefault(s => s.skillName == skillName);
    }
    
    /// <summary>
    /// 初始化UI
    /// </summary>
    private void InitializeUI()
    {
        // 初始化精力条
        if (energySlider != null)
        {
            // 计算有效最大精力
            float effectiveMaxEnergy = energySystem.MaxEnergy;
            if (attributePointSystem != null)
            {
                AttributeData maxEnergyAttr = attributePointSystem.GetAttribute("Max Energy");
                if (maxEnergyAttr != null)
                {
                    float maxEnergyModifier = 1f + maxEnergyAttr.GetProgressPercent();
                    effectiveMaxEnergy *= maxEnergyModifier;
                }
            }
            
            energySlider.maxValue = effectiveMaxEnergy;
            energySlider.value = energySystem.CurrentEnergy;
        }
        
        // 初始化技能UI
        for (int i = 0; i < skillUIElements.Count && i < skills.Count; i++)
        {
            if (skillUIElements[i] != null)
            {
                skillUIElements[i].Initialize(skills[i]);
            }
        }
    }
    
    /// <summary>
    /// 更新UI
    /// </summary>
    private void UpdateUI()
    {
        // 更新精力条
        if (energySlider != null)
        {
            // 计算有效最大精力
            float effectiveMaxEnergy = energySystem.MaxEnergy;
            if (attributePointSystem != null)
            {
                AttributeData maxEnergyAttr = attributePointSystem.GetAttribute("Max Energy");
                if (maxEnergyAttr != null)
                {
                    float maxEnergyModifier = 1f + maxEnergyAttr.GetProgressPercent();
                    effectiveMaxEnergy *= maxEnergyModifier;
                }
            }
            
            // 更新滑动条的最大值和当前值
            energySlider.maxValue = effectiveMaxEnergy;
            energySlider.value = energySystem.CurrentEnergy;
        }
        
        // 更新所有技能UI
        for (int i = 0; i < skillUIElements.Count && i < skills.Count; i++)
        {
            if (skillUIElements[i] != null)
            {
                skillUIElements[i].UpdateUI(skills[i]);
            }
        }
    }
    
    /// <summary>
    /// 更新技能UI
    /// </summary>
    private void UpdateSkillUI(Skill skill)
    {
        int skillIndex = skills.IndexOf(skill);
        if (skillIndex >= 0 && skillIndex < skillUIElements.Count && skillUIElements[skillIndex] != null)
        {
            skillUIElements[skillIndex].UpdateUI(skill);
        }
    }
    
    /// <summary>
    /// 精力系统变化事件
    /// </summary>
    private void OnEnergySystemChanged(float currentEnergy, float maxEnergy)
    {
        OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);
    }
    
    /// <summary>
    /// 精力耗尽事件
    /// </summary>
    private void OnEnergyEmpty()
    {
        if (showDebugInfo)
        {
            Debug.Log("精力耗尽，无法使用技能");
        }
    }
    
    /// <summary>
    /// 精力满值事件
    /// </summary>
    private void OnEnergyFull()
    {
        if (showDebugInfo)
        {
            Debug.Log("精力已满");
        }
    }
    
    /// <summary>
    /// 处理调试输入
    /// </summary>
    private void HandleDebugInput()
    {
        if (!showDebugInfo) return;
        
        if (keyboardInput.digit1Key.wasPressedThisFrame && skills.Count > 0)
        {
            TryActivateSkill(0);
        }
        
        if (keyboardInput.digit2Key.wasPressedThisFrame && skills.Count > 1)
        {
            TryActivateSkill(1);
        }
        
        if (keyboardInput.digit3Key.wasPressedThisFrame && skills.Count > 2)
        {
            TryActivateSkill(2);
        }
    }
    
    /// <summary>
    /// 重置技能系统
    /// </summary>
    public void ResetSkillSystem()
    {
        StopAllSkills();
        energySystem.Reset();
        
        foreach (var skill in skills)
        {
            skill.Reset();
        }
        
        if (showDebugInfo)
        {
            Debug.Log("技能系统已重置");
        }
    }
    
    private void OnDestroy()
    {
        // 清理事件订阅
        if (energySystem != null)
        {
            energySystem.OnEnergyChanged -= OnEnergySystemChanged;
            energySystem.OnEnergyEmpty -= OnEnergyEmpty;
            energySystem.OnEnergyFull -= OnEnergyFull;
        }
    }
}

/// <summary>
/// 技能UI元素
/// 用于在Inspector中配置技能UI引用
/// </summary>
[System.Serializable]
public class SkillUIElement
{
    [Header("UI引用")]
    public UnityEngine.UI.Image skillIcon;          // 技能图标
    public UnityEngine.UI.Image cooldownOverlay;    // 冷却遮罩
    public TMP_Text cooldownText;        // 冷却时间文本（可选）
    private Skill associatedSkill;
    
    /// <summary>
    /// 初始化UI元素
    /// </summary>
    public void Initialize(Skill skill)
    {
        associatedSkill = skill;
        
        if (cooldownOverlay != null)
        {
            cooldownOverlay.fillAmount = 0f;
            cooldownOverlay.gameObject.SetActive(false);
        }
        
        if (cooldownText != null)
        {
            cooldownText.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// 更新UI
    /// </summary>
    public void UpdateUI(Skill skill)
    {
        if (skill != associatedSkill) return;
        
        // 更新冷却遮罩
        if (cooldownOverlay != null)
        {
            if (skill.isOnCooldown)
            {
                cooldownOverlay.gameObject.SetActive(true);
                float progress = 1f - skill.GetCooldownProgress();
                cooldownOverlay.fillAmount = progress; // 直接设置，避免DOTween冲突
                
                // 更新冷却时间文本
                if (cooldownText != null)
                {
                    cooldownText.gameObject.SetActive(true);
                    cooldownText.text = Mathf.Ceil(skill.GetDisplayRemainingCooldown()).ToString();
                }
            }
            else
            {
                cooldownOverlay.gameObject.SetActive(false);
                if (cooldownText != null)
                {
                    cooldownText.gameObject.SetActive(false);
                }
            }
        }

        // 技能激活时的视觉效果（高亮状态）
        if (skill.isActive)
        {
            skillIcon.color = Color.orange;
        }
        else
        {
            skillIcon.color = Color.white;
        }
    }
} 