using System;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace UIManagement
{
    [RequireComponent(typeof(CanvasGroup))]
    public class UIPanel : MonoBehaviour
    {
        [SerializeField] private UIPanelAnimator _animator;
        
        [field: SerializeField] public CanvasGroup CanvasGroupComponent { get; private set; }
        
        public UIPanelState State { get; private set; } = UIPanelState.Hidden;

        public CancellationToken AnimationToken => _animationCts?.Token ?? CancellationToken.None;

        private CancellationTokenSource _animationCts;

        public bool IsInteractable => State == UIPanelState.Visible;
        
        public event Action OnShowStarted;
        public event Action OnShowCompleted;
        public event Action OnHideStarted;
        public event Action OnHideCompleted;

        protected virtual void Awake()
        {
            _animator.SetupInitialState(this);
            SetInteractable(false);
        }

        private void OnDestroy()
        {
            CancelCurrentAnimation();
        }

        public async UniTask Show()
        {
            if (State == UIPanelState.Visible || State == UIPanelState.Showing)
                return;

            CancelCurrentAnimation();
            _animationCts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());

            State = UIPanelState.Showing;
            gameObject.SetActive(true);
            SetInteractable(false);
            
            OnShowStarted?.Invoke();

            if (_animator != null)
            {
                try { await _animator.AnimateShow(this); }
                catch (OperationCanceledException) { return; }
            }

            State = UIPanelState.Visible;
            SetInteractable(true);
            
            OnShowCompleted?.Invoke();
        }

        public async UniTask Hide()
        {
            if (State == UIPanelState.Hidden || State == UIPanelState.Hiding)
                return;

            CancelCurrentAnimation();
            _animationCts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());

            State = UIPanelState.Hiding;
            SetInteractable(false);
            
            OnHideStarted?.Invoke();

            if (_animator != null)
            {
                try { await _animator.AnimateHide(this); }
                catch (OperationCanceledException) { return; }
            }

            State = UIPanelState.Hidden;
            gameObject.SetActive(false);
            
            OnHideCompleted?.Invoke();
        }

        public void ShowImmediate()
        {
            CancelCurrentAnimation();
            State = UIPanelState.Visible;
            gameObject.SetActive(true);
            SetInteractable(true);
        }

        public void HideImmediate()
        {
            CancelCurrentAnimation();
            State = UIPanelState.Hidden;
            gameObject.SetActive(false);
            SetInteractable(false);
        }

        private void CancelCurrentAnimation()
        {
            _animationCts?.Cancel();
            _animationCts?.Dispose();
            _animationCts = null;
        }

        private void SetInteractable(bool interactable)
        {
            if(CanvasGroupComponent != null)
                CanvasGroupComponent.blocksRaycasts = interactable;
        }
        
        private void OnValidate()
        {
            CanvasGroupComponent = GetComponent<CanvasGroup>();
        }
    }
}
