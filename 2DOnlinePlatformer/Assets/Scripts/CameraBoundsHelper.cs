using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBoundsHelper : MonoBehaviour {

	public GizmoHelper gizmoHelper;
	public BoxCollider2D boxCollider2D;

	//float pixelWidth = 320;
	//float pixelHeight = 180;

	float pixelWidth = 480;
	float pixelHeight = 270;

	private void OnDrawGizmosSelected () {
		if (gizmoHelper == null) {
			gizmoHelper = GameObject.Find("[GizmoHelper]").GetComponent<GizmoHelper>();
		}

		if (boxCollider2D == null) {
			boxCollider2D = GetComponent<BoxCollider2D>();
		}

		gizmoHelper.DrawCube(transform.position, Color.Lerp(Color.red, Color.yellow, 0.5f), new Vector2(boxCollider2D.size.x + (pixelWidth / 12), boxCollider2D.size.y + (pixelHeight / 12)), transform.eulerAngles);
	}

}
