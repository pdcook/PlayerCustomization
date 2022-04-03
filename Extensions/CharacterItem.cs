using UnityEngine;
namespace PlayerCustomization.Extensions
{
    public static class CharacterItemExtensions
    {
		private static readonly Vector3 defaultHealthbarOffset = new Vector3(0f, 0.851f, 0f);
        public static void SetMoveHealthBarUp(this CharacterItem characterItem, float moveHealthBarUp)
        {
			if (characterItem.transform.root.GetComponent<Player>())
			{
				if (moveHealthBarUp != 0f)
				{
					HealthBar componentInChildren = characterItem.transform.root.GetComponentInChildren<HealthBar>();
					if (componentInChildren)
					{
						componentInChildren.transform.localPosition = defaultHealthbarOffset + Vector3.up * moveHealthBarUp;
					}
				}
			}
			characterItem.moveHealthBarUp = moveHealthBarUp;
		}
        public static void SetScale(this CharacterItem characterItem, float scale)
        {
            characterItem.transform.localScale = Vector3.one * scale;
            characterItem.scale = scale;
        }
    }
}
