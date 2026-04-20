using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

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

        public override async UniTask AnimateShow(UIPanel panel)
        {
            var tasks = BuildShowTasks(panel);
            await UniTask.WhenAll(tasks);
        }

        public override async UniTask AnimateHide(UIPanel panel)
        {
            float maxDuration = GetMaxDuration();
            var tasks = BuildHideTasksAligned(panel, maxDuration);
            await UniTask.WhenAll(tasks);
        }

        private List<UniTask> BuildShowTasks(UIPanel panel)
        {
            var tasks = new List<UniTask>(_animators.Length);
            foreach (var animator in _animators)
            {
                if (animator != null)
                    tasks.Add(animator.AnimateShow(panel));
            }
            return tasks;
        }

        private List<UniTask> BuildHideTasksAligned(UIPanel panel, float maxDuration)
        {
            var tasks = new List<UniTask>(_animators.Length);
            foreach (var animator in _animators)
            {
                if (animator == null)
                    continue;

                float delay = maxDuration - animator.Duration;
                tasks.Add(delay > 0f
                    ? HideAfterDelay(animator, panel, delay)
                    : animator.AnimateHide(panel));
            }
            return tasks;
        }

        private async UniTask HideAfterDelay(UIPanelAnimator animator, UIPanel panel, float delay)
        {
            await UniTask.Delay(
                TimeSpan.FromSeconds(delay),
                DelayType.UnscaledDeltaTime,
                PlayerLoopTiming.Update,
                panel.AnimationToken);

            await animator.AnimateHide(panel);
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
