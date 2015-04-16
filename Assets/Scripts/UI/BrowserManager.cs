using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class BrowserManager : MonoBehaviour {

	// The UI prefab to be instantiated for each "room"
	public GameObject RoomRowPrefab;

	// The gameObject under which all the "room rows" will be
	public GameObject content;

	List<GameObject> roomRows = new List<GameObject>();
	HostData[] hostList;

	// http://answers.unity3d.com/questions/750448/unity-masterserver-host-list-empty.html
	/*
	 * If anyone was wondering I figured out that I was misunderstanding the use of PollHostList().
	 * Based on the examples given on the documentation page, I thought you only did RequestHostList() once and did PollHostList() on each subsequent frame.
	 * 
	 * RequestHostList() empties the stored HostData array until the OnMasterServerMessage callback triggers with a HostListReceived event.
	 * Within that time-frame, PollHostList() will always return a blank array.
	 * So in order to solve my problem, I made sure to only do m_HostList = MasterServer.PollHostList() AFTER I received the HostListReceived callback.
	 * 
	 */

	// Use this for initialization
	void Start () {
		MasterServer.ClearHostList();
		MasterServer.RequestHostList(NetworkManager.TypeName);
	}
	
	void OnEnable() {
		InvokeRepeating ("RefreshHostList", 0, 1);
	}

	void OnDisable() {
		CancelInvoke(); // stop the invoke repeating
	}

	// Updates the host list and displays it on the browser
	void RefreshHostList() {
		MasterServer.RequestHostList(NetworkManager.TypeName);
	}

	// Do when we receive the host list
	void OnMasterServerEvent(MasterServerEvent msEvent) {

		if (msEvent == MasterServerEvent.HostListReceived) {
			hostList = MasterServer.PollHostList();

			// Get rid of all the old room rows
			foreach (GameObject go in roomRows) {
				Destroy(go);
			}

			// Clear our list
			roomRows.Clear ();

			// Create UI to allow player to join a room
			if (hostList != null) {
				
				// Construct list of rooms
				for (int i = 0; i < hostList.Length; i++) {
					// Create the new room row
					GameObject room = (GameObject) Instantiate (RoomRowPrefab);
					room.transform.SetParent (content.transform, false);
					
					// Set text to the appropriate room name
					Text roomName = room.GetComponentsInChildren<Text> ()[0];
					roomName.text = hostList [i].gameName + "'s Game";
					
					// Set appropriate room status
					Text roomStatus = room.GetComponentsInChildren<Text> ()[1];
					if (hostList[i].connectedPlayers < NetworkManager.MaxConnections)
						roomStatus.text = hostList [i].connectedPlayers + "/4";
					else {
						roomStatus.text = "Full";
						room.GetComponentInChildren<Button>().interactable = false;
					}

					// Check if started, if started, also disable
					if (hostList[i].comment.Equals(NetworkManager.GameClosed)) {
						roomStatus.text = "Run";
						room.GetComponentInChildren<Button>().interactable = false;
					}
					
					// Passing the host data to the script so we know who to connect to
					room.GetComponentInChildren<OnJoinRoomClicked> ().hostData = hostList [i];
					
					roomRows.Add (room);
				}
			} else {
				// No host data? Clear out the browser of room rows
				foreach (GameObject go in roomRows) {
					Destroy(go);
				}
				
				// Clear our list
				roomRows.Clear ();
			}
		}
	}
}
