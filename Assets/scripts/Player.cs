using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour {

	// whether mobile UI enabled
	public static bool onMobile = false;
	public bool MobileGame;

	public GameObject radio;
	private bool radioPlaying;
	public GameObject menu;
	public GameObject exitPrompt;
	private Text exitText;
	private bool exitFull = true;
	private bool canRestart;
	[Tooltip ("override with this material if provided")]
	public Material overrideMaterial;

	[HideInInspector]
	public Radio rdio;
	private PlayerPhysics phys;
	private PlayerDance dance;
	private PlayerScore scr;
	public GameObject speech;
	private SpeechBubble speak;

	[HideInInspector]
	public Transform poserT;

	public GameObject[] poses;
	[HideInInspector]
	public int pose = 0;
	private int prevPose = 0;

	private Vector3 startPos;
	private Vector3 startPosReal;
	[Tooltip ("y below which we are considered dead")]
	public float yFall = 12f;
	[HideInInspector]
	public float fell;

	// animation variables
	[HideInInspector]
	public float spawnTime = 1.5f;
	public float spawnFlashFreq = 5f;
	[HideInInspector]
	public float spawned;
	private int spawnTick;
	private bool Show = true;
	private bool prevShow = true;

	[HideInInspector]
	public int phase = 0;

	[Tooltip ("max interval between taps to register a double tap")]
	public float timeForTap = .5f;
	private float timeTap;
	[Tooltip ("Input button names to use")]
	public string[] buttons;
	[HideInInspector]
	public int direction;
	[HideInInspector]
	public bool directionTap;

	private bool[] btn;
	private bool[] btnPrev;

	private Transform hitstars;
	public int starsEmit;

	public string[] moveNames;
	public string[] moveNamesLv2 = { "Cool Slide", "Cartwheel", "Joy Slide", "Bang the floor",
		"Dancing Queen", "Capoeira Spin" };

	public float xLimit = 30f;

	[HideInInspector]
	public AudioSource[] audios;

	public Text menuHiScore;

	public bool DisableReceiveShadows = true;

	public TrainHorn train;
	public GameObject borderD;
	private Vector3 trainAnchor;

	public GameObject confetti;

	void Awake () {

		if (MobileGame || Application.isMobilePlatform) onMobile = true;
		//if (onMobile) Screen.sleepTimeout = SleepTimeout.NeverSleep;

		direction = -1;
		phys = GetComponent<PlayerPhysics> ();
		dance = GetComponent<PlayerDance> ();
		scr = GetComponent<PlayerScore> ();

		startPos = transform.position;
		startPosReal = startPos;
		startPos.y += 3f;

		rdio = radio.GetComponent<Radio> ();
		speak = speech.GetComponent<SpeechBubble> ();

		poserT = transform.Find ("ManPoser");

		btn = new bool[4];
		btnPrev = new bool[4];

		hitstars = transform.Find ("hitstars");

		audios = GetComponents<AudioSource> ();

		exitText = exitPrompt.transform.Find ("text").GetComponent<Text> ();

		SaveLoad.Load ();
		RefreshMenuHiscore ();

		if (DisableReceiveShadows || overrideMaterial)
			foreach (var mr in transform.GetComponentsInChildren<MeshRenderer> (true)) {
				if (DisableReceiveShadows) mr.receiveShadows = false;
				if (overrideMaterial) mr.sharedMaterial = overrideMaterial;
			}
	}

	void FixedUpdate () {

		// dynamically set/unset SleepTimeout depending on whether paused
		bool nosleep = Radio.playing && Time.timeScale > 0;
		if (onMobile && radioPlaying != nosleep) {
			radioPlaying = nosleep;
			if (radioPlaying) Screen.sleepTimeout = SleepTimeout.NeverSleep;
			else Screen.sleepTimeout = SleepTimeout.SystemSetting;
		}

		// skip if game paused
		if (Time.timeScale <= 0) return;

		// doubletap timer
		timeTap = Mathf.Max (0f, timeTap - Time.deltaTime);

		// control while ingame
		if (phase == 1) {
			// check for button presses/releases
			int hadrelease = 0;
			for (int i = 0; i < 4; i++) {
				btnPrev[i] = btn[i];
				btn[i] = Input.GetButton (buttons[i]) && fell <= 0f;
				if (!btnPrev[i] && btn[i])
					hadrelease = -1;
				if (btnPrev[i] && !btn[i] && hadrelease == 0)
					hadrelease = 1;
			}
			// if a button was released, check if others are pressed, and update direction
			if (hadrelease == 1) {
				int to = -1;
				for (int i = 0; i < 4; i++)
					if (btn[i])
						to = i;
				TapTo (to);
			}
			// if a button was pressed, update direction
			for (int i = 0; i < 4; i++) {
				if (!btnPrev[i] && btn[i]) {
					TapTo (i);
					break;
				}
			}
		}

		// check fall (NOTE: "fell" is also used to signify hit by train)
		if (transform.position.y < yFall && fell <= 0f) {
			fell = 1f;
			audios[0].Play ();
		}
		if (fell > 0f) {
			if (!train) fell -= Time.deltaTime;
			else {
				// stick on train, release once we pass the threshold
				transform.position = train.transform.position + trainAnchor;
				if (transform.position.z > borderD.transform.position.z) fell = 0;
			}

			if (fell <= 0f) {
				// unfall
				fell = 0f;
				transform.position = startPos;
				phys.SetVelocity (Vector3.zero);
				dance.ResetMode ();
				spawned = spawnTime;
				spawnTick = 0;
				if (!train) scr.AddScore ("Fell Off", -2000, 2);
				else {
					scr.AddScore ("Train-Struck", -2000, 2);
					borderD.SetActive (true);
				}
				//audios[0].Play ();
			}
		}
		// check x limits
		if (Mathf.Abs (transform.position.x) > xLimit) {

			fell = 0f;
			transform.position = startPos;
			phys.SetVelocity (Vector3.zero);
			dance.ResetMode ();
			spawned = spawnTime;
			spawnTick = 0;
		}

		// respawn anim
		if (spawned > 0f) SpawnAnim ();

		// phases
		if (phase == 0) {
			if (Radio.playing && rdio.beats >= rdio.beatsStart) {
				// start game
				phase = 1;
				speech.SetActive (true);
				speak.SetText (0);
			}
		}
		if (phase == 1) {
			if (!Radio.playing) {
				// end game
				phase = 2;
				speech.SetActive (true);
				speak.SetText (1);
				Invoke (nameof (SaveGame), 2f);
			}
		}

		// poses
		/*if (pose != prevPose || Show != prevShow)*/
		UpdatePose ();

	}

	void Update () {

		//if (Input.GetKeyDown (KeyCode.C)) SpawnConfetti ();

		// exit prompt
		if (Input.GetKeyDown (KeyCode.Escape)) /*if (!Application.isWebPlayer)*/ {
			exitPrompt.SetActive (!exitPrompt.activeSelf);
		}
		if (exitPrompt.activeSelf) {

			Time.timeScale = 0;
			if (exitFull && phase == 1) {
				exitText.text = "Back to menu?";
				exitFull = false;
			}
			if (!exitFull && phase != 1) {
				exitText.text = "Quit?";
				exitFull = true;
			}

		}
		else Time.timeScale = 1;

		// skip if game paused
		if (Time.timeScale <= 0) return;

		if (!canRestart) return;
		// check restart triggers
		bool restartMe = false;
		if (!onMobile) {
			restartMe = Input.GetKeyDown (KeyCode.KeypadEnter)
				|| Input.GetKeyDown (KeyCode.Return)
				|| Input.GetKeyDown (KeyCode.Space);
		}
		else restartMe = Input.GetMouseButtonDown (0) || Input.GetKeyDown (KeyCode.Escape);

		if (restartMe) {
			Radio.playing = false;
			SceneManager.LoadScene (SceneManager.GetActiveScene ().buildIndex);
		}
	}

	public void TapTo (int to) {

		direction = to;
		directionTap = timeTap > 0f && direction != -1;
		if (direction != -1) timeTap = timeForTap;
	}

	public void UpdatePose () {

		if (pose == prevPose && Show == prevShow) return;

		/*foreach (GameObject go in poses)
			go.SetActive (false);
		poses [pose].SetActive (Show);*/
		for (int i = 0; i < poses.Length; i++) poses[i].SetActive (Show && i == pose);
		prevPose = pose;
		prevShow = Show;

	}

	void SpawnAnim () {

		int tick = Mathf.FloorToInt (Time.time * spawnFlashFreq);
		if (tick != spawnTick) {
			spawnTick = tick;
			Show = !Show;
			poses[pose].SetActive (Show || fell > 0);
		}
		spawned -= Time.deltaTime;
		if (spawned <= 0f) {
			Show = true;
			poses[pose].SetActive (Show);
		}

	}

	public void CarHit (int hard = 0) {

		if (spawned > 0f || transform.position.y < yFall) return;

		spawned = spawnTime;
		if (hard == 2) { // train hit
			hitstars.GetComponent<ParticleSystem> ().Emit (starsEmit);
			hitstars.GetComponent<AudioSource> ().Play ();
			if (Mathf.Abs (transform.position.x - train.transform.position.x) < train.trainRadius) {
				borderD.SetActive (false);
				fell = 1f;
				trainAnchor = transform.position - train.transform.position;
			}
			else {
				scr.AddScore ("Train-Pushed", -500, 2);
				var npos = transform.position;
				if (npos.x > train.transform.position.x)
					npos.x = train.transform.position.x + train.trainRadius;
				else npos.x = train.transform.position.x - train.trainRadius;
				transform.position = npos;
			}
		}
		else if (hard == 1) { // car hit, hard
			hitstars.GetComponent<ParticleSystem> ().Emit (starsEmit);
			hitstars.GetComponent<AudioSource> ().Play ();
			if (dance.mode > 1) scr.AddScore ("Hit & Interrupt", -800, 2);
			else scr.AddScore ("Hit", -400, 2);
		}
		else if (dance.mode > 1) { // car hit, soft
			audios[2].Play ();
			scr.AddScore ("Interrupt", -200, 2);
		}
		dance.ResetMode ();

	}

	void SaveGame () {

		float delaynewmove = 1.5f;

		if (dance.style == 0) {

			if (scr.scr.scoreGo > SaveLoad.hiscore) {
				// new hiscore
				SaveLoad.hiscore = scr.scr.scoreGo;
				speech.SetActive (true);
				speak.PinText (2);
			}
			else {
				// say current hiscore
				SayHiScore ();
			}

			// check if lv 2 unlocked
			bool lv2unlocked = (/*SaveLoad.unlocks >= 7 &&*/ SaveLoad.totalscore >= LevelSelect.scoreToUnlockLv2);
			//var prevTotal = SaveLoad.totalscore;
			SaveLoad.AddToTotal ((uint)scr.scr.scoreGo);
			//Debug.Log ("prev total = " + prevTotal + ", new total = " + SaveLoad.totalscore);
			if (!lv2unlocked && /* SaveLoad.unlocks >= 7 &&*/ SaveLoad.totalscore >= LevelSelect.scoreToUnlockLv2) {
				Invoke (nameof (NewLevel), delaynewmove);
				delaynewmove += 3f;
			}
		}
		else {
			if (scr.scr.scoreGo > SaveLoad.hiscoreLv2) {
				// new hiscore
				SaveLoad.hiscoreLv2 = scr.scr.scoreGo;
				speech.SetActive (true);
				speak.PinText (2);
			}
			else {
				// say current hiscore
				SayHiScore ();
			}

			/*var prevTotal = SaveLoad.totalscore;
			SaveLoad.AddToTotal ((uint)scr.scr.scoreGo);
			Debug.Log ("prev total = " + prevTotal + ", new total = " + SaveLoad.totalscore);
			if (SaveLoad.unlocks >= 7 && SaveLoad.totalscore >= LevelSelect.scoreToUnlockLv2) {
				Invoke ("newlevel", delaynewmove);
				delaynewmove += 3f;
			}*/
		}

		// count plays
		SaveLoad.plays++;

		// unlock new move
		if (dance.style == 0) {
			if (SaveLoad.unlocks < 7) {
				SaveLoad.unlocks++;
				Invoke (nameof (NewMove), delaynewmove);
			}
		}
		else {
			if (SaveLoad.unlocksLv2 < 7) {
				SaveLoad.unlocksLv2++;
				Invoke (nameof (NewMove), delaynewmove);
			}
		}

		// update hiscore
		RefreshMenuHiscore ();
		//menu.SetActive (true);
		canRestart = true;
		if (!onMobile)
			GameObject.Find ("Message").GetComponent<Message> ().SetMessage ("Press Enter", 0, 0, true);
		else {
			exitPrompt.SetActive (false);
			GameObject.Find ("Message").GetComponent<Message> ().SetMessage ("Tap Screen", 0, 0, true);
		}
		SaveLoad.Save ();
	}

	void SayHiScore () {
		speech.SetActive (true);
		if (dance.style == 0) speak.PinText (3, SaveLoad.hiscore.ToString ());
		else speak.PinText (3, SaveLoad.hiscoreLv2.ToString ());
	}

	void NewMove () {
		speech.SetActive (true);
		if (dance.style == 0) speak.PinText (4, moveNames[SaveLoad.unlocks - 2]);
		else speak.PinText (4, moveNamesLv2[SaveLoad.unlocksLv2 - 2]);
	}

	void NewLevel () {
		speech.SetActive (true);
		speak.PinText (5);
		SpawnConfetti ();
	}

	void RefreshMenuHiscore () {

		if (dance.style == 0) {
			if (SaveLoad.hiscore <= 0) menuHiScore.text = string.Empty;
			else menuHiScore.text = "Lv.1 Hiscore: " + SaveLoad.hiscore;
		}
		else {
			if (SaveLoad.hiscoreLv2 <= 0) menuHiScore.text = string.Empty;
			else menuHiScore.text = "Lv.2 Hiscore: " + SaveLoad.hiscoreLv2;
		}
	}

	public void ToStart (bool atShowPos = false) {

		pose = 0;
		var pos = startPosReal;
		if (atShowPos) pos.z = 40f;
		if ((transform.position - pos).sqrMagnitude > 1f) transform.position = pos;
		phys.SetVelocity (Vector3.zero);
		dance.ResetMode ();
	}

	public void SpawnConfetti () {

		var go = Instantiate (confetti, transform.position - Vector3.right * 2,
			confetti.transform.rotation);
		Instantiate (confetti, transform.position + Vector3.right * 2,
			confetti.transform.rotation);
		go.GetComponent<AudioSource> ().Play ();
	}
}
