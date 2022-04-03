using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnboundLib.Networking;
using UnboundLib;
using System.Linq;
using Photon.Pun;
using System;
using PlayerCustomization.Extensions;
using System.Collections.Generic;
using TMPro;
using Photon.Realtime;
namespace PlayerCustomization
{
    public class URLCharacterItem : MonoBehaviour
    {
        Texture2D texture; // the texture of the item
        Player player; // the player that owns this item

        // extra stuff for handling GIFs separately
        public bool IsGif { get; private set; } = false;
        public List<UniGif.GifTexture> GifTextures { get; private set; } = null;
        private float gifDelayTime = 0f;
        private int gifTextureIndex = 0;

        public string URL { get; private set; } // the url of the item
        CharacterItem Item => this.GetComponent<CharacterItem>();
        public int URLItemID { get; private set; } = -1; // the id of the item
        int SlotNr => (int)Item.GetFieldValue("slotNr");
        public CharacterItemType ItemType { get; private set; }
        public float Scale { get; private set; }
        public float MoveHealthBarUp { get; private set; }
        public bool IsCardChoiceFace { get; private set; } = false;
        public bool IsRWFFace => !String.IsNullOrEmpty(this.RWFFaceName);
        public string RWFFaceName { get; private set; } = null;
        
        public int ActorID { get; private set; } = -1;
        void ReadIdentification()
        {
            // read the itemType and the itemID from the gameobject's name
            string[] parts = this.gameObject.name.ToLower().Replace("(clone)", "").Split(new char[] { '_', ' ', '-' });
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i].Equals("url"))
                {
                    if (!Enum.TryParse(parts[i + 1].Substring(0, 1).ToUpper() + parts[i + 1].Substring(1), out CharacterItemType itemType) || !int.TryParse(parts[i + 2], out int ID))
                    {
                        UnityEngine.Debug.LogError($"URLCharacterItem::ReadIdentification: Unable to parse item name '{this.gameObject.name}'.");
                        return;
                    }
                    this.URLItemID = ID;
                    this.ItemType = itemType;
                }
            }
            if (this.URLItemID == -1)
            {
                UnityEngine.Debug.LogError($"URLCharacterItem::ReadIdentification: Unable to parse item name '{this.gameObject.name}'.");
            }
        }
        void ReadLocalSettings()
        {
            this.URL = URLCharacterItemManager.GetURL(this.URLItemID, this.ItemType);
            this.Scale = URLCharacterItemManager.GetScale(this.URLItemID, this.ItemType);
            this.MoveHealthBarUp = URLCharacterItemManager.GetHealthBarOffset(this.URLItemID, this.ItemType);
            this.Item.SetScale(this.Scale);
            this.Item.SetMoveHealthBarUp(this.MoveHealthBarUp);
        }

        void Start()
        {
            // try to read the identification
            this.ReadIdentification();

            // if this is a root gameobject, then it should be hidden since it is a prefab
            if (this.transform.root == this.transform)
            {
                this.transform.localScale = Vector3.zero;
                this.transform.position = 1000000f * Vector3.up;
                return;
            }

            this.player = this.transform?.root.GetComponent<Player>();

            // check if this is part of the CardChoiceVisuals, if so the player is set based on CardChoice.pickerID
            if (this.player is null && this.transform?.parent?.parent?.parent?.GetComponent<CardChoiceVisuals>() != null)
            {
                this.IsCardChoiceFace = true;
                this.player = PlayerManager.instance.players[CardChoice.instance.pickrID];
            }

            //if (this.transform?.root?.gameObject?.name == "CharacterCustom" || (this.player != null && this.player.data.view.IsMine))
            if (PhotonNetwork.OfflineMode || PhotonNetwork.CurrentRoom is null || (this.player != null && this.player.data.view.IsMine))
            {
                // if we're offline, then use local settings
                // if this is an editor object, we should use the local settings
                // if this is a local player, then we should use the local settings

                this.ReadLocalSettings();
                this.SetImage(this.URL);
            }
            else if (this.player != null && URLCharacterItemManager.GetRemoteURLObjectsEnabled())
            {
                // if there is a remote player, then this is a remote object and we should use the remote settings
                NetworkingManager.RPC(typeof(URLCharacterItem),
                    nameof(RPCS_RequestURLItemProperties),
                    new Photon.Realtime.RaiseEventOptions() { TargetActors = new int[] { this.player.data.view.OwnerActorNr } },
                    PhotonNetwork.LocalPlayer.ActorNumber,
                    this.URLItemID,
                    this.player.playerID,
                    this.GetInstanceID(),
                    (byte)this.ItemType,
                    this.IsCardChoiceFace);
            }
            // extra special case: RWF lobbies - only render local face, for performance and implementation reasons
            else if (this.player is null && this.transform?.parent?.parent?.parent?.parent?.parent?.parent?.Find("PlayerName")?.GetComponent<TextMeshProUGUI>() != null)
            {
                this.RWFFaceName = this.transform.parent.parent.parent.parent.parent.parent.Find("PlayerName").GetComponent<TextMeshProUGUI>().text;
                Photon.Realtime.Player owner = FindPlayerByNickname(this.RWFFaceName);
                this.ActorID = owner.ActorNumber;
                if (owner.IsLocal)
                {
                    // local object
                    this.ReadLocalSettings();
                    this.SetImage(this.URL);
                }
                else if (URLCharacterItemManager.GetRemoteURLObjectsEnabled())
                {
                    // remote object 
                    NetworkingManager.RPC(typeof(URLCharacterItem),
                        nameof(RPCS_RequestURLItemProperties_ByInstance),
                        new Photon.Realtime.RaiseEventOptions() { TargetActors = new int[] { this.ActorID } },
                        this.ActorID,
                        PhotonNetwork.LocalPlayer.ActorNumber,
                        this.GetInstanceID(),
                        this.URLItemID,
                        (byte)this.ItemType);
                }
                else
                {
                    // remote objects are disabled
                    GameObject.Destroy(this.gameObject);
                }

            }
            else
            {
                // either this is an online lobby object, or remote objects are disabled, so destroy this object
                GameObject.Destroy(this.gameObject);
            }
        }
        void Update()
        {
            // if the item is a GIF, then we need to update the texture every so often
            if (this.IsGif && this.GifTextures != null && this.GifTextures.Count() > 0 && this.gifDelayTime <= Time.time)
            {
                this.gifTextureIndex = (this.gifTextureIndex + 1) % this.GifTextures.Count();
                if (this.gifTextureIndex < 0) { this.gifTextureIndex += this.GifTextures.Count(); }
                this.gifDelayTime = Time.time + this.GifTextures[this.gifTextureIndex].m_delaySec;
                
                this.gameObject.GetComponent<SpriteRenderer>().sprite = Sprite.Create(this.GifTextures[this.gifTextureIndex].m_texture2d, new Rect(0, 0, this.GifTextures[this.gifTextureIndex].m_texture2d.width, this.GifTextures[this.gifTextureIndex].m_texture2d.height), new Vector2(0.5f, 0.5f));
            }
        }
        IEnumerator DownloadAndSetImage(string url)
        {
            if (url.EndsWith(".gif", true, System.Globalization.CultureInfo.InvariantCulture))
            {
                // if the url ends with .gif, then we should use the gif decoder
                using (UnityWebRequest www = UnityWebRequest.Get(url))
                {
                    yield return www.SendWebRequest();
                    if (www.isNetworkError || www.isHttpError)
                    {
                        UnityEngine.Debug.LogError($"URLCharacterItem: {www.error} | URL: {url} | (USING GIF DECODER)");
                    }
                    else
                    {
                        yield return UniGif.GetTextureListCoroutine(www.downloadHandler.data, (texList, loopCount, width, height) =>
                        {
                            if (texList != null)
                            {
                                this.GifTextures = texList;
                                this.IsGif = true;
                                this.gifTextureIndex = 0;
                                this.gifDelayTime = 0f;
                            }
                            else
                            {
                                UnityEngine.Debug.LogError($"URLCharacterItem: Failed to decode gif | URL: {url}");
                            }
                        }, FilterMode.Bilinear, TextureWrapMode.Clamp, true);
                    }
                }
            }
            else
            {
                this.IsGif = false;
                using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(url))
                {
                    yield return www.SendWebRequest();
                    if (www.isNetworkError || www.isHttpError)
                    {
                        UnityEngine.Debug.LogError($"URLCharacterItem: {www.error} | URL: {url}");
                    }
                    else
                    {
                        this.texture = DownloadHandlerTexture.GetContent(www);
                    }
                }

                if (this.texture is null) { yield break; }
                this.gameObject.GetComponent<SpriteRenderer>().sprite = Sprite.Create(this.texture, new Rect(0, 0, this.texture.width, this.texture.height), new Vector2(0.5f, 0.5f));
            }
        }

        internal void SetImage(string url)
        {
            StartCoroutine(DownloadAndSetImage(url));
        }
        /// <summary>
        /// Requests the image url from the owner of this item
        /// </summary>
        /// <param name="itemID"></param>
        /// <param name="playerID"></param>
        /// <param name="instanceID"></param>
        /// <param name="itemType_as_byte"></param>
        [UnboundRPC]
        private static void RPCS_RequestURLItemProperties(int requestingActor, int itemID, int playerID, int instanceID, byte itemType_as_byte, bool isCardChoiceFace)
        {
            Player player = PlayerManager.instance.players.FirstOrDefault(p => p.playerID == playerID);
            if (player is null || !player.data.view.IsMine)
            {
                // if the player is not local, then we should not do anything
                return;
            }
            // respond to the request
            NetworkingManager.RPC(typeof(URLCharacterItem), nameof(RPCA_SetURLItemProperties),
                new Photon.Realtime.RaiseEventOptions() { TargetActors = new int[] { requestingActor } },
                URLCharacterItemManager.GetURL(itemID, (CharacterItemType)itemType_as_byte),
                playerID,
                instanceID,
                URLCharacterItemManager.GetScale(itemID, (CharacterItemType)itemType_as_byte),
                URLCharacterItemManager.GetHealthBarOffset(itemID, (CharacterItemType)itemType_as_byte),
                isCardChoiceFace);

        }
        /// <summary>
        /// Requests the image url from the owner of this item, when the item isn't on a player or a card choice face, i.e. in lobby
        /// </summary>
        [UnboundRPC]
        private static void RPCS_RequestURLItemProperties_ByInstance(int ownerID, int requestingActor, int instanceID, int itemID, byte itemType_as_byte)
        {
            if (PhotonNetwork.LocalPlayer.ActorNumber != ownerID)
            {
                // if the owner is not local, then we should not do anything
                return;
            }
            // respond to the request
            NetworkingManager.RPC(typeof(URLCharacterItem),
                nameof(RPCA_SetURLItemProperties_ByInstance),
                new Photon.Realtime.RaiseEventOptions() { TargetActors = new int[] { requestingActor } },
                URLCharacterItemManager.GetURL(itemID,
                (CharacterItemType)itemType_as_byte),
                instanceID,
                URLCharacterItemManager.GetScale(itemID,
                (CharacterItemType)itemType_as_byte),
                URLCharacterItemManager.GetHealthBarOffset(itemID,
                (CharacterItemType)itemType_as_byte));
        }

        /// <summary>
        /// Sets the url of the item, the RPC is sent by the owner
        /// </summary>
        /// <param name="url">The url of the image to set</param>
        /// <param name="playerID">The playerID of the player using this item, which is also the owner</param>
        /// <param name="instanceID">The instanceID of the object</param>
        /// <param name="scale">The scale of the item</param>
        /// <param name="moveHealthBarUp">How far to move the health bar up if this item is a Detail</param>
        [UnboundRPC]
        private static void RPCA_SetURLItemProperties(string url, int playerID, int instanceID, float scale, float moveHealthBarUp, bool isCardChoiceFace)
        {
            URLCharacterItem item = FindMatchingURLItem(playerID, instanceID, isCardChoiceFace);
            if (item is null)
            {
                // there is no item
                return;
            }
            item.URL = url;
            item.SetImage(item.URL);
            item.Item.SetScale(scale);
            item.Item.SetMoveHealthBarUp(moveHealthBarUp);
        }
        /// <summary>
        /// Sets the url of an item BY INSTANCE, the RPC is sent by the owner
        /// </summary>
        [UnboundRPC]
        private static void RPCA_SetURLItemProperties_ByInstance(string url, int instanceID, float scale, float moveHealthBarUp)
        {
            URLCharacterItem item = FindMatchingURLItem_ByInstance(instanceID);
            if (item is null)
            {
                // there is no item
                return;
            }
            item.URL = url;
            item.SetImage(item.URL);
            item.Item.SetScale(scale);
            item.Item.SetMoveHealthBarUp(moveHealthBarUp);
        }
        private static URLCharacterItem FindMatchingURLItem(int playerID, int instanceID, bool isCardChoiceFace)
        {
            // find the matching item
            return (isCardChoiceFace ? CardChoiceVisuals.instance?.gameObject : PlayerManager.instance.players.FirstOrDefault(p => p.playerID == playerID)?.gameObject)?.GetComponentsInChildren<URLCharacterItem>(true).FirstOrDefault(d => d.GetInstanceID() == instanceID);
        }
        private static URLCharacterItem FindMatchingURLItem_ByInstance(int instanceID)
        {
            // find the matching item
            return GameObject.FindObjectsOfType<URLCharacterItem>().FirstOrDefault(i => i.GetInstanceID() == instanceID);
        }
        private static Photon.Realtime.Player FindPlayerByNickname(string nickname)
        {
            // find the first player of a given client by the client's nickname
            // the nickname is not unique, so this isn't a perfect solution
            // the nickname may also have an extra number at the end
            // so we first check if there are any perfect matches, then if not, we check for matches up to the last character, then up to the second to last character, and so on
            string nickNameToCheck = nickname;
            while (nickNameToCheck.Length > 0)
            {
                Photon.Realtime.Player player = PhotonNetwork.CurrentRoom.Players.Select(kv => kv.Value).FirstOrDefault(p => p.NickName == nickNameToCheck);
                if (player != null)
                {
                    return player;
                }
                nickNameToCheck = nickname.Substring(0, nickNameToCheck.Length - 1);
            }
            return null;
            // almost entirely generated by Copilot.
        }
    }
}
