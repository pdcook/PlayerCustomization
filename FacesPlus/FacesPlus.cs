using BepInEx;
using HarmonyLib;
using TMPro;
using UnboundLib;
using UnboundLib.Utils.UI;
using UnityEngine;
using PlayerCustomizationUtils;
using System.IO;
using System;
using System.Linq;

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
            img_files = img_files.Concat(Directory.GetFiles(Paths.PluginPath, "*.gif", SearchOption.AllDirectories)).ToArray();
            CharacterItemType itemType;
            foreach (string file in img_files)
            {
                if (!file.Contains("\\FACESPLUS\\")) { continue; }

                if (file.Contains("\\0-DETAIL\\") || file.Contains("\\1-BODY\\")) { itemType = CharacterItemType.Detail; }
                else if (file.Contains("\\2-EYES\\")) { itemType = CharacterItemType.Eyes; }
                else if (file.Contains("\\3-MOUTH\\")) { itemType = CharacterItemType.Mouth; }
                else { continue; }

                bool matchPlayerColor = false;
                bool isBodyItem = false;
                if (file.Contains("MATCHCOLOR")) { matchPlayerColor = true; }
                if (file.Contains("\\1-BODY\\")) { isBodyItem = true; }

                Vector3 scaleAndOffset = this.ReadExtraProperties(file);
                float scale = scaleAndOffset.x;
                float moveHealthBarUp = scaleAndOffset.y;
                float brightness = scaleAndOffset.z;

                if (!matchPlayerColor && !isBodyItem)
                {
                    CustomCharacterItemManager.AddCustomCharacterItem(file, itemType, scale, moveHealthBarUp, brightness);
                }
                else if (isBodyItem)
                {
                    GameObject item = BodyItem(file);
                    CustomCharacterItemManager.AddCustomCharacterItem(item, itemType, 1f, moveHealthBarUp, 1f);
                }
                else
                {
                    GameObject item = ColorMatchingItem(file);
                    CustomCharacterItemManager.AddCustomCharacterItem(item, itemType, scale, moveHealthBarUp, brightness);
                }
            }
            // add a blank eye object since none exist in the vanilla game
            CustomCharacterItemManager.AddCustomCharacterItem((Sprite)null, CharacterItemType.Eyes, 1, 0, 1f, "blank");

        }

        private Vector3 ReadExtraProperties(string filename)
        {
            float scale = 1f;
            float healthBarOffset = 0f;
            float brightness = 1f;
            string[] parts = filename.ToLower().Replace(".png","").Replace(".gif","").Split(new char[] { '_', ' ', '-' });
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
                if (parts[i].Equals("brightness"))
                {
                    float.TryParse(parts[i + 1], out brightness);
                }
            }
            return new Vector3(scale, healthBarOffset, brightness);
        }
        
        private GameObject ColorMatchingItem(string file)
        {
            byte[] bytes = File.ReadAllBytes(file);
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(bytes);
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            GameObject item = new GameObject(Path.GetFileNameWithoutExtension(file), typeof(SpriteRenderer), typeof(ColorMatchingCharacterItem));
            item.GetComponent<SpriteRenderer>().sprite = sprite;

            return item;
        }
        private GameObject BodyItem(string file)
        {
            byte[] bytes = File.ReadAllBytes(file);
            if(file.EndsWith(".gif", true, System.Globalization.CultureInfo.InvariantCulture))
            {
                GameObject item = new GameObject(Path.GetFileNameWithoutExtension(file), typeof(SpriteRenderer), typeof(GifBodyCharacterItem));
                item.GetComponent<SpriteRenderer>().enabled = false;
                item.GetComponent<GifBodyCharacterItem>().gifData = bytes;
                return item;
            }
            else
            {
                Texture2D tex = new Texture2D(2, 2);
                tex.LoadImage(bytes);
                Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                GameObject item = new GameObject(Path.GetFileNameWithoutExtension(file), typeof(SpriteRenderer), typeof(BodyCharacterItem));
                item.GetComponent<SpriteRenderer>().sprite = sprite;
                item.GetComponent<SpriteRenderer>().enabled = false;
                return item;
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