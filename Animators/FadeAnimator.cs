using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace UIManagement
{
    [CreateAssetMenu(fileName = "FadeAnimator", menuName = "UI System/Animators/Fade")]
    public class FadeAnimator : UIPanelAnimator
    {
        [SerializeField] private float _duration = 0.3f;
        [SerializeField] private AnimationCurve _curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        public override void SetupInitialState(UIPanel panel)
        {
            panel.CanvasGroupComponent.alpha = 0f;
        }

        public override async UniTask AnimateShow(UIPanel panel)
        {
            await AnimateFade(panel.CanvasGroupComponent, 1f, _duration, _curve, panel.GetCancellationTokenOnDestroy());
        }

        public override async UniTask AnimateHide(UIPanel panel)
        {
            await AnimateFade(panel.CanvasGroupComponent, 0f, _duration, _curve, panel.GetCancellationTokenOnDestroy());
        }

        private async UniTask AnimateFade(CanvasGroup canvasGroup, float targetAlpha, float duration, AnimationCurve curve, CancellationToken cancellationToken)
        {
            float startAlpha = canvasGroup.alpha;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = curve.Evaluate(elapsed / duration);
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            }

            canvasGroup.alpha = targetAlpha;
        }
    }
}
