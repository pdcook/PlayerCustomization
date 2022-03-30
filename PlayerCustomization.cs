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

namespace PlayerCustomization
{
    [BepInDependency("com.willis.rounds.unbound", BepInDependency.DependencyFlags.HardDependency)] // necessary for most modding stuff here
    [BepInPlugin(ModId, ModName, Version)]
    [BepInProcess("Rounds.exe")]
    public class PlayerCustomization : BaseUnityPlugin
    {
        private const string ModId = "pykess.rounds.plugins.playercustomization";
        internal const string ModName = "Player Customization";
        public const string Version = "0.0.0";
        internal static string CompatibilityModName => ModName.Replace(" ", "");

        public static PlayerCustomization instance;

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
            
            On.MainMenuHandler.Awake += (orig, self) =>
            {
                this.ExecuteAfterFrames(10, () =>
                {
                    URLCharacterItemManager.CreateURLCharacterItemPrefab("Test");
                    CustomCharacterItemManager.AddCustomCharacterItem(URLCharacterItemManager.GetURLCharacterItemPrefab("Test").gameObject, CharacterItemType.Detail);
                });

                orig(self);
            };
        }
        private void Start()
        {
            // add credits
            Unbound.RegisterCredits(ModName, new string[] { "Pykess" }, new string[] { "github", "Support Pykess" }, new string[] { "REPLACE WITH LINK", "https://ko-fi.com/pykess"});

            // add GUI to modoptions menu
            Unbound.RegisterMenu(ModName, () => { }, URLCharacterItemManager.URLItemsMenu, null, false);

        }

        private void OnDestroy()
        {
            harmony.UnpatchAll();
        }

        internal static string GetConfigKey(string key) => $"{PlayerCustomization.ModName}_{key}";

        private static void GUI(GameObject menu)
        {
            MenuHandler.CreateText(ModName, menu, out TextMeshProUGUI _, 60);
            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 30);
        }
    }
}