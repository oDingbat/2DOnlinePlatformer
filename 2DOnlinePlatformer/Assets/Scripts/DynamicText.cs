using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DynamicText : MonoBehaviour {
	
	public TextDictionary textDictionary;
	public Transform textContainer;

	public Color textColor;
	public string text;
	public GameObject prefab_Letter;
	public TextAlignment textAlignment;
	float pixelUnit = (1f / 12f);
	public int textSortingOrder = 499;

	private void Start () {
		SetText(text);
	}

	public void SetText (string newText) {
		text = newText;
		textDictionary = GameObject.FindGameObjectWithTag("TextDictionary").GetComponent<TextDictionary>();

		foreach (Transform textChild in textContainer) {
			Destroy(textChild.gameObject);
		}
		
		List<Sprite> textSprites = textDictionary.GetSprites(newText);

		float initialOffset = 0;

		if (textAlignment == TextAlignment.Center) {
			initialOffset = -pixelUnit / 2;
			foreach (Sprite textSprite in textSprites) {
				initialOffset -= (textSprite.textureRect.width / 24) - pixelUnit;
				if (textSprite != textSprites[0]) {
					initialOffset -= pixelUnit;
				}
			}
		} else if (textAlignment == TextAlignment.Right) {
			initialOffset = -pixelUnit;
			foreach (Sprite textSprite in textSprites) {
				initialOffset -= ((textSprite.textureRect.width / 12) - pixelUnit);
				if (textSprite != textSprites[0]) {
					initialOffset -= (1 / 12);
				}
			}
		}

		for (int i = 0; i < 5; i++) {
			float widthCurrent = 0;
			foreach (Sprite textSprite in textSprites) {
				GameObject newLetter = (GameObject)Instantiate(prefab_Letter, transform.position, Quaternion.identity, textContainer);
				newLetter.transform.localPosition = new Vector3((textSprite.textureRect.width / 24) + widthCurrent + initialOffset, 0);
				SpriteRenderer newLetterRenderer = newLetter.GetComponent<SpriteRenderer>();
				newLetterRenderer.sprite = textSprite;
				widthCurrent += (textSprite.textureRect.width / 12) - pixelUnit;
				newLetterRenderer.color = (i == 0 ? textColor : new Color(0.188f, 0.172f, 0.180f));
				
				if (i > 0) {    // Border
					Vector2 borderOffset = Vector2.zero;
					switch (i) {
						case (1):
							borderOffset = Vector2.up * pixelUnit;
							break;
						case (2):
							borderOffset = Vector2.up * -pixelUnit;
							break;
						case (3):
							borderOffset = Vector2.right * pixelUnit;
							break;
						case (4):
							borderOffset = Vector2.right * -pixelUnit;
							break;
					}

					newLetterRenderer.transform.position += (Vector3)borderOffset;
					newLetterRenderer.sortingOrder = textSortingOrder - 1;
				} else {
					newLetterRenderer.sortingOrder = textSortingOrder;
				}
			}
		}
	}

}
