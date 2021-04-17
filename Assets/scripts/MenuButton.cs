using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class MenuButton : MonoBehaviour {

	[Tooltip("operation to perform when pressed (see script)")]
	public int operation = 0;

	// current player style
	private int style = 0;
	// help messages script
	private Helper help;
	private bool disabled;
	private Moves moves;
	public static Player player;

	[Tooltip("button presses shown when pressed (for dancemove buttons)")]
	public int[] buttons;
	[Tooltip ("duration for each button press (for dancemove buttons)")]
	public float[] buttonDurations;

	public static bool cheatsEnabled = false;
	private int cheatCounter;

	void Start () {

		// add listener to button
		Button btn = GetComponent<Button> ();
		btn.onClick.AddListener (Operate);
		
		// generate colors dynamically (remnant from before switching to new Unity UI)
		var bc = btn.colors;
		bc.highlightedColor = new Color (.8f, .8f, .8f);
		bc.pressedColor = new Color (.7f, .7f, .7f);
		btn.colors = bc;

		if (!player) player = GameObject.Find ("Man").GetComponent<Player> ();
	}

	private void OnEnable () {

		if (operation > 6) {
			// dancemove button, get text from move
			moves = Moves.me;
			style = player.GetComponent<PlayerDance> ().style;
			if (IsUnlocked (operation, style)) {
				Text mytext = transform.GetChild (0).GetComponent<Text> ();
				//Debug.Log (moves); 
				//Debug.Log (moves.player);
				//Debug.Log (operation - 7);
				if (style == 0) mytext.text = moves.player.moveNames[operation - 7];
				else mytext.text = moves.player.moveNamesLv2[operation - 7];
			}
		}
	}

	private void OnDisable () {

		if (operation == -101 && (SaveLoad.scaleUI == 0 || ScaledUI.scale != SaveLoad.scaleUI * .01f)) {
			if (!SaveLoad.hasLoaded) SaveLoad.Load ();
			SaveLoad.scaleUI = Mathf.RoundToInt (ScaledUI.scale * 100f);
			SaveLoad.Save ();
			//Debug.Log ("Saved UI scale " + SaveLoad.scaleUI);
		}
	}

	public void Operate () {

		if (disabled) return;

		if (operation == -1) {
			// EXIT
			if (player.phase != 1) Application.Quit ();
			else {
				Radio.playing = false;
				/*var score = player.GetComponent<PlayerScore> ().scr;
				if (player.GetComponent<PlayerDance> ().style == 0 && score.scoreGo > 0) {
					var prevTotal = SaveLoad.totalscore;
					SaveLoad.AddToTotal ((uint)score.scoreGo);
					Debug.Log ("prev total = " + prevTotal + ", new total = " + SaveLoad.totalscore);
					SaveLoad.Save ();
				}*/
				SceneManager.LoadScene (SceneManager.GetActiveScene ().buildIndex);
			}
			return;
		}

		AudioSource au = null;
		if (operation < 4) au = transform.parent.GetComponent<AudioSource>();
		else {
			if (transform.parent.parent != null) {
				au = transform.parent.parent.GetComponent<AudioSource> ();
			}
		}
		if (au) au.Play ();

		if (operation <= 6) {
			// STOP MOVE
			Moves.me.Done ();
		}

		if (operation < -100) {

			// Scale up/down
			if (operation == -101) ScaledUI.scale -= ScaledUI.delta;
			else ScaledUI.scale += ScaledUI.delta;
			ScaledUI.scale = Mathf.Clamp (ScaledUI.scale, ScaledUI.minScale, ScaledUI.maxScale);
			ScaledUI.Refresh ();
		}
		else if (operation == 1) {
			
			// PLAY
			disabled = true;
			transform.parent.Find ("moves").gameObject.SetActive (false);
			transform.parent.Find ("helper").gameObject.SetActive (false);
			LevelSelect.me.gameObject.SetActive (false);
			if (Moves.me && Moves.me.dependable) {
				Moves.me.dependable.SetActive (false);
				Moves.hideDependable = true;
			}
			Invoke (nameof (ToPlay), .25f);
		}
		else if (operation == 2) {

			// MOVES
			transform.parent.Find ("moves").gameObject.SetActive (true);
			transform.parent.Find ("helper").gameObject.SetActive (false);
			//transform.parent.Find ("warning").gameObject.SetActive (false);
			LevelSelect.me.gameObject.SetActive (false);
			if (Moves.me && Moves.me.dependable) {
				Moves.me.dependable.SetActive (false);
				Moves.hideDependable = true;
			}
		}
		else if (operation == 3) {

			// HELP
			transform.parent.Find ("moves").gameObject.SetActive (false);
			transform.parent.Find ("helper").gameObject.SetActive (true);
			//transform.parent.Find ("warning").gameObject.SetActive (false);
			LevelSelect.me.gameObject.SetActive (false);
			if (Moves.me && Moves.me.dependable) {
				Moves.me.dependable.SetActive (false);
				Moves.hideDependable = true;
			}
		}
		else if (operation == 4) {

			// HELP PREV
			if (!help) help = transform.parent.GetComponent<Helper> ();
			help.Back ();
		}
		else if (operation == 5) {

			// HELP NEXT
			if (!help) help = transform.parent.GetComponent<Helper> ();
			help.Next ();
		}
		else if (operation == 6) {

			// CLOSE
			transform.parent.gameObject.SetActive (false);
		}
		else if (operation > 6) {

			// Move
			if (!IsUnlocked (operation, style)) return;
			moves.PlayMove (operation - 7, buttons, buttonDurations);
		}
		else if (operation == -8) {

			// LEVEL SELECT MENU
			transform.parent.Find ("moves").gameObject.SetActive (false);
			transform.parent.Find ("helper").gameObject.SetActive (false);
			//transform.parent.Find ("warning").gameObject.SetActive (false);
			LevelSelect.me.gameObject.SetActive (true);
			if (Moves.me && Moves.me.dependable) {
				Moves.me.dependable.SetActive (false);
				Moves.hideDependable = true;
			}
			// cheats enabler
			if (!cheatsEnabled) {
				cheatCounter++;
				if (cheatCounter >= 8) {
					cheatsEnabled = true;
					PlayerDance.UnlockAll ();
					transform.parent.Find ("notify").gameObject.SetActive (true);
					LevelSelect.me.gameObject.SetActive (false);
				}
			}
		}
	}

	void Reactivate () {
		gameObject.SetActive (true);
	}

	void ToPlay () {
		disabled = false;
		player.TapTo (-1);
		Radio rdio = GameObject.Find ("radioMesh").GetComponent<Radio> ();
		rdio.PlayMusic ();
		transform.parent.gameObject.SetActive (false);
	}

	bool IsUnlocked (int op, int style = 0) {
		if (cheatsEnabled) return true;
		if (style == 0) return op - 7 + 2 <= SaveLoad.unlocks;
		return op - 7 + 2 <= SaveLoad.unlocksLv2;
	}
}
