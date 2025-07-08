using UnityEngine;
using TMPro;

public class UITimerText : MonoBehaviour
{
    public TMP_Text timerText;
    private int currentHour;
    private int currentDay;

    void OnEnable()
    {
        SimpleGridCounter.OnGridCountChanged += AddOneHour;
        currentHour = 0;
        currentDay = 1;
        UpdateTimerText();
    }

    void OnDisable()
    {
        SimpleGridCounter.OnGridCountChanged -= AddOneHour;
    }

    public void UpdateTimerText()
    {
        timerText.text = $"Day {currentDay}\n{currentHour:D2}:00";
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
