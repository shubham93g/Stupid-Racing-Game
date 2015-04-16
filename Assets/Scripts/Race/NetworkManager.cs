using UnityEngine;
using System.Collections;

public class NetworkManager : MonoBehaviour {

	// This class is currently only used for its constants

	public const string TypeName = "720NoScopeGGNoRe.StupidRacingGame"; //unique game name
	public const string Password = "comeoneileen";
	
	public const int MaxConnections = 4;
	public const int Port = 62000;
	
	public const string GameOpen = "Open";
	public const string GameClosed = "Closed";

	public static NetworkView NMNetworkView;

	void Start() {
		NetworkManager.NMNetworkView = networkView;
	}
	
	// Disconnects and unregisters from the MasterServer if it is a host
	[RPC]
	public void Disconnect() {			
		Network.Disconnect ();
		MasterServer.UnregisterHost(); // Does nothing if the server is not registered or has already unregistered.
	}
	
	// Called when hosting (from clicking "Start Host")
	public static void createRoom(string roomName, string comment) {

		Network.incomingPassword = NetworkManager.Password;
		Network.InitializeServer(NetworkManager.MaxConnections, NetworkManager.Port, !Network.HavePublicAddress());
			
		MasterServer.RegisterHost(NetworkManager.TypeName, roomName, comment);
	}

	void OnPlayerConnected(NetworkPlayer player) {
		Debug.Log("Player connected from " + player.ipAddress + ":" + player.port);
	}
	
	// Disconnection from the server is handled in GameController
	void OnPlayerDisconnected(NetworkPlayer player) {
		Debug.Log("Cleaning up after player " + player);

		PlayerController toBeRemoved = null;

		foreach (PlayerController playerInstance in GameController.playerInstances)
			if (playerInstance.playerIdAndName.playerId.Equals(player.ToString()))
				toBeRemoved = playerInstance;

		if (toBeRemoved != null)
			GameController.playerInstances.Remove (toBeRemoved);
		
		if (!Application.loadedLevelName.Equals ("Main Menu")) {			
			Network.RemoveRPCs(player);
			Network.DestroyPlayerObjects(player);
		}
	}
	
	// Go back to main menu
	void OnDisconnectedFromServer(NetworkDisconnection info) {
		if (info == NetworkDisconnection.LostConnection)
			Debug.Log("Lost connection to the server");
		else
			Debug.Log("Successfully disconnected from the server");

		if (!Application.loadedLevelName.Equals("Main Menu"))
			Application.LoadLevel ("Main Menu");
	}

	// Should display something but...
	void OnFailedToConnect(NetworkConnectionError error) {
		Debug.Log("Could not connect to server: " + error);
	}
}
