using UnityEngine;

namespace UIManagement
{
    [CreateAssetMenu(fileName = "ScaleAnimator", menuName = "UI System/Animators/Scale")]
    public class ScaleAnimator : UIPanelAnimator
    {
        [SerializeField] private float _duration = 1f;
        [SerializeField] private AnimationCurve _curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private Vector3 _startScale = Vector3.zero;

        public override float Duration => _duration;

        public override void SetupInitialState(UIPanel panel)
        {
            panel.transform.localScale = Vector3.Lerp(_startScale, Vector3.one, _curve.Evaluate(0f));
        }

        public override void Evaluate(UIPanel panel, float progress)
        {
            panel.transform.localScale = Vector3.Lerp(_startScale, Vector3.one, _curve.Evaluate(progress));
        }
    }
}
