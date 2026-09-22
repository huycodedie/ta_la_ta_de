using System;
using System.Collections.Generic;
using UnityEngine;

namespace WuxiaGame.UI.Modal
{
    /// <summary>
    /// Central coordinator ensuring single-modal exclusivity, deterministic priority queuing (FIFO per priority),
    /// safe critical modal preemption, duplicate rejection, and backdrop raycast blocking.
    /// </summary>
    public class ModalCoordinator : MonoBehaviour
    {
        private static ModalCoordinator _instance;
        public static ModalCoordinator Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = UnityEngine.Object.FindAnyObjectByType<ModalCoordinator>();
                }
                return _instance;
            }
            private set => _instance = value;
        }

        [Header("Modal Layer & Backdrop")]
        [SerializeField] private GameObject modalLayer;
        [SerializeField] private ModalBackdrop backdrop;

        // Deterministic queues per priority - no System.Collections.Generic.PriorityQueue
        private readonly Queue<ModalRequest> _criticalQueue = new Queue<ModalRequest>();
        private readonly Queue<ModalRequest> _progressionQueue = new Queue<ModalRequest>();
        private readonly Queue<ModalRequest> _infoQueue = new Queue<ModalRequest>();

        // Registered views mapped by ModalId
        private readonly Dictionary<string, IModalView> _registeredViews = new Dictionary<string, IModalView>();

        // Active modal state
        private ModalRequest _activeRequest = null;
        private IModalView _activeView = null;
        private bool _isTransitioning = false;

        public int ActiveBlockingModalCount => _activeRequest != null ? 1 : 0;
        public ModalRequest ActiveRequest => _activeRequest;
        public IModalView ActiveView => _activeView;
        public ModalBackdrop Backdrop => backdrop;
        public GameObject ModalLayer => modalLayer;

        public int CriticalQueuedCount => _criticalQueue.Count;
        public int ProgressionQueuedCount => _progressionQueue.Count;
        public int InfoQueuedCount => _infoQueue.Count;
        public int TotalQueuedCount => _criticalQueue.Count + _progressionQueue.Count + _infoQueue.Count;

        public static void ResetInstance()
        {
            _instance = null;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                // Remove/destroy ONLY this duplicate component, NEVER Destroy(gameObject) on shared Canvas!
                if (Application.isPlaying)
                    Destroy(this);
                else
                    DestroyImmediate(this);
                return;
            }
            Instance = this;

            FindReferencesIfMissing();
            UpdateBackdrop(false);
        }

        private void Start()
        {
            FindReferencesIfMissing();
            UpdateBackdrop(_activeRequest != null);
        }

        private void Update()
        {
            // Escape / Android Back dismissal
            bool escapePressed = false;
#if ENABLE_INPUT_SYSTEM
            if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                escapePressed = true;
            }
#elif ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                escapePressed = true;
            }
#endif

            if (escapePressed)
            {
                if (_activeRequest != null && _activeRequest.IsDismissable)
                {
                    DismissActiveModal(DismissalReason.UserClosed);
                }
            }
        }

        private void OnDestroy()
        {
            ClearAll();
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void SetReferences(GameObject layerGO, ModalBackdrop backdropComp)
        {
            modalLayer = layerGO;
            backdrop = backdropComp;
            UpdateBackdrop(_activeRequest != null);
        }

        public void FindReferencesIfMissing()
        {
            if (modalLayer == null)
            {
                var t = transform.Find("ModalLayer");
                if (t != null) modalLayer = t.gameObject;
            }

            if (backdrop == null && modalLayer != null)
            {
                backdrop = modalLayer.GetComponentInChildren<ModalBackdrop>(true);
            }
            else if (backdrop == null)
            {
                backdrop = GetComponentInChildren<ModalBackdrop>(true);
            }
        }

        public void RegisterModalView(IModalView view)
        {
            if (view == null || string.IsNullOrEmpty(view.ModalId)) return;
            if (_registeredViews.TryGetValue(view.ModalId, out var existing) && existing != null && existing != view)
            {
                Debug.LogError($"[ModalCoordinator] Cannot register view '{view}' for ModalId '{view.ModalId}'. A different view '{existing}' is already registered!");
                return;
            }
            _registeredViews[view.ModalId] = view;
        }

        public void UnregisterModalView(IModalView view)
        {
            if (view == null || string.IsNullOrEmpty(view.ModalId)) return;
            if (_registeredViews.TryGetValue(view.ModalId, out var existing) && existing == view)
            {
                _registeredViews.Remove(view.ModalId);
            }
        }

        public IModalView GetRegisteredView(string modalId)
        {
            if (string.IsNullOrEmpty(modalId)) return null;
            _registeredViews.TryGetValue(modalId, out var view);
            return view;
        }

        /// <summary>
        /// Request to display a modal. Follows deterministic priority, FIFO within priority,
        /// duplicate coalescing/rejection, and safe critical preemption.
        /// </summary>
        public bool RequestModal(ModalRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.ModalId)) return false;

            // 1. Duplicate check across active modal and all queues
            var existing = FindExistingRequest(request.ModalId);
            if (existing != null)
            {
                if (request.Priority == ModalPriority.CriticalGameplay)
                {
                    if (!object.Equals(existing.Payload, request.Payload))
                    {
                        Debug.LogError($"[ModalCoordinator] Duplicate CriticalGameplay request '{request.ModalId}' contains differing payload. Rejected safely to protect pending gameplay authority.");
                        return false;
                    }
                    else
                    {
                        Debug.LogWarning($"[ModalCoordinator] Duplicate CriticalGameplay request '{request.ModalId}' with identical payload coalesced.");
                        return false;
                    }
                }
                else
                {
                    Debug.Log($"[ModalCoordinator] Duplicate request '{request.ModalId}' coalesced/rejected.");
                    return false;
                }
            }

            // If currently transitioning, enqueue safely without re-entrant presentation
            if (_isTransitioning)
            {
                EnqueueByPriority(request);
                return true;
            }

            // 2. Preemption evaluation if a modal is already active
            if (_activeRequest != null)
            {
                // CriticalGameplay can replace an active lower-priority modal ONLY IF that active modal is dismissable
                if (request.Priority == ModalPriority.CriticalGameplay && _activeRequest.IsDismissable)
                {
                    Debug.Log($"[ModalCoordinator] CriticalGameplay request '{request.ModalId}' preempting dismissable active modal '{_activeRequest.ModalId}'.");
                    _isTransitioning = true;
                    try
                    {
                        var preemptedReq = _activeRequest;
                        var preemptedView = _activeView;

                        _activeRequest = null;
                        _activeView = null;

                        if (preemptedView != null)
                        {
                            preemptedView.HideModal(DismissalReason.PreemptedByCritical);
                        }

                        // Enqueue critical request first so it occupies head of priority queue
                        EnqueueByPriority(request);

                        // Callback invoked under transition guard: cannot present re-entrantly
                        preemptedReq?.OnDismissed?.Invoke(preemptedReq, DismissalReason.PreemptedByCritical);

                        PresentNextModalInternal();
                        return true;
                    }
                    finally
                    {
                        _isTransitioning = false;
                    }
                }

                // Active modal is non-dismissable OR incoming request is not CriticalGameplay: queue it!
                EnqueueByPriority(request);
                return true;
            }

            // 3. No active modal: enqueue and present immediately
            EnqueueByPriority(request);
            TryPresentNextModal();
            return true;
        }

        private void EnqueueByPriority(ModalRequest request)
        {
            switch (request.Priority)
            {
                case ModalPriority.CriticalGameplay:
                    _criticalQueue.Enqueue(request);
                    break;
                case ModalPriority.SystemProgression:
                    _progressionQueue.Enqueue(request);
                    break;
                case ModalPriority.Informational:
                default:
                    _infoQueue.Enqueue(request);
                    break;
            }
        }

        private ModalRequest FindExistingRequest(string modalId)
        {
            if (_activeRequest != null && _activeRequest.ModalId == modalId)
            {
                return _activeRequest;
            }
            foreach (var req in _criticalQueue)
            {
                if (req.ModalId == modalId) return req;
            }
            foreach (var req in _progressionQueue)
            {
                if (req.ModalId == modalId) return req;
            }
            foreach (var req in _infoQueue)
            {
                if (req.ModalId == modalId) return req;
            }
            return null;
        }

        /// <summary>
        /// Presents the next modal by deterministic priority (Critical -> Progression -> Informational)
        /// preserving FIFO within each priority. Protected by transition guard against re-entrancy.
        /// </summary>
        public void TryPresentNextModal()
        {
            if (_isTransitioning) return;
            _isTransitioning = true;
            try
            {
                PresentNextModalInternal();
            }
            finally
            {
                _isTransitioning = false;
            }
        }

        private void PresentNextModalInternal()
        {
            if (_activeRequest != null)
            {
                return;
            }

            ModalRequest nextReq = null;
            if (_criticalQueue.Count > 0)
            {
                nextReq = _criticalQueue.Dequeue();
            }
            else if (_progressionQueue.Count > 0)
            {
                nextReq = _progressionQueue.Dequeue();
            }
            else if (_infoQueue.Count > 0)
            {
                nextReq = _infoQueue.Dequeue();
            }

            if (nextReq == null)
            {
                // All queues empty
                UpdateBackdrop(false);
                return;
            }

            if (!_registeredViews.TryGetValue(nextReq.ModalId, out var view) || view == null)
            {
                Debug.LogError($"[ModalCoordinator] No view registered for ModalId '{nextReq.ModalId}'. Dismissing request as SystemDismissed.");
                nextReq.OnDismissed?.Invoke(nextReq, DismissalReason.SystemDismissed);
                PresentNextModalInternal();
                return;
            }

            _activeRequest = nextReq;
            _activeView = view;
            UpdateBackdrop(true);

            view.ShowModal(nextReq);
            nextReq.OnShown?.Invoke(nextReq);
        }

        /// <summary>
        /// Primary dismissal path for dismissable modals (close button, Escape key, or backdrop click).
        /// Validates expectedRequest identity if provided.
        /// </summary>
        public void DismissActiveModal(DismissalReason reason = DismissalReason.UserClosed, ModalRequest expectedRequest = null)
        {
            if (_activeRequest == null) return;
            if (expectedRequest != null && expectedRequest != _activeRequest)
            {
                Debug.LogWarning($"[ModalCoordinator] DismissActiveModal called with request '{expectedRequest.ModalId}' (ID: {expectedRequest.RequestId}) that does not match active request '{_activeRequest.ModalId}' (ID: {_activeRequest.RequestId}). Ignored.");
                return;
            }

            if (!_activeRequest.IsDismissable)
            {
                Debug.LogWarning($"[ModalCoordinator] Cannot dismiss non-dismissable modal '{_activeRequest.ModalId}' via {reason}. Ignored.");
                return;
            }

            if (_isTransitioning) return;
            _isTransitioning = true;
            try
            {
                var req = _activeRequest;
                var view = _activeView;

                _activeRequest = null;
                _activeView = null;

                if (view != null)
                {
                    view.HideModal(reason);
                }
                req?.OnDismissed?.Invoke(req, reason);

                PresentNextModalInternal();
            }
            finally
            {
                _isTransitioning = false;
            }
        }

        /// <summary>
        /// Completes the active modal (e.g. primary action taken like Equip, Dismantle, Upgrade).
        /// Validates expectedRequest identity if provided.
        /// </summary>
        public void CompleteActiveModal(ModalRequest expectedRequest = null)
        {
            if (_activeRequest == null) return;
            if (expectedRequest != null && expectedRequest != _activeRequest)
            {
                Debug.LogWarning($"[ModalCoordinator] CompleteActiveModal called with request '{expectedRequest.ModalId}' (ID: {expectedRequest.RequestId}) that does not match active request '{_activeRequest.ModalId}' (ID: {_activeRequest.RequestId}). Ignored.");
                return;
            }

            if (_isTransitioning) return;
            _isTransitioning = true;
            try
            {
                var req = _activeRequest;
                var view = _activeView;

                _activeRequest = null;
                _activeView = null;

                if (view != null)
                {
                    view.HideModal(DismissalReason.ActionCompleted);
                }
                req?.OnCompleted?.Invoke(req);

                PresentNextModalInternal();
            }
            finally
            {
                _isTransitioning = false;
            }
        }

        public void OnBackdropClicked()
        {
            if (_activeRequest != null && _activeRequest.IsDismissable)
            {
                DismissActiveModal(DismissalReason.UserClosed, _activeRequest);
            }
        }

        public void ClearAll()
        {
            // 1. Detach/snapshot active and queued requests first
            var requestsToDismiss = new List<ModalRequest>();
            if (_activeRequest != null)
            {
                requestsToDismiss.Add(_activeRequest);
            }
            while (_criticalQueue.Count > 0)
            {
                requestsToDismiss.Add(_criticalQueue.Dequeue());
            }
            while (_progressionQueue.Count > 0)
            {
                requestsToDismiss.Add(_progressionQueue.Dequeue());
            }
            while (_infoQueue.Count > 0)
            {
                requestsToDismiss.Add(_infoQueue.Dequeue());
            }

            var activeView = _activeView;

            // 2. Clear internal state and backdrop
            _activeRequest = null;
            _activeView = null;
            _criticalQueue.Clear();
            _progressionQueue.Clear();
            _infoQueue.Clear();

            if (activeView != null)
            {
                activeView.HideModal(DismissalReason.SystemDismissed);
            }
            UpdateBackdrop(false);

            // 3. Invoke every required callback exactly once while guarded
            _isTransitioning = true;
            try
            {
                for (int i = 0; i < requestsToDismiss.Count; i++)
                {
                    var req = requestsToDismiss[i];
                    req?.OnDismissed?.Invoke(req, DismissalReason.SystemDismissed);
                }
            }
            finally
            {
                // 4. Prevent callback-created requests from corrupting the queue or presenting during the transition
                _criticalQueue.Clear();
                _progressionQueue.Clear();
                _infoQueue.Clear();
                _activeRequest = null;
                _activeView = null;
                UpdateBackdrop(false);
                _isTransitioning = false;
            }
        }

        private void UpdateBackdrop(bool active)
        {
            if (backdrop != null)
            {
                backdrop.gameObject.SetActive(active);
            }
        }
    }
}
