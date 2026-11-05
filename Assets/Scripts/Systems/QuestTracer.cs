using System;
using UnityEngine;

public class QuestTracker : MonoBehaviour
{
    [Serializable]
    private class Quest
    {
        public string id;
        public GameObject row;
        public bool activeAtStart;
        public string nextQuestId;

        [NonSerialized] public bool active;
        [NonSerialized] public bool completed;
    }

    [SerializeField] private Quest[] quests = Array.Empty<Quest>();

    private void Awake()
    {
        foreach (Quest quest in quests)
        {
            quest.completed = false;
            SetActive(quest, quest.activeAtStart);
        }
    }

    public void Complete(string questId)
    {
        Quest quest = FindQuest(questId);
        if (quest == null || !quest.active || quest.completed)
            return;

        quest.completed = true;
        SetActive(quest, false);

        if (string.IsNullOrWhiteSpace(quest.nextQuestId))
            return;

        Quest next = FindQuest(quest.nextQuestId);
        if (next != null && !next.completed)
            SetActive(next, true);
    }

    private Quest FindQuest(string questId)
    {
        foreach (Quest quest in quests)
        {
            if (quest.id == questId)
                return quest;
        }

        return null;
    }

    private void SetActive(Quest quest, bool active)
    {
        quest.active = active;

        if (quest.row != null)
            quest.row.SetActive(active);
    }
}