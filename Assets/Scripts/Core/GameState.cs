using System;
using System.Collections.Generic;
using UnityEngine;

public class GameState : MonoBehaviour
{
    [SerializeField, Range(0, 100)]
    private int energy = 100;

    [SerializeField, Range(0, 100)]
    private int mood = 100;

    [SerializeField, Range(0, 100)]
    private int stress;

    private float stressMultiplier = 1f;
    private bool backgroundChosen;

    [SerializeField, Range(0, 100)]
    private int satiety = 100;

    [SerializeField, Range(0, 100)]
    private int courseProgress;

    //金钱系统
    [SerializeField, Min(0)]
    private int money = 0;

    [SerializeField, Min(0)]
    private int totalMinutes = 480;

    public int Energy => energy;
    public int Mood => mood;
    public int Stress => stress;
    public int Satiety => satiety;
    public int CourseProgress => courseProgress;

    //钱
    public int Money => money;
    public int TotalMinutes => totalMinutes;

    public event Action Changed;

    public bool TryChooseStartingBackground(int startingMoney, float pressureMultiplier)
    {
        if (backgroundChosen || startingMoney < 0 || pressureMultiplier < 0f ||
            float.IsNaN(pressureMultiplier) || float.IsInfinity(pressureMultiplier))
            return false;

        backgroundChosen = true;
        money = startingMoney;
        stressMultiplier = pressureMultiplier;
        Changed?.Invoke();
        return true;
    }

    // Shared by activities that spend resources and advance the game clock.
    // Validate everything before applying any changes, then refresh the HUD once.
    public bool TryConsumeResources(
        int energyCost,
        int satietyCost,
        int minutes,
        out string message)
    {
        if (energyCost < 0 || satietyCost < 0 || minutes < 0)
        {
            message = "Resource costs and time must not be negative.";
            return false;
        }

        if (energy < energyCost)
        {
            message = $"Not enough energy. You need {energyCost}.";
            return false;
        }

        if (satiety < satietyCost)
        {
            message = $"You need food first. At least {satietyCost} satiety is required.";
            return false;
        }

        energy -= energyCost;
        satiety -= satietyCost;
        totalMinutes += minutes;
        message = "Activity started.";
        Changed?.Invoke();
        return true;
    }

    //钱
    public void AddMoney(int amount)
    {
        if (amount <= 0)
            return;

        money += amount;
        Changed?.Invoke();
    }
    //体力，钱 和人的交互
    public bool TryMuseumWork(out string message)
    {
        const int energyCost = 20;
        const int wage = 8;

        if (energy < energyCost)
        {
            message = "Not enough energy! You need 20.";
            return false;
        }

        energy -= energyCost;
        AddMoney(wage);

        message = "Work finished! Energy -20, Money +£8.";
        return true;
    }

    public bool TryStudy(out string message)
    {
        if (courseProgress >= 100)
        {
            message = "Course completed!";
            return false;
        }

        var warnings = new List<string>();

        if (energy < 15)
            warnings.Add("You need rest.");

        if (mood < 5)
            warnings.Add("You need to improve your mood.");

        if (satiety < 10)
            warnings.Add("You need food.");

        if (warnings.Count > 0)
        {
            message = string.Join("\n", warnings);
            return false;
        }

        // 所有条件满足后，才统一结算。
        energy = Mathf.Clamp(energy - 15, 0, 100);
        mood = Mathf.Clamp(mood - 5, 0, 100);
        const int studyStress = 8;
        int stressGain = Mathf.RoundToInt(Mathf.Min(100, studyStress * stressMultiplier));
        stress = Mathf.Min(100, stress + stressGain);
        satiety = Mathf.Clamp(satiety - 10, 0, 100);

        courseProgress = Mathf.Min(courseProgress + 10, 100);
        totalMinutes += 120;

        message =
            courseProgress >= 100
                ? "Study completed! Course completed!"
                : "Study completed! Progress +10%.";

        Changed?.Invoke();
        return true;
    }

    public bool TrySleep(out string message)
    {
        if (satiety < 20)
        {
            message = "You need food before sleeping.";
            return false;
        }

        energy = Mathf.Clamp(energy + 60, 0, 100);
        mood = Mathf.Clamp(mood + 15, 0, 100);
        stress = Mathf.Max(0, stress - 15);
        satiety = Mathf.Clamp(satiety - 20, 0, 100);

        totalMinutes += 8 * 60;

        message = "Rested! Leave the bed area to sleep again.";

        Changed?.Invoke();
        return true;
    }
}
