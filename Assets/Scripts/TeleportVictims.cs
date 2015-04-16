using UnityEngine;
using System.Collections;

public class TeleportVictims : MonoBehaviour {
	
	GameObject startLocation;

	// Use this for initialization
	void Start () {
		startLocation = GameObject.FindWithTag ("Start");
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerEnter2D(Collider2D other) {

		GameObject otherObject = other.gameObject;

		//if (otherObject.tag == "Player") {
			// this will pause the game, I plan to do some teleportation animation before resuming the game
			//Time.timeScale = 0.0f;
			otherObject.transform.position = new Vector2 (startLocation.transform.position.x, otherObject.transform.position.y);		
		//}
	}
}
