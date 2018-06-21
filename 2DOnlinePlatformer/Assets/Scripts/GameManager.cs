using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

	[Space(10)][Header ("Layer Masks")]
	public LayerMask playerMask;
	
	[Space(10)][Header ("Player Variables")]
	public List<PlayerController> players;
	public CharacterSettings[] characterSettings;
	public List<GameObject> corpses;
	public List<ScoreboardPlayer> scoreboard;
	
	[Space(10)][Header ("Game Settings")]
	public GameSettings gameSettings;
	public List<Transform> respawnPoints;
	public bool gameIsStarted = false;
	public bool gameIsOver = false;
	public int roundNumber = 0;

	[Space(10)][Header ("Level Settings")]
	public GameObject[] levels;
	public GameObject levelCurrent;
	public Transform[] levelRespawnPoints;

	[Space(10)][Header ("GameMode Settings")]
	public GameMode gameModeCurrent;
	public List<GameMode> gameModes;
	
	[Space(10)] [Header("Text")]
	public DynamicText text_Header;
	public DynamicText text_Subheader;

	[Space(10)][Header ("Prefabs")]
	public GameObject prefab_DynamicMovingText;

	[System.Serializable]
	public class CharacterSettings {
		public string characterName;
		public Color color;
	}

	[System.Serializable]
	public class GameSettings {
		public int corpseMax = 16;
		public bool liveCharacterChange = false;
		public float initialCountdown = 5;
	}

	[System.Serializable]
	public struct GameMode {
		public string name;

		public PlayerSettings playerSettings;

		[System.Serializable]
		public struct PlayerSettings {
			[Space(10)] [Header("Player Settings")]
			public int livesStarting;               // The initial number of lives players have at the beginning of the game
			public int livesMax;                    // The maximum number of lives a player can have (no max if == 0)
			public float respawnTime;               // The amount of time it takes for players to respawn
			public bool lifeStealing;               // Do players gain +1 life on kill?
			public float spawnInvincibility;		// The amount of time players in invincible after they respawn
		}
	}

	[System.Serializable]
	public class ScoreboardPlayer {
		public PlayerController player;
		public int kills = 0;
		public int deaths = 0;
		public int suicides = 0;
		public int score = 0;
		public int lives = 0;

		public ScoreboardPlayer(PlayerController _player, int _lives) {
			player = _player;
			lives = _lives;
		}

		public void Reset (int _livesStarting) {
			kills = 0;
			deaths = 0;
			suicides = 0;
			score = 0;
			lives = _livesStarting;
		}
	}

	private void Start() {
		gameModeCurrent = gameModes[0];

		GetPlayerEvents();
		StartCoroutine(StartGame());
	}

	private void GetPlayerEvents () {
		// Setup player events
		foreach (PlayerController player in players) {
			player.EventOnDie += OnPlayerDied;
			player.EventChangeCharacter += OnPlayerChangeCharacter;
		}
	}

	private IEnumerator StartGame () {
		// This method is responsible for starting a new game, setting up respawn points, positioning players, etc.
		gameIsStarted = false;
		gameIsOver = false;
		roundNumber++;

		// Setup scoreboard
		foreach (PlayerController p in players) {
			if (scoreboard.Exists(s => s.player == p)) {        // Is this player already in the scoreboard?
				scoreboard.Single(s => s.player == p).Reset(gameModeCurrent.playerSettings.livesStarting);
			} else {
				scoreboard.Add(new ScoreboardPlayer(p, gameModeCurrent.playerSettings.livesStarting));
			}
		}

		// Setup Level
		if (levelCurrent != null) { Destroy(levelCurrent); }        // If there is already a level, destroy it
		levelCurrent = Instantiate(levels[UnityEngine.Random.Range(0, levels.Length)], Vector2.zero, Quaternion.identity, GameObject.Find("[Stage]").transform);     // Create a new level

		// Setup RespawnPoints
		respawnPoints.Clear();		// Clear old respawn points
		foreach (Transform respawnPoint in levelCurrent.transform.Find("[RespawnPoints]")) {        // Get all of the respawnPoints in the new level
			respawnPoints.Add(respawnPoint);
		}

		// Put players in their spawn positions by picking spawn points at random
		List<Transform> spawnPointsNotUsed = new List<Transform>(respawnPoints);
		foreach (PlayerController player in players) {
			player.isEnabled = false;		// Disable all players
			player.Revive();                // Revive all players
			player.inputMovement = 0;		// Reset input
			Transform randomSpawn = spawnPointsNotUsed[UnityEngine.Random.Range(0, spawnPointsNotUsed.Count)];
			player.transform.position = randomSpawn.transform.position;
			player.spriteAnimator.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("Art/" + characterSettings[player.characterIndex].characterName);
			spawnPointsNotUsed.Remove(randomSpawn);
			DisplayText(player, player.playerName.ToLower(), 3.5f, 1f);
		}

		// Clear old corpses
		foreach (GameObject corpse in corpses) {
			Destroy(corpse);
		}

		// Set initial Game Text
		SetGameText(gameModeCurrent.name.Replace(' ', '_').ToLower(), "good_luck!");

		yield return new WaitForSeconds(3);
		
		// Start Game Countdown
		for (int i = (int)gameSettings.initialCountdown; i > 0; i--) {
			SetGameText(i.ToString(), "");
			yield return new WaitForSeconds(1);
		}

		SetGameText("go!", "");

		// Start the game!
		gameIsStarted = true;

		// Enable every player so that they can move
		foreach (PlayerController player in players) {
			player.isEnabled = true;
		}

		yield return new WaitForSeconds(1.5f);

		// Hide the GameText
		SetGameText("", "");
	}

	public void SetGameText (string header, string subHeader) {
		text_Header.SetText(header);
		text_Subheader.SetText(subHeader);
	}
	
	public Vector2 GetRespawnPoint(PlayerController player) {
		if (respawnPoints.Count != 0) {

			SortedList<float, Transform> furthestRespawns = new SortedList<float, Transform>();

			// Create a list of the respawn points with the furthest collective distances from all of the players
			foreach (Transform r in respawnPoints) {
				float collectiveDistance = 0f;
				foreach (PlayerController p in players) {
					if (p.isDead == false && p != player) {			// Ignore this player if they're dead or if it's the player we're respawning
						collectiveDistance += Mathf.Sqrt(Vector2.Distance(r.position, p.transform.position));
					}
				}
				while(furthestRespawns.ContainsKey(collectiveDistance)) {
					collectiveDistance += 0.1f;
				}
				furthestRespawns.Add(collectiveDistance, r);
			}

			// Check to make sure no one is standing on the respawn
			while (furthestRespawns.Count > 0) {
				int randomFurthestRespawnIndex = (int)UnityEngine.Random.Range(Mathf.Clamp(furthestRespawns.Count - 6, 0, furthestRespawns.Count - 1), furthestRespawns.Count - 1);
				if (Physics2D.OverlapBox(furthestRespawns.ElementAt(randomFurthestRespawnIndex).Value.position, Vector2.one * 0.25f, 0, playerMask) == true) {
					furthestRespawns.RemoveAt(randomFurthestRespawnIndex);
				} else {
					return furthestRespawns.ElementAt(randomFurthestRespawnIndex).Value.transform.position;
				}
			}
			return Vector2.zero;			
		} else {
			Debug.LogError("Error: No respawn points found.");
			return Vector2.zero;
		}
	}

	public ScoreboardPlayer GetPlayerScoreboardInformation(PlayerController player) {
		return scoreboard.Single(s => s.player == player);
	}

	public CharacterSettings GetCharacterSettings(string animControllerName) {
		return characterSettings.Single(c => c.characterName == animControllerName);
	}

	public void OnPlayerDied(PlayerController playerKilled, PlayerController playerKiller, GameObject newCorpse) {
		// Add the new corpse
		if (corpses.Count == gameSettings.corpseMax && gameSettings.corpseMax != 0) {
			Destroy(corpses[0]);
			corpses.RemoveAt(0);
		}
		corpses.Add(newCorpse);

		ScoreboardPlayer scoreboardPlayerKiller = (playerKiller == null ? null : scoreboard.Single(p => p.player == playerKiller));
		ScoreboardPlayer scoreboardPlayerKilled = scoreboard.Single(p => p.player == playerKilled);
		
		if (playerKiller == null) {
			scoreboardPlayerKilled.suicides += 1;
		} else {
			scoreboardPlayerKiller.kills += 1;

			if (gameModeCurrent.playerSettings.lifeStealing == true) {
				scoreboardPlayerKiller.lives = (int)Mathf.Clamp(scoreboardPlayerKiller.lives + 1, 0, (gameModeCurrent.playerSettings.livesMax != 0 ? gameModeCurrent.playerSettings.livesMax : Mathf.Infinity));
				DisplayLives(playerKiller, 1);
			}
		}
		
		scoreboardPlayerKilled.deaths += 1;
		scoreboardPlayerKilled.lives = (int)Mathf.Clamp(scoreboardPlayerKilled.lives - 1, 0, Mathf.Infinity);
		DisplayLives(playerKilled, -1);

		if (scoreboardPlayerKilled.lives > 0 || gameModeCurrent.playerSettings.livesStarting == 0) {
			StartCoroutine(RespawnPlayer(playerKilled, gameModeCurrent.playerSettings.respawnTime));
		}

		CheckForGameOver();
	}

	public void OnPlayerChangeCharacter (PlayerController player) {
		if (gameSettings.liveCharacterChange == true) {
			if (characterSettings.Length > 0) {
				List<int> characterIndicesAvailable = new List<int>();		// Copy the characters

				// Create available characterIndices
				for (int i = 0; i < characterSettings.Length; i++) {
					characterIndicesAvailable.Add(i);
				}

				// Remove any characters currently being used by other players
				foreach (PlayerController p in players) {
					if (p != player) {
						characterIndicesAvailable.RemoveAt(p.characterIndex);
					}
				}

				int thisIndex = characterIndicesAvailable.FindIndex(c => c == player.characterIndex);

				Debug.Log((thisIndex == characterIndicesAvailable.Count - 1 ? 0 : thisIndex + 1));

				int nextCharacter = characterIndicesAvailable[(thisIndex == characterIndicesAvailable.Count - 1 ? 0 : thisIndex + 1)];

				Debug.Log(nextCharacter);

				player.characterIndex = nextCharacter;
				player.spriteAnimator.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("Art/" + characterSettings[player.characterIndex].characterName);
			}
		}
	}

	private void CheckForGameOver() {
		List<ScoreboardPlayer> playersAlive = scoreboard.Where(p => p.lives > 0).ToList();
		if (playersAlive.Count == 1) {
			StartCoroutine(GameOver(playersAlive[0]));
		}
	}

	public void DisplayLives(PlayerController player, int livesIncrement) {
		if (player.transform.Find("(DynamicMovingText)")) {
			Destroy(player.transform.Find("(DynamicMovingText)").gameObject);
		}

		Vector2 textOffset = new Vector2(0, 0.5f);
		GameObject newDynamicMovingText = (GameObject)Instantiate(prefab_DynamicMovingText, player.transform.position + (Vector3)textOffset, Quaternion.identity, player.transform);
		ScoreboardPlayer playerScoreboard = GetPlayerScoreboardInformation(player);
		newDynamicMovingText.name = "(DynamicMovingText)";


		if (livesIncrement == -1) {
			newDynamicMovingText.GetComponent<DynamicMovingText>().Construct(new string[] { (playerScoreboard.lives + -livesIncrement).ToString(), playerScoreboard.lives.ToString() }, player.transform, new Vector2(0, 1f), characterSettings[player.characterIndex].color);
		} else {
			newDynamicMovingText.GetComponent<DynamicMovingText>().Construct(new string[] { (playerScoreboard.lives + -livesIncrement).ToString(), playerScoreboard.lives.ToString() }, player.transform, new Vector2(0, 1f), characterSettings[player.characterIndex].color);
		}
	}

	public void DisplayText(PlayerController player, string text, float timeSolid, float timeFlash) {
		if (player.transform.Find("(DynamicMovingText)")) {
			Destroy(player.transform.Find("(DynamicMovingText)").gameObject);
		}

		Vector2 textOffset = new Vector2(0, 0.5f);
		GameObject newDynamicMovingText = (GameObject)Instantiate(prefab_DynamicMovingText, player.transform.position + (Vector3)textOffset, Quaternion.identity, player.transform);
		newDynamicMovingText.name = "(DynamicMovingText)";

		newDynamicMovingText.GetComponent<DynamicMovingText>().Construct(text, player.transform, new Vector2(0, 1f), characterSettings[player.characterIndex].color, timeSolid, timeFlash);
	}

	private IEnumerator GameOver (ScoreboardPlayer winner) {
		if (gameIsOver == false) {
			gameIsOver = true;

			Time.timeScale = 0.25f;
			text_Header.SetText("winner!");
			text_Subheader.textColor = characterSettings.Single(c => c.characterName == winner.player.spriteAnimator.runtimeAnimatorController.name).color;
			text_Subheader.SetText(winner.player.playerName.ToLower() + "!");

			yield return new WaitForSeconds(2f);

			Time.timeScale = 1f;
			StartCoroutine(StartGame());
		}
	}

	private IEnumerator RespawnPlayer (PlayerController player, float delay) {
		int roundNumberBefore = roundNumber;

		yield return new WaitForSeconds(delay);

		if (roundNumberBefore == roundNumber) {		// Make sure we don't respawn the player in a new round, when they died last round
			// Position the player at their new respawn point
			Vector2 newRespawnPoint = GetRespawnPoint(player);
			player.transform.position = newRespawnPoint;
			player.playerPhysicsSimulator.transform.position = newRespawnPoint;

			player.Revive();
			StartCoroutine(player.SpawningInvincibility(gameModeCurrent.playerSettings.spawnInvincibility));        // Apply invincibility
		}
	}

}
