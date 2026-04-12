using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace UIManagement
{
    public enum SlideDirection
    {
        Left,
        Right,
        Top,
        Bottom
    }

    [CreateAssetMenu(fileName = "SlideAnimator", menuName = "UI System/Animators/Slide")]
    public class SlideAnimator : UIPanelAnimator
    {
        [SerializeField] private SlideDirection _direction = SlideDirection.Top;
        [SerializeField] private float _duration = 0.3f;
        [SerializeField] private AnimationCurve _showCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private AnimationCurve _hideCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private float _offset = 1000f;
        
        private readonly Dictionary<UIPanel, (Vector2 visible, Vector2 hidden)> _positions = new();

        public override void SetupInitialState(UIPanel panel)
        {
            var rt = panel.GetComponent<RectTransform>();
            var visiblePos = rt.anchoredPosition;

            var hiddenPos = _direction switch
            {
                SlideDirection.Left   => visiblePos + Vector2.left  * _offset,
                SlideDirection.Right  => visiblePos + Vector2.right * _offset,
                SlideDirection.Top    => visiblePos + Vector2.up    * _offset,
                SlideDirection.Bottom => visiblePos + Vector2.down  * _offset,
                _                    => visiblePos
            };

            _positions[panel] = (visiblePos, hiddenPos);
            rt.anchoredPosition = hiddenPos;
        }

        public override async UniTask AnimateShow(UIPanel panel)
        {
            if (!_positions.TryGetValue(panel, out var pos)) return;

            var cancellationToken = panel.GetCancellationTokenOnDestroy();

            await AnimatePosition(panel.GetComponent<RectTransform>(), pos.visible, _duration, _showCurve, cancellationToken);
        }

        public override async UniTask AnimateHide(UIPanel panel)
        {
            if (!_positions.TryGetValue(panel, out var pos)) return;

            var cancellationToken = panel.GetCancellationTokenOnDestroy();

            await AnimatePosition(panel.GetComponent<RectTransform>(), pos.hidden, _duration, _hideCurve, cancellationToken);
        }

        private async UniTask AnimatePosition(RectTransform rectTransform, Vector2 targetPosition, float duration, AnimationCurve curve, CancellationToken cancellationToken)
        {
            Vector2 startPosition = rectTransform.anchoredPosition;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = curve.Evaluate(elapsed / duration);
                rectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t);
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            }

            rectTransform.anchoredPosition = targetPosition;
        }
    }
}
