using UnityEngine;
using TMPro;
using System;
using JoostenProductions;
using UnityEngine.InputSystem;

public class UITimerText : OverridableMonoBehaviour
{
    public TMP_Text timerText;
    private int currentHour;
    private int currentDay;

    //Temp
    public static Action OnShopRefreshingTime;

    void OnEnable()
    {
        base.OnEnable();
        SimpleGridCounter.OnGridCountChanged += AddOneHour;
    }

    void OnDisable()
    {
        base.OnDisable();
        SimpleGridCounter.OnGridCountChanged -= AddOneHour;
    }

    void Start()
    {
        currentHour = 0;
        currentDay = 1;
        UpdateTimerText();
    }

    public override void UpdateMe()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            AddOneHour();
        }
    }

    public void UpdateTimerText()
    {
        timerText.text = $"Day {currentDay}\n{currentHour:D2}:00";

        ///Temp
        if(currentHour == 0 || currentHour == 8 || currentHour == 16 || currentHour == 24)
        {
            OnShopRefreshingTime?.Invoke();
        }
        timerText.text = $"<size=48>{8 - currentHour%8} </size>hours until the next shop refresh\n" + timerText.text;
        ///
    }

    public void AddOneHour(int stepCount = 0)
    {
        currentHour ++;
        if (currentHour >= 24)
        {
            currentDay++;
            currentHour = 0;
        }
        UpdateTimerText();
    }

    public void MinusOneHour()
    {
        currentHour--;
        if (currentHour < 0)
        {
            currentDay--;
            currentHour = 23;
        }
        UpdateTimerText();
    }
}
