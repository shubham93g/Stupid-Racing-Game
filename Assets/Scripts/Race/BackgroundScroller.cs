using UnityEngine;
using System.Collections;


public class BackgroundScroller : MonoBehaviour {

	public float speed;

	private float xPos;

	// Update is called once per frame
	void Update () {

		xPos = GameObject.FindWithTag ("MainCamera").GetComponent<CameraController>().transform.position.x; // only moves as camera moves

		//set background scroller to focus bosed on that position
		renderer.material.mainTextureOffset = new Vector2 ((xPos * speed) % 1, 0f);
	}
}
