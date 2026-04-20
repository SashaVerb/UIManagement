using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace UIManagement
{
    public abstract class ReversibleAnimatorBase : UIPanelAnimator
    {
        [SerializeField] protected float _duration = 1f;
        [SerializeField] protected AnimationCurve _curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        public override float Duration => _duration;
        
        private readonly Dictionary<int, float> _progress = new();
        
        protected float GetProgress(int instanceId) =>
            _progress.TryGetValue(instanceId, out var p) ? p : 0f;

        protected void SetProgress(int instanceId, float value) =>
            _progress[instanceId] = value;

        public override async UniTask AnimateShow(UIPanel panel) =>
            await AnimateToProgress(panel, 1f, panel.AnimationToken);

        public override async UniTask AnimateHide(UIPanel panel) =>
            await AnimateToProgress(panel, 0f, panel.AnimationToken);

        private async UniTask AnimateToProgress(UIPanel panel, float target, CancellationToken cancellationToken)
        {
            int id = panel.GetInstanceID();
            float speed = 1f / Mathf.Max(_duration, 0.0001f);

            while (true)
            {
                float current = GetProgress(id);
                Debug.Log($"Current: {current}, Target: {target} | {this.name}");
                float delta = speed * Time.unscaledDeltaTime;
                float next = target > current
                    ? Mathf.Min(current + delta, target)
                    : Mathf.Max(current - delta, target);

                SetProgress(id, next);
                ApplyCurveValue(panel, _curve.Evaluate(next));

                if (Mathf.Approximately(next, target)) break;

                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            }
        }

        protected abstract void ApplyCurveValue(UIPanel panel, float curveValue);
    }
}
