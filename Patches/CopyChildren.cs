using UnityEngine;
using HarmonyLib;
namespace PlayerCustomization.Patches
{
    [HarmonyPatch(typeof(CopyChildren), nameof(CopyChildren.DoUpdate))]
    class CopyChildren_Patch_DoUpdate
    {
        /// <summary>
        /// This patch replaces the really stupid implementation that the base-game uses.
        /// Instead of destroying and recreating the children, it just updates the existing ones.
        /// Unless their gameObject names have changed, then it will create new ones.
        /// </summary>
        /// <param name="__instance"></param>
        /// <returns></returns>
        static bool Prefix(CopyChildren __instance)
        {
            // iterate through all the children of the target, starting from the highest child index
            // if the corresponding child object on this gameobject has the same name, then update its position
            // if this object has fewer children than the target, then instantiate a copy of the target's child
            // if the name of this object's child is different from the target's child, then destroy this object's child and instantiate a copy of the target's child
            for (int i = __instance.target.transform.childCount - 1; i >= 0; i--)
            {
                Transform targetChild = __instance.target.transform.GetChild(i);
                if (__instance.transform.childCount <= i)
                {
                    // this object has fewer children than the target, so instantiate a copy of the target's child
                    GameObject newChild = GameObject.Instantiate(targetChild.gameObject, __instance.transform.TransformPoint(targetChild.localPosition), Quaternion.identity, __instance.transform);
                    newChild.transform.localScale = targetChild.transform.localScale;
                    newChild.name = targetChild.name;
                    newChild.transform.SetSiblingIndex(i);
                }
                else
                {
                    // this object has the same number of children as the target, so update the position of the corresponding child
                    Transform thisChild = __instance.transform.GetChild(i);
                    if (thisChild.name == targetChild.name)
                    {
                        thisChild.position = __instance.transform.TransformPoint(targetChild.localPosition);
                    }
                    else
                    {
                        // this object's child has a different name than the target's child, so destroy this object's child and instantiate a copy of the target's child
                        GameObject newChild = GameObject.Instantiate(targetChild.gameObject, __instance.transform.TransformPoint(targetChild.localPosition), Quaternion.identity, __instance.transform);
                        newChild.transform.localScale = targetChild.transform.localScale;
                        newChild.name = targetChild.name;
                        newChild.transform.SetSiblingIndex(i);
                    }
                }
            }
            // if this object has more children than the target, then destroy the extra children
            for (int i = __instance.transform.childCount - 1; i >= __instance.target.transform.childCount; i--)
            {
                GameObject.DestroyImmediate(__instance.transform.GetChild(i).gameObject);
            }




            return false; // Don't run the original method
        }
    }
}
