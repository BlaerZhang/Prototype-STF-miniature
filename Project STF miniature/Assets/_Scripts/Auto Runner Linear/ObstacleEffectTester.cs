using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 障碍物效果测试器 - 用于测试障碍物效果和技能免疫系统
/// </summary>
public class ObstacleEffectTester : MonoBehaviour
{
    [Header("测试目标")]
    [SerializeField] private AutoRunnerLinearController controller;
    
    [Header("测试选项")]
    [SerializeField] private bool enableKeyboardTesting = true;
    [SerializeField] private bool showTestInstructions = true;
    
    // 添加对障碍物系统的引用
    private RunnerObstacleSystem obstacleSystem;
    private SkillSystem skillSystem;
    
    void Start()
    {
        if (controller == null)
        {
            controller = FindObjectOfType<AutoRunnerLinearController>();
        }
        
        if (controller == null)
        {
            Debug.LogError("ObstacleEffectTester: 未找到AutoRunnerLinearController组件！");
            enabled = false;
            return;
        }
        
        // 获取子系统引用
        obstacleSystem = controller.GetComponent<RunnerObstacleSystem>();
        if (obstacleSystem == null)
        {
            Debug.LogError("ObstacleEffectTester: 未找到RunnerObstacleSystem组件！");
            enabled = false;
            return;
        }
        
        skillSystem = controller.GetComponent<SkillSystem>();
        if (skillSystem == null)
        {
            Debug.LogWarning("ObstacleEffectTester: 未找到SkillSystem组件，技能测试将不可用");
        }
    }
    
    void Update()
    {
        if (!enableKeyboardTesting || obstacleSystem == null) return;
        
        // 障碍物效果测试
        if (Input.GetKeyDown(KeyCode.T))
        {
            obstacleSystem.StartObstacleEffect(ObstacleType.Low);
            Debug.Log("测试：触发Low障碍物效果（踉跄）");
        }
        
        if (Input.GetKeyDown(KeyCode.Y))
        {
            obstacleSystem.StartObstacleEffect(ObstacleType.High);
            Debug.Log("测试：触发High障碍物效果（踉跄）");
        }
        
        if (Input.GetKeyDown(KeyCode.U))
        {
            obstacleSystem.StartObstacleEffect(ObstacleType.Slowing);
            Debug.Log("测试：触发Slowing障碍物效果（减速）");
        }
        
        if (Input.GetKeyDown(KeyCode.I))
        {
            obstacleSystem.EndObstacleEffect();
            Debug.Log("测试：强制结束障碍物效果");
        }
        
        // 技能免疫测试
        if (Input.GetKeyDown(KeyCode.J))
        {
            bool result = controller.TryActivateSkill("Jump");
            Debug.Log($"测试：激活Jump技能 - {(result ? "成功" : "失败")}");
        }
        
        if (Input.GetKeyDown(KeyCode.K))
        {
            bool result = controller.TryActivateSkill("Roll");
            Debug.Log($"测试：激活Roll技能 - {(result ? "成功" : "失败")}");
        }
        
        if (Input.GetKeyDown(KeyCode.L))
        {
            bool result = controller.TryActivateSkill("Dash");
            Debug.Log($"测试：激活Dash技能 - {(result ? "成功" : "失败")}");
        }
        
        // 组合测试：技能激活时测试障碍物免疫
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            // 激活Jump技能后立即测试Low障碍物
            if (controller.TryActivateSkill("Jump"))
            {
                obstacleSystem.StartObstacleEffect(ObstacleType.Low);
                Debug.Log("组合测试：Jump激活 + Low障碍物（应该免疫）");
            }
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            // 激活Roll技能后立即测试High障碍物
            if (controller.TryActivateSkill("Roll"))
            {
                obstacleSystem.StartObstacleEffect(ObstacleType.High);
                Debug.Log("组合测试：Roll激活 + High障碍物（应该免疫）");
            }
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            // 激活Dash技能后立即测试Slowing障碍物
            if (controller.TryActivateSkill("Dash"))
            {
                obstacleSystem.StartObstacleEffect(ObstacleType.Slowing);
                Debug.Log("组合测试：Dash激活 + Slowing障碍物（应该免疫）");
            }
        }
        
        // 测试障碍物效果期间的免疫
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            // 先触发一个障碍物效果，然后尝试触发另一个
            obstacleSystem.StartObstacleEffect(ObstacleType.Low);
            obstacleSystem.StartObstacleEffect(ObstacleType.Slowing);
            Debug.Log("重复触发测试：Low效果 + Slowing效果（第二个应该被忽略）");
        }
    }
    
    void OnGUI()
    {
        if (!showTestInstructions || obstacleSystem == null) return;
        
        // 显示测试说明
        GUILayout.BeginArea(new Rect(Screen.width - 350, 10, 340, 400));
        GUILayout.Label("=== 障碍物系统测试器 ===");
        
        GUILayout.Space(10);
        GUILayout.Label("障碍物效果测试:");
        GUILayout.Label("T - 触发Low障碍物（踉跄）");
        GUILayout.Label("Y - 触发High障碍物（踉跄）");
        GUILayout.Label("U - 触发Slowing障碍物（减速）");
        GUILayout.Label("I - 强制结束障碍物效果");
        
        GUILayout.Space(10);
        GUILayout.Label("技能激活测试:");
        GUILayout.Label("J - 激活Jump技能");
        GUILayout.Label("K - 激活Roll技能");
        GUILayout.Label("L - 激活Dash技能");
        
        GUILayout.Space(10);
        GUILayout.Label("免疫组合测试:");
        GUILayout.Label("1 - Jump + Low障碍物");
        GUILayout.Label("2 - Roll + High障碍物");
        GUILayout.Label("3 - Dash + Slowing障碍物");
        GUILayout.Label("4 - 重复触发测试");
        
        // 显示当前状态
        GUILayout.Space(10);
        GUILayout.Label("=== 当前状态 ===");
        
        if (obstacleSystem.IsInObstacleEffect)
        {
            GUILayout.Label($"障碍物效果: {obstacleSystem.CurrentEffectType}");
            GUILayout.Label($"剩余时间: {obstacleSystem.GetEffectRemainingTime():F1}s");
        }
        else
        {
            GUILayout.Label("无障碍物效果");
        }
        
        // 获取激活的免疫技能
        List<string> activeImmunitySkills = GetActiveImmunitySkills();
        if (activeImmunitySkills.Count > 0)
        {
            GUILayout.Label($"激活免疫: {string.Join(", ", activeImmunitySkills)}");
        }
        else
        {
            GUILayout.Label("无免疫技能激活");
        }
        
        GUILayout.EndArea();
    }
    
    // 获取当前激活的可提供免疫的技能列表
    private List<string> GetActiveImmunitySkills()
    {
        List<string> activeImmunitySkills = new List<string>();
        
        if (skillSystem != null)
        {
            if (controller.IsSkillActive("Jump"))
                activeImmunitySkills.Add("Jump (Low免疫)");
                
            if (controller.IsSkillActive("Roll"))
                activeImmunitySkills.Add("Roll (High免疫)");
                
            if (controller.IsSkillActive("Dash"))
                activeImmunitySkills.Add("Dash (Slowing免疫)");
        }
        
        return activeImmunitySkills;
    }
} 