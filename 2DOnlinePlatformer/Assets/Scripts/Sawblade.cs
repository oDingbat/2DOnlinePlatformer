using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sawblade : MonoBehaviour {
	
	public float velocity = 180;

	public GizmoHelper gizmoHelper;
	BoxCollider2D collider;
	RigidbodyConstraints2D constraints;
	Vector2 direction = Vector2.zero;

	private void Start() {
		collider = GetComponent<BoxCollider2D>();
		gizmoHelper = GameObject.Find("[GizmoHelper]").GetComponent<GizmoHelper>();
	}

	private void Update () {
		transform.eulerAngles += new Vector3(0, 0, velocity * Time.deltaTime);
	}

	private void OnTriggerEnter2D(Collider2D col) {
		if (col.gameObject.layer == LayerMask.NameToLayer("Players")) {
			PlayerController colPlayer = col.transform.GetComponent<PlayerController>();
			if (colPlayer != null) {
				colPlayer.Die(null);
			}
		} else if (col.gameObject.layer == LayerMask.NameToLayer("Ragdolls")) {
			Rigidbody2D colRigidbody = col.transform.GetComponent<Rigidbody2D>();
			if (colRigidbody) {
				Vector2 velocityDirection = (colRigidbody.transform.position - transform.position).normalized * 25f;
				Vector2 velocityPerpendicular = Quaternion.Euler(0, 0, 90 * Mathf.Sign(velocity)) * velocityDirection;
				colRigidbody.velocity += Vector2.Lerp(velocityDirection, velocityPerpendicular, 0.5f);
				HingeJoint2D colHingeJoint = colRigidbody.GetComponent<HingeJoint2D>();
				if (colHingeJoint) {
					Destroy(colHingeJoint);
				}
			}
		}
	}

}
