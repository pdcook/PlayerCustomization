using UnityEngine;
using System.Linq;
using UnboundLib;
using System.Collections.Generic;
using System;
using System.IO;
namespace PlayerCustomizationUtils
{
    public static class CustomCharacterItemManager
    {
        private static Dictionary<CharacterItemType, List<CharacterItem>> CustomItems = new Dictionary<CharacterItemType, List<CharacterItem>>();
        public static void AddCustomCharacterItem(string path, CharacterItemType itemType, float scale = 1f, float moveHealthBarUp = 0f, float brightness = 1f, string itemName = null)
        {
            byte[] bytes = File.ReadAllBytes(path);

            if (path.EndsWith(".png", true, System.Globalization.CultureInfo.InvariantCulture))
            {
                Texture2D tex = new Texture2D(2, 2);
                tex.LoadImage(bytes);
                Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                AddCustomCharacterItem(sprite, itemType, scale, moveHealthBarUp, brightness, itemName ?? Path.GetFileNameWithoutExtension(path));
            }
            else if (path.EndsWith(".gif", true, System.Globalization.CultureInfo.InvariantCulture))
            {
                AddCustomCharacterItem(bytes, itemType, scale, moveHealthBarUp, brightness, itemName ?? Path.GetFileNameWithoutExtension(path));
            }
            else
            {
                // filetype not supported
                UnityEngine.Debug.LogError("CustomCharacterItemManager: Filetype not supported: " + Path.GetFileName(path));
            }
        }
        public static void AddCustomCharacterItem(byte[] gifData, CharacterItemType itemType, float scale = 1f, float moveHealthBarUp = 0f, float brightness = 1f, string itemName = null)
        {
            GameObject newItem = new GameObject(itemName ?? Guid.NewGuid().ToString(), typeof(SpriteRenderer), typeof(CharacterItem), typeof(GifCharacterItem));
            newItem.GetComponent<GifCharacterItem>().gifData = gifData;
            AddCustomCharacterItem(newItem, itemType, scale, moveHealthBarUp, brightness);
        }
        public static void AddCustomCharacterItem(Sprite sprite, CharacterItemType itemType, float scale = 1f, float moveHealthBarUp = 0f, float brightness = 1f, string itemName = null)
        {
            GameObject newItem = new GameObject(itemName ?? Guid.NewGuid().ToString(), typeof(SpriteRenderer), typeof(CharacterItem));
            newItem.GetComponent<SpriteRenderer>().sprite = sprite;
            AddCustomCharacterItem(newItem, itemType, scale, moveHealthBarUp, brightness);
        }
        public static void AddCustomCharacterItem(GameObject item, CharacterItemType itemType, float scale = 1f, float moveHealthBarUp = 0f, float brightness = 1f)
        {
            item.name = $"(CUSTOM) {item.name}";
            item.layer = LayerMask.NameToLayer("Player");
            foreach (SpriteRenderer sprite in item.GetComponentsInChildren<SpriteRenderer>(true))
            {
                sprite.sortingLayerID = SortingLayer.NameToID("MostFront");
                switch (itemType)
                {
                    case CharacterItemType.Eyes:
                        sprite.sortingOrder = 3;
                        break;
                    case CharacterItemType.Mouth:
                        sprite.sortingOrder = 4;
                        break;
                    case CharacterItemType.Detail:
                        sprite.sortingOrder = 5;
                        break;
                }
            }
            CharacterItem characterItem = item.GetOrAddComponent<CharacterItem>();
            characterItem.itemType = itemType;
            characterItem.scale = scale;
            characterItem.moveHealthBarUp = moveHealthBarUp;

            // adjust brightness
            if (brightness != 1f)
            {
                foreach (SpriteRenderer sprite in item.GetComponentsInChildren<SpriteRenderer>(true))
                {
                    Color.RGBToHSV(sprite.color, out float h, out float s, out float v);
                    sprite.color = Color.HSVToRGB(h, s, Mathf.Clamp01(v * brightness));
                }
            }

            // save the item in DontDestroyOnLoad, and hide it by setting it's scale to 0 and position to 100000
            GameObject.DontDestroyOnLoad(item);
            item.transform.localScale = Vector3.zero;
            item.transform.position = new Vector3(0f, 100000f, 0f);

            if (!CustomItems.ContainsKey(itemType))
            {
                CustomItems.Add(itemType, new List<CharacterItem>() { });
            }
            CustomItems[itemType].Add(characterItem);
        }
        public static void UpdateCharacterCreator()
        {
            foreach (CharacterItemType itemType in Enum.GetValues(typeof(CharacterItemType)))
            {
                if (!CustomItems.ContainsKey(itemType))
                {
                    CustomItems.Add(itemType, new List<CharacterItem>() { });
                }
                CharacterItem[] array;
                switch (itemType)
                {
                    case CharacterItemType.Eyes:
                        array = CharacterCreatorItemLoader.instance.eyes;
                        break;
                    case CharacterItemType.Mouth:
                        array = CharacterCreatorItemLoader.instance.mouths;
                        break;
                    case CharacterItemType.Detail:
                        array = CharacterCreatorItemLoader.instance.accessories;
                        break;
                    default:
                        continue;
                }
                CharacterCreatorItemLoader.instance.UpdateItems(itemType, array.Concat(CustomItems[itemType]).Distinct().ToArray());
            }
        }
    }
}
