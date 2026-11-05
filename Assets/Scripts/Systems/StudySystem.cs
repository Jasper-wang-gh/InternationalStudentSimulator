using TMPro;
using UnityEngine;

public class StudySystem : MonoBehaviour
{
    [SerializeField] private GameState gameState;
    [SerializeField] private TMP_Text studyInfo;

    private string feedback = "";

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

    public void Study()
    {
        if (gameState == null)
        {
            Debug.LogError("StudySystem 尚未连接 GameState。");
            return;
        }

        gameState.TryStudy(out string message);
        feedback = message;
        RefreshDisplay();
    }

    private void RefreshDisplay()
    {
        if (gameState == null || studyInfo == null) return;

        studyInfo.text =
            $"Course Progress: {gameState.CourseProgress}%\n" +
            feedback;
    }
}