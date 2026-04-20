using UnityEngine;
using Cysharp.Threading.Tasks;

namespace UIManagement
{
    public abstract class UIPanelAnimator : ScriptableObject
    {
        public virtual float Duration => 0f;
        
        public abstract UniTask AnimateShow(UIPanel panel);
        public abstract UniTask AnimateHide(UIPanel panel);
        public abstract void SetupInitialState(UIPanel panel);
    }
}
