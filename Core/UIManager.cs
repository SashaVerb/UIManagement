using System;
using System.Collections.Generic;
using UnityEngine;

namespace UIManagement
{
    public static class UIManager
    {
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
        
        public static T Instantiate<T>(T prefab, int groupId = 0) where T : UIPanel
        {
            EnsureInitialized();

            if (prefab == null)
            {
                Debug.LogError("[UIManager] Instantiate was called with a null prefab.");
                return null;
            }

            var container = GetOrCreateGroupContainer(groupId);
            var instance = UnityEngine.Object.Instantiate(prefab, container);

            if (instance == null)
            {
                Debug.LogError($"[UIManager] Failed to instantiate prefab of type {typeof(T).Name}.");
                return null;
            }

            instance.HideImmediate();

            return instance;
        }

        public static T Instantiate<T, TGroup>(T prefab, TGroup group)
            where T : UIPanel
            where TGroup : System.Enum
        {
            return Instantiate(prefab, Convert.ToInt32(group));
        }

        public static void Clear()
        {
            _groupContainers.Clear();

            if (_canvas != null)
            {
                UnityEngine.Object.Destroy(_canvas.gameObject);
                _canvas = null;
                _canvasTransform = null;
            }

            _isInitialized = false;
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
    }
}
