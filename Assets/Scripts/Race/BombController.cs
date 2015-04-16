using UnityEngine;
using System.Collections;

public class BombController : MonoBehaviour {
	
	public GameObject explosionPrefab;

	bool isActive = true;
	
	// Explode on contact with player
	void OnCollisionEnter2D(Collision2D other) {
		if (isActive) {
			// Only explode if it's the player of course
			if (other.gameObject.tag == "Player") {
				// Authoritative Style
				if (Network.isServer) {					
					isActive = false;
					networkView.RPC ("ExplodeBomb", RPCMode.All, other.gameObject.GetComponent<PlayerController> ().playerIdAndName.playerId);
				}
			}
		}
	}
	
	[RPC]
	void ExplodeBomb(string playerId) {
		PlayerController p = null;
		
		foreach (PlayerController scratch in GameController.playerInstances) {
			if (scratch.playerIdAndName.playerId.Equals (playerId)) {
				p = scratch;
				break;
			}
		}
		
		if (p.isShielded <= 0) {
			p.isKnockbacked = 0.5f;
			p.isBuffed = true;
			p.workingAcceleration = p.workingAcceleration * -3.5f;
			p.rigidbody2D.AddForce (Vector2.up * p.launchForce);
		}

		AudioSource.PlayClipAtPoint (audio.clip, transform.position);
		Instantiate(explosionPrefab, transform.position , Quaternion.identity);
		Destroy (gameObject);
	}
}