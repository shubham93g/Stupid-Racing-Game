using UnityEngine;
using System.Collections;

public class Scintillation : MonoBehaviour {
	
	SpriteRenderer spriteRenderer;

	// Use this for initialization
	void Start () {
		spriteRenderer = GetComponent<SpriteRenderer> ();
	}
	
	void FixedUpdate () {

		float red, green, blue, alpha;

		// R, G, B values are between 0 and 1, so we normalize
		// This is the normalization (0 to 1) formula for X: (x - xMin)/(xMax - xMin) : (Mathf.Sin (Time.time) + 1) / 2
		// Normalize alpha between 0.5 and 1 : ((Mathf.Sin (Time.time) + 1) / 2)/2 + 0.5f
		red = ((Mathf.Sin (Time.time) + 1) / 2)/2 + 0.5f; 
		green = ((Mathf.Cos (Time.time) + 1) / 2)/2 + 0.5f; 
		blue = ((Mathf.Cos (Time.time) + 1) / 2)/2 + 0.5f; 
		alpha = ((Mathf.Sin (Time.time) + 1) / 2)/2 + 0.5f; 

		spriteRenderer.color = new Color (red, green, blue, alpha);
	}
}
