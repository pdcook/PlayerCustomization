using BepInEx;
using HarmonyLib;
using UnboundLib;
using UnityEngine;
using UnboundLib.Utils.UI;
using UnboundLib.GameModes;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnboundLib.Networking;
using Photon.Pun;
using On;

namespace PlayerCustomizationUtils
{
    [BepInDependency("com.willis.rounds.unbound", BepInDependency.DependencyFlags.HardDependency)] // necessary for most modding stuff here
    [BepInPlugin(ModId, ModName, Version)]
    [BepInProcess("Rounds.exe")]
    public class PlayerCustomizationUtils : BaseUnityPlugin
    {
        private const string ModId = "pykess.rounds.plugins.playercustomizationutils";
        internal const string ModName = "Player Customization Utilities";
        public const string Version = "1.0.0";
        internal static string CompatibilityModName => ModName.Replace(" ", "");

        public static PlayerCustomizationUtils instance;

        public const int CharacterCreatorColumns = 17;

        private Harmony harmony;

#if DEBUG
        public static readonly bool DEBUG = true;
#else
        public static readonly bool DEBUG = false;
#endif
        internal static void Log(object log)
        {
            if (DEBUG)
            {
                UnityEngine.Debug.Log($"[{ModName}] {log}");
            }
        }


        private void Awake()
        {
            instance = this;
            
            harmony = new Harmony(ModId);
            harmony.PatchAll();
            
            On.MainMenuHandler.Awake += (orig, self) =>
            {
                this.ExecuteAfterFrames(10, () =>
                {
                    CustomCharacterItemManager.UpdateCharacterCreator();
                });

                orig(self);
            };
        }
        private void Start()
        {
            // add credits
            Unbound.RegisterCredits(ModName, new string[] { "Pykess" }, new string[] { "github", "Support Pykess" }, new string[] { "https://github.com/pdcook/PlayerCustomization", "https://ko-fi.com/pykess"});
        }

        private void OnDestroy()
        {
            harmony.UnpatchAll();
        }

        internal static string GetConfigKey(string key) => $"{PlayerCustomizationUtils.ModName}_{key}";

        private static void GUI(GameObject menu)
        {
            MenuHandler.CreateText(ModName, menu, out TextMeshProUGUI _, 60);
            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 30);
        }
    }
}