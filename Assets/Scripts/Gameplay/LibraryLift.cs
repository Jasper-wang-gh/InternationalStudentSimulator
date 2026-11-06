using UnityEngine;
using UnityEngine.InputSystem;

public class LibraryLift : MonoBehaviour
{
    [SerializeField] private PlayerMovement player;

    [SerializeField] private GameObject lobbyFloor;
    [SerializeField] private BoxCollider2D lobbyLiftZone;
    [SerializeField] private Transform lobbyLiftSpawn;

    [SerializeField] private GameObject stacksFloor;
    [SerializeField] private BoxCollider2D stacksLiftZone;
    [SerializeField] private Transform stacksLiftSpawn;

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (!Application.isFocused || keyboard == null ||
            !keyboard.eKey.wasPressedThisFrame ||
            player == null || !player.isActiveAndEnabled)
            return;

        Vector2 playerPosition = player.transform.position;

        if (lobbyFloor.activeInHierarchy &&
            lobbyLiftZone.OverlapPoint(playerPosition))
        {
            ChangeFloor(lobbyFloor, stacksFloor, stacksLiftSpawn);
        }
        else if (stacksFloor.activeInHierarchy &&
                 stacksLiftZone.OverlapPoint(playerPosition))
        {
            ChangeFloor(stacksFloor, lobbyFloor, lobbyLiftSpawn);
        }
    }

    private void ChangeFloor(GameObject from, GameObject to, Transform spawn)
    {
        from.SetActive(false);
        to.SetActive(true);

        Rigidbody2D body = player.GetComponent<Rigidbody2D>();
        body.linearVelocity = Vector2.zero;
        body.position = spawn.position;
    }
}
