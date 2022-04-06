using UnityEngine;
using UnboundLib;
using System.Collections.Generic;
namespace PlayerCustomizationUtils
{
    public class BodyCharacterItem : MonoBehaviour
    {
        public SFPolygon SFPolygon => this.Body?.GetComponent<SFPolygon>();
        public GameObject Body { get; private set; } = null;
        public Sprite BodySprite => this.GetComponent<SpriteRenderer>()?.sprite ?? OriginalBody;
        public Sprite OriginalBody { get; private set; } = null;
        void Start()
        {
            this.GetOriginal();
            this.ApplyBodySprite(this.BodySprite, false);
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
        void ApplyBodySprite(Sprite sprite, bool reset)
        {
            GameObject body = this.gameObject?.transform?.parent?.parent?.Find("Health")?.gameObject;
            if (body is null)
            {
                body = this.gameObject?.transform?.parent?.parent?.Find("Face")?.gameObject;
            }
            if (body != null && this.BodySprite != null)
            {
                this.Body = body;
                // check if the body is an image (character editor)
                Sprite spriteToSet = null;
                if (reset)
                {
                    spriteToSet = this.OriginalBody;
                }
                else if (sprite != null & this.OriginalBody != null)
                {
                    spriteToSet = Sprite.Create(sprite.texture, new Rect(0, 0, sprite.texture.width, sprite.texture.height), new Vector2(0.5f, 0.5f), this.OriginalBody.pixelsPerUnit * sprite.texture.width / this.OriginalBody.texture.width, 0, SpriteMeshType.FullRect, Vector4.zero, true);
                }
                if (!(body.GetComponent<UnityEngine.UI.Image>() is null)) { body.GetComponent<UnityEngine.UI.Image>().sprite = spriteToSet; }
                if (!(body.GetComponent<SpriteRenderer>() is null)) { body.GetComponent<SpriteRenderer>().sprite = spriteToSet; }
                if (!(body.GetComponent<SpriteMask>() is null)) { body.GetComponent<SpriteMask>().sprite = spriteToSet; }
                if (reset)
                {
                    if (body.GetComponent<CharacterItemMirror>() != null)
                    {
                        Destroy(body.GetComponent<CharacterItemMirror>());
                    }
                }
                else
                {
                    if (body.GetComponentInParent<Player>() != null)
                    {
                        body.gameObject.GetOrAddComponent<CharacterItemMirror>();
                    }
                }
                this.UpdateShadows(spriteToSet);
            }
        }
        void UpdateShadows(Sprite sprite)
        {
            // update the path of the SFPolygon so that shadows match the new sprite
            if (this.SFPolygon != null && sprite != null)
            {
                List<Vector2> path = new List<Vector2>();
                this.SFPolygon.pathCount = sprite.GetPhysicsShapeCount();
                for (int i = 0; i < sprite.GetPhysicsShapeCount(); i++)
                {
                    path.Clear();
                    sprite.GetPhysicsShape(i, path);
                    this.SFPolygon.SetPath(i, path.ToArray());
                }
            }
        }
        void OnDestroy()
        {
            this.ApplyBodySprite(OriginalBody, true); 
        }
    }

}
