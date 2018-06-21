using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Elevator : MonoBehaviour {

	public LayerMask playerAndRagdollMask;
	public LayerMask playerMask;
	public LayerMask environmentMask;
	public GizmoHelper gizmoHelper;
	public BoxCollider2D collider;

	public Vector2 initialPosition;

	public PatrolType patrolType = PatrolType.Backtracking;
	public enum PatrolType { Backtracking, Linear }

	public MovementDirection movementDirection = MovementDirection.Forwards;
	public enum MovementDirection { Forwards, Backwards};

	public float speed;
	public float pauseTimeCurrent;
	public float pauseTimeInterval;
	public bool pausing;
	public Vector2[] positions;
	public int positionIndex;

	float railPointSize = 10f * (1f / 12f);

	public GameObject prefab_elevatorPoint;
	public GameObject prefab_elevatorRail;

	private void Start () {
		for (int i = 0; i < positions.Length; i++) {
			GameObject elevatorPoint = (GameObject)Instantiate(prefab_elevatorPoint, initialPosition + positions[i], Quaternion.identity);
			elevatorPoint.transform.parent = transform.parent;
			elevatorPoint.transform.name = "ElevatorPoint";

			if (i != 0) {
				GameObject elevatorRail = (GameObject)Instantiate(prefab_elevatorRail, initialPosition + ((positions[i - 1] + positions[i]) / 2), Quaternion.identity);

				float railLength = Vector2.Distance(positions[i - 1], positions[i]) - railPointSize;
				elevatorRail.transform.localScale = new Vector3(1, railLength, 1);
				elevatorRail.transform.parent = transform.parent;
				elevatorRail.transform.name = "ElevatorRail";
				elevatorRail.transform.GetComponent<Renderer>().material.SetTextureScale("_MainTex", new Vector2(1, railLength));
			}
		}
	}

	private void Update () {
		if (pausing == true) {
			pauseTimeCurrent = Mathf.Clamp(pauseTimeCurrent - Time.deltaTime, 0, Mathf.Infinity);
			if (pauseTimeCurrent == 0) {
				pausing = false;
			}
		} else {
			// Movement
			Vector2 desiredPositionDirection = (initialPosition + positions[positionIndex]) - (Vector2)transform.position;
			Vector2 deltaP = Vector2.ClampMagnitude(desiredPositionDirection.normalized * speed * Time.deltaTime, desiredPositionDirection.magnitude);

			// Move Vertically
			float deltaY = deltaP.y;
			List<RaycastHit2D> hitPlayers = new List<RaycastHit2D>();
			int raycasts = 10;
			float skinWidth = 0.05f;

			// Pushing
			for (int i = 0; i < raycasts; i++) {
				float colliderXSmaller = collider.size.x - (skinWidth / 2);
				float raycastIncrement = colliderXSmaller / (float)(raycasts - 1);       // Gets the increment between each raycast
				Vector2 origin = (Vector2)transform.position + new Vector2(-(colliderXSmaller / 2) + i * raycastIncrement, ((collider.size.y / 2) - skinWidth) * Mathf.Sign(deltaY)) + collider.offset;
				Vector2 direction = new Vector2(0, Mathf.Sign(deltaY));
				Debug.DrawRay(origin, direction, Color.red, 0);

				RaycastHit2D[] hits = Physics2D.RaycastAll(origin, direction, Mathf.Abs(deltaY) + skinWidth, playerAndRagdollMask);
				
				foreach (RaycastHit2D hit in hits) {
					if (hit.distance > skinWidth / 2) {
						Transform hitTransform = (hit.transform.gameObject.layer == LayerMask.NameToLayer("Players") ? hit.transform : hit.transform.parent);
						if (hitPlayers.Exists(h => h.transform == hitTransform) == false) {
							hitPlayers.Add(hit);
							// Move Player/Ragdoll
							//float trueDistance = Mathf.Clamp(hit.distance - skinWidth, 0, Mathf.Infinity);		// This should work I thought, but can't get it too :( Works good enough tho amirite?
							if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Players")) {
								hitTransform.position += (Vector3)new Vector2(0, deltaY);
							} else {
								hit.transform.GetComponent<Rigidbody2D>().velocity = new Vector2(hit.transform.GetComponent<Rigidbody2D>().velocity.x, speed * direction.y * 1.35f);
							}
						}
					}
				}
			}

			// Pulling
			if (Mathf.Sign(deltaY) == -1) {
				Collider2D[] pulledPlayers = Physics2D.OverlapBoxAll((Vector2)transform.position + new Vector2(0, collider.size.y / 2), new Vector2(collider.size.x - 0.66666666f, 0.125f), 0, playerMask);
				foreach (Collider2D pulledPlayer in pulledPlayers) {
					pulledPlayer.transform.position += (Vector3)new Vector2(0, deltaY);
				}
			}

			// Move elevator
			transform.position += (Vector3)deltaP;

			// Kill Players if now in environment (ie: in wall/other elevator)
			foreach (RaycastHit2D hit in hitPlayers) {
				if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Players")) {
					if (Physics2D.OverlapBox((Vector2)hit.transform.position + hit.transform.GetComponent<BoxCollider2D>().offset, hit.transform.GetComponent<BoxCollider2D>().size - new Vector2(skinWidth, skinWidth), 0, environmentMask)) {
						hit.transform.GetComponent<PlayerController>().Die(null);
						hit.transform.position -= (Vector3)new Vector2(0, deltaY);
					}
				}
			}

			if (transform.position == (Vector3)(initialPosition + positions[positionIndex])) {
				pausing = true;
				pauseTimeCurrent = pauseTimeInterval;
				
				// Change positionIndex
				if (patrolType == PatrolType.Backtracking) {
					// Backtracking patrolling
					int indexDirection = (movementDirection == MovementDirection.Forwards ? 1 : -1);

					if (positionIndex == 0 && movementDirection == MovementDirection.Backwards || positionIndex == positions.Length - 1 && movementDirection == MovementDirection.Forwards) {
						movementDirection = (movementDirection == MovementDirection.Forwards ? MovementDirection.Backwards : MovementDirection.Forwards);
						indexDirection *= -1;
					}

					positionIndex += indexDirection;
				} else if (patrolType == PatrolType.Linear) {
					if (positionIndex == 0 && movementDirection == MovementDirection.Backwards || positionIndex == positions.Length - 1 && movementDirection == MovementDirection.Forwards) {
						positionIndex = (movementDirection == MovementDirection.Forwards ? 0 : positions.Length - 1);
					} else {
						positionIndex += (movementDirection == MovementDirection.Forwards ? 1 : -1);
					}
				}
			}
		}
	}

	private void OnDrawGizmosSelected () {
		if (gizmoHelper == null) {
			gizmoHelper = GameObject.Find("[GizmoHelper]").GetComponent<GizmoHelper>();
		}

		if (collider == null) {
			collider = GetComponent<BoxCollider2D>();
		}

		if (initialPosition == Vector2.zero) {
			initialPosition = transform.position;
		}

		foreach (Vector2 pos in positions) {
			gizmoHelper.DrawCube(initialPosition + pos, (pos == positions[positionIndex] ? Color.cyan : Color.red), collider.size);
		}
	}

}
