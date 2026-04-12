using Cysharp.Threading.Tasks;

namespace UIManagement
{
    public interface IUIPanelAnimator
    {
        UniTask AnimateShow(UIPanel panel);
        UniTask AnimateHide(UIPanel panel);
        void SetupInitialState(UIPanel panel);
    }
}
