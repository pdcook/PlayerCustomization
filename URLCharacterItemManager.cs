using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnboundLib;
using TMPro;
using UnityEngine.UI;
using UnboundLib.Utils.UI;
namespace PlayerCustomization
{
    public static class URLCharacterItemManager
    {
        public const int ITEMS_PER_TYPE = 6;

        public const string defaultURL = "https://upload.wikimedia.org/wikipedia/commons/5/50/Smile_Image.png";

        public static Dictionary<string, URLCharacterItem> items = new Dictionary<string, URLCharacterItem>() { };

        private static Dictionary<CharacterItemType, List<List<TextMeshProUGUI>>> itemButtons = new Dictionary<CharacterItemType, List<List<TextMeshProUGUI>>>() { };

        public static void CreateURLCharacterItemPrefab(string name, string url = defaultURL)
        {
            if (items.ContainsKey(name))
            {
                UnityEngine.Debug.LogError("URLCharacterItemManager: CreateURLCharacterItemPrefab: Item with name " + name + " already exists");
                return;
            }
            GameObject prefab = new GameObject(name);
            UnityEngine.GameObject.DontDestroyOnLoad(prefab);
            URLCharacterItem item = prefab.AddComponent<URLCharacterItem>();
            prefab.AddComponent<SpriteRenderer>();
            item.name = name;
            item.URL = url;
            URLCharacterItemManager.SetURLCharacterItemPrefab(item);
        }

        public static URLCharacterItem GetURLCharacterItemPrefab(string name)
        {
            name = name.Replace("(Clone)", "").ToLower();
            if (items.ContainsKey(name))
            {
                return items[name];
            }
            else
            {
                UnityEngine.Debug.LogError("URLCharacterItemManager: GetURLCharacterItemPrefab: Item with name " + name + " does not exist");
                return null;
            }
        }
        public static void SetURLCharacterItemPrefab(URLCharacterItem item)
        {
            items[item.name.Replace("(Clone)", "").ToLower()] = item;
        }
        internal static string GetConfigKey(string key) => $"{PlayerCustomization.CompatibilityModName}_URLItem_{key}";

        public static string GetURL(int itemID, CharacterItemType itemType)
        {
            return PlayerPrefs.GetString(GetConfigKey($"{itemType}{itemID}URL"), defaultURL);
        }
        public static void SetURL(int itemID, CharacterItemType itemType, string url)
        {
            PlayerPrefs.SetString(GetConfigKey($"{itemType}{itemID}URL"), url);
        }
        public static float GetScale(int itemID, CharacterItemType itemType)
        {
            return PlayerPrefs.GetFloat(GetConfigKey($"{itemType}{itemID}Scale"), 1f);
        }
        public static void SetScale(int itemID, CharacterItemType itemType, float scale)
        {
            PlayerPrefs.SetFloat(GetConfigKey($"{itemType}{itemID}Scale"), scale);
        }
        public static float GetHealthBarOffset(int itemID, CharacterItemType itemType)
        {
            return PlayerPrefs.GetFloat(GetConfigKey($"{itemType}{itemID}HealthBarOffset"), 0f);
        }
        public static void SetHealthBarOffset(int itemID, CharacterItemType itemType, float offset)
        {
            PlayerPrefs.SetFloat(GetConfigKey($"{itemType}{itemID}HealthBarOffset"), offset);
        }

        internal static string GetItemName(int id, CharacterItemType itemType)
        {
            string name = PlayerPrefs.GetString(GetConfigKey($"{itemType}{id}Name"), $"{itemType} {id + 1}".ToUpper());
            if (name.Length <= 0 || name.Length > 32)
            {
                SetItemName(id, itemType, $"{itemType} {id + 1}".ToUpper());
                name = PlayerPrefs.GetString(GetConfigKey($"{itemType}{id}Name"), $"{itemType} {id + 1}".ToUpper());
            }
            return name;
        }
        internal static void SetItemName(int id, CharacterItemType itemType, string name)
        {
            PlayerPrefs.SetString(GetConfigKey($"{itemType}{id}Name"), name);
        }

        internal static void URLItemsMenu(GameObject menu)
        {
            // initialize dictionaries
            itemButtons[CharacterItemType.Eyes] = new List<List<TextMeshProUGUI>>() { };
            itemButtons[CharacterItemType.Mouth] = new List<List<TextMeshProUGUI>>() { };
            itemButtons[CharacterItemType.Detail] = new List<List<TextMeshProUGUI>>() { };
            for (int i = 0; i < ITEMS_PER_TYPE; i++)
            {
                itemButtons[CharacterItemType.Eyes].Add(new List<TextMeshProUGUI>() { });
                itemButtons[CharacterItemType.Mouth].Add(new List<TextMeshProUGUI>() { });
                itemButtons[CharacterItemType.Detail].Add(new List<TextMeshProUGUI>() { });
            }

            GameObject eyesMenu = MenuHandler.CreateMenu("Eyes", () => { }, menu, 60, true, true, menu.transform.parent.gameObject);
            GameObject mouthsMenu = MenuHandler.CreateMenu("Mouths", () => { }, menu, 60, true, true, menu.transform.parent.gameObject);
            GameObject detailsMenu = MenuHandler.CreateMenu("Details", () => { }, menu, 60, true, true, menu.transform.parent.gameObject);
            ItemsMenu(eyesMenu, CharacterItemType.Eyes);
            ItemsMenu(mouthsMenu, CharacterItemType.Mouth);
            ItemsMenu(detailsMenu, CharacterItemType.Detail);
        }
        private static void ItemsMenu(GameObject menu, CharacterItemType itemType)
        {
            for (int i = 0; i < ITEMS_PER_TYPE; i++)
            {
                // must create a local copy by value of the index
                int j = i;
                string name = $"{GetItemName(j, itemType)}";
                GameObject editItemObj = MenuHandler.CreateMenu(name, () => { }, menu, 60, true, true, menu.transform.parent.gameObject);
                EditItemMenu(editItemObj, itemType, j);
                if (menu.transform.Find("Group/Grid/Scroll View/Viewport/Content"))
                {
                    itemButtons[itemType][j].Add(menu.transform.Find("Group/Grid/Scroll View/Viewport/Content").Find(name).GetComponentInChildren<TextMeshProUGUI>());
                }
                else if (menu.transform.Find("Group"))
                {
                    itemButtons[itemType][j].Add(menu.transform.Find("Group").Find(name).GetComponentInChildren<TextMeshProUGUI>());
                }
                else
                {
                    UnityEngine.Debug.LogError("Button item was null.");
                }
            }
        }

        private static void EditItemMenu(GameObject menu, CharacterItemType itemType, int ID)
        {
            void ChangeItemName(string newName)
            {
                if (newName.Length > 0 && newName.Length <= 32)
                {
                    UpdateItemNameObjects(ID, itemType, newName);
                    SetItemName(ID, itemType, newName);
                }
            }
            itemButtons[itemType][ID].Add(MenuHandler.CreateText(GetItemName(ID, itemType), menu, out TextMeshProUGUI _, 60).GetComponentInChildren<TextMeshProUGUI>());
            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 30);
            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 30);
            // TODO: create preview
            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 30);
            MenuHandler.CreateInputField(GetItemName(ID, itemType), 60, menu, ChangeItemName);
            void ChangeURL(string newURL)
            {
                SetURL(ID, itemType, newURL);
            }
            MenuHandler.CreateInputField(GetURL(ID, itemType), 60, menu, ChangeURL);
            void ChangeScale(float newScale)
            {
            }
            MenuHandler.CreateSlider("Scale", menu, 45, 0f, 10f, 1f, ChangeScale, out Slider _, false);
            void ChangeHealthBarOffset(float newOffset)
            {
                
            }
            MenuHandler.CreateSlider("Health Bar Offset", menu, 45, 0f, 2f, 0f, ChangeHealthBarOffset, out Slider _, false);
        }
        private static void UpdateItemNameObjects(int id, CharacterItemType itemType, string newName)
        {
            foreach (TextMeshProUGUI text in itemButtons[itemType][id].Where(t => t != null))
            {
                text.text = newName.ToUpper();
            }
        }
    }
}
