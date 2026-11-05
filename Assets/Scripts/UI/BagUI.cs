using UnityEngine;

public class BagUI : MonoBehaviour
{
    [SerializeField] private GameObject bagPanel;
    [SerializeField] private GameObject modalOverlay;
    [SerializeField] private PlayerMovement playerMovement;

    private bool isOpen;
    private bool restoreMovement;

    private void Start()
    {
        if (bagPanel != null)
            bagPanel.SetActive(false);
    }

    public void Toggle()
    {
        if (!isOpen)
            Open();
    }

    public void Open()
    {
        if (isOpen || bagPanel == null) return;

        // 已有其他窗口占用遮罩时，不再打开。
        if (modalOverlay != null && modalOverlay.activeSelf)
            return;

        restoreMovement =
            playerMovement != null && playerMovement.enabled;

        if (restoreMovement)
            playerMovement.enabled = false;

        if (modalOverlay != null)
        {
            modalOverlay.SetActive(true);
            modalOverlay.transform.SetAsLastSibling();
        }

        bagPanel.SetActive(true);
        bagPanel.transform.SetAsLastSibling();
        isOpen = true;
    }

    public void Close()
    {
        if (!isOpen) return;

        if (bagPanel != null)
            bagPanel.SetActive(false);

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