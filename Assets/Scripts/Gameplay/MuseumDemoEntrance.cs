using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Temporary travel for reviewing the museum before the team's map entrance is ready.
// Only the existing player's runtime position changes; other area roots stay intact.
public class MuseumDemoEntrance : MonoBehaviour
{
    [SerializeField]
    private PlayerMovement playerMovement;

    [SerializeField]
    private Transform museumSpawn;

    [SerializeField]
    private Button travelButton;

    [SerializeField]
    private TMP_Text travelLabel;

    [SerializeField]
    private GameObject inputBlocker;

    [SerializeField]
    private ReceptionInteraction reception;

    [SerializeField]
    private ExhibitPanelController exhibitPanel;

    [SerializeField]
    private MuseumWorkGame workGame;

    private Vector3 returnPosition;
    private Vector3 returnCameraPosition;
    private bool hasReturnCamera;

    public bool IsVisiting { get; private set; }

    private bool MuseumModalOpen =>
        (reception != null && reception.IsOpen)
        || (exhibitPanel != null && exhibitPanel.IsOpen)
        || (workGame != null && workGame.IsOpen);

    private bool CanTravel =>
        playerMovement != null
        && playerMovement.isActiveAndEnabled
        && museumSpawn != null
        && !MuseumModalOpen;

    private void Start()
    {
        if (travelButton != null)
            travelButton.onClick.AddListener(ToggleVisit);
        RefreshInterface();
    }

    private void LateUpdate()
    {
        RefreshInterface();
    }

    private void RefreshInterface()
    {
        if (travelButton != null)
        {
            // Do not cover the shared phone/bag or a museum dialog while it owns input.
            travelButton.gameObject.SetActive(
                playerMovement != null && playerMovement.isActiveAndEnabled && !MuseumModalOpen
            );
            travelButton.interactable = CanTravel;
        }
        if (travelLabel != null)
            travelLabel.text = IsVisiting ? "Return to previous area" : "Visit museum (test)";
        if (inputBlocker != null)
            inputBlocker.SetActive(MuseumModalOpen);
    }

    public void ToggleVisit()
    {
        if (IsVisiting)
            ReturnToPreviousArea();
        else
            EnterMuseum();
    }

    public void EnterMuseum()
    {
        if (IsVisiting || !CanTravel)
            return;

        returnPosition = playerMovement.transform.position;
        Camera camera = Camera.main;
        hasReturnCamera = camera != null;
        if (hasReturnCamera)
            returnCameraPosition = camera.transform.position;

        Vector3 destination = museumSpawn.position;
        destination.z = returnPosition.z;
        MovePlayer(destination);
        if (hasReturnCamera)
            camera.transform.position = returnCameraPosition + destination - returnPosition;
        IsVisiting = true;
        RefreshInterface();
    }

    public void ReturnToPreviousArea()
    {
        if (!IsVisiting || !CanTravel)
            return;

        MovePlayer(returnPosition);
        Camera camera = Camera.main;
        if (hasReturnCamera && camera != null)
            camera.transform.position = returnCameraPosition;
        IsVisiting = false;
        RefreshInterface();
    }

    private void MovePlayer(Vector3 position)
    {
        playerMovement.transform.position = position;
        Rigidbody2D body = playerMovement.GetComponent<Rigidbody2D>();
        if (body != null)
        {
            body.position = position;
            body.linearVelocity = Vector2.zero;
        }
        Physics2D.SyncTransforms();
    }

    private void OnDisable()
    {
        if (inputBlocker != null)
            inputBlocker.SetActive(false);
    }

    private void OnDestroy()
    {
        if (travelButton != null)
            travelButton.onClick.RemoveListener(ToggleVisit);
    }
}
