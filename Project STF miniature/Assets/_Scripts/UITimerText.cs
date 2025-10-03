using UnityEngine;
using TMPro;
using System;
using JoostenProductions;
using UnityEngine.InputSystem;

public class UITimerText : OverridableMonoBehaviour
{
    public TMP_Text timerText;
    public int startHour;
    public int startDay;
    public int startMinute;
    private int currentHour;
    private int currentDay;
    private int currentMinute;

    //Temp
    public static Action OnShopRefreshingTime;
    public static Action OnDayChanged;

    void OnEnable()
    {
        base.OnEnable();
        // SimpleGridCounter.OnGridCountChanged += AddOneHour;
        SimpleGridCounter.OnTimeSpentforGrid += AddMinutes;
    }

    void OnDisable()
    {
        base.OnDisable();
        // SimpleGridCounter.OnGridCountChanged -= AddOneHour;
        SimpleGridCounter.OnTimeSpentforGrid -= AddMinutes;
    }

    void Start()
    {
        currentHour = startHour;
        currentDay = startDay;
        currentMinute = startMinute;
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
        timerText.text = $"Day {currentDay}\n{currentHour:D2}:{currentMinute:D2}";

        // ///Temp
        // if(currentHour == 0 || currentHour == 8 || currentHour == 16 || currentHour == 24)
        // {
        //     OnShopRefreshingTime?.Invoke();
        // }
        // timerText.text = $"<size=48>{8 - currentHour%8} </size>hours until the next shop refresh\n" + timerText.text;
        // ///
    }

    public void AddMinutes(int minutes)
    {
        currentMinute += minutes;
        if (currentMinute >= 60)
        {
            int addedHours = currentMinute / 60;
            currentMinute = currentMinute % 60;
            for (int i = 0; i < addedHours; i++)
            {
                AddOneHour();
            }
        }
        UpdateTimerText();
    }

    public void MinusMinutes(int minutes)
    {
        currentMinute -= minutes;
        if (currentMinute < 0)
        {
            int subtractedHours = - currentMinute / 60;
            currentMinute = currentMinute % 60 + 60;
            for (int i = 0; i < subtractedHours; i++)
            {
                MinusOneHour();
            }
        }
        UpdateTimerText();
    }

    public void AddOneHour()
    {
        currentHour ++;
        if (currentHour >= 24)
        {
            currentDay++;
            currentHour = 0;
            OnDayChanged?.Invoke();
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
