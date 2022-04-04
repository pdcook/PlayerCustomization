using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnboundLib.Networking;
using UnboundLib;
using System.Linq;
using Photon.Pun;
using System;
using PlayerCustomizationUtils.Extensions;
using System.Collections.Generic;
using TMPro;
using Photon.Realtime;
using UniGif;
namespace PlayerCustomizationUtils
{
    public class GifCharacterItem : MonoBehaviour
    {
        public List<UniGif.UniGif.GifTexture> GifTextures { get; private set; } = null;
        private float gifDelayTime = 0f;
        private int gifTextureIndex = 0;
        public byte[] gifData = null;

        void Start()
        {
            SetGif(this.gifData);
        }
        void Update()
        {
            // update the texture every so often
            if (this.GifTextures != null && this.GifTextures.Count() > 0 && this.gifDelayTime <= Time.time)
            {
                this.gifTextureIndex = (this.gifTextureIndex + 1) % this.GifTextures.Count();
                if (this.gifTextureIndex < 0) { this.gifTextureIndex += this.GifTextures.Count(); }
                this.gifDelayTime = Time.time + this.GifTextures[this.gifTextureIndex].m_delaySec;

                this.gameObject.GetComponent<SpriteRenderer>().sprite = Sprite.Create(this.GifTextures[this.gifTextureIndex].m_texture2d, new Rect(0, 0, this.GifTextures[this.gifTextureIndex].m_texture2d.width, this.GifTextures[this.gifTextureIndex].m_texture2d.height), new Vector2(0.5f, 0.5f));
            }
        }
        IEnumerator LoadGif(byte[] bytes)
        {
            yield return UniGif.UniGif.GetTextureListCoroutine(bytes, (texList, loopCount, width, height) =>
            {
                if (texList != null)
                {
                    this.GifTextures = texList;
                    this.gifTextureIndex = 0;
                    this.gifDelayTime = 0f;
                }
                else
                {
                    UnityEngine.Debug.LogError($"GifCharacterItem: Failed to decode gif: {this.gameObject.name}");
                }
            }, FilterMode.Bilinear, TextureWrapMode.Clamp, true);
        }

        internal void SetGif(byte[] bytes)
        {
            StartCoroutine(LoadGif(bytes));
        }
    }
}
