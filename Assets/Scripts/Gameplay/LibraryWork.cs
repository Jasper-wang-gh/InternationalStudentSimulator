using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class LibraryWork : MonoBehaviour
{
    [SerializeField] private PlayerMovement player;
    [SerializeField] private GameState gameState;

    [Header("Interaction zones")]
    [SerializeField] private BoxCollider2D receptionZone;
    [SerializeField] private BoxCollider2D returnDeskZone;
    [SerializeField] private BoxCollider2D computingShelfZone;
    [SerializeField] private BoxCollider2D historyShelfZone;
    [SerializeField] private BoxCollider2D economicsShelfZone;

    [Header("Library UI")]
    [SerializeField] private TMP_Text workText;

    private const int EnergyCost = 20;
    private const int SatietyCost = 10;
    private const int WorkMinutes = 120;
    private const int Wage = 22;

    private enum ShiftStage
    {
        NotStarted,
        CollectBooks,
        ShelveBooks,
        CollectPay
    }

    private enum Shelf
    {
        Computing,
        History,
        Economics
    }

    private static readonly (string Title, Shelf Destination)[] Books =
    {
        ("Python for Beginners", Shelf.Computing),
        ("A Short History of Scotland", Shelf.History),
        ("Introduction to Economics", Shelf.Economics)
    };

    private ShiftStage stage;
    private int booksSorted;

    private void Awake()
    {
        if (workText != null)
            workText.text = "";
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (!Application.isFocused || keyboard == null ||
            !keyboard.eKey.wasPressedThisFrame ||
            player == null || !player.isActiveAndEnabled ||
            gameState == null)
        {
            return;
        }

        Vector2 position = player.transform.position;

        if (IsNear(receptionZone, position))
        {
            TalkToReception();
            return;
        }

        if (stage == ShiftStage.CollectBooks && IsNear(returnDeskZone, position))
        {
            stage = ShiftStage.ShelveBooks;
            ShowCurrentBook();
            return;
        }

        if (stage != ShiftStage.ShelveBooks)
            return;

        if (IsNear(computingShelfZone, position))
            ShelveCurrentBook(Shelf.Computing);
        else if (IsNear(historyShelfZone, position))
            ShelveCurrentBook(Shelf.History);
        else if (IsNear(economicsShelfZone, position))
            ShelveCurrentBook(Shelf.Economics);
    }

    private void TalkToReception()
    {
        switch (stage)
        {
            case ShiftStage.NotStarted:
                if (!gameState.TryConsumeResources(
                        EnergyCost, SatietyCost, WorkMinutes, out string message))
                {
                    ShowMessage(message);
                    return;
                }

                booksSorted = 0;
                stage = ShiftStage.CollectBooks;
                ShowMessage($"Shift started. Collect {Books.Length} books at the return desk.");
                break;

            case ShiftStage.CollectBooks:
                ShowMessage("Collect the books at the return desk first.");
                break;

            case ShiftStage.ShelveBooks:
                ShowCurrentBook();
                break;

            case ShiftStage.CollectPay:
                stage = ShiftStage.NotStarted;
                gameState.AddMoney(Wage);
                ShowMessage($"Shift complete. You earned £{Wage}.");
                break;
        }
    }

    private void ShelveCurrentBook(Shelf shelf)
    {
        var currentBook = Books[booksSorted];

        if (currentBook.Destination != shelf)
        {
            ShowCurrentBook("Wrong shelf. Try another one.");
            return;
        }

        booksSorted++;

        if (booksSorted == Books.Length)
        {
            stage = ShiftStage.CollectPay;
            ShowMessage(
                $"Books sorted: {booksSorted}/{Books.Length}\n" +
                "Return to reception on Level 2 to collect your pay."
            );
            return;
        }

        ShowCurrentBook("Book shelved!");
    }

    private void ShowCurrentBook(string feedback = "")
    {
        string message =
            $"Books sorted: {booksSorted}/{Books.Length}\n" +
            $"Current book: {Books[booksSorted].Title}";

        if (!string.IsNullOrEmpty(feedback))
            message += "\n" + feedback;

        ShowMessage(message);
    }

    private void ShowMessage(string message)
    {
        if (workText != null)
            workText.text = message;

        Debug.Log(message);
    }

    private static bool IsNear(BoxCollider2D zone, Vector2 position)
    {
        return zone != null && zone.isActiveAndEnabled &&
               zone.OverlapPoint(position);
    }
}