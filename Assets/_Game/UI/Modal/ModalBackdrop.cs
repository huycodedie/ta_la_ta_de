using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WuxiaGame.UI.Modal
{
    /// <summary>
    /// Full-screen backdrop that intercepts pointer raycasts behind active modals
    /// and passes backdrop dismissal clicks to ModalCoordinator.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class ModalBackdrop : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Image backdropImage;

        public Image BackdropImage => backdropImage;
        public bool IsVisible => gameObject.activeInHierarchy;

        private void Awake()
        {
            if (backdropImage == null)
            {
                backdropImage = GetComponent<Image>();
            }

            if (backdropImage != null)
            {
                backdropImage.raycastTarget = true;
            }
        }

        public void SetReferences(Image img)
        {
            backdropImage = img;
            if (backdropImage != null)
            {
                backdropImage.raycastTarget = true;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (ModalCoordinator.Instance != null)
            {
                ModalCoordinator.Instance.OnBackdropClicked();
            }
        }
    }
}
