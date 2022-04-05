using UnityEngine;
namespace PlayerCustomizationUtils
{
    public class ColorMatchingCharacterItem : MonoBehaviour
    {
        void Update()
        {
            Color? color = this.gameObject?.transform?.parent?.parent?.Find("Health")?.GetComponent<SpriteRenderer>()?.color;
            if (color is null)
            {
                color = this.gameObject?.transform?.parent?.parent?.Find("Face")?.GetComponent<SpriteRenderer>()?.color;
            }
            if (color is null)
            {
                color = this.gameObject?.transform?.parent?.parent?.Find("Face")?.GetComponent<UnityEngine.UI.Image>()?.color;
            }
            if (color != null)
            {
                this.gameObject.GetComponent<SpriteRenderer>().color = color.Value;
            }
        }
    }

}
