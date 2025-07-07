using UnityEngine;
using TMPro;

public class UITimerText : MonoBehaviour
{
    public TMP_Text timerText;
    private int currentHour;
    private int currentDay;

    void OnEnable()
    {
        SimpleGridCounter.OnGridCountChanged += UpdateTimerText;
        currentHour = 0;
        currentDay = 1;
    }

    void OnDisable()
    {
        SimpleGridCounter.OnGridCountChanged -= UpdateTimerText;
    }

    void UpdateTimerText(int stepCount)
    {
        currentHour ++;
        if (currentHour >= 24)
        {
            currentDay++;
            currentHour = 0;
        }

        timerText.text = $"Day {currentDay}\n{currentHour:D2}:00";
    }

    public void AddOneHour()
    {
        UpdateTimerText(1);
    }

    public void MinusOneHour()
    {
        currentHour--;
        if (currentHour < 0)
        {
            currentDay--;
            currentHour = 23;
        }
        timerText.text = $"Day {currentDay}\n{currentHour:D2}:00";
    }
}
