using UnityEngine;
using System.Collections;

// Used to give a static reference to editor-placed scene objects (my panels)
public class GameInitialization : MonoBehaviour {

	public static AudioSource PrimaryButtonSound;
	public static GameObject MainMenuPanel;
	public static GameObject HostedLobbyPanel;
	public static GameObject BrowseGamesPanel;

	public AudioSource primaryButtonSound;
	public GameObject mainMenuPanel;
	public GameObject hostedLobbyPanel;
	public GameObject browseGamesPanel;

	// Use this for initialization
	void Start () {
		// Disable screen dimming
		Screen.sleepTimeout = SleepTimeout.NeverSleep;

		PrimaryButtonSound = primaryButtonSound;
		MainMenuPanel = mainMenuPanel;
		HostedLobbyPanel = hostedLobbyPanel;
		BrowseGamesPanel = browseGamesPanel;
	}
}
