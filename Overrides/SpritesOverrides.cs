using GameResources.Conversation;
using GameResources.MapData;
using GameViews.BottomNav;
using GameViews.Converse;
using HarmonyLib;
using UnityEngine;

namespace NeuroValet.Overrides
{
    public static class SpritesOverrides
    {
        private static Texture2D vedalBubbleViewTexture;
        public static Sprite vedalBubbleSprite;

        private static Texture2D vedalPortraitTexture;
        public static Sprite vedalPortraitSprite;

        private static Texture2D neuroPortraitTexture;
        public static Sprite neuroPortraitSprite;

        private static ConversationCharacterData neuroPortraitData;

        public static void LoadSprites(string pluginDir)
        {
            LoadVedalBubbleViewSprite(pluginDir);
            LoadVedalPortraitSprite(pluginDir);
            LoadNeuroPortraitSprite(pluginDir);
        }
        
        private static void LoadVedalBubbleViewSprite(string pluginDir)
        {
            // Load replacement texture from disk
            string cursorPath = System.IO.Path.Combine(pluginDir, @"Assets\VedalBubbleView.png");
            byte[] imgBytes = System.IO.File.ReadAllBytes(cursorPath);
            vedalBubbleViewTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            vedalBubbleViewTexture.LoadImage(imgBytes);

            // Create Unity Sprite
            vedalBubbleSprite = Sprite.Create(
                vedalBubbleViewTexture,
                new Rect(0, 0, vedalBubbleViewTexture.width, vedalBubbleViewTexture.height),
                new Vector2(0.5f, 0.5f) // pivot in center
            );
            vedalBubbleSprite.name = "VedalBubbleView.png";
        }

        private static void LoadVedalPortraitSprite(string pluginDir)
        {
            // Load replacement texture from disk
            string cursorPath = System.IO.Path.Combine(pluginDir, @"Assets\Vedal.png");
            byte[] imgBytes = System.IO.File.ReadAllBytes(cursorPath);
            vedalPortraitTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            vedalPortraitTexture.LoadImage(imgBytes);

            // Create Unity Sprite
            vedalPortraitSprite = Sprite.Create(
                vedalPortraitTexture,
                new Rect(0, 0, vedalPortraitTexture.width, vedalPortraitTexture.height),
                new Vector2(0.5f, 0.5f) // pivot in center
            );
            vedalPortraitSprite.name = "Vedal.png";
        }


        private static void LoadNeuroPortraitSprite(string pluginDir)
        {
            // Load replacement texture from disk
            string cursorPath = System.IO.Path.Combine(pluginDir, @"Assets\NeuroPortrait.png");
            byte[] imgBytes = System.IO.File.ReadAllBytes(cursorPath);
            neuroPortraitTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            neuroPortraitTexture.LoadImage(imgBytes);

            // Create Unity Sprite
            neuroPortraitSprite = Sprite.Create(
                neuroPortraitTexture,
                new Rect(0, 0, neuroPortraitTexture.width, neuroPortraitTexture.height),
                new Vector2(0.5f, 0.5f) // pivot in center
            );
            neuroPortraitSprite.name = "NeuroPortrait.png";

            neuroPortraitData = ConversationCharacterData.CreateInstance<ConversationCharacterData>();
                neuroPortraitData.sprite = SpritesOverrides.neuroPortraitSprite;
            neuroPortraitData.offset = new Vector2(-0.1f, -0.1f);
            neuroPortraitData.scaleFactor = 0.9f;
        }


        public static void OverrideSprite()
        {
            OverrideFoggBubblePortrait();
            OverrideConversationPassepartoutPortrait();
        }

        private static void OverrideFoggBubblePortrait()
        {
            FoggPanelView foggPanel = (FoggPanelView)GameViews.Static.bottomNavView?.foggPanelView;
            if (foggPanel != null)
            {
                var foggPortrait = foggPanel.GetComponentsInChildren<UnityEngine.UI.Image>().First();
                if (foggPortrait.sprite != SpritesOverrides.vedalBubbleSprite)
                {
                    var rt = foggPortrait.rectTransform;
                    Vector2 oldSize = rt.sizeDelta;

                    foggPortrait.sprite = SpritesOverrides.vedalBubbleSprite;
                    foggPortrait.SetNativeSize();

                    // scale back to old height while preserving aspect
                    float scale = oldSize.y / rt.sizeDelta.y;
                    rt.sizeDelta = new Vector2(rt.sizeDelta.x * scale, rt.sizeDelta.y * scale);
                    // Adjust image position so it will be at about the same location as the original despite different width
                    rt.anchoredPosition = Vector2.zero;
                    rt.pivot = new Vector2(0.625f, -0.05f);
                }
            }
        }

        private static void OverrideConversationPassepartoutPortrait()
        {
            ConversationView conversationView = (ConversationView)GameViews.Static.converseView;
            if (conversationView != null)
            {
                conversationView.leftCharacter.SetImage(neuroPortraitData);
            }
        }
    }

    [HarmonyPatch(typeof(ConversationView))]
    [HarmonyPatch("StartConversationWithCharacterOnJourney")]
    class Patch_ConversationView_StartConversation
    {
        // Match the original method signature
        static bool Prefix(ConversationView __instance, ref ICastMember characterData, IJourneyInfo journey)
        {
            if (characterData.isFogg)
            {
                characterData.characterImage.sprite = SpritesOverrides.vedalPortraitSprite;
                characterData.characterImage.pivot = new Vector2(0.0f, 0.0f);
            }

            return true;
        }
    }
}
