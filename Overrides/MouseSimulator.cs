using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections;
using UnityEngine;

namespace NeuroValet.Overrides
{
    public class MouseSimulator : MonoBehaviour
    {
        // Implement a singleton pattern for the GlobeViewParser
        private static MouseSimulator _instance;

        public static MouseSimulator Instance
        {
            get
            {
                if (_instance == null)
                {
                    var obj = new GameObject("MouseSimulator");
                    _instance = obj.AddComponent<MouseSimulator>();
                    DontDestroyOnLoad(obj); // Optional: persists across scenes. probably unnecessary but just in case
                }
                return _instance;
            }
        }

        public bool OverrideMouse { get => overrideMouse; private set => overrideMouse = value; }
        public Vector3 ForcedPos { get => forcedPos; private set => forcedPos = value; }
        public bool OverrideButtons { get => overrideButtons; private set => overrideButtons = value; }

        private bool overrideMouse = false;
        private Vector3 forcedPos = Vector3.zero;
        private bool overrideButtons = false;
        private Texture2D _cursorTex;

        private MouseSimulator()
        {
        }

        private void Awake()
        {
            // Make a simple white square cursor texture
            _cursorTex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            var pixels = new Color[16 * 16];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.red; // red square
            _cursorTex.SetPixels(pixels);
            _cursorTex.Apply();
        }

        public void LoadCursorTexture(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
            {
                Debug.LogError($"Cursor texture not found at {filePath}");
                return;
            }

            byte[] fileData = System.IO.File.ReadAllBytes(filePath);
            Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (tex.LoadImage(fileData))
            {
                _cursorTex = tex;
            }
            else
            {
                Debug.LogError("Failed to load cursor image");
            }
        }

        /// <summary>
        /// Sets the fake mouse position (screen coords, bottom-left = (0,0)).
        /// </summary>
        public void SetMousePosition(Vector3 pos, ManualLogSource logger = null)
        {
            if (logger != null)
            {
                logger.LogInfo($"Mouse position set to: {pos.ToString()}");
            }

            ForcedPos = pos;
            OverrideMouse = true;
        }

        /// <summary>
        /// Releases control of the mouse position to the real user.
        /// </summary>
        public void ReleaseMousePosition()
        {
            OverrideMouse = false;
        }

        /// <summary>
        /// Releases control of button states to the real user.
        /// </summary>
        public void ReleaseMouseButtons()
        {
            OverrideButtons = false;
        }

        // Draw the fake cursor
        public void DrawCursor()
        {
            if (OverrideMouse)
            {
                // Unity GUI Y-axis is inverted from Input.mousePosition
                var guiPos = new Vector2(ForcedPos.x, Screen.height - ForcedPos.y);
                GUI.DrawTexture(new Rect(guiPos.x - 8, guiPos.y - 8, 48, 48), _cursorTex);
            }
        }

        internal void ToggleDebugView()
        {
            OverrideMouse = !OverrideMouse;
        }

        internal void DragItem(UnityEngine.Vector3 sourcePosition, UnityEngine.Vector3 targetPosition, Action OnStart, Action<bool> OnReachTarget)
        {
            SetMousePosition(sourcePosition); // move mouse to current item position
            OnStart(); // hold mouse down to pick up item
            StartCoroutine(DragItemOverTime(sourcePosition, targetPosition, OnReachTarget));
        }

        internal IEnumerator DragItemOverTime(Vector3 startPosition, Vector3 targetPosition, Action<bool> OnReachTarget)
        {
            float duration = 1.5f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                ForcedPos = Vector3.Lerp(startPosition, targetPosition, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            // Ensure final position is set
            ForcedPos = targetPosition;
            OnReachTarget(true);
            ReleaseMousePosition();
        }
    }

    // ===== Harmony patches =====

    [HarmonyPatch(typeof(Input), "mousePosition", MethodType.Getter)]
    public static class MousePositionPatch
    {
        static bool Prefix(ref Vector3 __result)
        {
            if (MouseSimulator.Instance.OverrideMouse)
            {
                __result = MouseSimulator.Instance.ForcedPos;
                return false;
            }
            return true;
        }
    }
}
