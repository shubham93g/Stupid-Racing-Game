using UnityEngine;
using System.Collections;

public class PowerUpController : MonoBehaviour {

	public enum PowerUpTypes { Shield, MassSlow, SlowFastest, SpawnBomb };	
	public PowerUpTypes type;
	
	[Tooltip("How long the power up affects the players")]
	public float duration;
	
	[Tooltip("The prefab to be spawned")]
	public GameObject prefab;

	private GameObject triggeringPlayer;
	private Vector2 initialPosition;
	private float maxHeight;
	
	bool isActive = true;

	void Start() {
		maxHeight = renderer.bounds.extents.y / 2;
		initialPosition = transform.position;
	}

	// Bob up and down
	void FixedUpdate() {
		transform.position = new Vector2(
			initialPosition.x,
			initialPosition.y + (Mathf.Sin (GameController.timeElapsed * 2) * maxHeight)
			);
	}

	// What to do when the player collides with the power up
	void OnTriggerEnter2D(Collider2D other) {
		triggeringPlayer = other.gameObject;

		if (other.gameObject.tag.Equals("Player") && isActive) {
			isActive = false;
			PlayerController playerScript = triggeringPlayer.GetComponent<PlayerController> ();

			switch (type) {
			case PowerUpTypes.Shield:
				if (Network.isServer)
					networkView.RPC ("ActivateShieldForNetworkPlayers", RPCMode.All, playerScript.playerIdAndName.playerId);
				break;		

			case PowerUpTypes.SpawnBomb:
				if (Network.isServer) {					
					Vector3 newPos = triggeringPlayer.transform.position;
					newPos.x = newPos.x - (triggeringPlayer.renderer.bounds.extents.x * 1.2f);
					newPos.y = newPos.y + (triggeringPlayer.renderer.bounds.extents.y);

					Network.Instantiate (prefab, newPos, Quaternion.identity, 0);

					networkView.RPC ("SyncDestroy", RPCMode.All, playerScript.playerIdAndName.playerId);
				}
				break;

			case PowerUpTypes.MassSlow:
				if (Network.isServer)
					networkView.RPC ("SlowAllPlayersExcept", RPCMode.All, playerScript.playerIdAndName.playerId);
				break;

			case PowerUpTypes.SlowFastest:
				if (Network.isServer)
					networkView.RPC ("SlowFastestPlayerExcluding", RPCMode.All, playerScript.playerIdAndName.playerId);
				break;			
			}
		}
	}

	// Spawns a bomb behind the player
	[RPC]
	void SyncDestroy(string playerId) {
		PlayerController playerScript = null;
		
		foreach (PlayerController player in GameController.playerInstances) {   
			if (player.playerIdAndName.playerId.Equals(playerId)) {
				playerScript = player;
				break;
			}
		}

		AudioSource.PlayClipAtPoint (audio.clip, playerScript.transform.position);
		Destroy (gameObject);
	}

	// activate shield for network players
	[RPC]
	void ActivateShieldForNetworkPlayers(string playerId) {
		foreach (PlayerController player in GameController.playerInstances) {   
			if (player.playerIdAndName.playerId.Equals(playerId)) {
				player.ActivateShield(duration);
			}
		}

		AudioSource.PlayClipAtPoint (audio.clip, transform.position);
		Destroy (gameObject);
	}
	
	// Slow all players except this guy
	[RPC]
	void SlowAllPlayersExcept(string playerId) {
		foreach (PlayerController player in GameController.playerInstances) {			
			if (player.isShielded <= 0 && !player.playerIdAndName.playerId.Equals(playerId)) {
				player.ActivateSlow(duration);
			}
		}

		AudioSource.PlayClipAtPoint (audio.clip, transform.position);
		Destroy (gameObject);
	}

	// Slow the fastest dude (changed to always slow fastest player)
	[RPC]
	void SlowFastestPlayerExcluding(string playerId) {

		PlayerController fastestPlayer = GameController.playerInstances[0];
		
		// Find fastest player
		foreach (PlayerController player in GameController.playerInstances) {			
			if (fastestPlayer.transform.position.x < player.transform.position.x) {
				// && player.playerIdAndName.playerId != playerId (No longer excluding yourself)
				fastestPlayer = player;
			}
		}

		// Slow him! But only if he's not shielded...
		if (fastestPlayer.isShielded <= 0) {
			fastestPlayer.ActivateSlow(duration);
		}

		AudioSource.PlayClipAtPoint (audio.clip, transform.position);
		Destroy (gameObject);
	}	
}
