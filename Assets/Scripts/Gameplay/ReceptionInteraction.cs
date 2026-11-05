using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class ReceptionInteraction : MonoBehaviour
{
    [SerializeField]
    private TMP_Text interactionPrompt;

    [SerializeField]
    private GameObject receptionMenu;

    [SerializeField]
    private PlayerMovement playerMovement;

    private bool isOpen;

    public bool IsOpen => isOpen;

    private readonly HashSet<Collider2D> players = new();

    private void Start()
    {
        CloseMenu();
    }

    private void Update()
    {
        RefreshPrompt();

        if (!Application.isFocused)
            return;

        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
            return;

        if (isOpen)
        {
            if (keyboard.escapeKey.wasPressedThisFrame)
                CloseMenu();

            return;
        }

        if (
            players.Count > 0
            && playerMovement != null
            && playerMovement.isActiveAndEnabled
            && keyboard.eKey.wasPressedThisFrame
        )
        {
            OpenMenu();
        }
    }

    private void OpenMenu()
    {
        if (receptionMenu == null || playerMovement == null)
            return;

        isOpen = true;
        playerMovement.enabled = false;
        receptionMenu.SetActive(true);
        RefreshPrompt();
    }

    public void CloseMenu()
    {
        if (isOpen && playerMovement != null)
            playerMovement.enabled = true;

        isOpen = false;

        if (receptionMenu != null)
            receptionMenu.SetActive(false);

        RefreshPrompt();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponentInParent<PlayerMovement>() == null)
            return;

        players.Add(other);
        RefreshPrompt();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        players.Remove(other);
        RefreshPrompt();
    }

    private void RefreshPrompt()
    {
        if (interactionPrompt != null)
            interactionPrompt.gameObject.SetActive(
                players.Count > 0
                    && !isOpen
                    && playerMovement != null
                    && playerMovement.isActiveAndEnabled
            );
    }

    private void OnDisable()
    {
        CloseMenu();
        players.Clear();
        RefreshPrompt();
    }
}
