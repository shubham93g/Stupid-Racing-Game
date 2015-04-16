using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

	public static GameObject player;
	public static float offset = 0;
	
	//public float dampTime = 0.2f;
	//private Vector3 velocity = Vector3.zero;

	void Start() {
		offset = (camera.ScreenToWorldPoint(new Vector3(camera.pixelWidth, 0, 0)).x)/10;
	}

	// Called after update
	void LateUpdate() {
		if (player != null) {
			transform.position = new Vector3 (player.transform.position.x + offset, player.transform.position.y, transform.position.z);

			/* Smooth camera code, made player look jittery - not using anymore */
			//Vector3 destination = new Vector3(player.transform.position.x, player.transform.position.y, transform.position.z);

			//transform.position = Vector3.SmoothDamp (transform.position, destination, ref velocity, dampTime);
		}
	}
}
