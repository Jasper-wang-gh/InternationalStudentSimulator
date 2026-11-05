using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DeskInteraction : MonoBehaviour
{
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private GameObject studyPanel;

    private readonly HashSet<Collider2D> players = new();
    private PlayerMovement playerMovement;
    private bool isOpen;

    private void Start()
    {
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);

        if (studyPanel != null)
            studyPanel.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerMovement movement =
            other.GetComponentInParent<PlayerMovement>();

        if (movement == null) return;

        players.Add(other);
        playerMovement = movement;
        RefreshPrompt();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        players.Remove(other);
        RefreshPrompt();
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (isOpen)
        {
            if (keyboard.escapeKey.wasPressedThisFrame)
                CloseStudy();

            return;
        }

        if (players.Count > 0 && keyboard.eKey.wasPressedThisFrame)
            OpenStudy();
    }

    private void OpenStudy()
    {
        if (studyPanel == null || playerMovement == null) return;

        isOpen = true;
        playerMovement.enabled = false;
        studyPanel.SetActive(true);
        RefreshPrompt();
    }

    public void CloseStudy()
    {
        isOpen = false;

        if (studyPanel != null)
            studyPanel.SetActive(false);

        if (playerMovement != null)
            playerMovement.enabled = true;

        RefreshPrompt();
    }

    private void RefreshPrompt()
    {
        if (interactionPrompt != null)
            interactionPrompt.SetActive(players.Count > 0 && !isOpen);
    }

    private void OnDisable()
    {
        CloseStudy();
        players.Clear();

        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }
}