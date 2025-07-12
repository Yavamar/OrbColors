using CodeTalker.Networking;
using HarmonyLib;
using Mirror;
using UnityEngine;

namespace OrbColors
{
    [HarmonyPatch(typeof(PlayerVisual), "Handle_BlockOrbEffects")]
    internal class PlayerVisualPatch
    {
        public static bool Prefix(PlayerVisual __instance)
        {
            // If you're playing offline, CodeTalker doesn't work. So we have to set the orb color here.
            if(AtlyssNetworkManager._current._soloMode)
            {
                Color color = new(Plugin._myOrbColor["Red"].Value, Plugin._myOrbColor["Green"].Value, Plugin._myOrbColor["Blue"].Value, Plugin._myOrbColor["Alpha"].Value);

                if (!Plugin._playerOrbColors.TryAdd(__instance._player._steamID, (Plugin._customOrbColorEnabled.Value, color)))
                {
                    Plugin._playerOrbColors[__instance._player._steamID] = (Plugin._customOrbColorEnabled.Value, color);
                }
            }

            if (NetworkClient.active && Plugin._playerOrbColors.TryGetValue(__instance._player._steamID, out (bool enabled, Color color) value) && value.enabled)
            {
                __instance._blockOrbRender.material.SetColor("_EmissionColor", value.color);

                if (__instance._pCombat._inParryWindow && !__instance._parryWindowEffect.isEmitting)
                {
                    __instance._parryWindowEffect.Play();
                }

                if (!__instance._pCombat._isBlocking || !__instance._pCombat._inParryWindow)
                {
                    __instance._parryWindowEffect.Stop();
                }
            }
            else
            {
                return true;
            }
            return false;
        }
    }
}
