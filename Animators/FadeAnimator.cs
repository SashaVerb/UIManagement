using UnityEngine;

namespace UIManagement
{
    [CreateAssetMenu(fileName = "FadeAnimator", menuName = "UI System/Animators/Fade")]
    public class FadeAnimator : UIPanelAnimator
    {
        [SerializeField] private float _duration = 1f;
        [SerializeField] private AnimationCurve _curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        public override float Duration => _duration;

        public override void SetupInitialState(UIPanel panel)
        {
            panel.CanvasGroupComponent.alpha = _curve.Evaluate(0f);
        }

        public override void Evaluate(UIPanel panel, float progress)
        {
            panel.CanvasGroupComponent.alpha = _curve.Evaluate(progress);
        }
    }
}
