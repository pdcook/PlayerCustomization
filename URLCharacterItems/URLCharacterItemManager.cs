using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnboundLib;
using TMPro;
using UnityEngine.UI;
using UnboundLib.Utils.UI;
using PlayerCustomizationUtils;
namespace URLCharacterItems
{
    public static class URLCharacterItemManager
    {
        public const int ITEMS_PER_TYPE = 6;

        public const string defaultURL = "https://upload.wikimedia.org/wikipedia/commons/5/50/Smile_Image.png";

        private static Dictionary<CharacterItemType, List<URLCharacterItem>> items = new Dictionary<CharacterItemType, List<URLCharacterItem>>() { };

        private static Dictionary<CharacterItemType, List<List<TextMeshProUGUI>>> itemButtons = new Dictionary<CharacterItemType, List<List<TextMeshProUGUI>>>() { };
        private static bool inited = false;

        private static URLCharacterItem currentPreviewItem = null;

        public static void Init(bool force = false)
        {
            if (inited && !force) { return; }
            inited = true;

            items[CharacterItemType.Eyes] = Enumerable.Repeat<URLCharacterItem>(null, ITEMS_PER_TYPE).ToList();
            items[CharacterItemType.Mouth] = Enumerable.Repeat<URLCharacterItem>(null, ITEMS_PER_TYPE).ToList();
            items[CharacterItemType.Detail] = Enumerable.Repeat<URLCharacterItem>(null, ITEMS_PER_TYPE).ToList();

            for (int i = 0; i < ITEMS_PER_TYPE; i++)
            {
                CreateURLCharacterItemPrefab(i, CharacterItemType.Eyes, GetURL(i, CharacterItemType.Eyes));
                CreateURLCharacterItemPrefab(i, CharacterItemType.Mouth, GetURL(i, CharacterItemType.Mouth));
                CreateURLCharacterItemPrefab(i, CharacterItemType.Detail, GetURL(i, CharacterItemType.Detail));
            }

        }
        public static string ItemGameObjectName(int ID, CharacterItemType type)
        {
            return $"URL_{type}_{ID}".ToUpper();
        }

        public static void CreateURLCharacterItemPrefab(int ID, CharacterItemType itemType, string url = defaultURL)
        {
            if (ID < 0 || ID >= ITEMS_PER_TYPE)
            {
                UnityEngine.Debug.LogError("URLCharacterItemManager::CreateURLCharacterItemPrefab: ID out of range.");
                return;
            }

            // construct an empty list if it doesn't exist already
            if (items[itemType] is null) { items[itemType] = Enumerable.Repeat<URLCharacterItem>(null, ITEMS_PER_TYPE).ToList(); }

            if (items.ContainsKey(itemType) && items[itemType][ID] != null)
            {
                UnityEngine.Debug.LogError($"URLCharacterItemManager: CreateURLCharacterItemPrefab: {itemType} item with ID {ID} already exists.");
                return;
            }
            GameObject prefab = new GameObject(ItemGameObjectName(ID, itemType));
            UnityEngine.GameObject.DontDestroyOnLoad(prefab);
            URLCharacterItem item = prefab.GetOrAddComponent<URLCharacterItem>();
            prefab.GetOrAddComponent<SpriteRenderer>();
            items[itemType][ID] = item;
        }
        public static void AddItemsToCharacterCreator()
        {
            for (int i = 0; i < ITEMS_PER_TYPE; i++)
            {
                CustomCharacterItemManager.AddCustomCharacterItem(GetURLCharacterItemPrefab(i, CharacterItemType.Eyes).gameObject, CharacterItemType.Eyes);
                CustomCharacterItemManager.AddCustomCharacterItem(GetURLCharacterItemPrefab(i, CharacterItemType.Mouth).gameObject, CharacterItemType.Mouth);
                CustomCharacterItemManager.AddCustomCharacterItem(GetURLCharacterItemPrefab(i, CharacterItemType.Detail).gameObject, CharacterItemType.Detail);
            }
        }

        public static URLCharacterItem GetURLCharacterItemPrefab(int ID, CharacterItemType itemType)
        {
            if (ID < 0 || ID >= ITEMS_PER_TYPE)
            {
                UnityEngine.Debug.LogError("URLCharacterItemManager::GetURLCharacterItemPrefab: ID out of range.");
                return null;
            }
            else if (items[itemType][ID] is null)
            {
                UnityEngine.Debug.LogError($"URLCharacterItemManager::GetURLCharacterItemPrefab: {itemType} item with ID {ID} does not exist.");
                return null;
            }
            return items[itemType][ID];
        }
        internal static string GetConfigKey(string key) => $"{URLCharacterItems.CompatibilityModName}_URLItem_{key}";
        public static bool GetRemoteURLObjectsEnabled()
        {
            return PlayerPrefs.GetInt(GetConfigKey("RemoteURLObjectsEnabled"), 1) == 1;
        }
        public static void SetRemoteURLObjectsEnabled(bool enabled)
        {
            PlayerPrefs.SetInt(GetConfigKey("RemoteURLObjectsEnabled"), enabled ? 1 : 0);
        }
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

        internal static string GetDefaultItemName(int id, CharacterItemType itemType)
        {
            return $"{itemType} {id + 1}".ToUpper();
        }
        internal static string GetItemName(int id, CharacterItemType itemType)
        {
            string name = PlayerPrefs.GetString(GetConfigKey($"{itemType}{id}Name"), GetDefaultItemName(id, itemType));
            if (name.Length <= 0 || name.Length > 32)
            {
                SetItemName(id, itemType, GetDefaultItemName(id, itemType));
                name = PlayerPrefs.GetString(GetConfigKey($"{itemType}{id}Name"), GetDefaultItemName(id, itemType));
            }
            return name;
        }
        internal static void SetItemName(int id, CharacterItemType itemType, string name)
        {
            PlayerPrefs.SetString(GetConfigKey($"{itemType}{id}Name"), name);
        }

        internal static void URLItemsMenu(GameObject menu)
        {
            MenuHandler.CreateText("CUSTOM PLAYER OBJECTS", menu, out var _, 60);
            MenuHandler.CreateText(" ", menu, out var _, 30);
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
            MenuHandler.CreateText(" ", menu, out var _, 30);
            // add toggle for enabling/disabling remote URL objects
            MenuHandler.CreateToggle(GetRemoteURLObjectsEnabled(),"Show Online Players' Custom Items", menu, (bool enabled) =>
            {
                SetRemoteURLObjectsEnabled(enabled);
            }, 60, alignmentOptions: TextAlignmentOptions.Center);
        }
        private static void ItemsMenu(GameObject menu, CharacterItemType itemType)
        {
            for (int i = 0; i < ITEMS_PER_TYPE; i++)
            {
                // must create a local copy by value of the index
                int j = i;
                string name = $"{GetItemName(j, itemType)}";
                GameObject editItemObj = MenuHandler.CreateMenu(name, () => { SetupItemPreview(j, itemType); }, menu, 60, true, true, menu.transform.parent.gameObject);
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
        private static GameObject _PlayerPreview = null;
        private static GameObject PlayerPreview
        {
            get
            {
                if (_PlayerPreview is null)
                {
                    _PlayerPreview = GameObject.Instantiate(CharacterCreatorHandler.instance.transform.GetChild(0).GetChild(1).GetChild(0).gameObject);
                    _PlayerPreview.transform.Find("PutItemsUnderHere").gameObject.GetComponent<CharacterCreatorDragging>().enabled = false;
                    _PlayerPreview.transform.position = new Vector3(0f, 2.25f, 0f);
                    GameObject.DontDestroyOnLoad(_PlayerPreview);
                }
                return _PlayerPreview;
            }
        }
        private static void SetupItemPreview(int ID, CharacterItemType itemType)
        {
            PlayerPreview.SetActive(true);
            DestroyAllChildren(PlayerPreview.transform.Find("PutItemsUnderHere"));

            currentPreviewItem = GameObject.Instantiate(GetURLCharacterItemPrefab(ID, itemType).gameObject, PlayerPreview.transform.Find("PutItemsUnderHere")).GetComponent<URLCharacterItem>();
            currentPreviewItem.transform.localScale = Vector3.one * GetScale(ID, itemType);
            currentPreviewItem.transform.localPosition = Vector3.zero;
        }

        private static void EditItemMenu(GameObject menu, CharacterItemType itemTypeToEdit, int IDToEdit)
        {
            // copy by value certain parameters
            int ID_ = IDToEdit;
            CharacterItemType itemType_ = itemTypeToEdit;

            void ChangeItemName(string newName, int ID, CharacterItemType itemType)
            {
                if (newName.Length > 0 && newName.Length <= 32)
                {
                    UpdateItemNameObjects(ID, itemType, newName);
                    SetItemName(ID, itemType, newName);
                }
            }
            itemButtons[itemType_][ID_].Add(MenuHandler.CreateText(GetItemName(ID_, itemType_), menu, out TextMeshProUGUI _, 60).GetComponentInChildren<TextMeshProUGUI>());

            // create space for player preview
            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 60);
            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 60);
            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 60);

            MenuHandler.CreateInputField(GetItemName(ID_, itemType_), 60, menu, v=>ChangeItemName(v, ID_, itemType_));
            void ChangeURL(string newURL, int ID, CharacterItemType itemType)
            {
                // check that the new URL starts with 'http'
                if (!newURL.StartsWith("http", true, System.Globalization.CultureInfo.InvariantCulture))
                {
                    return;
                }
                // check if the new URL ends with '.png', '.jpg', '.jpeg', or '.gif' (special case)
                if (  !newURL.EndsWith(".png", true, System.Globalization.CultureInfo.InvariantCulture)
                    & !newURL.EndsWith(".jpg", true, System.Globalization.CultureInfo.InvariantCulture)
                    & !newURL.EndsWith(".jpeg", true, System.Globalization.CultureInfo.InvariantCulture)
                    & !newURL.EndsWith(".gif", true, System.Globalization.CultureInfo.InvariantCulture)
                    )
                {
                    return;
                }

                SetURL(ID, itemType, newURL);
                if (currentPreviewItem != null)
                {
                    currentPreviewItem.SetImage(newURL);
                }
            }
            MenuHandler.CreateInputField(GetURL(ID_, itemType_), 60, menu, v => ChangeURL(v, ID_, itemType_));
            void ChangeScale(float newScale, int ID, CharacterItemType itemType)
            {
                SetScale(ID, itemType, newScale);
                if (currentPreviewItem != null) { currentPreviewItem.transform.localScale = Vector3.one * newScale; }
            }
            MenuHandler.CreateSlider("Scale", menu, 45, 0f, 10f, GetScale(ID_, itemType_), v => ChangeScale(v, ID_, itemType_), out Slider _, false);
            void ChangeHealthBarOffset(float newOffset, int ID, CharacterItemType itemType)
            {
                SetHealthBarOffset(ID, itemType, newOffset);
            }
            MenuHandler.CreateSlider("Health Bar Offset", menu, 45, 0f, 2f, GetHealthBarOffset(ID_, itemType_), v => ChangeHealthBarOffset(v, ID_, itemType_), out Slider _, false);

            // add action to back button to destroy the item and hide the preview object
            menu.transform.Find("Group/Back").gameObject.GetComponent<Button>().onClick.AddListener(() =>
            {
                PlayerPreview.SetActive(false);
                DestroyAllChildren(PlayerPreview.transform.Find("PutItemsUnderHere"));
            });
        }
        private static void DestroyAllChildren(Transform transform)
        {
            while (transform.childCount > 0)
            {
                GameObject.DestroyImmediate(transform.GetChild(0).gameObject);
            }
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
