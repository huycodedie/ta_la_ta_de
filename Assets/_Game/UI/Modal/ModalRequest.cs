using System;

namespace WuxiaGame.UI.Modal
{
    /// <summary>
    /// Modal priority levels for deterministic modal scheduling.
    /// Higher numerical values indicate higher scheduling priority.
    /// </summary>
    public enum ModalPriority
    {
        Informational = 0,     // Tooltips, info panels, tutorial notices
        SystemProgression = 1, // Loot tier upgrades, Title breakthroughs (e.g. LootTierProgressionUI, TitleBreakthroughUI)
        CriticalGameplay = 2   // Loot decisions, combat blocking prompts (e.g. LootDecisionUI)
    }

    /// <summary>
    /// Encapsulates a request to display a modal in the ModalCoordinator queue.
    /// </summary>
    public class ModalRequest
    {
        private static long _nextRequestId = 1;

        public long RequestId { get; private set; }
        public string ModalId { get; set; }
        public ModalPriority Priority { get; set; }
        public bool IsDismissable { get; set; }
        public object Payload { get; set; }

        public Action<ModalRequest> OnShown { get; set; }
        public Action<ModalRequest, DismissalReason> OnDismissed { get; set; }
        public Action<ModalRequest> OnCompleted { get; set; }

        public ModalRequest(string modalId, ModalPriority priority, bool isDismissable = true, object payload = null)
        {
            RequestId = _nextRequestId++;
            ModalId = modalId;
            Priority = priority;
            IsDismissable = isDismissable;
            Payload = payload;
        }

        public static void ResetSequence()
        {
            _nextRequestId = 1;
        }
    }
}
