using UnityEngine;
using System.Linq;
namespace PlayerCustomization
{
    public static class CustomCharacterItemManager
    {
        public static void AddCustomCharacterItem(GameObject item, CharacterItemType itemType, float scale = 1f, float moveHealthBarUp = 0f)
        {
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
            CharacterItem characterItem = item.AddComponent<CharacterItem>();
            characterItem.itemType = itemType;
            characterItem.scale = scale;
            characterItem.moveHealthBarUp = moveHealthBarUp;
            switch (itemType)
            {
                case CharacterItemType.Eyes:
                    CharacterCreatorItemLoader.instance.UpdateItems(itemType, CharacterCreatorItemLoader.instance.eyes.Concat(new CharacterItem[] { characterItem }).ToArray());
                    break;
                case CharacterItemType.Mouth:
                    CharacterCreatorItemLoader.instance.UpdateItems(itemType, CharacterCreatorItemLoader.instance.mouths.Concat(new CharacterItem[] { characterItem }).ToArray());
                    break;
                case CharacterItemType.Detail:
                    CharacterCreatorItemLoader.instance.UpdateItems(itemType, CharacterCreatorItemLoader.instance.accessories.Concat(new CharacterItem[] { characterItem }).ToArray());
                    break;
            }
        }
    }
}
