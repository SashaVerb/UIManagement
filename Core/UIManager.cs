using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace UIManagement
{
    public static class UIManager
    {
        private struct PanelData
        {
            public int GroupId;
            public GameObject Prefab;
            public GameObject GameObject;
            public UIPanel UIPanel;

            public PanelData(int groupId, GameObject prefab, GameObject gameObject, UIPanel uiPanel)
            {
                GroupId = groupId;
                Prefab = prefab;
                GameObject = gameObject;
                UIPanel = uiPanel;
            }
        }

        private static readonly Dictionary<Type, PanelData> _panels = new();
        private static readonly Dictionary<int, Transform> _groupContainers = new();
        private static Canvas _canvas;
        private static Transform _canvasTransform;
        private static UIManagerSettings _settings;
        private static bool _isInitialized;

        private static void EnsureInitialized()
        {
            if (_isInitialized)
                return;

            LoadSettings();
            CreateCanvas();
            _isInitialized = true;
        }

        private static void LoadSettings()
        {
            _settings = Resources.Load<UIManagerSettings>("UIManagerSettings");
            
            if (_settings == null)
            {
                Debug.LogError("[UIManager] UIManagerSettings not found in Resources folder! Create it via Assets/Create/UI System/UI Manager Settings");
            }
        }

        private static void CreateCanvas()
        {
            if (_settings?.CanvasPrefab == null)
            {
                Debug.LogError("[UIManager] Canvas prefab is not assigned in UIManagerSettings!");
                CreateDefaultCanvas();
                return;
            }

            _canvas = UnityEngine.Object.Instantiate(_settings.CanvasPrefab);
            _canvas.name = "[UIManager] Canvas";
            _canvasTransform = _canvas.transform;
            UnityEngine.Object.DontDestroyOnLoad(_canvas.gameObject);
        }

        private static void CreateDefaultCanvas()
        {
            var canvasGO = new GameObject("[UIManager] Canvas (Default)");
            _canvas = canvasGO.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 100;
            
            var scaler = canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            
            canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            
            _canvasTransform = _canvas.transform;
            UnityEngine.Object.DontDestroyOnLoad(canvasGO);
            
            Debug.LogWarning("[UIManager] Created default Canvas. Consider creating a Canvas prefab and assigning it in UIManagerSettings.");
        }

        private static Transform GetOrCreateGroupContainer(int groupId)
        {
            if (_groupContainers.TryGetValue(groupId, out var container))
                return container;

            var containerGO = new GameObject($"[Group_{groupId}]");
            containerGO.transform.SetParent(_canvasTransform, false);
            containerGO.transform.SetSiblingIndex(groupId);
            var rectTransform = containerGO.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;

            _groupContainers[groupId] = containerGO.transform;

            return _groupContainers[groupId];
        }
        
        public static T Instantiate<T>(T prefab) where T : UIPanel
        {
            EnsureInitialized();
            
            if (prefab == null)
            {
                Debug.LogError("[UIManager] Instantiate was called with a null prefab.");
                return null;
            }

            var type = typeof(T);

            if (_panels.TryGetValue(type, out var existing))
            {
                Debug.LogWarning($"[UIManager] Prefab of type {type.Name} is already registered. Replacing.");

                if (existing.GameObject != null)
                {
                    UnityEngine.Object.Destroy(existing.GameObject);
                }
            }

            var container = GetOrCreateGroupContainer(0);
            var instance = UnityEngine.Object.Instantiate(prefab, container);
            if (instance == null)
            {
                Debug.LogError($"[UIManager] Failed to instantiate prefab of type {type.Name}.");
                return null;
            }

            instance.HideImmediate();

            _panels[type] = new PanelData(
                0,
                prefab.gameObject,
                instance.gameObject,
                instance);

            return instance;
        }
        
        public static void RegisterPrefab<T>(T prefab, int groupId = 0) where T : Component
        {
            EnsureInitialized();
            
            var type = typeof(T);
            
            if (_panels.TryGetValue(type, out var existing))
            {
                Debug.LogWarning($"[UIManager] Prefab of type {type.Name} is already registered. Replacing.");
                _panels[type] = new PanelData(groupId, prefab.gameObject, existing.GameObject, existing.UIPanel);
            }
            else
            {
                _panels[type] = new PanelData(groupId, prefab.gameObject, null, null);
            }
        }

        public static void RegisterPrefab<TComponent, TGroup>(TComponent prefab, TGroup group) 
            where TComponent : Component 
            where TGroup : System.Enum
        {
            RegisterPrefab(prefab, Convert.ToInt32(group));
        }

        public static void RegisterPrefab(GameObject prefab, Type type, int groupId = 0)
        {
            EnsureInitialized();
            
            if (_panels.TryGetValue(type, out var existing))
            {
                Debug.LogWarning($"[UIManager] Prefab of type {type.Name} is already registered. Replacing.");
                _panels[type] = new PanelData(groupId, prefab, existing.GameObject, existing.UIPanel);
            }
            else
            {
                _panels[type] = new PanelData(groupId, prefab, null, null);
            }
        }

        private static PanelData CreatePanel<T>() where T : Component
        {
            var type = typeof(T);
            
            if (!_panels.TryGetValue(type, out var data) || data.Prefab == null)
            {
                Debug.LogError($"[UIManager] Prefab for panel type {type.Name} is not registered. Call RegisterPrefab first.");
                return default;
            }

            var container = GetOrCreateGroupContainer(data.GroupId);

            var instance = UnityEngine.Object.Instantiate(data.Prefab, container);
            instance.name = type.Name;
            
            var uiPanel = instance.GetComponent<UIPanel>();
            if (uiPanel != null)
            {
                uiPanel.HideImmediate();
            }
            else
            {
                instance.SetActive(false);
            }
            
            return new PanelData(data.GroupId, data.Prefab, instance, uiPanel);
        }

        public static void Unregister<T>() where T : Component
        {
            _panels.Remove(typeof(T));
        }

        public static async UniTask Show<T>(Action onComplete = null) where T : Component
        {
            EnsureInitialized();
            
            var panelData = GetOrCreatePanel<T>();
            if (panelData.GameObject == null)
                return;

            if (panelData.UIPanel != null)
            {
                if(panelData.UIPanel.State == UIPanelState.Hidden)
                {
                    await panelData.UIPanel.Show();
                }
            }
            else
            {
                panelData.GameObject.SetActive(true);
            }
            
            onComplete?.Invoke();
        }

        public static async UniTask Hide<T>(Action onComplete = null) where T : Component
        {
            var panelData = GetPanelData<T>();
            if (panelData.GameObject == null)
                return;

            if (panelData.UIPanel != null)
            {
                if(panelData.UIPanel.State == UIPanelState.Visible)
                {
                    await panelData.UIPanel.Hide();
                }
            }
            else
            {
                panelData.GameObject.SetActive(false);
            }
            
            onComplete?.Invoke();
        }

        public static void ShowImmediate<T>() where T : Component
        {
            EnsureInitialized();
            
            var panelData = GetOrCreatePanel<T>();
            if (panelData.GameObject == null)
                return;

            if (panelData.UIPanel != null)
            {
                panelData.UIPanel.ShowImmediate();
            }
            else
            {
                panelData.GameObject.SetActive(true);
            }
        }

        public static void HideImmediate<T>() where T : Component
        {
            var panelData = GetPanelData<T>();
            if (panelData.GameObject == null)
                return;

            if (panelData.UIPanel != null)
            {
                panelData.UIPanel.HideImmediate();
            }
            else
            {
                panelData.GameObject.SetActive(false);
            }
        }

        public static async UniTask HideAll(Action onComplete = null)
        {
            var tasks = new List<UniTask>();
            
            foreach (var panelData in _panels.Values)
            {
                if (panelData.GameObject == null)
                    continue;
                    
                if (panelData.UIPanel != null)
                {
                    if (panelData.UIPanel.State == UIPanelState.Visible || panelData.UIPanel.State == UIPanelState.Showing)
                    {
                        tasks.Add(panelData.UIPanel.Hide());
                    }
                }
                else if (panelData.GameObject.activeSelf)
                {
                    panelData.GameObject.SetActive(false);
                }
            }

            await UniTask.WhenAll(tasks);
            onComplete?.Invoke();
        }

        private static PanelData GetOrCreatePanel<T>() where T : Component
        {
            var type = typeof(T);
            
            if (_panels.TryGetValue(type, out var existingPanel) && existingPanel.GameObject != null)
            {
                return existingPanel;
            }

            var newPanel = CreatePanel<T>();
            if (newPanel.GameObject != null)
            {
                _panels[type] = newPanel;
            }
            
            return newPanel;
        }
        
        private static PanelData GetPanelData<T>() where T : Component
        {
            if (_panels.TryGetValue(typeof(T), out var panelData))
            {
                return panelData;
            }

            return default;
        }

        public static T GetPanel<T>() where T : Component
        {
            if (_panels.TryGetValue(typeof(T), out var panelData))
            {
                return panelData.GameObject.GetComponent<T>();
            }

            return null;
        }

        public static bool IsVisible<T>() where T : Component
        {
            var panelData = GetPanelData<T>();
            if (panelData.GameObject == null)
                return false;
                
            if (panelData.UIPanel != null)
            {
                return panelData.UIPanel.State == UIPanelState.Visible;
            }
            
            return panelData.GameObject.activeSelf;
        }

        public static UIPanelState GetState<T>() where T : Component
        {
            var panelData = GetPanelData<T>();
            if (panelData.GameObject == null)
                return UIPanelState.Hidden;
                
            return panelData.UIPanel?.State ?? UIPanelState.Hidden;
        }

        public static void DestroyPanel<T>() where T : Component
        {
            var type = typeof(T);
            
            if (_panels.TryGetValue(type, out var panelData))
            {
                UnityEngine.Object.Destroy(panelData.GameObject);
                _panels.Remove(type);
            }
        }

        public static void Clear()
        {
            foreach (var panelData in _panels.Values)
            {
                if (panelData.GameObject != null)
                {
                    UnityEngine.Object.Destroy(panelData.GameObject);
                }
            }
            
            _panels.Clear();
            _groupContainers.Clear();
            
            if (_canvas != null)
            {
                UnityEngine.Object.Destroy(_canvas.gameObject);
                _canvas = null;
                _canvasTransform = null;
            }
            
            _isInitialized = false;
        }

        public static Transform GetGroupContainer<TGroup>(TGroup group) where TGroup : System.Enum
        {
            return GetOrCreateGroupContainer(Convert.ToInt32(group));
        }
    }
}
