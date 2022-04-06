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
        public List<Sprite> GifSprites { get; private set; } = null;
        private float gifDelayTime = 0f;
        private int gifSpriteIndex = 0;
        public byte[] gifData = null;
        
        void Start()
        {
            SetGif(this.gifData);
        }
        void Update()
        {
            // update the texture every so often
            if (this.GifSprites != null && this.GifSprites.Count() > 0 && this.gifDelayTime <= Time.time)
            {
                this.gifSpriteIndex = (this.gifSpriteIndex + 1) % this.GifSprites.Count();
                if (this.gifSpriteIndex < 0) { this.gifSpriteIndex += this.GifSprites.Count(); }
                this.gifDelayTime = Time.time + this.GifTextures[this.gifSpriteIndex].m_delaySec;

                this.gameObject.GetComponent<SpriteRenderer>().sprite = this.GifSprites[this.gifSpriteIndex];
            }
        }
        IEnumerator LoadGif(byte[] bytes)
        {
            yield return UniGif.UniGif.GetTextureListCoroutine(bytes, (texList, loopCount, width, height) =>
            {
                if (texList != null)
                {
                    this.GifTextures = texList;
                    this.GifSprites = texList.Select(t => Sprite.Create(t.m_texture2d, new Rect(0, 0, t.m_texture2d.width, t.m_texture2d.height), new Vector2(0.5f, 0.5f))).ToList();
                    this.gifSpriteIndex = 0;
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
