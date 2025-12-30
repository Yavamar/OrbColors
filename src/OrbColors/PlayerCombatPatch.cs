using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using UnityEngine;

namespace OrbColors
{
    [HarmonyPatch(typeof(PlayerCombat), "OnBlockingChange")]
    internal class PlayerCombatPatch
    {
        [HarmonyPrefix]
        public static bool OnBlockingChange(PlayerCombat __instance, bool _newBool)
        {
            if (!__instance._player || __instance._player._bufferingStatus || !Plugin._playerOrbColors.ContainsKey(__instance._player._steamID) || !Plugin._playerOrbColors[__instance._player._steamID]._orbColorsEnabled)
            {
                return true;
            }

            float size = Plugin._playerOrbColors[__instance._player._steamID]._size;

            if (_newBool)
            {
                __instance._pSound._aSrcGeneral.PlayOneShot(__instance._pSound._generalSounds[12], 0.35f);
                if ((bool)__instance._equippedShield && __instance._currentScriptableWeaponType._weaponHandedness == WeaponHandedness.One_Handed)
                {
                    __instance._pVisual.Local_CrossFadeAnim("Block_Shield", 0f, 3);
                }
                else
                {
                    __instance._pVisual.Local_CrossFadeAnim(__instance._currentScriptableWeaponType._weaponData._weaponBlockAnimation ?? "", 0f, 3);
                }

                __instance._pVisual._blockOrbEffect.transform.localScale = Vector3.one * size*2;
                __instance._pVisual._blockOrbEffect.setScale = new Vector3(size, size, size);
                __instance._pVisual._preventRotationLerp = false;
                if (__instance.isLocalPlayer)
                {
                    __instance._pVisual.Cmd_ResetSpinPlayerModel();
                }
            }
            else
            {
                __instance._pVisual.Local_CrossFadeAnim("Empty", 0f, 3);
                __instance._pVisual._blockOrbEffect.setScale = Vector3.zero;
            }

            return false;
        }
    }
}
