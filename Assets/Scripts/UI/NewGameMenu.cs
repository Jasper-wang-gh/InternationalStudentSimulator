using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NewGameMenu : MonoBehaviour
{
    [Serializable]
    private class Background
    {
        public string title;
        [Min(0)] public int startingMoney;
        [Min(0)] public float pressureMultiplier = 1f;
        public Button button;
        public TMP_Text label;
    }

    [SerializeField] private GameState gameState;
    [SerializeField] private PlayerMovement player;
    [SerializeField] private GameObject titleScreen;
    [SerializeField] private GameObject backgroundScreen;
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button backButton;
    [SerializeField] private Background[] backgrounds;

    private void Awake()
    {
        foreach (Background background in backgrounds)
        {
            background.label.text =
                $"<size=26><b>{background.title}</b></size>\n\n" +
                $"Starting money  £{background.startingMoney}\n" +
                FormattableString.Invariant($"Stress growth  ×{background.pressureMultiplier:0.##}");
            background.button.onClick.AddListener(() => StartGame(background));
        }

        newGameButton.onClick.AddListener(ShowBackgrounds);
        backButton.onClick.AddListener(ShowTitle);
        player.gameObject.SetActive(false);
    }

    private void Start()
    {
        ShowTitle();
    }

    private void ShowTitle()
    {
        backgroundScreen.SetActive(false);
        titleScreen.SetActive(true);
        newGameButton.Select();
    }

    private void ShowBackgrounds()
    {
        titleScreen.SetActive(false);
        backgroundScreen.SetActive(true);
        backButton.Select();
    }

    private void StartGame(Background background)
    {
        if (!gameState.TryChooseStartingBackground(background.startingMoney, background.pressureMultiplier))
            return;

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        player.gameObject.SetActive(true);
        gameObject.SetActive(false);
    }
}
