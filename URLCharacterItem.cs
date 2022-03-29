using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnboundLib.Networking;
using UnboundLib;
using System.Linq;
using Photon.Pun;
namespace PlayerCustomization
{
    public class URLCharacterItem : MonoBehaviour
    {
        Texture2D texture; // the texture of the item
        Player player; // the player that owns this item

        public string URL { get; internal set; } // the url of the item
        CharacterItem Item => this.GetComponent<CharacterItem>();
        int SlotNr => (int)Item.GetFieldValue("slotNr");
        CharacterItemType ItemType => Item.itemType;
        float Scale => Item.scale;
        float MoveHealthBarUp => Item.moveHealthBarUp;

        void Start()
        {
            // if this is a root gameobject, then it should be hidden since it is a prefab
            if (this.transform.root == this.transform)
            {
                this.transform.localScale = Vector3.zero;
                this.transform.position = 1000000f * Vector3.up;
            }

            this.player = this.transform?.root.GetComponent<Player>();
            
            if (this.player is null || this.player.data.view.IsMine)
            {
                // if there is no player, then this is an editor object and we should use the local settings
                // if this is a local player, then we should use the local settings (though this should already be set)

                this.URL = URLCharacterItemManager.GetURLCharacterItemPrefab(this.gameObject.name).URL;
                this.SetImage(this.URL);
            }
            else
            {
                // if there is a player, then this is a remote object and we should use the remote settings
                NetworkingManager.RPC(typeof(URLCharacterItem), nameof(RPCS_RequestURL), new Photon.Realtime.RaiseEventOptions() { TargetActors = new int[] { this.player.data.view.OwnerActorNr } }, PhotonNetwork.LocalPlayer.ActorNumber, this.gameObject.name, this.player.playerID, this.SlotNr, (byte)this.ItemType);
            }
        }
        IEnumerator DownloadAndSetImage(string url)
        {
            using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(url))
            {
                yield return www.SendWebRequest();
                if (www.isNetworkError || www.isHttpError)
                {
                    UnityEngine.Debug.LogError(www.error);
                }
                else
                {
                    this.texture = DownloadHandlerTexture.GetContent(www);
                }
            }
            
            if (this.texture is null) { yield break; }
            this.gameObject.GetComponent<SpriteRenderer>().sprite = Sprite.Create(this.texture, new Rect(0, 0, this.texture.width, this.texture.height), new Vector2(0.5f, 0.5f));
        }

        private void SetImage(string url)
        {
            StartCoroutine(DownloadAndSetImage(url));
        }
        /// <summary>
        /// Requests the image url from the owner of this item
        /// </summary>
        /// <param name="name"></param>
        /// <param name="playerID"></param>
        /// <param name="slotNr"></param>
        /// <param name="itemType_as_byte"></param>
        [UnboundRPC]
        private static void RPCS_RequestURL(int requestingActor, string name, int playerID, int slotNr, byte itemType_as_byte)
        {
            Player player = PlayerManager.instance.players.FirstOrDefault(p => p.playerID == playerID);
            if (player is null || !player.data.view.IsMine)
            {
                // if the player is not local, then we should not do anything
                return;
            }
            URLCharacterItem prefab = URLCharacterItemManager.GetURLCharacterItemPrefab(name);
            // respond to the request
            NetworkingManager.RPC(typeof(URLCharacterItem), nameof(RPCA_SetURL), new Photon.Realtime.RaiseEventOptions() { TargetActors = new int[] { requestingActor } }, prefab.URL, name, playerID, slotNr, itemType_as_byte, prefab.Scale, prefab.MoveHealthBarUp);
        }

        /// <summary>
        /// Sets the url of the item, the RPC is sent by the owner
        /// </summary>
        /// <param name="url">The url of the image to set</param>
        /// <param name="name">The name of the gameobject</param>
        /// <param name="playerID">The playerID of the player using this item, which is also the owner</param>
        /// <param name="slotNr">The slot that the item is in, used to distinguish between the two details</param>
        /// <param name="itemType_as_byte">The item type, cast to a byte so it can be sent via RPC</param>
        /// <param name="scale">The scale of the item</param>
        /// <param name="moveHealthBarUp">How far to move the health bar up if this item is a Detail</param>
        [UnboundRPC]
        private static void RPCA_SetURL(string url, string name, int playerID, int slotNr, byte itemType_as_byte, float scale, float moveHealthBarUp)
        {
            CharacterItemType itemType = (CharacterItemType)itemType_as_byte;
            URLCharacterItem item = FindMatchingDynamicItem(name, playerID, slotNr, itemType);
            if (item is null)
            {
                // there is no item
                return;
            }
            item.URL = url;
            item.SetImage(item.URL);
        }
        private static URLCharacterItem FindMatchingDynamicItem(string name, int playerID, int slotNr, CharacterItemType itemType)
        {
            // find the matching item
            Player player = PlayerManager.instance.players.FirstOrDefault(p => p.playerID == playerID);
            if (player is null) { return null; }

            return player.GetComponentsInChildren<URLCharacterItem>(true).FirstOrDefault(d => d.name == name && d.SlotNr == slotNr && d.ItemType == itemType);
        }
    }
}
