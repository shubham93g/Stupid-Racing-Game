using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour {
	
	/* *
	 * // How to determine drag based on desired terminal velocity and acceleration
	 * // http://forum.unity3d.com/threads/terminal-velocity.34667/
	 * 
	 * // Manual
	 * var drag = (acceleration / topSpeed);
	 * velocity += Vector3.right * acceleration * Time.fixedDeltaTime;
	 * velocity -= drag * velocity * Time.fixedDeltaTime;
	 * 
	 * // PhysX - I'm using this right now
	 * rigidbody.drag = (acceleration / topSpeed);
	 * rigidbody.AddForce(Vector3.right * acceleration);
	 * 
	 * */
	
	// Note: networkView.owner refers to whoever instantiated the prefab
	// however, clients only know about the server, to each client,
	// networkView.owner will be seen as the server even though it might have
	// been instantiated on another client -.-
	
	public static List<Color> PlayerColor = new List<Color> ();

	// Populate colors
	static PlayerController() {
		PlayerColor.Add(Color.green);
		PlayerColor.Add(Color.red);
		PlayerColor.Add(Color.blue);
		PlayerColor.Add(Color.yellow);
	}
	
	[Tooltip("Constant acceleration applied to player.")]
	public float defaultAcceleration;
	public float workingAcceleration;

	[Tooltip("Maximum Speed the player can reach.")]
	public float terminalVelocity;

	[Tooltip("Force to apply to player when jumping up!")]
	public float launchForce;
	
	[Tooltip("Multiplies with acceleration when jumping (use value < 1 to slow down player in air)")]
	public float jumpAccelerationMultiplier;
	
	public bool raceFinished = false;

	public LobbyManager.PlayerIdAndName playerIdAndName = new LobbyManager.PlayerIdAndName("Default", "Default");

	// powerup booleans and variables
	public float isSlowed = 0.0f, isShielded = 0.0f, isKnockbacked = 0.0f;
	public bool isBuffed = false;

	public GameObject shieldGraphic;
	public GameObject slowTimeGraphic;

	private GameObject activeShieldGraphic;
	private GameObject activeSlowTimeGraphic;

	private bool canJump = false;
	
	private const float angleUpperBound = 30;
	private const float angleLowerBound = 330;

	// For state synchronization
	private const float SyncPeriod = 1.0f; // in seconds
	private float syncTime = 0.0f;
	private float previousSyncTime = 0.0f;

	// For lag compensation
	private double transitTime;

	// Used in jump checks
	public Transform frontBottom;
	public Transform centerBottom;
	public Transform backBottom;

	// Used for player physics
	private float verticalForceModifier;

	private BoxCollider2D boxCollider;

	private float floorDrag;

	// Used to update the animator variables which control playback speed
	Animator animator;

	// Use this for initialization
	void Start () {
		raceFinished = false;

		GameController.playerInstances.Add (this);
		
		// Attach camera to player
		if (networkView.isMine) {
			CameraController.player = gameObject;
		}

		workingAcceleration = defaultAcceleration;

		animator = GetComponent<Animator> ();

		// Physics
		boxCollider = GetComponent<BoxCollider2D>();

		floorDrag = defaultAcceleration / terminalVelocity;
		rigidbody2D.drag = floorDrag;
		rigidbody2D.centerOfMass = new Vector2(0, -boxCollider.bounds.extents.y);

		verticalForceModifier = -Physics2D.gravity.y / defaultAcceleration;

		// Things that require sync
		if (networkView.isMine) {			
			playerIdAndName.playerId = Network.player.ToString();
			playerIdAndName.playerName = PlayerPrefs.GetString("Player Name");
			
			// Floating text
			GetComponentInChildren<TextMesh> ().text = playerIdAndName.playerName;

			// Appearance
			renderer.material.SetColor ("_Color", PlayerColor[System.Convert.ToInt32 (playerIdAndName.playerId)]);

			networkView.RPC ("SynchronizeOverNetwork", RPCMode.Others, playerIdAndName.playerId, playerIdAndName.playerName);
			
			Debug.Log ("M Player Name: " + playerIdAndName.playerName + " id: " + playerIdAndName.playerId);
		}
	}

	// Tell everyone the real name and id for this dude
	[RPC]
	void SynchronizeOverNetwork(string realId, string realName) {

		playerIdAndName.playerId = realId;
		playerIdAndName.playerName = realName;

		// Floating text
		GetComponentInChildren<TextMesh> ().text = playerIdAndName.playerName;

		// Appearance
		renderer.material.SetColor ("_Color", PlayerColor[System.Convert.ToInt32 (playerIdAndName.playerId)]);		
		
		Debug.Log ("O Player Name: " + playerIdAndName.playerName + " id: " + playerIdAndName.playerId);
	}

	// Customize State Synchronization
	void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info) {
		// Must be in Vector3 for stream.Serialize though we are only working in 2D
		Vector3 syncVelocity = Vector3.zero;
		Vector3 syncPosition = Vector3.zero;
		bool notInAir = false;
		
		// Properties to synchronize to other clients
		if (stream.isWriting) {
			syncPosition = rigidbody2D.position;
			stream.Serialize(ref syncPosition);
			
			syncVelocity = rigidbody2D.velocity;
			stream.Serialize(ref syncVelocity);

			notInAir = canJump;
			stream.Serialize(ref notInAir);
			
		// Read properties to be synchronized
		} else {			
			stream.Serialize(ref syncPosition);
			stream.Serialize(ref syncVelocity);
			stream.Serialize(ref notInAir);

			// I want to sync everytime the player leaves the ground
			// canJump is the "previous" value, notInAir is the "new" value
			if (canJump && !notInAir)
				syncTime = SyncPeriod + 1; // will sync now

			canJump = notInAir; // set it for animation

			// This is how old the information (position/velocity) is
			transitTime = Network.time - info.timestamp;

			// Accumulated time since last position synchronization
			syncTime += (Time.time - previousSyncTime);		
			previousSyncTime = Time.time;

			rigidbody2D.velocity = syncVelocity;

			// Syncs the position every "SyncPeriod" seconds
			if (syncTime > SyncPeriod) {
				// Extrapolate position from transitTime
				Vector2 syncPosition2D = syncPosition;
				transform.position = syncPosition2D + (((float) transitTime) * rigidbody2D.velocity);				

				// Reset accumulator
				syncTime = 0.0f;
			}
		}
	}

	// Clamps rotation to between 2 magic values
	float ClampAngle(float angle) {
		if (angle > angleUpperBound && angle < 180)
			return angleUpperBound;
		else if (angle < angleLowerBound && angle > 180)
			return angleLowerBound;
		else
			return angle;
	}

	// Casts ray to 4 specific places (assigned in prefab) to check if an environment block is underneath the player
	void JumpCheck() {

		canJump = false;

		Vector2 position = transform.position;
		RaycastHit2D [] jumpCheckRays = {
			Physics2D.Linecast (position, frontBottom.position, LayerMask.GetMask("Level")),
			Physics2D.Linecast (position, centerBottom.position, LayerMask.GetMask("Level")),
			Physics2D.Linecast (position, backBottom.position, LayerMask.GetMask("Level"))
		};

		foreach (RaycastHit2D scratch in jumpCheckRays) {
			canJump = canJump || scratch;
		}

		// Special check for player (ignore's collider that we're starting in)
		Collider2D lineCastCollider = Physics2D.Linecast (position, centerBottom.position, LayerMask.GetMask ("Player")).collider;
		canJump = canJump || (lineCastCollider != null && lineCastCollider != boxCollider); // make sure it's not my own

		// Also clamp angles
		transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, ClampAngle(transform.eulerAngles.z));
	}

	// Called every fixed framerate frame
	void FixedUpdate() {
		if (!raceFinished) {
			if (GameController.raceStarted) {
			
				JumpCheck ();

				// Only send input commands to my own runner
				if (networkView.isMine) {
					float yAxis = Input.GetAxisRaw ("Vertical");

					if (canJump) {
			
						// PC
						if (yAxis > 0.0f) {
							Jump ();

							// Android
						} else {		
							foreach (Touch touch in Input.touches) {
								if (touch.phase == TouchPhase.Began) {
									Jump ();
								}
							}
						}
					}
				}		
		
				// Control animation
				animator.SetBool ("In Air", !canJump);

				// Go go! Accelerate my runner right-ward, drag controls max speed
				rigidbody2D.AddForce (new Vector2 (
					// Accelerate 3 times slower when in the air
					canJump ? 1 : jumpAccelerationMultiplier,
					// Add vertical force depending on angle if I'm on floor (to help with climbing slopes)
					canJump ? verticalForceModifier * Mathf.Sin (Mathf.Deg2Rad * transform.eulerAngles.z) : 0
					) * workingAcceleration);

				// Modifiers based on power-up effects
				handlePowerUps();
			}
		}
		
		// The 2 is just an experimental constant... whatever looks nice!
		animator.speed = (rigidbody2D.velocity.x / terminalVelocity) * 2;
	}

	void handlePowerUps() {

		// Decrement Timer
		if (isSlowed > 0) {
			workingAcceleration = defaultAcceleration/3; // slow
			isSlowed -= Time.deltaTime;
		} else {
			isSlowed = 0;		
			DestroySlowTimeGraphic();
		}

		// Decrement Timer
		if (isShielded > 0) {
			isShielded -= Time.deltaTime;
		} else {
			isShielded = 0;
			DestroyShieldGraphic();
		}

		// Decrement Timer
		if (isKnockbacked > 0) {
			isKnockbacked -= Time.deltaTime;
		} else {			
			isKnockbacked = 0;
		}

		if(!isBuffed) {
			workingAcceleration = defaultAcceleration; // set back to original
		}

		// Remove buff when slow/knockback expires
		if (isSlowed <= 0 && isKnockbacked <= 0) {
			isBuffed = false;
		}
	}

	// Show slow graphic and start the duration
	public void ActivateSlow(float duration) {
		if (isSlowed <= 0) {
			activeSlowTimeGraphic = (GameObject) Instantiate (slowTimeGraphic, transform.position, Quaternion.identity);
			activeSlowTimeGraphic.transform.SetParent (transform);
			activeSlowTimeGraphic.transform.position = new Vector3(
				activeSlowTimeGraphic.transform.position.x,
				activeSlowTimeGraphic.transform.position.y + renderer.bounds.extents.y + activeSlowTimeGraphic.renderer.bounds.extents.y
				);

			isBuffed = true;
		}
		
		isSlowed = duration;
	}

	// Show shield graphic and start the duration
	public void ActivateShield(float duration) {
		if (isShielded <= 0) {
			activeShieldGraphic = (GameObject) Instantiate (shieldGraphic, transform.position, Quaternion.identity);
			activeShieldGraphic.transform.SetParent (transform);
		}
		
		isShielded = duration;
	}

	// Destroy the shield graphic
	void DestroySlowTimeGraphic() {
		Destroy (activeSlowTimeGraphic);
	}
	
	// Destroy the shield graphic
	void DestroyShieldGraphic() {
		Destroy (activeShieldGraphic);
	}
	
	// Authoritative Style
	public void finishedRaceSync() {
		if (!raceFinished && Network.isServer) { // Only call on server
			networkView.RPC ("finishedRace", RPCMode.All);
		}
	}

	// Called on everyone by the server to end the player's run
	[RPC]
	public void finishedRace() {
		if (!raceFinished) {
			raceFinished = true;
			((GameController) GameObject.FindGameObjectWithTag("GameController").GetComponent(typeof(GameController))).finishedRace(playerIdAndName);
		}
	}

	// Makes the player jump based on launch velocity;
	void Jump() {
		rigidbody2D.AddForce (Vector2.up * launchForce);
		transform.position = new Vector2(transform.position.x, transform.position.y + 0.5f); // counteract inertia (else will jump multiple times)
		canJump = false;	
	}
}