using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace UIManagement
{
    [CreateAssetMenu(fileName = "ScaleAnimator", menuName = "UI System/Animators/Scale")]
    public class ScaleAnimator : UIPanelAnimator
    {
        [SerializeField] private float _duration = 0.3f;
        [SerializeField] private AnimationCurve _showCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private AnimationCurve _hideCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private Vector3 _startScale = Vector3.zero;

        public override void SetupInitialState(UIPanel panel)
        {
            panel.transform.localScale = _startScale;
        }

        public override async UniTask AnimateShow(UIPanel panel)
        {
            var cancellationToken = panel.GetCancellationTokenOnDestroy();
            
            await AnimateScale(panel.transform, Vector3.one, _duration, _showCurve, cancellationToken);
        }

        public override async UniTask AnimateHide(UIPanel panel)
        {
            var cancellationToken = panel.GetCancellationTokenOnDestroy();
            
            await AnimateScale(panel.transform, _startScale, _duration, _hideCurve, cancellationToken);
        }

        private async UniTask AnimateScale(Transform transform, Vector3 targetScale, float duration, AnimationCurve curve, CancellationToken cancellationToken)
        {
            Vector3 startScale = transform.localScale;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = curve.Evaluate(elapsed / duration);
                transform.localScale = Vector3.Lerp(startScale, targetScale, t);
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            }

            transform.localScale = targetScale;
        }
    }
}
