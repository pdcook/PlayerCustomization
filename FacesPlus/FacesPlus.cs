using BepInEx;
using HarmonyLib;
using TMPro;
using UnboundLib;
using UnboundLib.Utils.UI;
using UnityEngine;
using PlayerCustomizationUtils;
using System.IO;
using System;

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
            CharacterItemType itemType;
            foreach (string file in img_files)
            {
                if (!file.Contains("\\FACESPLUS\\")) { continue; }

                if (file.Contains("\\DETAIL\\")) { itemType = CharacterItemType.Detail; }
                else if (file.Contains("\\EYES\\")) { itemType = CharacterItemType.Eyes; }
                else if (file.Contains("\\MOUTH\\")) { itemType = CharacterItemType.Mouth; }
                else { continue; }

                bool matchPlayerColor = false;
                if (file.Contains("MATCHCOLOR")) { matchPlayerColor = true; }

                Vector2 scaleAndOffset = this.ReadScaleAndHealthBarOffset(file);
                float scale = scaleAndOffset.x;
                float moveHealthBarUp = scaleAndOffset.y;

                if (!matchPlayerColor)
                {
                    CustomCharacterItemManager.AddCustomCharacterItem(file, itemType, scale, moveHealthBarUp);
                }
                else
                {
                    GameObject item = ColorMatchingItem(file);
                    CustomCharacterItemManager.AddCustomCharacterItem(item, itemType, scale, moveHealthBarUp);
                }
            }
            // add a blank eye object since none exist in the vanilla game
            CustomCharacterItemManager.AddCustomCharacterItem((Sprite)null, CharacterItemType.Eyes, 1, 0, "blank");

        }

        private Vector2 ReadScaleAndHealthBarOffset(string filename)
        {
            float scale = 1f;
            float healthBarOffset = 0f;
            string[] parts = filename.ToLower().Replace(".png","").Split(new char[] { '_', ' ', '-' });
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i].Equals("scale"))
                {
                    float.TryParse(parts[i + 1], out scale);
                }
                if (parts[i].Equals("healthbaroffset"))
                {
                    float.TryParse(parts[i + 1], out healthBarOffset);
                }
            }
            return new Vector2(scale, healthBarOffset);
        }
        
        private GameObject ColorMatchingItem(string file)
        {
            byte[] bytes = File.ReadAllBytes(file);
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(bytes);
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            GameObject item = new GameObject(Guid.NewGuid().ToString(), typeof(SpriteRenderer), typeof(MatchPlayerColor));
            item.GetComponent<SpriteRenderer>().sprite = sprite;

            return item;
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
    class MatchPlayerColor : MonoBehaviour
    {
        void Update()
        {
            Color? color = this.gameObject?.transform?.parent?.parent?.Find("Health")?.GetComponent<SpriteRenderer>()?.color;
            if (color is null)
            {
                color = this.gameObject?.transform?.parent?.parent?.Find("Face")?.GetComponent<SpriteRenderer>()?.color;
            }
            if (color != null)
            {
                this.gameObject.GetComponent<SpriteRenderer>().color = color.Value;
            }
        }
    }
}