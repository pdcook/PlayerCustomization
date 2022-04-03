using UnityEngine;
using System.Linq;
using UnboundLib;
using System.Collections.Generic;
using System;
namespace PlayerCustomizationUtils
{
    public static class CustomCharacterItemManager
    {
        private static Dictionary<CharacterItemType, List<CharacterItem>> CustomItems = new Dictionary<CharacterItemType, List<CharacterItem>>();
        public static void AddCustomCharacterItem(GameObject item, CharacterItemType itemType, float scale = 1f, float moveHealthBarUp = 0f)
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
