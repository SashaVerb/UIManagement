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
        private float _animationTime = 0f;

        public bool IsInteractable => State == UIPanelState.Visible;
        public bool IsVisible => State == UIPanelState.Visible || State == UIPanelState.Showing;
        
        public event Action OnShowStarted;
        public event Action OnShowCompleted;
        public event Action OnHideStarted;
        public event Action OnHideCompleted;

        protected virtual void Awake()
        {
            _animator?.SetupInitialState(this);
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
                try { await AnimateToProgress(1f, _animationCts.Token); }
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
                try { await AnimateToProgress(0f, _animationCts.Token); }
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
        
        private async UniTask AnimateToProgress(float targetProgress, CancellationToken cancellationToken)
        {
            float duration = _animator.Duration;
            if (duration <= 0.0001f)
            {
                _animationTime = targetProgress * duration;
                _animator.Evaluate(this, targetProgress);
                return;
            }

            float startTime = _animationTime;
            float startProgress = startTime / duration;
            float targetTime = targetProgress * duration;
            bool isForward = targetProgress > startProgress;

            while (true)
            {
                float delta = Time.unscaledDeltaTime;
                _animationTime = isForward
                    ? Mathf.Min(_animationTime + delta, targetTime)
                    : Mathf.Max(_animationTime - delta, targetTime);

                float currentProgress = Mathf.Clamp01(_animationTime / duration);
                _animator.Evaluate(this, currentProgress);

                if (Mathf.Approximately(_animationTime, targetTime))
                    break;

                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            }
        }
        
        private void OnValidate()
        {
            CanvasGroupComponent = GetComponent<CanvasGroup>();
        }
    }
}
