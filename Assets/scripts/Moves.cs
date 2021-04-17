using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Moves : MonoBehaviour {

	public static Moves me;
	public Player player;
	private Text labeltext;

	[Tooltip("arrow images to update while playing move")]
	public Image[] arrows;
	// buttons to press, and respective durations
	private int[] buttons;
	private float[] buttonDurations;

	private int button;
	private float timer;
	private bool playing;
	public GameObject movesGO;
	public GameObject movelabelGO;
	//public GameObject helperGO;
	public GameObject dependable;
	public static bool hideDependable = false;

	void Awake () {

		me = this; 
		//player = GameObject.Find ("Man").GetComponent<Player> ();

		//Transform tr = transform.Find ("label");
		labeltext = movelabelGO.transform.Find ("text").GetComponent<Text>();
		if (hideDependable && dependable) dependable.SetActive (false);
	}

	/*private void OnEnable () {
		if (dependable) dependable.SetActive (true);
	}
	private void OnDisable () {
		if (dependable) dependable.SetActive (false);
	}*/

	void FixedUpdate () {

		if (Time.timeScale <= 0) return;
		if (!playing) return;

		/*if (dependable && dependable.activeSelf) {
			if (helperGO.activeSelf || movesGO.activeSelf) dependable.SetActive (false);
		}*/

		// playback move
		if (timer <= 0f) {
			button++;
			if (button >= buttons.Length) {
				Done ();
				return;
			}
			timer = buttonDurations [button];
			player.TapTo (buttons [button]);
			for (int i = 0; i < 4; i++) arrows [i].enabled = false;
			if (buttons [button] != -1) arrows [buttons [button]].enabled = true;
		}
		else timer -= Time.deltaTime;
	}

	public void PlayMove (int move, int[] Buttons, float[] ButtonDurations) {

		if (move >= player.moveNames.Length) return;
		buttons = Buttons;
		buttonDurations = ButtonDurations;
		button = -1;
		timer = 1f;
		playing = true;
		player.ToStart (true);
		/*foreach (Transform tr in transform) if (tr.name != "arrows") {
			if (tr.name != "label") tr.gameObject.SetActive (false);
			else tr.gameObject.SetActive (true);
		}*/
		movesGO.SetActive (false);
		movelabelGO.SetActive (true);
		int style = player.GetComponent<PlayerDance> ().style;
		if (style == 0) labeltext.text = player.moveNames [move];
		else labeltext.text = player.moveNamesLv2[move];
		for (int i = 0; i < 4; i++) arrows[i].enabled = false;
	}

	public void Done () {

		playing = false;
		player.TapTo (-1);
		player.ToStart ();
		/*foreach (Transform tr in transform) if (tr.name != "arrows") {
			if (tr.name != "label") tr.gameObject.SetActive (true);
			else tr.gameObject.SetActive (false);
		}*/
		movesGO.SetActive (true);
		movelabelGO.SetActive (false);
		for (int i = 0; i < 4; i++) arrows [i].enabled = false;
	}
}
