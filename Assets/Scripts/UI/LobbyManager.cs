using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class LobbyManager : MonoBehaviour {

	// Oh my god! Remember this shit - the types that RPC can take
	// int, float, string, NetworkPlayer, NetworkViewID, Vector3, Quaternion

	[Tooltip("The text to display for each player slot (the players' names)")]
	public Text[] playerSlot;

	[Tooltip("The button that starts the game")]
	public GameObject startGameButton;

	[Tooltip("The currently active panel")]
	public GameObject currentPanel;

	[Tooltip("The panel to switch to")]
	public GameObject newPanel;

	[Tooltip("The scene/level to load, put 0 for random")]
	public int level = 0;

	// Used only by the host to update everyone (including himself) on the room
	public static List<PlayerIdAndName> playerIdNameList = new List<PlayerIdAndName>();

	public class PlayerIdAndName {

		public string playerName;
		public string playerId;

		public PlayerIdAndName(string playerName, string playerId) {
			this.playerName = playerName;
			this.playerId = playerId;
		}
	}

	private string hostName;

	// Initialization / Activation
	void OnEnable() {
		playerIdNameList.Clear ();
		
		// Reset all to "Waiting..."
		foreach (Text txt in playerSlot) {
			txt.color = Color.red;
			txt.text = "Waiting...";
		}
	}

	// Set back to disabled (or else the start host button will appear next time)
	void OnDisable() {
		startGameButton.SetActive (false);
		CancelInvoke(); // stop the invoke repeating
	}
	
	// Switch to the game scene
	public void StartGameSync() {
		
		string stringLevel;

		if (level <= 0)
			stringLevel = "Level " + (int)Random.Range (1, 4);
		else
			stringLevel = "Level " + level;

		if (Network.isServer) {
			// Convert to closed game
			Network.maxConnections = -1;			
			MasterServer.RegisterHost(NetworkManager.TypeName, hostName, NetworkManager.GameClosed);

			networkView.RPC ("StartGame", RPCMode.Others, stringLevel);
		}
	
		Application.LoadLevel (stringLevel);
		
	}

	// Switch to the game scene
	[RPC]
	public void StartGame(string level) {
		Application.LoadLevel (level);
	}
	
	// Called when hosting (from clicking "Start Host")
	public void Host() {

		startGameButton.SetActive (true);

		hostName = PlayerPrefs.GetString ("Player Name");
		NetworkManager.createRoom (hostName, NetworkManager.GameOpen);

		playerSlot [0].text = hostName;
		playerSlot [0].color = Color.black;

		playerIdNameList.Add (new PlayerIdAndName(hostName, Network.player.ToString()));
	}
	
	// Disconnects and unregisters from the MasterServer if it is a host
	[RPC]
	public void DisconnectIndividual() {			
		Network.Disconnect ();
		MasterServer.UnregisterHost(); // Does nothing if the server is not registered or has already unregistered.
	}

	[RPC]
	public void DisconnectFromServer() {
		if (Network.isServer) {
			NetworkManager.NMNetworkView.RPC("Disconnect", RPCMode.AllBuffered);
		} else {
			networkView.RPC ("UpdateAllClientsPlayers", RPCMode.Server, PlayerPrefs.GetString ("Player Name"), Network.player, false);
		}
	}

	// Called on the client when it connects to the server
	void OnConnectedToServer() {
		if (Network.isServer) {
			if (Network.connections.Length == Network.maxConnections) {				
				// Should someone close connection and reopen when possible, code below doesn't work properly
				//Network.maxConnections = -1; // supposed to limit to current max connections but doesn't work (?)
				//NetworkManager.createRoom (hostName, NetworkManager.GameClosed);
			}
		}

		// Tell the server to update the list of player names and the tell everyone else to update their player names
		networkView.RPC ("UpdateAllClientsPlayers", RPCMode.Server, PlayerPrefs.GetString ("Player Name"), Network.player , true); 
	}

	// Called on both client and server when they disconnect
	void OnDisconnectedFromServer() {
		currentPanel.SetActive (false); // disable current panel (title screen)
		newPanel.SetActive (true); // enable the new panel to show (lobby)
	}

	// Server calls this and tells all other clients to update their player slots
	// bool connecting is true when connecting, false when DISCONNECTING
	[RPC]
	void UpdateAllClientsPlayers(string newPlayerName, NetworkPlayer player, bool connecting) {

		if (connecting) {

			playerIdNameList.Add (new PlayerIdAndName (newPlayerName, player.ToString ()));

			// Let's just make things easier for me and waste some CPU cycles
			foreach (PlayerIdAndName scratch in playerIdNameList) {
				networkView.RPC ("UpdatePlayerIdAndName", RPCMode.Others, scratch.playerName, scratch.playerId, true);
			}
		} else {

			PlayerIdAndName toBeRemoved = null;

			// Update all
			foreach (PlayerIdAndName scratch in playerIdNameList) {
				if (scratch.playerName.Equals(newPlayerName) && scratch.playerId.Equals(player.ToString())) {
					networkView.RPC ("DisconnectIndividual", player);
					toBeRemoved = scratch;
					break;
				}
			}

			if (toBeRemoved != null)
				playerIdNameList.Remove(toBeRemoved);

			networkView.RPC ("UpdatePlayerIdAndName", RPCMode.Others, newPlayerName, player.ToString (), false);
		}

		for (int i = 0; i < playerSlot.Length; i++) {
			if (i < playerIdNameList.Count) {
				playerSlot [i].color = Color.black;
				playerSlot [i].text = playerIdNameList [i].playerName;

				networkView.RPC ("UpdatePlayerSlot", RPCMode.Others, i, playerIdNameList [i].playerName, true);

			} else {
				playerSlot [i].color = Color.red;
				playerSlot [i].text = "Waiting...";

				networkView.RPC ("UpdatePlayerSlot", RPCMode.Others, i, "Nobody!", false);
			}
		}
	}
	
	// Called on all clients by the server, telling them to update their player list
	[RPC]
	void UpdatePlayerIdAndName(string newPlayerName, string playerId, bool connecting) {
		
		if (connecting) {
			bool alreadyExists = false;

			// Check if already exists before putting in again
			foreach (PlayerIdAndName scratch in playerIdNameList) {
				if (scratch.playerName.Equals(newPlayerName) && scratch.playerId.Equals(playerId)) {
					alreadyExists = true;
					break;
				}
			}

			if (!alreadyExists)
				playerIdNameList.Add (new PlayerIdAndName(newPlayerName, playerId));

		} else {
			PlayerIdAndName toBeRemoved = null;
			foreach (PlayerIdAndName scratch in playerIdNameList) {
				if (scratch.playerName.Equals(newPlayerName) && scratch.playerId.Equals(playerId)) {
					toBeRemoved = scratch;
					break;
				}
			}
			
			if (toBeRemoved != null)
				playerIdNameList.Remove(toBeRemoved);
		}
	}

	// Called on all clients by the server, telling them to update their player slots
	[RPC]
	void UpdatePlayerSlot(int index, string newPlayerName, bool occupiedSlot) {

		if (occupiedSlot) {
			playerSlot [index].color = Color.black;
			playerSlot [index].text = newPlayerName;
		} else {	
			playerSlot [index].color = Color.red;
			playerSlot [index].text = "Waiting...";
		}
	}
}
