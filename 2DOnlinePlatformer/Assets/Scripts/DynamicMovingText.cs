using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicMovingText : DynamicText {

	public Transform target;
	public Vector2 offset;

	public string[] messages;

	public float timeSolid;
	public float timeFlash;

	void Update () {
		Vector2 desiredPosition = target.position + (Vector3)offset;
		transform.position = new Vector2(Mathf.Round(desiredPosition.x * 12) / 12, Mathf.Round(desiredPosition.y * 12) / 12);
	}

	public void Construct (string[] newMessages, Transform newTarget, Vector2 newOffset, Color color) {
		target = newTarget;
		offset = newOffset;
		messages = newMessages;
		textColor = color;
		timeSolid = 1f;
		timeFlash = 1f;

		SetText(messages[0]);
		StartCoroutine(Disappear());
	}

	public void Construct(string newMessage, Transform newTarget, Vector2 newOffset, Color color) {
		target = newTarget;
		offset = newOffset;
		messages = new string[] { newMessage };
		textColor = color;
		timeSolid = 1f;
		timeFlash = 1f;

		SetText(messages[0]);
		StartCoroutine(Disappear());
	}

	public void Construct(string newMessage, Transform newTarget, Vector2 newOffset, Color color, float newTimeSolid, float newTimeFlash) {
		target = newTarget;
		offset = newOffset;
		messages = new string[] { newMessage };
		textColor = color;
		timeSolid = newTimeSolid;
		timeFlash = newTimeFlash;

		SetText(messages[0]);
		StartCoroutine(Disappear());
	}

	IEnumerator Disappear() {
		yield return new WaitForSeconds(timeSolid / 2);

		if (messages.Length > 1) {
			for (int m = 1; m < messages.Length; m++) {
				SetText(messages[m]);
				yield return new WaitForSeconds(timeSolid / 2);
			}
		} else {
			yield return new WaitForSeconds(timeSolid / 2);
		}

		for (int i = 0; i < (timeFlash / 0.1f); i++) {
			textContainer.gameObject.SetActive(!textContainer.gameObject.activeSelf);
			yield return new WaitForSeconds(0.1f);
		}

		Destroy(gameObject);
	}
}
