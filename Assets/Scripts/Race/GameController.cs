using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GameController : MonoBehaviour {
		
	[Tooltip("The player prefab to instantiate")]
	public GameObject playerPrefab;

	[Tooltip("The button onClick sound to play")]
	public AudioSource buttonClickSound;
	
	[Tooltip("The Win panel to display")]
	public GameObject gameEndPanel;
	
	public GameObject countdownTextPrefab;

	public AudioSource countdownThreeAnnouncement;
	public AudioSource countdownTwoAnnouncement;
	public AudioSource countdownOneAnnouncement;
	public AudioSource raceStartAnnouncement;
	
	public AudioSource firstPlaceAnnouncement;
	public AudioSource secondPlaceAnnouncement;
	public AudioSource thirdPlaceAnnouncement;
	public AudioSource fourthPlaceeAnnouncement;

	public AudioSource [] music;

	public static bool raceStarted = false;
	public static bool raceEnded = false;

	public static float timeElapsed = 0.0f;
	public static float startTime = 0.0f;
	public static float countDownStartTime = 0.0f;

	public static LobbyManager.PlayerIdAndName firstPlace = null;
	public static LobbyManager.PlayerIdAndName secondPlace = null;
	public static LobbyManager.PlayerIdAndName thirdPlace = null;
	public static LobbyManager.PlayerIdAndName fourthPlace = null;

	public static List<PlayerController> playerInstances = new List<PlayerController>();

	private bool countdownStarted = false;
	private bool two = false;
	private bool one = false;
	
	// Use this for initialization
	void Start() {
		playerInstances.Clear ();

		firstPlace = null;
		secondPlace = null;
		thirdPlace = null;
		fourthPlace = null;

		timeElapsed = 0.0f;
		startTime = 0.0f;
		countDownStartTime = 0.0f;

		AudioSource musicToPlay = music [Random.Range (0, music.Length)]; // random's max is EXCLUSIVE
		musicToPlay.Play ();

		raceStarted = false;
		raceEnded = false;

		SpawnPlayer();
	}

	// End the race with the appropriate positions
	public void finishedRace(LobbyManager.PlayerIdAndName playerIdAndName) {
		GameObject countdown = (GameObject) Instantiate (countdownTextPrefab, Vector3.zero, Quaternion.identity);
		
		// Shout winner's name store
		if (firstPlace == null) {
			firstPlace = playerIdAndName;
			firstPlaceAnnouncement.PlayOneShot(firstPlaceAnnouncement.clip);	
		} else if (secondPlace == null) {
			secondPlace = playerIdAndName;
			secondPlaceAnnouncement.PlayOneShot(secondPlaceAnnouncement.clip);	
		} else if (thirdPlace == null) {
			thirdPlace = playerIdAndName;
			thirdPlaceAnnouncement.PlayOneShot(thirdPlaceAnnouncement.clip);	
		} else if (fourthPlace == null) {
			fourthPlace = playerIdAndName;
			fourthPlaceeAnnouncement.PlayOneShot(fourthPlaceeAnnouncement.clip);	
		} else {
			Debug.Log ("Wtf? Error in GameController man."); // Error!
		}

		// Splash winner's name
		Text countdownText = countdown.GetComponentInChildren<Text> ();
		countdownText.text = playerIdAndName.playerName;
		countdownText.color = PlayerController.PlayerColor[System.Convert.ToInt32(playerIdAndName.playerId)];
		countdown.GetComponentInChildren<Animator> ().speed = 0.5f;

		bool gameOver = true;

		foreach (PlayerController pc in playerInstances)
			if (!pc.raceFinished)
				gameOver = false;

		// Show exit to menu overlay if game is over
		if (gameOver) {
			raceEnded = true;
			gameEndPanel.SetActive (true);
			gameEndPanel.GetComponentInChildren<Text> ().text = firstPlace.playerName + " wins!";
			List<string> names = new List<string>();
			if(firstPlace!=null){
				names.Add(firstPlace.playerName);
				if(Network.isServer)
					StartCoroutine(submitScores(firstPlace.playerName,10));
			}
			if(secondPlace!=null){
				names.Add (secondPlace.playerName);
				if(Network.isServer)
					StartCoroutine(submitScores(secondPlace.playerName,8));
				
			}
			if(thirdPlace!=null){
				names.Add (thirdPlace.playerName);
				if(Network.isServer)
					StartCoroutine(submitScores(thirdPlace.playerName,5));
				
			}
			if(fourthPlace!=null){
				names.Add (fourthPlace.playerName);
				if(Network.isServer)
					StartCoroutine(submitScores(fourthPlace.playerName,2));
			}
			
			StartCoroutine(getScores (names));
		}
	}
	
	public IEnumerator submitScores(string name, int score){
		WWW submit = new WWW("http://52.74.115.74/dbconn.php?action=submit&player="+name+"&score="+score.ToString());
		yield return submit;
		Debug.Log (submit.text);
	}
	
	public IEnumerator getScores(List<string> names)
	{
		//WWWForm form = new WWWForm();
		//form.AddField("player","download1");
		
		//to ensure other coroutines finish
		yield return new WaitForSeconds (2);
		
		string text = "Player\tScore\tWins\n";
		foreach(string name in names){
			if(name != null){
				WWW www = new WWW("http://52.74.115.74/dbconn.php?action=show&player="+name);
				yield return www;
				text +=www.text;
			}
		}
		
		int rowCount = 0;
		
		foreach (string row in text.Split("\n"[0])) {
			string[] words = row.Split ("\t"[0]);
			
			for(int i=0;i<words.Length;i++){
				GameObject tag = GameObject.Instantiate(Resources.Load("Score Tag")) as GameObject;
				tag.GetComponent<Text>().text = words[i];
				tag.transform.SetParent(GameObject.FindGameObjectWithTag("CanvasOverlay").transform);
				Vector2 pos = new Vector2(Screen.width*(i*2+2)/8,Screen.height*(7-rowCount)/8);
				tag.transform.position = pos;
			}
			rowCount++;                         
		}
	}
	
	private void SpawnPlayer() {
		Vector3 spawnPosition = new Vector3 (
			transform.position.x - System.Convert.ToInt32(Network.player.ToString()) * playerPrefab.renderer.bounds.size.x * 1.5f,
		    transform.position.y,
		    transform.position.z);

		Network.Instantiate (playerPrefab, spawnPosition, Quaternion.identity, 0);

		Debug.Log ("Network Connections: " + Network.connections.Length);
	}
	
	// Update is called once per frame
	void Update () {
		if (raceStarted) {
			timeElapsed = Time.time - startTime; // Maybe I'll use this for a timer display? See how much time we have to implement...
		} else if (!raceEnded) {
			if (countdownStarted) {
				timeElapsed = Time.time - countDownStartTime;

				if (timeElapsed > 1 && !two) {
					// Two
					GameObject scratch = (GameObject) Instantiate (countdownTextPrefab, Vector3.zero, Quaternion.identity);
					scratch.GetComponentInChildren<Text>().text = "2...";

					countdownTwoAnnouncement.PlayOneShot(countdownTwoAnnouncement.clip);
					two = true;
				} else if (timeElapsed > 2 && !one) {
					// One
					GameObject scratch = (GameObject) Instantiate (countdownTextPrefab, Vector3.zero, Quaternion.identity);
					scratch.GetComponentInChildren<Text>().text = "1...";
					countdownOneAnnouncement.PlayOneShot(countdownOneAnnouncement.clip);

					one = true;
				} else if (timeElapsed > 3) {
					// Go
					GameObject scratch = (GameObject) Instantiate (countdownTextPrefab, Vector3.zero, Quaternion.identity);
					scratch.GetComponentInChildren<Text>().text = "Go!!!";

					raceStartAnnouncement.PlayOneShot(raceStartAnnouncement.clip);
					raceStarted = true;
				}

			} else if (Network.isServer)
				if (GameObject.FindGameObjectsWithTag ("Player").Length >= (Network.connections.Length + 1)) // connections are edges in the tree of coonnected computers
					networkView.RPC ("StartRace", RPCMode.All);
		}
	}

	// Start the race
	[RPC]
	void StartRace() {
		countdownStarted = true;
		countDownStartTime = Time.time;

		GameObject scratch = (GameObject) Instantiate (countdownTextPrefab, Vector3.zero, Quaternion.identity);
		scratch.GetComponentInChildren<Text>().text = "3...";

		countdownThreeAnnouncement.PlayOneShot(countdownThreeAnnouncement.clip);
	}
}