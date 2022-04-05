using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
namespace PlayerCustomizationUtils.Patches
{
    [HarmonyPatch(typeof(CharacterCreatorHandler), nameof(CharacterCreatorHandler.EditCharacterLocalMultiplayer))]
    class CharacterCreatorHandler_Patch_EditCharacterLocalMultiplayer
    {
        // set all gridlayout components on children named "Items" to have a constrained number of columns
        static void Postfix(CharacterCreatorHandler __instance)
        {
            foreach (GridLayoutGroup grid in __instance.GetComponentsInChildren<GridLayoutGroup>())
            {
                if (grid.gameObject.name == "Items")
                {
                    grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                    grid.constraintCount = PlayerCustomizationUtils.CharacterCreatorColumns;
                }
            }
        }
    }
    [HarmonyPatch(typeof(CharacterCreatorHandler), nameof(CharacterCreatorHandler.EditCharacterPortrait))]
    class CharacterCreatorHandler_Patch_EditCharacterPortrait
    {
        // set all gridlayout components on children named "Items" to have a constrained number of columns
        static void Postfix(CharacterCreatorHandler __instance)
        {
            foreach (GridLayoutGroup grid in __instance.GetComponentsInChildren<GridLayoutGroup>())
            {
                if (grid.gameObject.name == "Items")
                {
                    grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                    grid.constraintCount = PlayerCustomizationUtils.CharacterCreatorColumns;
                }
            }
        }
    }
}
