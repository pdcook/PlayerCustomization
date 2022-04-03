using BepInEx;
using HarmonyLib;
using TMPro;
using UnboundLib;
using UnboundLib.Utils.UI;
using UnityEngine;
using PlayerCustomizationUtils;
using System.IO;

namespace FacesPlus
{
    [BepInDependency("com.willis.rounds.unbound", BepInDependency.DependencyFlags.HardDependency)] // necessary for most modding stuff here
    [BepInPlugin(ModId, ModName, Version)]
    [BepInProcess("Rounds.exe")]
    public class FacesPlus : BaseUnityPlugin
    {
        private const string ModId = "pykess.rounds.plugins.facesplus";
        internal const string ModName = "Faces Plus";
        public const string Version = "0.0.0";
        internal static string CompatibilityModName => ModName.Replace(" ", "");

        public static FacesPlus instance;

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
            Unbound.RegisterCredits(ModName, new string[] { "Pykess" }, new string[] { "github", "Support Pykess" }, new string[] { "REPLACE WITH LINK", "https://ko-fi.com/pykess" });

            string[] img_files = Directory.GetFiles(Paths.PluginPath, "*.png", SearchOption.AllDirectories);
            foreach (string file in img_files)
            {
                if (!file.Contains("FACESPLUS")) { continue; }
                float scale = 1f;
                float moveHealthBarUp = 0f;
                if (file.Contains("helmet")) { scale = 0.8f; }
                CustomCharacterItemManager.AddCustomCharacterItem(file, CharacterItemType.Detail, scale, moveHealthBarUp);
            }

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