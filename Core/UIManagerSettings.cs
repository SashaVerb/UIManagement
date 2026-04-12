using UnityEngine;

namespace UIManagement
{
    [CreateAssetMenu(fileName = "UIManagerSettings", menuName = "UI System/UI Manager Settings")]
    public class UIManagerSettings : ScriptableObject
    {
        [SerializeField] private Canvas _canvasPrefab;

        public Canvas CanvasPrefab => _canvasPrefab;
    }
}
