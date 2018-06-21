using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextDictionary : MonoBehaviour {

	[SerializeField]
	public List<Sprite> fontSprites = new List<Sprite>();
	public Dictionary<char, Sprite> fontDictionary = new Dictionary<char, Sprite>();

	public SpriteRenderer spriteExample;

	private void Start () {
		foreach (Sprite sprite in fontSprites) {
			fontDictionary.Add(sprite.name[0], sprite);
		}
	}
	
	public List<Sprite> GetSprites (string text) {
		List<Sprite> newSprites = new List<Sprite>();

		foreach (char charCurrent in text) {
			newSprites.Add(fontDictionary[charCurrent]);
		}

		return newSprites;
	}

}
