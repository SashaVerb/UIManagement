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
    public class SlideAnimator : UIPanelAnimator
    {
        [SerializeField] private float _duration = 1f;
        [SerializeField] private AnimationCurve _curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private SlideDirection _direction = SlideDirection.Top;
        [SerializeField] private float _offset = 1000f;

        public override float Duration => _duration;

        public override void SetupInitialState(UIPanel panel)
        {
            var rt = panel.GetComponent<RectTransform>();
            var state = GetOrCreateState(panel);
            
            state.visiblePosition = rt.anchoredPosition;
            state.hiddenPosition = _direction switch
            {
                SlideDirection.Left   => state.visiblePosition + Vector2.left  * _offset,
                SlideDirection.Right  => state.visiblePosition + Vector2.right * _offset,
                SlideDirection.Top    => state.visiblePosition + Vector2.up    * _offset,
                SlideDirection.Bottom => state.visiblePosition + Vector2.down  * _offset,
                _                     => state.visiblePosition
            };

            rt.anchoredPosition = Vector2.Lerp(state.hiddenPosition, state.visiblePosition, _curve.Evaluate(0f));
        }

        public override void Evaluate(UIPanel panel, float progress)
        {
            var state = panel.GetComponent<SlideAnimatorState>();
            if (state == null) return;
            
            panel.GetComponent<RectTransform>().anchoredPosition = 
                Vector2.Lerp(state.hiddenPosition, state.visiblePosition, _curve.Evaluate(progress));
        }
        
        private SlideAnimatorState GetOrCreateState(UIPanel panel)
        {
            var state = panel.GetComponent<SlideAnimatorState>();
            if (state == null)
                state = panel.gameObject.AddComponent<SlideAnimatorState>();
            return state;
        }
    }
    
    public class SlideAnimatorState : MonoBehaviour
    {
        public Vector2 visiblePosition;
        public Vector2 hiddenPosition;
    }
}
