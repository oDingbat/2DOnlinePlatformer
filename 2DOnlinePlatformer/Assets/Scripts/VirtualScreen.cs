using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualScreen : MonoBehaviour {

	public Camera virtualCamera;

	float gameHeight = 270;
	float gameWidth = 480;

	public void Update () {
		virtualCamera.orthographicSize = Screen.height / 2;

		int screenScaleMultiplier = 1;
		float closestDistance = Mathf.Infinity;

		for (int i = 0; i < 10; i++) {
			float thisDistance = Mathf.Abs(Screen.height - i * gameHeight);
			if (thisDistance < closestDistance) {
				closestDistance = thisDistance;
				screenScaleMultiplier = i;
			}
		}
		
		Debug.Log(screenScaleMultiplier);
		
		transform.localScale = new Vector3(gameWidth * screenScaleMultiplier, gameHeight * screenScaleMultiplier, 1);
	}

}
