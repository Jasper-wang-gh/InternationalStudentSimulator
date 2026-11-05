using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class ExhibitPanelController : MonoBehaviour
{
    [SerializeField]
    private GameObject exhibitPanel;

    [SerializeField]
    private TMP_Text titleText;

    [SerializeField]
    private TMP_Text descriptionText;

    [SerializeField]
    private PlayerMovement playerMovement;

    private bool isOpen;

    public bool IsOpen => isOpen;

    private void Start()
    {
        CloseDetails();
    }

    private void Update()
    {
        if (!isOpen || !Application.isFocused)
            return;

        Keyboard keyboard = Keyboard.current;

        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            CloseDetails();
    }

    public void OpenDetails(string title, string description)
    {
        if (isOpen || exhibitPanel == null || playerMovement == null)
            return;

        if (!playerMovement.isActiveAndEnabled)
            return;

        if (titleText != null)
            titleText.text = title;

        if (descriptionText != null)
            descriptionText.text = description;

        isOpen = true;
        playerMovement.enabled = false;
        exhibitPanel.SetActive(true);
    }

    public void CloseDetails()
    {
        if (isOpen && playerMovement != null)
            playerMovement.enabled = true;

        isOpen = false;

        if (exhibitPanel != null)
            exhibitPanel.SetActive(false);
    }

    private void OnDisable()
    {
        CloseDetails();
    }
}
