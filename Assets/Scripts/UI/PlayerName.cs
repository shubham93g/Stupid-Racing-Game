using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerName : MonoBehaviour {

	// Refer to player name from here, No_Name is the default, it loads the stored name on Start();
	public static string playerName = "No_Name";
	public Text initialTextName;
	public Text textName;

	// Use this for initialization
	void Start () {

		string defaultName = SystemInfo.deviceName;
		defaultName = (defaultName.Length < 3) ? "Default" + defaultName : defaultName; // Force 3 or more characters
		defaultName = (defaultName.Length > 10) ? defaultName.Substring(0, 10) : defaultName; // Force less than 10 characters
		
		playerName = (PlayerPrefs.GetString ("Player Name").Length) < 3 ? defaultName: PlayerPrefs.GetString ("Player Name");

		// Gets the stored player name. Sets it to deviceName if none exists, store.
		initialTextName.text = playerName;
		textName.text = playerName;
		PlayerPrefs.SetString ("Player Name", playerName);
		PlayerPrefs.Save ();
	}

	// Saves the player name as a PlayerPref
	public void SavePlayerName() {
		playerName = textName.text;
		playerName = (playerName.Length < 3) ? "Default!" + playerName : playerName; // Force 3 or more characters
		PlayerPrefs.SetString ("Player Name", playerName);
		PlayerPrefs.Save ();
	}
}