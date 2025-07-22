using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Sirenix.OdinInspector;

namespace SpinWheel
{
    [Serializable]
    public class PrizeItem
    {
        [LabelText("奖项名称")]
        public string prizeName;
        
        [LabelText("奖项图标")]
        public Sprite prizeIcon;
        
        [LabelText("权重"), Range(0.1f, 100f)]
        public float weight = 1f;

        [LabelText("奖项内容")]
        public List<CirclingItemForSale> prizeContent;
        
        [HideInInspector]
        public float normalizedWeight; // 归一化权重，用于计算角度
        
        [HideInInspector]
        public float startAngle; // 扇形起始角度
        
        [HideInInspector]
        public float endAngle; // 扇形结束角度
    }

    [CreateAssetMenu(fileName = "New Spin Wheel Prize Pool", menuName = "Spin Wheel/Prize Pool")]
    public class SpinWheelPrizePool : ScriptableObject
    {
        [LabelText("奖池名称")]
        public string poolName;
        
        [LabelText("奖项列表")]
        [SerializeField]
        private SerializedDictionary<string, PrizeItem> prizes = new SerializedDictionary<string, PrizeItem>();
        
        public Dictionary<string, PrizeItem> Prizes => prizes;
        
        [Button("计算权重分布")]
        public void CalculateWeightDistribution()
        {
            float totalWeight = 0f;
            foreach (var prize in prizes.Values)
            {
                totalWeight += prize.weight;
            }
            
            float currentAngle = 0f;
            foreach (var prize in prizes.Values)
            {
                prize.normalizedWeight = prize.weight / totalWeight;
                prize.startAngle = currentAngle;
                prize.endAngle = currentAngle + (prize.normalizedWeight * 360f);
                currentAngle = prize.endAngle;
            }
        }
        
        public PrizeItem GetRandomPrize()
        {
            float randomValue = UnityEngine.Random.Range(0f, 1f);
            float currentWeight = 0f;
            
            foreach (var prize in prizes.Values)
            {
                currentWeight += prize.normalizedWeight;
                if (randomValue <= currentWeight)
                {
                    return prize;
                }
            }
            
            // 如果没有找到，返回第一个奖项
            foreach (var prize in prizes.Values)
            {
                return prize;
            }
            
            return null;
        }
        
        /// <summary>
        /// 获取奖项对应的角度，考虑指针在顶部的情况
        /// </summary>
        public float GetAngleForPrize(PrizeItem targetPrize)
        {
            // 指针在顶部(12点方向)，所以需要计算转盘应该转到什么角度让奖项对准指针
            float prizeAngle = UnityEngine.Random.Range(targetPrize.startAngle, targetPrize.endAngle);
            
            // 转换角度：因为指针在顶部，我们需要让奖项的角度转到顶部位置
            // Unity的角度系统：0度在右侧，90度在上方，顺时针为负
            return prizeAngle;
        }
        
        private void OnValidate()
        {
            CalculateWeightDistribution();
        }
    }
} 