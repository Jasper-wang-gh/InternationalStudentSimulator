using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatusHUD : MonoBehaviour
{
    [SerializeField] private GameState gameState;

    [SerializeField] private TMP_Text timeText;
    //钱
    [SerializeField] private TMP_Text moneyText;
    [SerializeField] private TMP_Text stressText;

    [SerializeField] private Slider energyBar;
    [SerializeField] private TMP_Text energyText;

    [SerializeField] private Slider moodBar;
    [SerializeField] private TMP_Text moodText;

    [SerializeField] private Slider satietyBar;
    [SerializeField] private TMP_Text satietyText;

    private void OnEnable()
    {
        if (gameState == null) return;

        gameState.Changed += RefreshDisplay;
        RefreshDisplay();
    }

    private void OnDisable()
    {
        if (gameState != null)
            gameState.Changed -= RefreshDisplay;
    }

    private void RefreshDisplay()
    {
        int total = gameState.TotalMinutes;
        int day = total / 1440 + 1;
        int hour = total % 1440 / 60;
        int minute = total % 60;

        if (timeText != null)
            timeText.text = $"Day {day}\n{hour:00}:{minute:00}";

        SetStat(energyBar, energyText, gameState.Energy);
        SetStat(moodBar, moodText, gameState.Mood);
        SetStat(satietyBar, satietyText, gameState.Satiety);
        //看余额
        if (moneyText != null)
            moneyText.text = $"Money: £{gameState.Money}";
        if (stressText != null)
            stressText.text = $"Stress: {gameState.Stress}/100";
    }

    private void SetStat(Slider bar, TMP_Text text, int value)
    {
        if (bar != null)
        {
            bar.minValue = 0;
            bar.maxValue = 100;
            bar.wholeNumbers = true;
            bar.value = value;
        }

        if (text != null)
            text.text = $"{value}/100";
    }
}