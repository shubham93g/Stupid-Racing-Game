using UnityEngine;
using System.Collections;

public class OnClickedPanelChanger : MonoBehaviour {
	
	[Tooltip("The button onClick sound to play")]
	public AudioSource mainAudioSource;
	
	[Tooltip("The currently active panel")]
	public GameObject currentPanel;
	
	[Tooltip("The panel to switch to")]
	public GameObject newPanel;
	
	public void SwitchToNewPanel () {
		
		mainAudioSource.PlayOneShot(mainAudioSource.clip);
		
		currentPanel.SetActive (false); // disable current panel (title screen)
		newPanel.SetActive (true); // enable the new panel to show (lobby)
	}
}