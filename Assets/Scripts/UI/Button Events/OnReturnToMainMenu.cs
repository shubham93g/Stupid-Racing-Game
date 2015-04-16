using UnityEngine;
using System.Collections;

public class OnReturnToMainMenu : MonoBehaviour {
	
	[Tooltip("The button onClick sound to play")]
	public AudioSource mainAudioSource; // Used to play button click sounds
		
	// Network Manager will send me back to main menu
	public void ReturnToMainMenu () {
		mainAudioSource.PlayOneShot (mainAudioSource.clip);
		Network.Disconnect ();
		MasterServer.UnregisterHost(); // Does nothing if the server is not registered or has already unregistered.
	}
}
