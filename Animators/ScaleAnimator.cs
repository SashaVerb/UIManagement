using UnityEngine;

namespace UIManagement
{
    [CreateAssetMenu(fileName = "ScaleAnimator", menuName = "UI System/Animators/Scale")]
    public class ScaleAnimator : ReversibleAnimatorBase
    {
        [SerializeField] private Vector3 _startScale = Vector3.zero;

        public override void SetupInitialState(UIPanel panel)
        {
            SetProgress(panel.GetInstanceID(), 0f);
            panel.transform.localScale = Vector3.Lerp(_startScale, Vector3.one, _curve.Evaluate(0f));
        }

        protected override void ApplyCurveValue(UIPanel panel, float curveValue)
        {
            panel.transform.localScale = Vector3.Lerp(_startScale, Vector3.one, curveValue);
        }
    }
}
