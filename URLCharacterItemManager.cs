using UnityEngine;
using System.Collections.Generic;
using System.Linq;
namespace PlayerCustomization
{
    public static class URLCharacterItemManager
    {
        public const string defaultURL = "https://upload.wikimedia.org/wikipedia/commons/5/50/Smile_Image.png";

        public static Dictionary<string, URLCharacterItem> items = new Dictionary<string, URLCharacterItem>() { };

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

        public static string GetURL(int itemID)
        {
            return PlayerPrefs.GetString("URLCharacterItem" + itemID, defaultURL);
        }
        public static void SetURL(int itemID, string url)
        {
            PlayerPrefs.SetString("URLCharacterItem" + itemID, url);
        }
        public static float GetScale(int itemID)
        {
            return PlayerPrefs.GetFloat("URLCharacterItem" + itemID, 1f);
        }
        public static void SetScale(int itemID, float scale)
        {
            PlayerPrefs.SetFloat("URLCharacterItem" + itemID, scale);
        }
        public static float GetHealthBarUp(int itemID)
        {
            return PlayerPrefs.GetFloat("URLCharacterItem" + itemID, 0f);
        }
        public static void SetHealthBarUp(int itemID, float healthBarUp)
        {
            PlayerPrefs.SetFloat("URLCharacterItem" + itemID, healthBarUp);
        }
    }
}
