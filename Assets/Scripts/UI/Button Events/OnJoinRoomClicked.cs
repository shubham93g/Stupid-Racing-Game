using UnityEngine;
using System.Collections;

public class OnJoinRoomClicked : MonoBehaviour {

	// The host to connect to, this is set using a script elsewhere (must be public)!
	public HostData hostData = null;

	HostData[] hostList;

	// We check if it's OK to connect for realsies before we connect
	public void ConnectToHost () {
		if (hostData.connectedPlayers < NetworkManager.MaxConnections) {
			Network.Connect (hostData, NetworkManager.Password);
			SwitchToNewPanel ();
		}
	}
	
	public void SwitchToNewPanel () {
		GameInitialization.PrimaryButtonSound.PlayOneShot(GameInitialization.PrimaryButtonSound.clip);	
		GameInitialization.BrowseGamesPanel.SetActive (false); // disable current panel (BrowseGamesPanel)
		GameInitialization.HostedLobbyPanel.SetActive (true); // enable the new panel to show (HostedLobbyPanel)
	}
}
