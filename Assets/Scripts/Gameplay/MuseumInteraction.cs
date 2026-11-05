using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class MuseumInteraction : MonoBehaviour
{
    [SerializeField] private GameState gameState;
    [SerializeField] private TMP_Text museumPrompt;

    private readonly HashSet<Collider2D> players = new();
    private PlayerMovement playerMovement;
    private bool workedThisVisit;

    private void Start()
    {
        RefreshPrompt();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerMovement movement =
            other.GetComponentInParent<PlayerMovement>();

        if (movement == null)
            return;

        players.Add(other);
        playerMovement = movement;
        RefreshPrompt();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!players.Remove(other))
            return;

        if (players.Count == 0)
        {
            playerMovement = null;
            workedThisVisit = false;
        }

        RefreshPrompt();
    }

    private void Update()
    {
        if (players.Count == 0 || workedThisVisit)
            return;

        if (playerMovement == null || !playerMovement.isActiveAndEnabled)
            return;

        if (!Application.isFocused)
            return;

        Keyboard keyboard = Keyboard.current;

        if (keyboard == null || !keyboard.eKey.wasPressedThisFrame)
            return;

        if (gameState == null)
        {
            Debug.LogError("MuseumInteraction 尚未连接 GameState。");
            return;
        }

        workedThisVisit = gameState.TryMuseumWork(out string message);

        if (museumPrompt != null)
        {
            museumPrompt.text = workedThisVisit
                ? message + "\nLeave the museum area to work again."
                : message;
        }
    }

    private void RefreshPrompt()
    {
        if (museumPrompt == null)
            return;

        museumPrompt.gameObject.SetActive(players.Count > 0);

        museumPrompt.text = workedThisVisit
            ? "Leave the museum area to work again."
            : "Press E to work (-20 Energy, +£8)";
    }

    private void OnDisable()
    {
        players.Clear();
        playerMovement = null;
        workedThisVisit = false;

        if (museumPrompt != null)
            museumPrompt.gameObject.SetActive(false);
    }
}