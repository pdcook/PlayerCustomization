using BepInEx;
using HarmonyLib;
using TMPro;
using UnboundLib;
using UnboundLib.Utils.UI;
using UnityEngine;

namespace URLCharacterItems
{
    [BepInDependency("com.willis.rounds.unbound", BepInDependency.DependencyFlags.HardDependency)] // necessary for most modding stuff here
    [BepInPlugin(ModId, ModName, Version)]
    [BepInProcess("Rounds.exe")]
    public class URLCharacterItems : BaseUnityPlugin
    {
        private const string ModId = "pykess.rounds.plugins.urlcharacteritems";
        internal const string ModName = "URL Character Items";
        public const string Version = "1.0.0";
        internal static string CompatibilityModName => ModName.Replace(" ", "");

        public static URLCharacterItems instance;

        private Harmony harmony;

#if DEBUG
        public static readonly bool DEBUG = true;
#else
        public static readonly bool DEBUG = false;
#endif
        internal static void Log(string str)
        {
            if (DEBUG)
            {
                UnityEngine.Debug.Log($"[{ModName}] {str}");
            }
        }


        private void Awake()
        {
            instance = this;

            harmony = new Harmony(ModId);
            harmony.PatchAll();
        }
        private void Start()
        {
            // add credits
            Unbound.RegisterCredits(ModName, new string[] { "Pykess" }, new string[] { "github", "Support Pykess" }, new string[] { "https://github.com/pdcook/PlayerCustomization", "https://ko-fi.com/pykess" });

            // add GUI to modoptions menu
            Unbound.RegisterMenu(ModName, () => { }, URLCharacterItemManager.URLItemsMenu, null, false);

            // init url character item manager
            URLCharacterItemManager.Init();
            URLCharacterItemManager.AddItemsToCharacterCreator();

        }

        private void OnDestroy()
        {
            harmony.UnpatchAll();
        }

        private static void GUI(GameObject menu)
        {
            MenuHandler.CreateText(ModName, menu, out TextMeshProUGUI _, 60);
            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 30);
        }
    }
}