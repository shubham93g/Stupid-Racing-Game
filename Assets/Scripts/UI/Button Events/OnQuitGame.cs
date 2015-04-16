using UnityEngine;
using System.Collections;

public class OnQuitGame : MonoBehaviour {
	
	[Tooltip("The button onClick sound to play")]
	public AudioSource mainAudioSource; // Used to play button click sounds
		
	public void Quit () {
		mainAudioSource.PlayOneShot (mainAudioSource.clip);
		Application.Quit ();
	}
}
