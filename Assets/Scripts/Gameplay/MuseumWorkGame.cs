using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// The receptionist starts this modal activity. All resource changes go through GameState.
public class MuseumWorkGame : MonoBehaviour
{
    public const int EnergyCost = 20;
    public const int SatietyCost = 10;
    public const int WorkMinutes = 120;
    public const int QuestionCount = 3;
    public const int BasePay = 12;
    private static readonly (string Name, int SecondsOffset, int Bonus)[] Difficulties =
    {
        ("Easy", 6, 3),
        ("Normal", 0, 4),
        ("Hard", -4, 6),
    };

    public bool IsOpen => phase != Phase.Closed;

    [Serializable]
    public class GuideFact
    {
        public string title;

        [TextArea(2, 5)]
        public string description;
        public string question;
        public string correctAnswer;
        public string wrongAnswerOne;
        public string wrongAnswerTwo;
    }

    [SerializeField]
    private GameState gameState;

    [SerializeField]
    private PlayerMovement playerMovement;

    [SerializeField]
    private ReceptionInteraction reception;

    [SerializeField]
    private GameObject overlay;

    [SerializeField]
    private TMP_Text stageText;

    [SerializeField]
    private TMP_Text statusText;

    [SerializeField]
    private TMP_Text bodyText;

    [SerializeField]
    private TMP_Text countdownText;

    [SerializeField]
    private GameObject memoryCards;

    [SerializeField]
    private TMP_Text[] factTitles = new TMP_Text[3];

    [SerializeField]
    private TMP_Text[] factDescriptions = new TMP_Text[3];

    [SerializeField]
    private Button[] answerButtons = new Button[3];

    [SerializeField]
    private TMP_Text feedbackText;

    [SerializeField]
    private Button primaryButton;

    [SerializeField]
    private Button secondaryButton;

    [SerializeField, Min(1)]
    private float memoriseSeconds = 10f;

    [SerializeField]
    private GuideFact[] facts =
    {
        new GuideFact
        {
            title = "Avant Armour",
            description =
                "Early plate armour made in Milan, Italy, around 1440-1445.",
            question = "A visitor asks: Which city was the Avant Armour made in?",
            correctAnswer = "Milan",
            wrongAnswerOne = "Paris",
            wrongAnswerTwo = "Edinburgh",
        },
        new GuideFact
        {
            title = "Alexander Reid",
            description =
                "Vincent van Gogh painted art dealer Alexander Reid in 1887.",
            question = "A visitor asks: Who painted the portrait of Alexander Reid?",
            correctAnswer = "Vincent van Gogh",
            wrongAnswerOne = "Claude Monet",
            wrongAnswerTwo = "Pablo Picasso",
        },
        new GuideFact
        {
            title = "Shabti",
            description =
                "A small funerary figurine from ancient Egypt.",
            question = "A visitor asks: Which ancient civilisation made shabti figurines?",
            correctAnswer = "Ancient Egypt",
            wrongAnswerOne = "Ancient Rome",
            wrongAnswerTwo = "Ancient Greece",
        },
    };

    private enum Phase
    {
        Closed,
        Difficulty,
        Offer,
        Memorise,
        Question,
        Feedback,
        Result,
        ConfirmAbort,
    }

    private Phase phase;
    private Phase phaseBeforeAbort;
    private float remainingSeconds;
    private GuideFact[] shuffledFacts;
    private int difficultyIndex;
    private int AnswerBonus => Difficulties[difficultyIndex].Bonus;

    private float StudySeconds(int index) =>
        Mathf.Max(1f, memoriseSeconds + Difficulties[index].SecondsOffset);
    private readonly string[] choices = new string[3];
    private int questionIndex;
    private int correctChoiceIndex;
    private int selectedChoiceIndex;
    private int correctAnswers;
    private int earnedWage;
    private bool lastAnswerCorrect;
    private bool ownsMovementLock;
    private bool settlementSucceeded;
    private bool shiftInProgress;
    private static readonly Color Ink = new Color32(35, 43, 54, 255);
    private static readonly Color Success = new Color32(30, 107, 68, 255);
    private static readonly Color Failure = new Color32(159, 53, 43, 255);

    private bool HasActiveShift =>
        phase == Phase.Memorise
        || phase == Phase.Question
        || phase == Phase.Feedback
        || phase == Phase.ConfirmAbort;

    private void Awake()
    {
        if (primaryButton != null)
            primaryButton.onClick.AddListener(HandlePrimary);
        if (secondaryButton != null)
            secondaryButton.onClick.AddListener(HandleSecondary);
        for (int i = 0; i < answerButtons.Length; i++)
        {
            int choice = i;
            if (answerButtons[i] != null)
                answerButtons[i].onClick.AddListener(() => SelectOption(choice));
        }
    }

    private void OnEnable()
    {
        if (gameState != null)
            gameState.Changed += RefreshStatus;
    }

    private void Start()
    {
        if (overlay != null)
            overlay.SetActive(false);
    }

    private void Update()
    {
        if (phase == Phase.Closed || !Application.isFocused)
            return;
        if (phase == Phase.Memorise)
        {
            remainingSeconds = Mathf.Max(0f, remainingSeconds - Time.unscaledDeltaTime);
            countdownText.text = $"Fact sheet closes in {Mathf.CeilToInt(remainingSeconds)} s";
            if (remainingSeconds <= 0f)
                PrepareQuestion();
        }

        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
        {
            if (phase == Phase.ConfirmAbort)
                ResumeShift();
            else if (HasActiveShift)
                ConfirmAbort();
            else if (phase == Phase.Offer)
                ReturnToDifficulty();
            else
                ClosePanel();
        }
    }

    public void OpenOffer()
    {
        if (phase != Phase.Closed)
            return;
        if (!ReferencesReady())
        {
            Debug.LogError("MuseumWorkGame: connect the game state, player and work UI.", this);
            return;
        }

        bool fromReception = reception != null && reception.IsOpen;
        if (!fromReception && !playerMovement.isActiveAndEnabled)
            return;
        if (fromReception)
            reception.CloseMenu();
        ownsMovementLock = true;
        playerMovement.enabled = false;
        ReturnToDifficulty();
    }

    private void ReturnToDifficulty()
    {
        phase = Phase.Difficulty;
        Render();
    }

    private bool ReferencesReady()
    {
        if (
            gameState == null
            || playerMovement == null
            || overlay == null
            || stageText == null
            || statusText == null
            || bodyText == null
            || countdownText == null
            || memoryCards == null
            || feedbackText == null
            || primaryButton == null
            || secondaryButton == null
            || facts == null
            || facts.Length < QuestionCount
            || answerButtons.Length != 3
            || factTitles.Length != 3
            || factDescriptions.Length != 3
        )
            return false;
        for (int i = 0; i < 3; i++)
            if (
                answerButtons[i] == null
                || factTitles[i] == null
                || factDescriptions[i] == null
            )
                return false;
        foreach (GuideFact fact in facts)
            if (fact == null)
                return false;
        return true;
    }

    private void SelectOption(int index)
    {
        if (phase == Phase.Difficulty)
        {
            difficultyIndex = index;
            phase = Phase.Offer;
            Render();
        }
        else
            Answer(index);
    }

    private void StartShift()
    {
        if (phase != Phase.Offer)
            return;
        if (
            !gameState.TryConsumeResources(EnergyCost, SatietyCost, WorkMinutes, out string message)
        )
        {
            feedbackText.text = message;
            feedbackText.color = Failure;
            return;
        }

        shiftInProgress = true;
        settlementSucceeded = false;
        earnedWage = 0;
        correctAnswers = 0;
        questionIndex = 0;
        shuffledFacts = (GuideFact[])facts.Clone();
        Shuffle(shuffledFacts);
        remainingSeconds = StudySeconds(difficultyIndex);
        phase = Phase.Memorise;
        Render();
    }

    private void PrepareQuestion()
    {
        GuideFact fact = shuffledFacts[questionIndex];
        choices[0] = fact.correctAnswer;
        choices[1] = fact.wrongAnswerOne;
        choices[2] = fact.wrongAnswerTwo;
        Shuffle(choices);
        correctChoiceIndex = Array.IndexOf(choices, fact.correctAnswer);
        selectedChoiceIndex = -1;
        phase = Phase.Question;
        Render();
    }

    private static void Shuffle<T>(T[] items)
    {
        for (int i = items.Length - 1; i > 0; i--)
        {
            int other = UnityEngine.Random.Range(0, i + 1);
            (items[i], items[other]) = (items[other], items[i]);
        }
    }

    private void Answer(int index)
    {
        if (phase != Phase.Question || index < 0 || index >= choices.Length)
            return;
        // Lock the question before updating its score, so a double click cannot score twice.
        phase = Phase.Feedback;
        selectedChoiceIndex = index;
        lastAnswerCorrect = index == correctChoiceIndex;
        if (lastAnswerCorrect)
            correctAnswers++;
        Render();
    }

    private void FinishShift()
    {
        if (phase != Phase.Feedback)
            return;
        phase = Phase.Result;
        settlementSucceeded = shiftInProgress;
        shiftInProgress = false;
        earnedWage = settlementSucceeded
            ? BasePay + correctAnswers * AnswerBonus
            : 0;
        if (settlementSucceeded)
            gameState.AddMoney(earnedWage);
        Render();
    }

    private void HandlePrimary()
    {
        switch (phase)
        {
            case Phase.Offer:
                StartShift();
                break;
            case Phase.Memorise:
                PrepareQuestion();
                break;
            case Phase.Feedback:
                if (questionIndex + 1 == QuestionCount)
                    FinishShift();
                else
                {
                    questionIndex++;
                    PrepareQuestion();
                }
                break;
            case Phase.Result:
                ClosePanel();
                break;
            case Phase.ConfirmAbort:
                ResumeShift();
                break;
        }
    }

    private void HandleSecondary()
    {
        if (phase == Phase.Offer)
            ReturnToDifficulty();
        else if (phase == Phase.Difficulty)
            ClosePanel();
        else if (phase == Phase.ConfirmAbort)
        {
            shiftInProgress = false;
            ClosePanel();
        }
        else if (HasActiveShift)
            ConfirmAbort();
    }

    private void ConfirmAbort()
    {
        phaseBeforeAbort = phase;
        phase = Phase.ConfirmAbort;
        Render();
    }

    private void ResumeShift()
    {
        phase = phaseBeforeAbort;
        Render();
    }

    private void ClosePanel()
    {
        phase = Phase.Closed;
        if (overlay != null)
            overlay.SetActive(false);
        ReleaseMovement();
    }

    private void ReleaseMovement()
    {
        if (ownsMovementLock && playerMovement != null)
            playerMovement.enabled = true;
        ownsMovementLock = false;
    }

    private void RefreshStatus()
    {
        if (statusText == null || gameState == null)
            return;
        int total = gameState.TotalMinutes;
        statusText.text =
            $"Energy {gameState.Energy}/100   |   Satiety {gameState.Satiety}/100"
            + $"   |   Day {total / 1440 + 1}  {total % 1440 / 60:00}:{total % 60:00}"
            + $"   |   £{gameState.Money}";
    }

    private void Render()
    {
        overlay.SetActive(true);
        RefreshStatus();
        memoryCards.SetActive(false);
        countdownText.gameObject.SetActive(false);
        foreach (Button button in answerButtons)
        {
            button.gameObject.SetActive(false);
            button.interactable = true;
            button.image.color = Color.white;
        }
        primaryButton.gameObject.SetActive(true);
        secondaryButton.gameObject.SetActive(true);
        feedbackText.text = "";
        feedbackText.color = Ink;
        bodyText.alignment = TextAlignmentOptions.Center;
        SetBodyLayout(0, 285);
        Label(secondaryButton, "End shift");

        switch (phase)
        {
            case Phase.Difficulty:
                stageText.text = "CHOOSE SHIFT DIFFICULTY";
                SetBodyLayout(112, 95);
                bodyText.text = "Each shift picks three exhibits from the museum's guide notes.";
                for (int i = 0; i < answerButtons.Length; i++)
                {
                    var difficulty = Difficulties[i];
                    answerButtons[i].gameObject.SetActive(true);
                    Label(
                        answerButtons[i],
                        $"{difficulty.Name}  |  {Mathf.CeilToInt(StudySeconds(i))} s  |  +£{difficulty.Bonus} per correct"
                    );
                }
                primaryButton.gameObject.SetActive(false);
                Label(secondaryButton, "Return to museum");
                break;
            case Phase.Offer:
                stageText.text = $"SHIFT BRIEFING  /  {Difficulties[difficultyIndex].Name.ToUpperInvariant()}";
                bodyText.text =
                    "Help three visitors learn about the exhibits.\n\n"
                    + $"Study for {Mathf.CeilToInt(StudySeconds(difficultyIndex))} seconds, then answer three multiple-choice questions.\n\n"
                    + $"Cost: {EnergyCost} energy, {SatietyCost} satiety"
                    + $" and {WorkMinutes / 60} game hours.\n"
                    + $"Pay: £{BasePay} + £{AnswerBonus} per correct answer.\n\n"
                    + "Resources are spent only when you start the shift.";
                Label(primaryButton, "Start shift");
                Label(secondaryButton, "Back");
                break;
            case Phase.Memorise:
                stageText.text = "MEMORISE THE EXHIBITS";
                SetBodyLayout(118, 55);
                bodyText.text = "Remember the facts below. Your visitors will ask about them.";
                memoryCards.SetActive(true);
                countdownText.gameObject.SetActive(true);
                countdownText.text = $"Fact sheet closes in {Mathf.CeilToInt(remainingSeconds)} s";
                for (int i = 0; i < QuestionCount; i++)
                {
                    factTitles[i].text = shuffledFacts[i].title;
                    factDescriptions[i].text = shuffledFacts[i].description;
                }
                Label(primaryButton, "I'm ready");
                break;
            case Phase.Question:
            case Phase.Feedback:
                RenderQuestion();
                break;
            case Phase.Result:
                stageText.text = "SHIFT COMPLETE";
                bodyText.text = settlementSucceeded
                    ? $"You helped all three visitors.\n\nCorrect answers: {correctAnswers}/{QuestionCount}\n\n"
                        + $"Basic wage: £{BasePay}\n"
                        + $"Answer bonus: £{correctAnswers * AnswerBonus}\n\n"
                        + $"<size=34><b>Total earned: £{earnedWage}</b></size>"
                    : "This shift could not be settled. Return to the museum and try again.";
                feedbackText.text = "Your time, energy and satiety were charged once at the start.";
                Label(primaryButton, "Return to museum");
                secondaryButton.gameObject.SetActive(false);
                break;
            case Phase.ConfirmAbort:
                stageText.text = "END THIS SHIFT EARLY?";
                bodyText.text =
                    "You can return to the museum now, but this shift will pay no wage.\n\n"
                    + "The time, energy and satiety already spent will not be refunded.\n\n"
                    + "Your fact-sheet timer is paused while you decide.";
                Label(primaryButton, "Keep working");
                Label(secondaryButton, "End without pay");
                break;
        }
    }

    private void RenderQuestion()
    {
        stageText.text = $"VISITOR {questionIndex + 1} OF {QuestionCount}   /   {correctAnswers} CORRECT";
        SetBodyLayout(112, 95);
        bodyText.text = shuffledFacts[questionIndex].question;
        for (int i = 0; i < answerButtons.Length; i++)
        {
            answerButtons[i].gameObject.SetActive(true);
            Label(answerButtons[i], choices[i]);
            answerButtons[i].interactable = phase == Phase.Question;
            if (phase == Phase.Feedback)
            {
                if (i == correctChoiceIndex)
                    answerButtons[i].image.color = new Color32(191, 225, 203, 255);
                else if (i == selectedChoiceIndex)
                    answerButtons[i].image.color = new Color32(241, 202, 195, 255);
            }
        }
        primaryButton.gameObject.SetActive(phase == Phase.Feedback);
        if (phase == Phase.Feedback)
        {
            feedbackText.text = lastAnswerCorrect
                ? "Correct! Well explained."
                : $"The correct answer is {choices[correctChoiceIndex]}.";
            feedbackText.color = lastAnswerCorrect ? Success : Failure;
            Label(primaryButton, questionIndex + 1 == QuestionCount ? "Finish shift" : "Next visitor");
        }
    }

    private void SetBodyLayout(float y, float height)
    {
        RectTransform rect = bodyText.rectTransform;
        rect.anchoredPosition = new Vector2(0, y);
        rect.sizeDelta = new Vector2(690, height);
    }

    private static void Label(Button button, string text)
    {
        TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
        if (label != null)
            label.text = text;
    }

    private void OnDisable()
    {
        if (gameState != null)
        {
            gameState.Changed -= RefreshStatus;
        }
        phase = Phase.Closed;
        shiftInProgress = false;
        if (overlay != null)
            overlay.SetActive(false);
        ReleaseMovement();
    }
}
