using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerController : Entity {


	[Space(10)][Header ("Player Information")]
	public string playerName;

	[Space(10)][Header ("LayerMasks")]
	public LayerMask collisionMask;
	
	[Space(10)]
	[Header("Control Settings")]
	public ControlScheme controlScheme;
	public ControlScheme.ControlSchemePreset controlSchemePreset;

	[Space(10)][Header ("References")]
	public Camera camera;						
	public BoxCollider2D collider;							// The collider for the player
	public Rigidbody2D playerPhysicsSimulator;				// The physics capsule which interacts with ragdolls and other physics details
	public Animator spriteAnimator;							// The spriteAnimator responsible for the animations of the player's character

	float touchingWallTestScale = 0.85f;
	int touchingWallRaycasts = 10;
	bool touchingWall;
	float touchingWallDirection;
	
	float lookRange = 7.25f;
	public Vector2 lookDirection;

	public GameObject corpse;
	public Vector2 impactVelocity;
	public Vector2[] positionLastFrame = new Vector2[2];

	// Raycast stuff
	int horizontalRaycasts = 12;
	int verticalRaycasts = 5;
	float skinWidth = 0.01f;

	[Space(10)] [Header("Character Settings")]
	public int characterIndex;
	
	// Movement variables
	public float inputMovement;
	public Vector2 velocity;
	float speed = 12.5f;
	float jumpForce = 19.5f;
	bool grounded = true;
	float timeLastJumped;
	float timeLastPressedJump;
	float jumpForgiveness = 0.1f;
	public bool isEnabled;				// Is the playerController currently enabled?

	// Events
	public event Action <PlayerController, PlayerController, GameObject> EventOnDie;
	public event Action <PlayerController> EventChangeCharacter;

	private void Start () {
		collider = GetComponent<BoxCollider2D>();
		spriteAnimator = transform.Find("Animator").GetComponent<Animator>();

		// Setup physics simulator
		playerPhysicsSimulator = transform.Find("(PhysicsSimulator)").GetComponent<Rigidbody2D>();
		playerPhysicsSimulator.GetComponent<CapsuleCollider2D>().offset = collider.offset;
		playerPhysicsSimulator.GetComponent<CapsuleCollider2D>().size = new Vector2(collider.size.x, collider.size.y - 0.5f);
		playerPhysicsSimulator.transform.parent = transform.parent;

		// Set control scheme
		if (controlSchemePreset != ControlScheme.ControlSchemePreset.Null) {
			controlScheme = new ControlScheme(controlSchemePreset);
		}
	}

	public IEnumerator SpawningInvincibility (float invincibilityTime) {
		isInvincible = true;

		if (isDead == false) {

			float flashTime = 0.1f;
			int totalTime = (int)Mathf.Clamp(Mathf.Round(invincibilityTime / flashTime), 2, Mathf.Infinity);

			totalTime = (totalTime % 2 == 1 ? totalTime + 1 : totalTime); // Make sure totalTime is even

			Debug.Log(totalTime);

			for (int i = 0; i < totalTime; i++) {
				if (isDead == false) {
					spriteAnimator.gameObject.SetActive(!spriteAnimator.gameObject.activeSelf);
					yield return new WaitForSeconds(flashTime);
				}
			}

			if (isDead == false) {
				isInvincible = false;
			}
		}
	}

	private void FixedUpdate() {
		positionLastFrame[1] = positionLastFrame[0];
		positionLastFrame[0] = transform.position;
	}

	private void Update () {
		UpdateInput();
		if (isDead == false) {
			UpdateMovement();
			UpdateAnimator();
			UpdatePhysicsSimulator();
		} else {
			if (corpse != null) {
				transform.position = corpse.transform.Find("Ragdoll_Head").position;
			}
		}
	}

	private void UpdateInput() {
		if (isEnabled) {
			// Movement
			inputMovement = (Input.GetKey(controlScheme.left) != Input.GetKey(controlScheme.right)) ? (Input.GetKey(controlScheme.left) ? -1 : 1) : 0;

			// Jumping
			if (Input.GetKeyDown(controlScheme.jump)) {
				timeLastPressedJump = Time.time;
			}
		}

		// Character Swap
		if (Input.GetKeyDown(controlScheme.characterSwap)) {
			EventChangeCharacter(this);
		}

		// Zoom Toggle
		if (Input.GetKeyDown(controlScheme.zoom)) {
			if (camera.orthographicSize == 7.5f) {
				camera.orthographicSize = 7.5f * 2f;
			} else {
				camera.orthographicSize = 7.5f;
			}
			
		}

		// Looking
		if (Input.GetKey(controlScheme.up)) {
			lookDirection = Vector2.Lerp(lookDirection, new Vector2(0, lookRange), 10 * Time.deltaTime);
		} else if (Input.GetKey(controlScheme.down)) {
			lookDirection = Vector2.Lerp(lookDirection, new Vector2(0, -lookRange), 10 * Time.deltaTime);
		} else {
			lookDirection = Vector2.Lerp(lookDirection, new Vector2(0, 0), 10 * Time.deltaTime);
		}

		// Suiciding
		if (Input.GetKeyDown(controlScheme.suicide)) {
			if (isDead == false) {
				Die(null);
			}
		}
	}

	private void UpdateMovement () {
		velocity = Vector2.Lerp(velocity, new Vector2(inputMovement * speed, velocity.y), (grounded == true ? 5f : 4.25f) * Time.deltaTime);

		// Jumping
		if (timeLastJumped + 0.025f < Time.time && timeLastPressedJump + jumpForgiveness > Time.time) {	// Make sure we didnt just jump, give jumping a little wiggle room
			if (grounded == true) {
				timeLastPressedJump = 0;
				velocity.y = jumpForce;
				timeLastJumped = Time.time;
				grounded = false;
			} else if (touchingWall == true) {
				timeLastPressedJump = 0;
				velocity = new Vector2(-touchingWallDirection, 1.5f).normalized * jumpForce * 1.125f;
				timeLastJumped = Time.time;
			}
		}

		velocity += new Vector2(0, ((touchingWall == true && velocity.y < 0 && inputMovement == touchingWallDirection) ? -10 : -45) * Time.deltaTime);		// Apply gravity

		// Move player horizontally
		float hitDistanceH = Mathf.Infinity;
		float dx = (velocity.x > 0 ? 1 : -1);
		List<PlayerController> hitPlayers = new List<PlayerController>();
		for (int i = 0; i < horizontalRaycasts; i++) {
			float colliderYSmaller = collider.size.y - (skinWidth / 2);
			float raycastIncrement = colliderYSmaller / (float)(horizontalRaycasts - 1);       // Gets the increment between each raycast
			Vector2 origin = (Vector2)transform.position + new Vector2(((collider.size.x / 2) - skinWidth) * dx, -(colliderYSmaller / 2) + i * raycastIncrement) + collider.offset;
			Vector2 direction = Vector2.right * dx;
			//Debug.DrawRay(origin, direction, Color.red, 0);
			RaycastHit2D hit = Physics2D.Raycast(origin, direction, Mathf.Abs(velocity.x * Time.deltaTime) + skinWidth, collisionMask);

			if (hit.transform != null) {
				if (hit.distance - skinWidth < hitDistanceH) {
					hitDistanceH = hit.distance - skinWidth;
				}

				// Player -> Player collision
				PlayerController hitPlayer = hit.transform.GetComponent<PlayerController>();
				if (hitPlayer && hitPlayers.Contains(hitPlayer) == false) {
					hitPlayers.Add(hitPlayer);
				}
			}
		}

		// Apply Player -> Player collision
		foreach (PlayerController hitPlayer in hitPlayers) {
			hitPlayer.velocity.x += velocity.x;
		}

		if (hitDistanceH != Mathf.Infinity) {
			transform.position += (Vector3)new Vector2(hitDistanceH * dx, 0);
				velocity.x = 0;
		} else {
			transform.position += (Vector3)new Vector2(velocity.x, 0) * Time.deltaTime;
		}

		// Move player vertically
		float hitDistanceV = Mathf.Infinity;
		float dy = (velocity.y > 0 ? 1 : -1);
		List<PlayerController> headstompedPlayers = new List<PlayerController>();
		for (int i = 0; i < verticalRaycasts; i++) {
			float colliderXSmaller = collider.size.x - (skinWidth / 2);
			float raycastIncrement = colliderXSmaller / (float)(verticalRaycasts - 1);       // Gets the increment between each raycast
			Vector2 origin = (Vector2)transform.position + new Vector2(-(colliderXSmaller / 2) + i * raycastIncrement, ((collider.size.y / 2) - skinWidth) * dy) + collider.offset;
			Vector2 direction = Vector2.up * dy;
			Debug.DrawRay(origin, direction, Color.blue, 0);
			RaycastHit2D hit = Physics2D.Raycast(origin, direction, Mathf.Abs(velocity.y * Time.deltaTime) + skinWidth, collisionMask);

			if (hit.transform != null) {
				if (hit.distance - skinWidth < hitDistanceV) {
					hitDistanceV = hit.distance - skinWidth;
				}
				
				PlayerController hitPlayer = hit.transform.GetComponent<PlayerController>();
				if (hitPlayer) {
					if (dy == -1) {
						if (headstompedPlayers.Contains(hitPlayer) == false) {
							headstompedPlayers.Add(hitPlayer);
							TriggerHeadStomp(this, hitPlayer, false);
						}
					} else if (dy == 1) {
						TriggerHeadStomp(hitPlayer, this, true);
						return;
					}

				}
			}
		}

		if (hitDistanceV != Mathf.Infinity) {
			transform.position += (Vector3)new Vector2(0, hitDistanceV * dy);
			if (headstompedPlayers.Count == 0) {
				if (velocity.y < 0) {
					grounded = true;
				}
				velocity.y = 0;
			}
		} else {
			grounded = false;
			transform.position += (Vector3)new Vector2(0, velocity.y) * Time.deltaTime;
		}

		// Get touching wall information left
		bool touchingWallThisFrame = false;
		for (int d = -1; d <= 1; d += 2) {
			for (int i = 0; i < touchingWallRaycasts; i++) {
				float colliderYScaled = collider.size.y * touchingWallTestScale;
				float colliderYInitial = -(colliderYScaled / 2);
				float raycastIncrement = colliderYScaled / (float)(touchingWallRaycasts - 1);       // Gets the increment between each raycast
				Vector2 origin = (Vector2)transform.position + new Vector2(((collider.size.x / 2) - skinWidth) * d, colliderYInitial + i * raycastIncrement) + collider.offset;
				Vector2 direction = Vector2.right * d;
				Debug.DrawRay(origin, direction, Color.green, 0);
				RaycastHit2D hit = Physics2D.Raycast(origin, direction, 0.025f + skinWidth, collisionMask);

				if (hit.transform != null) {
					touchingWallThisFrame = true;
					touchingWallDirection = d;
				}
			}
		}
		
		if (touchingWallThisFrame == true) {
			touchingWall = true;
		} else {
			touchingWallDirection = 0;
			touchingWall = false;
		}
	}

	private void TriggerHeadStomp(PlayerController stomper, PlayerController stompee, bool isReverseHeadstomp) {
		if (isReverseHeadstomp) {
			stomper.velocity = new Vector2(stomper.velocity.x, Mathf.Clamp(stomper.velocity.y + stompee.velocity.y, jumpForce, Mathf.Infinity));
		} else {
			stomper.velocity = new Vector2(stomper.velocity.x, Mathf.Clamp(Mathf.Abs(stomper.velocity.y), jumpForce, Mathf.Infinity));
		}

		if (stompee.isInvincible == false) {
			stompee.impactVelocity = new Vector2(stomper.velocity.x + stompee.velocity.x, Mathf.Clamp(stomper.velocity.y * 15f, -600, -jumpForce * 5));
			stompee.Die(stomper);
		}
	}

	private void UpdateAnimator () {
		string characterName = spriteAnimator.runtimeAnimatorController.name;

		if (grounded == true) {
			spriteAnimator.transform.localScale = new Vector3(Mathf.Sign(velocity.x), 1, 1);
			spriteAnimator.speed = Mathf.Abs(velocity.x) / 10 * 1.25f;
			if (Mathf.Abs(velocity.x) < 0.125f) {
				spriteAnimator.Play(characterName + "_Standing");
			} else {
				spriteAnimator.Play(characterName + "_Walking");
			}
		} else {
			spriteAnimator.speed = 1;
			if (touchingWall == true) {
				spriteAnimator.transform.localScale = new Vector3(-touchingWallDirection, 1, 1);
				spriteAnimator.Play(characterName + "_WallSliding");
			} else {
				spriteAnimator.transform.localScale = new Vector3(Mathf.Sign(velocity.x), 1, 1);
				if (velocity.y > 0.125f) {
					spriteAnimator.Play(characterName + "_Jumping");
				} else {
					spriteAnimator.Play(characterName + "_Falling");
				}
			}
		}

	}

	private void UpdatePhysicsSimulator () {
		playerPhysicsSimulator.velocity = (transform.position - playerPhysicsSimulator.transform.position) * 50f;
	}

	public override void OnDie(PlayerController killer) {
		spriteAnimator.gameObject.SetActive(false);
		playerPhysicsSimulator.gameObject.SetActive(false);
		collider.enabled = false;

		GameObject prefabRagdoll = Resources.Load<GameObject>("Prefabs/Ragdolls/Ragdoll (" + spriteAnimator.runtimeAnimatorController.name + ")");

		corpse = (GameObject)Instantiate(prefabRagdoll, transform.position + new Vector3(0, 0.05f, 0), Quaternion.identity);

		EventOnDie(this, killer, corpse);

		foreach (Transform bodyPart in corpse.transform) {
			Rigidbody2D bodyPartRigidbody = bodyPart.GetComponent<Rigidbody2D>();
			if (bodyPartRigidbody != null) {
				bodyPartRigidbody.velocity = velocity * 2.75f * (bodyPartRigidbody.mass / 50);

				// Add headstomp impact velocity
				if (bodyPart.name == "Ragdoll_Head") {
					bodyPartRigidbody.velocity = impactVelocity;
					Debug.Log(impactVelocity);
				}
			}
		}
	}

	public override void OnRevive() {
		spriteAnimator.gameObject.SetActive(true);
		collider.enabled = true;
		playerPhysicsSimulator.gameObject.SetActive(true);

		impactVelocity = Vector2.zero;
		
		velocity = Vector2.zero;
	}

}
