using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CameraController : MonoBehaviour {

	public GameManager gameManager;

	public PlayerController trackedPlayer;
	public List<PlayerController> players;
	public int spectatedPlayerIndex;

	public Transform cameraBoundsContainer;
	public List<BoxCollider2D> allCameraBounds;
	public BoxCollider2D trackBox;

	Vector2 softLockPosition = Vector2.zero;
	float softLockMinDistance = 0f;

	public CameraMode cameraMode;
	public enum CameraMode { Static, Mobile }

	Vector3 cameraPosition;
	Vector3 desiredPosition;
	
	private void Start () {
		gameManager = GameObject.Find("[GameManager]").GetComponent<GameManager>();
		
		softLockPosition = trackedPlayer.transform.position;
	}

	private void Update() {
		UpdateCameraBounds();
		UpdateInput();
		UpdateMovement();
		UpdateSpectateSwapping();
	}

	private void UpdateCameraBounds() {
		if (cameraBoundsContainer == null) {
			if (gameManager.levelCurrent != null) {
				allCameraBounds.Clear();
				cameraBoundsContainer = gameManager.levelCurrent.transform.Find("[CameraBoundsContainer]");
				foreach (Transform child in cameraBoundsContainer) {
					BoxCollider2D childBoxCol2D = child.GetComponent<BoxCollider2D>();
					if (childBoxCol2D != null) {
						allCameraBounds.Add(childBoxCol2D);
					}
				}
			}
		}
	}

	private void UpdateInput() {
		if (trackedPlayer.timeOfDeath + 3 < Time.time && gameManager.scoreboard.Single(p => p.player == trackedPlayer).lives == 0) {
			if (Input.GetKeyDown(trackedPlayer.controlScheme.right)) {
				ChangeSpectatePlayer(1);
			}
			if (Input.GetKeyDown(trackedPlayer.controlScheme.left)) {
				ChangeSpectatePlayer(-1);
			}
		}
	}

	private void ChangeSpectatePlayer (int direction) {
		if (players.Exists(p => p.timeOfDeath + 3 >= Time.time) || gameManager.scoreboard.Exists(p => p.lives > 0 && p.player != trackedPlayer) == true) {       // Make sure theres atleast 1 player alive, otherwise, don't bother
			spectatedPlayerIndex = (spectatedPlayerIndex + direction == -1 ? players.Count - 1 : (spectatedPlayerIndex + direction == players.Count ? 0 : spectatedPlayerIndex + direction));
			if (players[spectatedPlayerIndex].timeOfDeath + 3 < Time.time && gameManager.scoreboard.Single(p => p.player == players[spectatedPlayerIndex]).lives == 0) {
				ChangeSpectatePlayer(direction);					// Then change again
			} else {
				gameManager.DisplayText(players[spectatedPlayerIndex], "v", 0, 0.75f);
			}
		}
	}

	private void UpdateMovement () {
		switch (cameraMode) {
			case (CameraMode.Mobile):
				Vector2 trackedPosition = Vector2.zero;
				Vector2 lookDir = Vector2.zero;
				Vector2 velocityDir = Vector2.zero;
				if (trackedPlayer.timeOfDeath + 3 < Time.time && gameManager.scoreboard.Single(p => p.player == trackedPlayer).lives == 0) {
					trackedPosition = players[spectatedPlayerIndex].transform.position;
				} else {
					trackedPosition = trackedPlayer.transform.position;
					lookDir = trackedPlayer.lookDirection;
					//velocityDir = Vector2.ClampMagnitude(trackedPlayer.velocity * 0.1f, 3f);
				}

				Vector2 softLockMoveDir = (trackedPosition - softLockPosition);
				softLockPosition += softLockMoveDir.normalized * Mathf.Clamp(softLockMoveDir.magnitude - softLockMinDistance, 0, Mathf.Infinity);

				trackBox.transform.position = softLockPosition + lookDir + velocityDir;

				// Find the closest position for the camera within the camera bounds
				Vector2 closestPoint = Vector2.zero;
				float closestDistance = Mathf.Infinity;

				foreach (BoxCollider2D cameraBounds in allCameraBounds) {
					if (cameraBounds.OverlapPoint(trackBox.transform.position) == true) {
						closestPoint = trackBox.transform.position;
						closestDistance = 0;
						break;
					}

					ColliderDistance2D colDist2D = Physics2D.Distance(cameraBounds, trackBox);
					float thisDistance = Vector2.Distance(trackBox.transform.position, colDist2D.pointA);
					if (thisDistance < closestDistance) {
						closestDistance = thisDistance;
						closestPoint = colDist2D.pointA;
					}
				}

				desiredPosition = closestPoint;

				cameraPosition = Vector3.Lerp(cameraPosition, desiredPosition, 20f * Time.deltaTime);

				break;
			case (CameraMode.Static):
				// Be static?
				break;
		}
		// Move camera
		transform.position = new Vector3(Mathf.Round(cameraPosition.x * 12) / 12, Mathf.Round(cameraPosition.y * 12) / 12, -1);
	}

	private void UpdateSpectateSwapping () {
		if (players.Exists(p => p.timeOfDeath + 3 >= Time.time) || gameManager.scoreboard.Exists(p => p.lives > 0 && p.player != trackedPlayer) == true) {       // Make sure theres atleast 1 player alive, otherwise, don't bother
			if (players[spectatedPlayerIndex].timeOfDeath + 3 < Time.time && gameManager.scoreboard.Single(p => p.player == players[spectatedPlayerIndex]).lives == 0) {
				ChangeSpectatePlayer(1);                    // Then change again
			}
		}
	}	
	
}
