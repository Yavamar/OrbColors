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
            if (NetworkClient.active && Plugin._playerOrbColors.TryGetValue(__instance._player._steamID, out (bool _orbColorsEnabled, Color _color, float _size) value) && value._orbColorsEnabled)
            {
                __instance._blockOrbRender.material.SetColor("_EmissionColor", value._color);

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
