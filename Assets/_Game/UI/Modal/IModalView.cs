using System;

namespace WuxiaGame.UI.Modal
{
    /// <summary>
    /// Lifecycle dismissal reasons for modal views.
    /// </summary>
    public enum DismissalReason
    {
        UserClosed,          // Close button, Escape key, or backdrop click
        ActionCompleted,     // Primary action completed (e.g. Equip, Dismantle, Upgrade)
        PreemptedByCritical, // Dismissed by incoming CriticalGameplay modal takeover
        SystemDismissed      // Scene transition, reset, or coordinator teardown
    }

    /// <summary>
    /// Contract for blocking modal views managed by ModalCoordinator.
    /// </summary>
    public interface IModalView
    {
        string ModalId { get; }
        ModalPriority DefaultPriority { get; }
        bool IsDismissable { get; }
        bool IsVisible { get; }

        void ShowModal(ModalRequest request = null);
        void HideModal(DismissalReason reason = DismissalReason.UserClosed);
    }
}
