using GameViews.BottomNav;
using GameViews.Converse;
using GameViews.Story;
using HarmonyLib;
using System.Text.RegularExpressions;
using TMPro;

namespace NeuroValet.Overrides
{
    internal static class NamesOverrides
    {
        public static string ReplacePassepartoutName(string original)
        {
            // Replace Jean Passepartout with Neurosama
            original = Regex.Replace(original, "Jean Passepartout", "Neurosama");

            // Replace just 'Passepartout' with Neuro
            original = Regex.Replace(original, "Passepartout", "Neuro");

            return original;
        }

        public static string ReplaceFoggName(string original)
        {
            // Replace Fogg full name with Vedal987
            original = Regex.Replace(original, "Phileas Fogg", "Vedal987");

            // Replace just 'Fogg' with Vedal
            original = Regex.Replace(original, "Fogg", "Vedal");

            return original;
        }

        [HarmonyPatch(typeof(FoggPanelView))]
        [HarmonyPatch("AddFoggBubble")]
        class Patch_FoggPanelView_AddFoggBubble
        {
            static bool Prefix(FoggPanelView __instance, ref FoggBubbleItem data)
            {
                data.text = ReplacePassepartoutName(data.text);

                return true;
            }
        }

        [HarmonyPatch(typeof(Conversation), "characterGreeting", MethodType.Getter)]
        class Patch_Conversation_characterGreeting
        {
            static bool Prefix(Conversation __instance, ref string __result)
            {
                if (__instance.talkingToFogg)
                {
                    __result = "Neuro?";
                }
                else
                {
                    __result = ReplacePassepartoutName(__instance._character.salute);
                }
                if (__result == null)
                {
                    __result = "Greetings, Neurosama!";
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(Conversation), "playerGreeting", MethodType.Getter)]
        class Patch_Conversation_playerGreeting
        {
            static bool Prefix(Conversation __instance, ref string __result)
            {
                if (__instance.talkingToFogg)
                {
                    __result = "Vedal?";
                    return false;
                }
                else
                {
                    // let original getter handle other scenarios
                    return true;
                }
            }
        }

        [HarmonyPatch(typeof(StorySentenceElement), "text", MethodType.Setter)]
        class Patch_StorySentenceElement_text
        {
            public static void Prefix(ref string value, StorySentenceElement __instance)
            {
                value = ReplaceFoggName(value);
                value = ReplacePassepartoutName(value);
            }
        }

        [HarmonyPatch(typeof(StoryViewElement), "textComponent", MethodType.Setter)]
        class Patch_StoryViewElement_textComponent
        {
            public static void Prefix(ref string value, StoryViewElement __instance)
            {
                value = ReplaceFoggName(value);
                value = ReplacePassepartoutName(value);
            }
        }
    }
}
