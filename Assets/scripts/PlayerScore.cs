using UnityEngine;
using System.Collections;

public class PlayerScore : MonoBehaviour {

	public static PlayerScore instance;
	public GameObject score;
	[HideInInspector]
	public Score scr;
	public GameObject timer;
	private Score tmr;
	public GameObject messager;
	private Message msg;

	private Player player;
	//private PlayerPhysics phys;
	private Radio rdio;

	private bool timerOn;
	private bool scoreOn;

	private float timeLastBeep;
	public float scoreBeepDuration = .25f;

	public int carFallScore = 2000;
	public string carFallLabel = "Car Off the Edge";

	void Awake () {
		instance = this;
	}

	void Start () {

		player = GetComponent<Player> ();
		//phys = GetComponent<PlayerPhysics> ();
		rdio = player.rdio;

		scr = score.GetComponent<Score> ();
		tmr = timer.GetComponent<Score> ();
		msg = messager.GetComponent<Message> ();
	}

	void Update () {

		// skip if paused or not playing
		if (Time.timeScale <= 0) return;
		if (!Radio.playing) return;

		// enable timer display after first 3 beats
		if (rdio.beats >= rdio.beatsStart + 3f && !timerOn) {
			timerOn = true;
			timer.SetActive (true);
		}
		// enable score display after first 6 beats
		if (rdio.beats >= rdio.beatsStart + 6f && !scoreOn) {
			scoreOn = true;
			score.SetActive (true);
		}
		// update timer text
		if (timerOn) tmr.scoreGo = Mathf.FloorToInt (Time.time - rdio.playTime);
	}

	public void AddScore (string text, int x, int col) {

		// ignore if not playing
		if (player.phase != 1) return;

		// add score
		scr.scoreGo = Mathf.Max (0, scr.scoreGo + x);
		if (text != string.Empty) {
			// set message
			msg.SetMessage (text, x, col);
			// play score sound
			if (col != 2 && Time.time > timeLastBeep + scoreBeepDuration) {
				timeLastBeep = Time.time;
				player.audios[1].Play ();
			}
		}
	}
}
