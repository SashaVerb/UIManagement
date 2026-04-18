using UnityEngine;

namespace UIManagement
{
    [CreateAssetMenu(fileName = "FadeAnimator", menuName = "UI System/Animators/Fade")]
    public class FadeAnimator : ReversibleAnimatorBase
    {
        public override void SetupInitialState(UIPanel panel)
        {
            SetProgress(panel.GetInstanceID(), 0f);
            panel.CanvasGroupComponent.alpha = _curve.Evaluate(0f);
        }

        protected override void ApplyCurveValue(UIPanel panel, float curveValue)
        {
            panel.CanvasGroupComponent.alpha = curveValue;
        }
    }
}
