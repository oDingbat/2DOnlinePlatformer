using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Spike : MonoBehaviour {

	public GizmoHelper gizmoHelper;
	BoxCollider2D collider;
	RigidbodyConstraints2D constraints;
	Vector2 direction = Vector2.zero;

	private void Start () {
		collider = GetComponent<BoxCollider2D>();
		gizmoHelper = GameObject.Find("[GizmoHelper]").GetComponent<GizmoHelper>();

		if (transform.localEulerAngles.z == 0 || transform.localEulerAngles.z == 180) {
			constraints = RigidbodyConstraints2D.FreezePositionX;
		} else {
			constraints = RigidbodyConstraints2D.FreezePositionY;
		}

		switch (Mathf.Round(transform.localEulerAngles.z).ToString()) {
			case ("0"):
				direction = new Vector2(0, 1);
				break;
			case ("90"):
				direction = new Vector2(-1, 0);
				break;
			case ("180"):
				direction = new Vector2(0, -1);
				break;
			case ("270"):
				direction = new Vector2(1, 0);
				break;
		}
	}

	private void OnTriggerEnter2D (Collider2D col) {
		if (col.gameObject.layer == LayerMask.NameToLayer("Players")) {
			PlayerController colPlayer = col.transform.GetComponent<PlayerController>();
			if (colPlayer != null) {
				BoxCollider2D boxCol = col as BoxCollider2D;
				Vector2 playerEdgeCenterpointPos = (Vector2)colPlayer.positionLastFrame[1] + boxCol.offset + ((-direction * boxCol.size) / 2);
				Vector2 spikeEdgeCenterpointPos = (Vector2)transform.position + collider.offset + ((direction * collider.size) / 2);
				
				gizmoHelper.DrawSphere(playerEdgeCenterpointPos, Color.green, 0.25f);
				gizmoHelper.DrawSphere(spikeEdgeCenterpointPos, Color.red, 0.25f);
				gizmoHelper.DrawCube(colPlayer.positionLastFrame[1] + boxCol.offset, Color.green, boxCol.size);
				gizmoHelper.DrawCube((Vector2)transform.position + collider.offset, Color.red, collider.size);
				gizmoHelper.DrawCube(colPlayer.positionLastFrame[0] + boxCol.offset, Color.blue, boxCol.size);

				playerEdgeCenterpointPos += direction * 0.15f;

				if (direction == new Vector2(0, 1)) {
					if (playerEdgeCenterpointPos.y > spikeEdgeCenterpointPos.y) {
						colPlayer.Die(null);
					}
				} else if (direction == new Vector2(0, -1)) {
					if (playerEdgeCenterpointPos.y < spikeEdgeCenterpointPos.y) {
						colPlayer.Die(null);
					}
				} else if (direction == new Vector2(1, 0)) {
					if (playerEdgeCenterpointPos.x > spikeEdgeCenterpointPos.x) {
						colPlayer.Die(null);
					}
				} else if (direction == new Vector2(-1, 0)) {
					if (playerEdgeCenterpointPos.x < spikeEdgeCenterpointPos.x) {
						colPlayer.Die(null);
					}
				}
			}
		} else if (col.gameObject.layer == LayerMask.NameToLayer("Ragdolls")) {
			Rigidbody2D colRigidbody = col.transform.GetComponent<Rigidbody2D>();
			if (colRigidbody) {
				colRigidbody.constraints = constraints;
				colRigidbody.drag = 450;
				colRigidbody.angularDrag = 10;
			}
		}
	}

	private void OnTriggerExit2D (Collider2D col) {
		if (col.gameObject.layer == LayerMask.NameToLayer("Ragdolls")) {
			Rigidbody2D colRigidbody = col.transform.GetComponent<Rigidbody2D>();
			if (colRigidbody) {
				colRigidbody.constraints = new RigidbodyConstraints2D();
				colRigidbody.drag = 0;
				colRigidbody.angularDrag = 0;
			}
		}
	}

}
