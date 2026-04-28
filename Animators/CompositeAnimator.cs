using UnityEngine;

namespace UIManagement
{
    [CreateAssetMenu(fileName = "CompositeAnimator", menuName = "UI System/Animators/Composite")]
    public class CompositeAnimator : UIPanelAnimator
    {
        [SerializeField] private UIPanelAnimator[] _animators;

        public override float Duration => GetMaxDuration();

        public override void SetupInitialState(UIPanel panel)
        {
            foreach (var animator in _animators)
                animator?.SetupInitialState(panel);
        }

        public override void Evaluate(UIPanel panel, float progress)
        {
            float maxDuration = GetMaxDuration();
            
            foreach (var animator in _animators)
            {
                if (animator == null) continue;
                
                float normalizedProgress = CalculateNormalizedProgress(progress, animator.Duration, maxDuration);
                animator.Evaluate(panel, normalizedProgress);
            }
        }
        
        private float CalculateNormalizedProgress(float globalProgress, float animatorDuration, float maxDuration)
        {
            if (animatorDuration <= 0f || maxDuration <= 0f)
                return globalProgress;
            
            float delay = maxDuration - animatorDuration;
            float delayNormalized = delay / maxDuration;
            
            if (globalProgress < delayNormalized)
                return 0f;
            
            float adjustedProgress = (globalProgress - delayNormalized) / (1f - delayNormalized);
            return Mathf.Clamp01(adjustedProgress);
        }

        private float GetMaxDuration()
        {
            float max = 0f;
            foreach (var animator in _animators)
            {
                if (animator != null)
                    max = Mathf.Max(max, animator.Duration);
            }
            return max;
        }
    }
}
