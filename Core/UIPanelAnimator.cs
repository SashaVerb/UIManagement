using UnityEngine;

namespace UIManagement
{
    public abstract class UIPanelAnimator : ScriptableObject
    {
        public virtual float Duration => 0f;
        
        public abstract void Evaluate(UIPanel panel, float progress);
        public abstract void SetupInitialState(UIPanel panel);
    }
}
