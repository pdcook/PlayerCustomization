using HarmonyLib;
using UnboundLib;
namespace GameModeCollection.Patches
{
	[HarmonyPatch(typeof(CharacterItemMirror), "Start")]
	class CharacterItemMirror_Patch_Start
	{
		/// <summary>
		/// Fix the base-game mirror issue where items on one side of the character all end up on the other side
		/// </summary>
		/// <param name="__instance"></param>
		static void Postfix(CharacterItemMirror __instance)
		{
			__instance.SetFieldValue("leftRight", LeftRight.Right);
		}
	}
}