using System.Collections.Generic;
using UnityEngine;

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
    public class SlideAnimator : ReversibleAnimatorBase
    {
        [SerializeField] private SlideDirection _direction = SlideDirection.Top;
        [SerializeField] private float _offset = 1000f;

        private readonly Dictionary<int, (Vector2 visible, Vector2 hidden)> _positions = new();

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

            int id = panel.GetInstanceID();
            _positions[id] = (visiblePos, hiddenPos);
            SetProgress(id, 0f);
            rt.anchoredPosition = Vector2.Lerp(hiddenPos, visiblePos, _curve.Evaluate(0f));
        }

        protected override void ApplyCurveValue(UIPanel panel, float curveValue)
        {
            if (!_positions.TryGetValue(panel.GetInstanceID(), out var pos)) return;
            panel.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(pos.hidden, pos.visible, curveValue);
        }
    }
}
