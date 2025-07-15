using UnityEngine;
using TMPro;
using System;

public class UITimerText : MonoBehaviour
{
    public TMP_Text timerText;
    private int currentHour;
    private int currentDay;

    //Temp
    public static Action OnShopRefreshingTime;

    void OnEnable()
    {
        SimpleGridCounter.OnGridCountChanged += AddOneHour;
    }

    void OnDisable()
    {
        SimpleGridCounter.OnGridCountChanged -= AddOneHour;
    }

    void Start()
    {
        currentHour = 0;
        currentDay = 1;
        UpdateTimerText();
    }

    public void UpdateTimerText()
    {
        timerText.text = $"Day {currentDay}\n{currentHour:D2}:00";

        //Temp
        if(currentHour == 0 || currentHour == 12 || currentHour == 24)
        {
            OnShopRefreshingTime?.Invoke();
        }
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
