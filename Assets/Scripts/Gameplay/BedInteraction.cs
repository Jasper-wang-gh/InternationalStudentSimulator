using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class BedInteraction : MonoBehaviour
{
    [SerializeField] private GameState gameState;
    [SerializeField] private TMP_Text sleepPrompt;
    [SerializeField] private QuestTracker questTracker;
    

    private readonly HashSet<Collider2D> players = new();

    // 离开床的范围后，才能再次睡眠。
    private bool sleptThisVisit;

    private void Start()
    {
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

        if (players.Count == 0)
            sleptThisVisit = false;

        RefreshPrompt();
    }

    private void Update()
    {
        if (players.Count == 0 || sleptThisVisit)
            return;

        Keyboard keyboard = Keyboard.current;

        if (keyboard == null || !keyboard.eKey.wasPressedThisFrame)
            return;

        // 学习面板打开、角色移动被禁用时，不触发睡眠。
        bool canInteract = false;

        foreach (Collider2D player in players)
        {
            if (player == null) continue;

            PlayerMovement movement =
                player.GetComponentInParent<PlayerMovement>();

            if (movement != null && movement.isActiveAndEnabled)
            {
                canInteract = true;
                break;
            }
        }

        if (!canInteract) return;

        if (gameState == null)
        {
            Debug.LogError("BedInteraction 尚未连接 GameState。");
            return;
        }
        bool success = gameState.TrySleep(out string message);

        // 只有睡眠成功，才限制本次停留期间再次睡眠。
        sleptThisVisit = success;

        if (sleepPrompt != null)
        {
            sleepPrompt.gameObject.SetActive(true);
            sleepPrompt.text = message;
        }

        if (success && questTracker != null)
        questTracker.Complete("rest");
    }

    private void RefreshPrompt()
    {
        if (sleepPrompt == null) return;

        sleepPrompt.gameObject.SetActive(players.Count > 0);

        sleepPrompt.text = sleptThisVisit
            ? "Rested! Leave the bed area to sleep again."
            : "Press E to sleep (8 hours)";
    }

    private void OnDisable()
    {
        players.Clear();
        sleptThisVisit = false;

        if (sleepPrompt != null)
            sleepPrompt.gameObject.SetActive(false);
    }
}