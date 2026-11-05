using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class ExhibitInteraction : MonoBehaviour
{
    [SerializeField]
    private string exhibitTitle = "Avant Armour";

    [SerializeField, TextArea(3, 8)]
    private string description =
        "Made in Milan, Italy, around 1440-1445.\n\n"
        + "An early example of European plate armour.";

    [SerializeField]
    private ExhibitPanelController panelController;

    [SerializeField]
    private TMP_Text interactionPrompt;

    private readonly HashSet<Collider2D> players = new();
    private PlayerMovement playerMovement;

    private bool CanInteract =>
        players.Count > 0
        && playerMovement != null
        && playerMovement.isActiveAndEnabled
        && panelController != null
        && !panelController.IsOpen;

    private void Start()
    {
        RefreshPrompt();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerMovement movement = other.GetComponentInParent<PlayerMovement>();

        if (movement == null)
            return;

        players.Add(other);
        playerMovement = movement;
        RefreshPrompt();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        players.Remove(other);

        if (players.Count == 0)
            playerMovement = null;

        RefreshPrompt();
    }

    private void Update()
    {
        RefreshPrompt();

        if (!CanInteract || !Application.isFocused)
            return;

        Keyboard keyboard = Keyboard.current;

        if (keyboard != null && keyboard.eKey.wasPressedThisFrame)
        {
            panelController.OpenDetails(exhibitTitle, description);
            RefreshPrompt();
        }
    }

    private void RefreshPrompt()
    {
        if (interactionPrompt != null)
            interactionPrompt.gameObject.SetActive(CanInteract);
    }

    private void OnDisable()
    {
        players.Clear();
        playerMovement = null;
        RefreshPrompt();
    }
}
