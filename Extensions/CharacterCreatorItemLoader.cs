using System.Linq;
using UnboundLib;

namespace PlayerCustomizationUtils.Extensions
{
    public static class CharacterCreatorItemLoaderExtensions
    {
        public static int GetItemIDByName(this CharacterCreatorItemLoader instance, string name, CharacterItemType type)
        {
            CharacterItem item;
            switch (type)
            {
                case CharacterItemType.Eyes:
                    item = instance.eyes.FirstOrDefault(e => e.name.Equals(name));
                    if (item is null) { return -1; }
                    else { return System.Array.IndexOf(instance.eyes, item); }
                case CharacterItemType.Mouth:
                    item = instance.mouths.FirstOrDefault(m => m.name.Equals(name));
                    if (item is null) { return -1; }
                    else { return System.Array.IndexOf(instance.mouths, item); }
                case CharacterItemType.Detail:
                    item = instance.accessories.FirstOrDefault(a => a.name.Equals(name));
                    if (item is null) { return -1; }
                    else { return System.Array.IndexOf(instance.accessories, item); }
                default:
                    return -1;
            }
        }
        public static int GetRandomItemID(this CharacterCreatorItemLoader instance, CharacterItemType type, string[] bannedItemNames = null, bool allowCustomItems = true)
        {
            CharacterItem[] items;

            switch (type)
            {
                case CharacterItemType.Eyes:
                    items = instance.eyes;
                    break;
                case CharacterItemType.Mouth:
                    items = instance.mouths;
                    break;
                case CharacterItemType.Detail:
                    items = instance.accessories;
                    break;
                default:
                    return -1;
            }

            bannedItemNames = bannedItemNames ?? new string[0];

            if (bannedItemNames.Length == 0 && allowCustomItems)
            {
                return UnityEngine.Random.Range(0, items.Count());
            }
            else if (allowCustomItems)
            {
                CharacterItem[] validItems = items.Where(i => !bannedItemNames.Contains(i.name)).ToArray();
                if (validItems.Count() == 0) { return UnityEngine.Random.Range(0, items.Count()); }
                return (int)instance.InvokeMethod("GetItemID", validItems.GetRandom<CharacterItem>(), type);
            }
            else
            {
                CharacterItem[] validItems = items.Where(i => !bannedItemNames.Contains(i.name) && !i.name.Contains("CUSTOM")).ToArray();
                if (validItems.Count() == 0) { return UnityEngine.Random.Range(0, items.Count()); }
                return (int)instance.InvokeMethod("GetItemID", validItems.GetRandom<CharacterItem>(), type);
            }
        }
    }
}
