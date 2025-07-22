using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using TMPro;

namespace SpinWheel
{
    public class SpinWheelUI : MonoBehaviour
    {
        [Header("转盘UI组件")]
        [LabelText("转盘背景")]
        [SerializeField] private Image wheelBackground;
        
        [LabelText("转盘容器 (旋转的部分)")]
        [SerializeField] private RectTransform wheelContainer;
        
        [LabelText("指针")]
        [SerializeField] private Image pointer;
        
        [LabelText("奖项显示预制体")]
        [SerializeField] private GameObject prizeDisplayPrefab;
        
        [Header("转盘设置")]
        [LabelText("转盘半径")]
        [SerializeField] private float wheelRadius = 250f;
        
        [LabelText("奖项显示半径 (从中心到奖项文字的距离)")]
        [SerializeField] private float prizeDisplayRadius = 150f;
        
        [LabelText("分割线颜色")]
        [SerializeField] private Color divisionLineColor = Color.white;
        
        [LabelText("分割线宽度")]
        [SerializeField] private float divisionLineWidth = 2f;
        
        // 运行时数据
        private List<GameObject> currentPrizeDisplays = new List<GameObject>();
        private List<LineRenderer> divisionLines = new List<LineRenderer>();
        
        public RectTransform WheelTransform => wheelContainer;
        
        private void Awake()
        {
            ValidateComponents();
            SetupDefaultUI();
        }
        
        private void ValidateComponents()
        {
            if (wheelBackground == null)
                wheelBackground = GetComponentInChildren<Image>();
            
            if (wheelContainer == null)
            {
                // 如果没有指定，创建一个转盘容器
                GameObject container = new GameObject("Wheel Container");
                container.transform.SetParent(transform);
                wheelContainer = container.AddComponent<RectTransform>();
                wheelContainer.anchoredPosition = Vector2.zero;
                wheelContainer.sizeDelta = new Vector2(wheelRadius * 2, wheelRadius * 2);
            }
        }
        
        private void SetupDefaultUI()
        {
            // 设置转盘背景为圆形
            if (wheelBackground != null)
            {
                wheelBackground.rectTransform.sizeDelta = new Vector2(wheelRadius * 2, wheelRadius * 2);
            }
            
            // 设置指针位置 (在转盘顶部，12点方向)
            if (pointer != null)
            {
                RectTransform pointerRect = pointer.rectTransform;
                pointerRect.anchoredPosition = new Vector2(0, wheelRadius);
            }
        }
        
        /// <summary>
        /// 根据奖池配置设置转盘
        /// </summary>
        public void SetupWheel(SpinWheelPrizePool prizePool)
        {
            if (prizePool == null) return;
            
            // 清除现有显示
            ClearCurrentDisplay();
            
            // 确保权重分布已计算
            prizePool.CalculateWeightDistribution();
            
            // 创建分割线和奖项显示
            CreateDivisionLines(prizePool);
            CreatePrizeDisplays(prizePool);
        }
        
        private void ClearCurrentDisplay()
        {
            // 清除奖项显示
            foreach (var display in currentPrizeDisplays)
            {
                if (display != null)
                    DestroyImmediate(display);
            }
            currentPrizeDisplays.Clear();
            
            // 清除分割线
            foreach (var line in divisionLines)
            {
                if (line != null)
                    DestroyImmediate(line.gameObject);
            }
            divisionLines.Clear();
        }
        
        private void CreateDivisionLines(SpinWheelPrizePool prizePool)
        {
            foreach (var prize in prizePool.Prizes.Values)
            {
                // 为每个奖项创建起始分割线
                CreateDivisionLine(prize.startAngle);
            }
        }
        
        private void CreateDivisionLine(float angle)
        {
            GameObject lineObj = new GameObject($"Division Line {angle:F1}°");
            lineObj.transform.SetParent(wheelContainer);
            lineObj.transform.localPosition = Vector3.zero;
            lineObj.transform.localScale = Vector3.one;
            
            LineRenderer line = lineObj.AddComponent<LineRenderer>();
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.startColor = divisionLineColor;
            line.endColor = divisionLineColor;
            line.startWidth = divisionLineWidth;
            line.endWidth = divisionLineWidth;
            line.positionCount = 2;
            line.useWorldSpace = false;
            line.sortingOrder = 1000;
            
            // 计算线条的起点和终点
            // 角度转换：我们的角度系统0度在顶部，Unity的0度在右侧
            // 指针在顶部(90度位置)，所以需要将角度转换
            float unityAngle = 90f - angle; // 转换为Unity角度系统
            float radians = unityAngle * Mathf.Deg2Rad;
            Vector3 direction = new Vector3(Mathf.Cos(radians), Mathf.Sin(radians), 0);
            
            Vector3 startPos = Vector3.zero;
            Vector3 endPos = direction * wheelRadius;
            
            line.SetPosition(0, startPos);
            line.SetPosition(1, endPos);
            
            divisionLines.Add(line);
        }
        
        private void CreatePrizeDisplays(SpinWheelPrizePool prizePool)
        {
            foreach (var prize in prizePool.Prizes.Values)
            {
                CreatePrizeDisplay(prize);
            }
        }
        
        private void CreatePrizeDisplay(PrizeItem prize)
        {
            GameObject displayObj;
            
            if (prizeDisplayPrefab != null)
            {
                displayObj = Instantiate(prizeDisplayPrefab, wheelContainer);
            }
            else
            {
                // 如果没有预制体，创建默认的文字显示
                displayObj = CreateDefaultPrizeDisplay(prize);
            }
            
            // 计算奖项显示位置 (扇形中心)
            float centerAngle = (prize.startAngle + prize.endAngle) / 2f;
            // 角度转换：我们的角度系统0度在顶部，Unity的0度在右侧
            float unityAngle = 90f - centerAngle; // 转换为Unity角度系统
            float radians = unityAngle * Mathf.Deg2Rad;
            Vector3 direction = new Vector3(Mathf.Cos(radians), Mathf.Sin(radians), 0);
            Vector3 position = direction * prizeDisplayRadius;
            
            RectTransform displayRect = displayObj.GetComponent<RectTransform>();
            displayRect.anchoredPosition = position;
            // 设置奖项显示的旋转角度
            displayRect.rotation = Quaternion.LookRotation(Vector3.forward, direction);
            displayRect.rotation *= Quaternion.Euler(0, 0, 90);
            
            // 设置奖项信息
            SetPrizeDisplayInfo(displayObj, prize);
            
            currentPrizeDisplays.Add(displayObj);
        }
        
        private GameObject CreateDefaultPrizeDisplay(PrizeItem prize)
        {
            GameObject displayObj = new GameObject($"Prize Display - {prize.prizeName}");
            displayObj.transform.SetParent(wheelContainer);
            displayObj.transform.localScale = Vector3.one;
            
            // 添加RectTransform
            RectTransform rect = displayObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(100, 50);
            
            // 添加文字组件
            TMP_Text text = displayObj.AddComponent<TMP_Text>();
            text.text = prize.prizeName;
            text.fontSize = 14;
            text.color = Color.black;
            text.alignment = TextAlignmentOptions.Center;
            
            return displayObj;
        }
        
        private void SetPrizeDisplayInfo(GameObject displayObj, PrizeItem prize)
        {
            // 尝试设置文字
            TMP_Text text = displayObj.GetComponentInChildren<TMP_Text>();
            if (text != null)
            {
                text.text = prize.prizeName;
            }
            
            // 尝试设置图标
            Image icon = displayObj.GetComponentInChildren<Image>();
            if (icon != null && prize.prizeIcon != null)
            {
                icon.sprite = prize.prizeIcon;
            }
            
            // 如果有自定义的PrizeDisplay组件，可以在这里调用
            var customDisplay = displayObj.GetComponent<IPrizeDisplay>();
            if (customDisplay != null)
            {
                customDisplay.SetPrizeInfo(prize);
            }
        }
        
        // 编辑器辅助方法
        [Button("测试UI设置")]
        [ShowInInspector]
        private void TestUISetup()
        {
            SetupDefaultUI();
        }
    }
    
    /// <summary>
    /// 自定义奖项显示接口，用户可以实现这个接口来自定义奖项显示
    /// </summary>
    public interface IPrizeDisplay
    {
        void SetPrizeInfo(PrizeItem prize);
    }
} 