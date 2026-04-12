using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace UIManagement
{
    [RequireComponent(typeof(CanvasGroup))]
    public class UIPanel : MonoBehaviour
    {
        [SerializeField] private UIPanelAnimator _animator;
        
        public CanvasGroup CanvasGroupComponent { get; private set; }
        
        public UIPanelState State { get; private set; } = UIPanelState.Hidden;

        public bool IsInteractable => State == UIPanelState.Visible;
        
        public event Action OnShowStarted;
        public event Action OnShowCompleted;
        public event Action OnHideStarted;
        public event Action OnHideCompleted;

        private void Awake()
        {
            CanvasGroupComponent = GetComponent<CanvasGroup>();
            
            _animator.SetupInitialState(this);
            SetInteractable(false);
        }

        public async UniTask Show()
        {
            if (State == UIPanelState.Visible || State == UIPanelState.Showing)
                return;

            State = UIPanelState.Showing;
            gameObject.SetActive(true);
            SetInteractable(false);
            
            OnShowStarted?.Invoke();

            if (_animator != null)
            {
                await _animator.AnimateShow(this);
            }

            State = UIPanelState.Visible;
            SetInteractable(true);
            
            OnShowCompleted?.Invoke();
        }

        public async UniTask Hide()
        {
            if (State == UIPanelState.Hidden || State == UIPanelState.Hiding)
                return;

            State = UIPanelState.Hiding;
            SetInteractable(false);
            
            OnHideStarted?.Invoke();

            if (_animator != null)
            {
                await _animator.AnimateHide(this);
            }

            State = UIPanelState.Hidden;
            gameObject.SetActive(false);
            
            OnHideCompleted?.Invoke();
        }

        public void ShowImmediate()
        {
            State = UIPanelState.Visible;
            gameObject.SetActive(true);
            SetInteractable(true);
        }

        public void HideImmediate()
        {
            State = UIPanelState.Hidden;
            gameObject.SetActive(false);
            SetInteractable(false);
        }

        private void SetInteractable(bool interactable)
        {
            CanvasGroupComponent.blocksRaycasts = interactable;
        }
    }
}
