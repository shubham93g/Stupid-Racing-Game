using UnityEngine;
using System.Collections;

public class OnEnterFinishFlag : MonoBehaviour {

	void OnTriggerEnter2D(Collider2D other) {
		if (other.gameObject.tag.Equals("Player")) {
			((PlayerController) other.gameObject.GetComponent(typeof(PlayerController))).finishedRaceSync();
		}
	}
}