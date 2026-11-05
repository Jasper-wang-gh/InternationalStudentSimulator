using UnityEngine;

public class PhoneUI : MonoBehaviour
{
    [SerializeField] private GameObject phonePanel;
    [SerializeField] private GameObject homePage;
    [SerializeField] private GameObject modalOverlay;
    [SerializeField] private PlayerMovement playerMovement;

    private bool isOpen;
    private bool restoreMovement;

    private void Start()
    {
        if (phonePanel != null)
            phonePanel.SetActive(false);
    }

    public void Toggle()
    {
        if (!isOpen)
            Open();
    }

    public void Open()
    {
        if (isOpen || phonePanel == null) return;

        if (modalOverlay != null && modalOverlay.activeSelf)
            return;

        restoreMovement =
            playerMovement != null && playerMovement.enabled;

        if (restoreMovement)
            playerMovement.enabled = false;

        if (homePage != null)
            homePage.SetActive(true);

        if (modalOverlay != null)
        {
            modalOverlay.SetActive(true);
            modalOverlay.transform.SetAsLastSibling();
        }

        phonePanel.SetActive(true);
        phonePanel.transform.SetAsLastSibling();
        isOpen = true;
    }

    public void Close()
    {
        if (!isOpen) return;

        if (phonePanel != null)
            phonePanel.SetActive(false);

        if (modalOverlay != null)
            modalOverlay.SetActive(false);

        if (restoreMovement && playerMovement != null)
            playerMovement.enabled = true;

        restoreMovement = false;
        isOpen = false;
    }

    private void OnDisable()
    {
        Close();
    }
}