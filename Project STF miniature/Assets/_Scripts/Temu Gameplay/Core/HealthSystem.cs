using System;
using UnityEngine;

namespace TemuGameplay.Core
{
    /// <summary>
    /// 血量系统
    /// 管理玩家血量，处理扣血逻辑
    /// </summary>
    public class HealthSystem : MonoBehaviour
    {
        [Header("Blood Settings")]
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private int currentHealth = 100;
        [SerializeField] private float damageMultiplier = 5f; // 差值倍数
        
        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;

        public int MaxHealth => maxHealth;
        public int CurrentHealth => currentHealth;
        public float HealthPercentage => (float)currentHealth / maxHealth;
        public bool IsAlive => currentHealth > 0;
        
        // 事件
        public event Action<int, int> OnHealthChanged; // (currentHealth, maxHealth)
        public event Action<int> OnDamageTaken; // (damageAmount)
        public event Action OnHealthDepleted; // 血量耗尽

        private void Awake()
        {
            // 确保血量在合理范围内
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        }

        /// <summary>
        /// 设置血量
        /// </summary>
        public void SetHealth(int health)
        {
            int previousHealth = currentHealth;
            currentHealth = Mathf.Clamp(health, 0, maxHealth);
            
            if (currentHealth != previousHealth)
            {
                OnHealthChanged?.Invoke(currentHealth, maxHealth);
                
                if (enableDebugLogs)
                {
                    Debug.Log($"Health set to: {currentHealth}/{maxHealth}");
                }
                
                if (currentHealth <= 0)
                {
                    OnHealthDepleted?.Invoke();
                }
            }
        }

        /// <summary>
        /// 扣除血量
        /// </summary>
        public void TakeDamage(int damage)
        {
            if (damage <= 0) return;
            
            int previousHealth = currentHealth;
            currentHealth = Mathf.Max(0, currentHealth - damage);
            
            OnDamageTaken?.Invoke(damage);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            
            if (enableDebugLogs)
            {
                Debug.Log($"Took {damage} damage. Health: {currentHealth}/{maxHealth}");
            }
            
            if (currentHealth <= 0 && previousHealth > 0)
            {
                OnHealthDepleted?.Invoke();
                if (enableDebugLogs)
                {
                    Debug.Log("Health depleted!");
                }
            }
        }

        /// <summary>
        /// 恢复血量
        /// </summary>
        public void Heal(int healAmount)
        {
            if (healAmount <= 0) return;
            
            currentHealth = Mathf.Min(maxHealth, currentHealth + healAmount);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            
            if (enableDebugLogs)
            {
                Debug.Log($"Healed {healAmount}. Health: {currentHealth}/{maxHealth}");
            }
        }

        /// <summary>
        /// 重置血量到最大值
        /// </summary>
        public void ResetHealth()
        {
            SetHealth(maxHealth);
        }

        /// <summary>
        /// 根据未满足的trait要求计算应扣除的血量
        /// 规则：部分满足视为不满足，扣除完整要求数量对应的血量
        /// </summary>
        public int CalculateDamageFromUnmetRequirements(System.Collections.Generic.Dictionary<Data.TraitType, int> requirements, 
                                                      System.Collections.Generic.Dictionary<Data.TraitType, Data.TraitCounter> currentCounts)
        {
            int totalDamage = 0;
            
            foreach (var requirement in requirements)
            {
                var trait = requirement.Key;
                var requiredCount = requirement.Value;
                var currentCount = currentCounts.ContainsKey(trait) ? currentCounts[trait].CurrentCount : 0;
                
                if (currentCount < requiredCount)
                {
                    // 部分满足视为不满足，扣除完整要求数量对应的血量
                    int damage = Mathf.RoundToInt(requiredCount * damageMultiplier);
                    totalDamage += damage;
                    
                    if (enableDebugLogs)
                    {
                        Debug.Log($"Trait {trait}: Need {requiredCount}, Have {currentCount} - UNMET! Full damage: {damage}");
                    }
                }
                else
                {
                    if (enableDebugLogs)
                    {
                        Debug.Log($"Trait {trait}: Need {requiredCount}, Have {currentCount} - MET! No damage");
                    }
                }
            }
            
            return totalDamage;
        }

        /// <summary>
        /// 设置伤害倍数
        /// </summary>
        public void SetDamageMultiplier(float multiplier)
        {
            damageMultiplier = Mathf.Max(0, multiplier);
        }

        /// <summary>
        /// 设置最大血量
        /// </summary>
        public void SetMaxHealth(int newMaxHealth)
        {
            maxHealth = Mathf.Max(1, newMaxHealth);
            currentHealth = Mathf.Min(currentHealth, maxHealth);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        #region Context Menu Methods
        [ContextMenu("Take 10 Damage")]
        public void DebugTakeDamage()
        {
            if (Application.isPlaying)
            {
                TakeDamage(10);
            }
        }

        [ContextMenu("Heal 10")]
        public void DebugHeal()
        {
            if (Application.isPlaying)
            {
                Heal(10);
            }
        }

        [ContextMenu("Reset Health")]
        public void DebugResetHealth()
        {
            if (Application.isPlaying)
            {
                ResetHealth();
            }
        }

        [ContextMenu("Show Health Info")]
        public void ShowHealthInfo()
        {
            Debug.Log($"Health: {currentHealth}/{maxHealth} ({HealthPercentage:P1})");
            Debug.Log($"Damage Multiplier: {damageMultiplier}");
            Debug.Log($"Is Alive: {IsAlive}");
        }
        #endregion
    }
} 