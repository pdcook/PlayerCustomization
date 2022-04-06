using UnityEngine;
using UnboundLib;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
namespace PlayerCustomizationUtils
{
    public class GifBodyCharacterItem : MonoBehaviour
    {
        public SFPolygon SFPolygon => this.Body?.GetComponent<SFPolygon>();
        public GameObject Body { get; private set; } = null;
        public Sprite OriginalBody { get; private set; } = null;
        public List<UniGif.UniGif.GifTexture> GifTextures { get; private set; } = null;
        public List<Sprite> GifSprites { get; private set; } = null;
        private float gifDelayTime = 0f;
        private int gifSpriteIndex = 0;
        public byte[] gifData = null;
        void Start()
        {
            this.GetOriginal();
            if (this.OriginalBody != null) { this.SetGif(this.gifData); }
        }

        void Update()
        {
            // update the texture every so often
            if (this.GifSprites != null && this.GifSprites.Count() > 0 && this.gifDelayTime <= Time.time)
            {
                this.gifSpriteIndex = (this.gifSpriteIndex + 1) % this.GifSprites.Count();
                if (this.gifSpriteIndex < 0) { this.gifSpriteIndex += this.GifSprites.Count(); }
                this.gifDelayTime = Time.time + this.GifTextures[this.gifSpriteIndex].m_delaySec;

                this.ApplyBodySprite(this.gifSpriteIndex, false);
            }
        }
        IEnumerator LoadGif(byte[] bytes)
        {
            yield return UniGif.UniGif.GetTextureListCoroutine(bytes, (texList, loopCount, width, height) =>
            {
                if (texList != null)
                {
                    this.GifTextures = texList;
                    this.GifSprites = texList.Select(t => Sprite.Create(t.m_texture2d, new Rect(0, 0, t.m_texture2d.width, t.m_texture2d.height), new Vector2(0.5f, 0.5f), this.OriginalBody.pixelsPerUnit * t.m_texture2d.width / this.OriginalBody.texture.width, 0, SpriteMeshType.FullRect, Vector4.zero, true)).ToList();
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
        void GetOriginal()
        {
            GameObject body = this.gameObject?.transform?.parent?.parent?.Find("Health")?.gameObject;
            if (body is null)
            {
                body = this.gameObject?.transform?.parent?.parent?.Find("Face")?.gameObject;
            }
            if (body != null)
            {
                this.OriginalBody = body.GetComponent<SpriteRenderer>()?.sprite;
                if (this.OriginalBody is null)
                {
                    this.OriginalBody = body.GetComponent<UnityEngine.UI.Image>()?.sprite;
                }
            }                
        }
        void ApplyBodySprite(int spriteID, bool reset)
        {
            if (this.Body is null)
            {
                this.Body = this.gameObject?.transform?.parent?.parent?.Find("Health")?.gameObject;
                if (this.Body is null)
                {
                    this.Body = this.gameObject?.transform?.parent?.parent?.Find("Face")?.gameObject;
                }
            }                
            if (this.Body != null)
            {
                // check if the this.Body is an image (character editor)
                if (spriteID != -1)
                {
                    if (!(this.Body.GetComponent<UnityEngine.UI.Image>() is null)) { this.Body.GetComponent<UnityEngine.UI.Image>().sprite = this.GifSprites[spriteID]; }
                    if (!(this.Body.GetComponent<SpriteRenderer>() is null)) { this.Body.GetComponent<SpriteRenderer>().sprite = this.GifSprites[spriteID]; }
                    if (!(this.Body.GetComponent<SpriteMask>() is null)) { this.Body.GetComponent<SpriteMask>().sprite = this.GifSprites[spriteID]; }
                }
                else
                {
                    if (!(this.Body.GetComponent<UnityEngine.UI.Image>() is null)) { this.Body.GetComponent<UnityEngine.UI.Image>().sprite = this.OriginalBody; }
                    if (!(this.Body.GetComponent<SpriteRenderer>() is null)) { this.Body.GetComponent<SpriteRenderer>().sprite = this.OriginalBody; }
                    if (!(this.Body.GetComponent<SpriteMask>() is null)) { this.Body.GetComponent<SpriteMask>().sprite = this.OriginalBody; }
                }
                if (reset)
                {
                    if (this.Body.GetComponent<CharacterItemMirror>() != null)
                    {
                        Destroy(this.Body.GetComponent<CharacterItemMirror>());
                    }
                }
                else
                {
                    if (this.Body.GetComponentInParent<Player>() != null)
                    {
                        this.Body.gameObject.GetOrAddComponent<CharacterItemMirror>();
                    }
                }
                this.UpdateShadows(spriteID);
            }
        }
        void UpdateShadows(int spriteID)
        {
            // update the path of the SFPolygon so that shadows match the new sprite
            if (spriteID == -1 && this.SFPolygon != null && this.OriginalBody != null)
            {

                List<Vector2> path = new List<Vector2>();
                this.SFPolygon.pathCount = this.OriginalBody.GetPhysicsShapeCount();
                for (int i = 0; i < this.OriginalBody.GetPhysicsShapeCount(); i++)
                {
                    path.Clear();
                    this.OriginalBody.GetPhysicsShape(i, path);
                    this.SFPolygon.SetPath(i, path.ToArray());
                }
                return;
            }
            if (this.SFPolygon != null && this.GifSprites != null && this.GifSprites[spriteID] != null)
            {
                List<Vector2> path = new List<Vector2>();
                this.SFPolygon.pathCount = this.GifSprites[spriteID].GetPhysicsShapeCount();
                for (int i = 0; i < this.GifSprites[spriteID].GetPhysicsShapeCount(); i++)
                {
                    path.Clear();
                    this.GifSprites[spriteID].GetPhysicsShape(i, path);
                    this.SFPolygon.SetPath(i, path.ToArray());
                }
            }
        }
        void OnDestroy()
        {
            this.ApplyBodySprite(-1, true); 
        }
    }

}
