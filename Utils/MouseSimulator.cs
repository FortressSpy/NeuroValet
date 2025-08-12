using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace NeuroValet.Utils
{
    public static class MouseSimulator
    {
        public static bool OverrideMouse = false;
        public static Vector3 ForcedPos = Vector3.zero;

        public static bool OverrideButtons = false;
        public static bool[] ForcedButtons = new bool[3]; // left, right, middle

        private static Texture2D _cursorTex;

        static MouseSimulator()
        {
            // Make a simple white square cursor texture
            _cursorTex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            var pixels = new Color[16 * 16];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.red; // red square
            _cursorTex.SetPixels(pixels);
            _cursorTex.Apply();
        }

        public static void LoadCursorTexture(string filePath)
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
        public static void SetMousePosition(Vector3 pos, ManualLogSource logger = null)
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
        public static void ReleaseMousePosition()
        {
            OverrideMouse = false;
        }

        /// <summary>
        /// Sets a mouse button to pressed (true) or released (false).
        /// </summary>
        public static void SetMouseButton(int button, bool pressed)
        {
            if (button < 0 || button >= ForcedButtons.Length) return;
            ForcedButtons[button] = pressed;
            OverrideButtons = true;
        }

        /// <summary>
        /// Releases control of button states to the real user.
        /// </summary>
        public static void ReleaseMouseButtons()
        {
            OverrideButtons = false;
        }

        // Draw the fake cursor
        public static void DrawCursor()
        {
            if (OverrideMouse)
            {
                // Unity GUI Y-axis is inverted from Input.mousePosition
                var guiPos = new Vector2(ForcedPos.x, Screen.height - ForcedPos.y);
                GUI.DrawTexture(new Rect(guiPos.x - 8, guiPos.y - 8, 48, 48), _cursorTex);
            }
        }
    }

    // ===== Harmony patches =====

    [HarmonyPatch(typeof(Input), "mousePosition", MethodType.Getter)]
    public static class MousePositionPatch
    {
        static bool Prefix(ref Vector3 __result)
        {
            if (MouseSimulator.OverrideMouse)
            {
                __result = MouseSimulator.ForcedPos;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Input), "GetMouseButton")]
    public static class MouseButtonPatch
    {
        static bool Prefix(int button, ref bool __result)
        {
            if (MouseSimulator.OverrideButtons && button >= 0 && button < MouseSimulator.ForcedButtons.Length)
            {
                __result = MouseSimulator.ForcedButtons[button];
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Input), "GetMouseButtonDown")]
    public static class MouseButtonDownPatch
    {
        private static bool[] prevState = new bool[3];

        static bool Prefix(int button, ref bool __result)
        {
            if (MouseSimulator.OverrideButtons && button >= 0 && button < MouseSimulator.ForcedButtons.Length)
            {
                __result = MouseSimulator.ForcedButtons[button] && !prevState[button];
                prevState[button] = MouseSimulator.ForcedButtons[button];
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Input), "GetMouseButtonUp")]
    public static class MouseButtonUpPatch
    {
        private static bool[] prevState = new bool[3];

        static bool Prefix(int button, ref bool __result)
        {
            if (MouseSimulator.OverrideButtons && button >= 0 && button < MouseSimulator.ForcedButtons.Length)
            {
                __result = !MouseSimulator.ForcedButtons[button] && prevState[button];
                prevState[button] = MouseSimulator.ForcedButtons[button];
                return false;
            }
            return true;
        }
    }
}
