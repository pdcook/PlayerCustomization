using HarmonyLib;
using UnityEngine;
using UnboundLib.Extensions;
namespace PlayerCustomizationUtils.Patches
{
    [HarmonyPatch(typeof(CardChoiceVisuals), nameof(CardChoiceVisuals.Show))]
    internal class CardChoiceVisuals_Patch_Show
    {
        private static void Postfix(CardChoiceVisuals __instance, int pickerID)
        {
            // set the sprite renderer's color to the player's skin color
            try
            {
                __instance.transform.GetChild(0).Find("Health").GetComponent<SpriteRenderer>().color = PlayerSkinBank.GetPlayerSkinColors(PlayerManager.instance.players[pickerID].colorID()).color;
            }
            catch { }
        }
    }
}
