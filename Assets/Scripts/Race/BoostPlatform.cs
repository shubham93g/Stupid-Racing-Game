using UnityEngine;
using System.Collections;

public class BoostPlatform : MonoBehaviour {

	public float boostForce;

	void OnTriggerStay2D(Collider2D other) {

		GameObject playerMaybe = other.gameObject;

		if (playerMaybe.tag == "Player")
			playerMaybe.rigidbody2D.AddForce(Vector2.right * boostForce);
	}
}