using UnityEngine;
using HarmonyLib;
namespace PlayerCustomization.Patches
{
    [HarmonyPatch(typeof(CharacterCreatorItemLoader), "GetItemID")]
    class CharacterCreatorItemLoader_Patch_GetItemID
    {
        /// <summary>
        /// Instead of comparing item sprites, compare item names.
        /// </summary>
        /// <param name="__result"></param>
        static void Postfix(CharacterCreatorItemLoader __instance, CharacterItem newSprite, CharacterItemType itemType, ref int __result)
        {
            CharacterItem[] array;
			if (itemType == CharacterItemType.Eyes)
			{
				array = __instance.eyes;
			}
			else if (itemType == CharacterItemType.Mouth)
			{
				array = __instance.mouths;
			}
			else
			{
				array = __instance.accessories;
			}
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].name.Replace("(Clone)","") == newSprite.name.Replace("(Clone)",""))
				{
					__result = i;
				}
			}
		}
    }
}
